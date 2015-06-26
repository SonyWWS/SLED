/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;

using Sce.Atf;
using Sce.Atf.Adaptation;

using Sce.Lua.Utilities;

using Sce.Sled.Shared;
using Sce.Sled.Shared.Dom;
using Sce.Sled.Shared.Plugin;
using Sce.Sled.Shared.Services;
using Sce.Sled.Shared.Utilities;

namespace Sce.Sled.Lua
{
    [Export(typeof(IInitializable))]
    [Export(typeof(ISledLuaVariableParserService))]
    [Export(typeof(SledLuaVariableParserService))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    sealed class SledLuaVariableParserService : IInitializable, ISledLuaVariableParserService
    {
        #region IInitializable Interface

        void IInitializable.Initialize()
        {
            m_languageParserService.RegisterFilesParserFunction(m_luaLanguagePlugin, ParseFiles, this);
            m_languageParserService.FilesParserFinished += LanguageParserServiceFilesParserFinished;

            m_projectService.Opened += ProjectServiceOpened;
            m_projectService.Created += ProjectServiceCreated;     
            m_projectService.FileRemoving += ProjectServiceFileRemoving;
            m_projectService.Closed += ProjectServiceClosed;
        }

        #endregion

        #region ISledLuaVariableParser Interface

        public Dictionary<string, List<VariableResult>> ParsedGlobals
        {
            get { return m_parsedGlobals; }
        }

        public Dictionary<string, List<VariableResult>> ParsedLocals
        {
            get { return m_parsedLocals; }
        }

        public Dictionary<string, List<VariableResult>> ParsedUpvalues
        {
            get { return m_parsedUpvalues; }
        }

        public IEnumerable<int> ValidBreakpointLineNumbers(SledProjectFilesFileType file)
        {
            List<int> lines;
            if (!m_validBreakpoints.TryGetValue(file, out lines))
                yield break;

            foreach (var line in lines)
                yield return line;
        }

        public IEnumerable<SledProjectFilesFileType> AllParsedFiles
        {
            get { return m_parsedResults.Keys; }
        }

        public event EventHandler ParsingFiles;

        public event EventHandler<SledLuaVariableParserServiceEventArgs> ParsingFile;

        public event EventHandler<SledLuaVariableParserServiceEventArgs> ParsedFile;

        public event EventHandler ParsedFiles;

        #endregion

        #region ISledLanguageParserService Events

        private void LanguageParserServiceFilesParserFinished(object sender, SledLanguageParserEventArgs e)
        {
            ParsingFiles.Raise(this, EventArgs.Empty);

            m_parsedGlobals.Clear();
            m_parsedLocals.Clear();
            m_parsedUpvalues.Clear();
            m_validBreakpoints.Clear();

            foreach (var kv in e.FilesAndResults)
            {
                ParsingFile.Raise(
                    this,
                    new SledLuaVariableParserServiceEventArgs(kv.Key, EmptyEnumerable<Result>.Instance));

                List<SledLanguageParserResult> items;
                if (m_parsedResults.TryGetValue(kv.Key, out items))
                {
                    // Clear data on existing key
                    items.Clear();

                    // Add latest results
                    items.AddRange(kv.Value);
                }
                else
                {
                    // Completely new key with new results
                    items = new List<SledLanguageParserResult>(kv.Value);
                    m_parsedResults.Add(kv.Key, items);
                }

                ParsedFile.Raise(
                    this,
                    new SledLuaVariableParserServiceEventArgs(kv.Key, items.Cast<Result>()));
            }

            // Pull out some specific information
            foreach (var kv in m_parsedResults)
            {
                var file = kv.Key;
                var results = kv.Value;

                // Grab all variables
                {
                    var variables = results
                        .Where(item => item.Is<VariableResult>())
                        .Select(item => item.As<VariableResult>());

                    foreach (var variable in variables)
                    {
                        Dictionary<string, List<VariableResult>> container = null;

                        switch (variable.VariableType)
                        {
                            case VariableResultType.Global:
                                container = m_parsedGlobals;
                                break;
                            case VariableResultType.Local:
                                container = m_parsedLocals;
                                break;
                            case VariableResultType.Upvalue:
                                container = m_parsedUpvalues;
                                break;
                        }

                        if (container == null)
                            continue;

                        List<VariableResult> items;
                        if (container.TryGetValue(variable.Name, out items))
                        {
                            items.Add(variable);
                        }
                        else
                        {
                            items = new List<VariableResult> { variable };
                            container.Add(variable.Name, items);
                        }
                    }
                }

                // Grab valid line nubmers for breakpoints
                {
                    var validBpLineNumbers = results
                        .Where(item => item.Is<BreakpointResult>())
                        .Select(item => item.As<BreakpointResult>())
                        .Select(item => item.Line);

                    m_validBreakpoints.Add(file, validBpLineNumbers.ToList());
                }
            }

            ParsedFiles.Raise(this, EventArgs.Empty);
        }

        #endregion

        #region ISledProjectService Events

        private void ProjectServiceCreated(object sender, SledProjectServiceProjectEventArgs e)
        {
            ClearAll();
        }

        private void ProjectServiceOpened(object sender, SledProjectServiceProjectEventArgs e)
        {
            ClearAll();
        }

        private void ProjectServiceFileRemoving(object sender, SledProjectServiceFileEventArgs e)
        {
            m_parsedResults.Remove(e.File);
            m_validBreakpoints.Remove(e.File);
        }

        private void ProjectServiceClosed(object sender, SledProjectServiceProjectEventArgs e)
        {
            ClearAll();
        }

        #endregion

        #region Public Classes

        public abstract class Result : SledLanguageParserResult
        {
            protected Result(ISledLanguagePlugin plugin, SledProjectFilesFileType file, int line)
                : base(plugin, file)
            {
                Line = line;
            }

            public int Line { get; private set; }
        }

        public enum VariableResultType
        {
            Global,
            Local,
            Upvalue,
        }

        public sealed class VariableResult : Result
        {
            public VariableResult(ISledLanguagePlugin plugin, SledProjectFilesFileType file, string name, int line, int occurence, VariableResultType variableType)
                : base(plugin, file, line)
            {
                Name = name;
                Occurence = occurence;
                VariableType = variableType;
            }

            public string Name { get; private set; }

            public int Occurence { get; private set; }

            public VariableResultType VariableType { get; private set; }
        }

        public sealed class FunctionResult : Result
        {
            public FunctionResult(ISledLanguagePlugin plugin, SledProjectFilesFileType file, string name, int lineDefined, int lastLineDefined)
                : base(plugin, file, lineDefined)
            {
                Name = name;
                LineDefined = lineDefined;
                LastLineDefined = lastLineDefined;
            }

            public string Name { get; private set; }

            public int LineDefined { get; private set; }

            public int LastLineDefined { get; private set; }
        }

        public sealed class BreakpointResult : Result
        {
            public BreakpointResult(ISledLanguagePlugin plugin, SledProjectFilesFileType file, int line)
                : base(plugin, file, line)
            {
            }
        }

        #endregion

        #region Member Methods

        private void ClearAll()
        {
            m_parsedResults.Clear();
            m_parsedGlobals.Clear();
            m_parsedLocals.Clear();
            m_parsedUpvalues.Clear();
            m_validBreakpoints.Clear();
        }

        private static IEnumerable<SledLanguageParserResult> ParseFiles(IEnumerable<SledProjectFilesFileType> files, SledLanguageParserVerbosity verbosity, object userData, SledUtil.BoolWrapper shouldCancel)
        {
            SledHiPerfTimer timer = null;

            var enumeratedFiles = new List<SledProjectFilesFileType>(files);

            var results = new List<SledLanguageParserResult>();
            var fileCount = enumeratedFiles.Count;

            try
            {
                if (verbosity > SledLanguageParserVerbosity.None)
                {
                    timer = new SledHiPerfTimer();
                    timer.Start();
                }

                var allWorkItems = new ParserWorkItem[fileCount];

                for (var i = 0; i < fileCount; ++i)
                    allWorkItems[i] = new ParserWorkItem(enumeratedFiles[i], verbosity, userData, shouldCancel);

                var workerCount = Math.Min(ProducerConsumerQueue.WorkerCount, fileCount);
                using (var pcqueue = new ProducerConsumerQueue(workerCount, shouldCancel))
                {
                    pcqueue.EnqueueWorkItems(allWorkItems);
                }

                if (shouldCancel.Value)
                    return EmptyEnumerable<SledLanguageParserResult>.Instance;

                // gather all results from all work items
                foreach (var workItem in allWorkItems)
                    results.AddRange(workItem.Results);
            }
            catch (Exception ex)
            {
                SledOutDevice.OutLine(
                    SledMessageType.Error,
                    "{0}: Exception parsing files: {1}",
                    typeof(SledLuaVariableParserService), ex.Message);
            }
            finally
            {
                if ((timer != null) && (!shouldCancel.Value))
                {
                    SledOutDevice.OutLine(
                        SledMessageType.Info,
                        "[Lua] Parsed {0} files in {1} seconds",
                        fileCount, timer.Elapsed);
                }
            }

            return results;
        }

        #endregion

#pragma warning disable 649 // Field is never assigned to and will always have its default value null

        [Import]
        private ISledProjectService m_projectService;

        [Import]
        private ISledLanguageParserService m_languageParserService;

        [Import]
        private SledLuaLanguagePlugin m_luaLanguagePlugin;

#pragma warning restore 649

        private readonly Dictionary<string, List<VariableResult>> m_parsedGlobals =
            new Dictionary<string, List<VariableResult>>();

        private readonly Dictionary<string, List<VariableResult>> m_parsedLocals =
            new Dictionary<string, List<VariableResult>>();

        private readonly Dictionary<string, List<VariableResult>> m_parsedUpvalues =
            new Dictionary<string, List<VariableResult>>();

        private readonly Dictionary<SledProjectFilesFileType, List<int>> m_validBreakpoints =
            new Dictionary<SledProjectFilesFileType, List<int>>();

        private readonly Dictionary<SledProjectFilesFileType, List<SledLanguageParserResult>> m_parsedResults =
            new Dictionary<SledProjectFilesFileType, List<SledLanguageParserResult>>();

        #region Private Classes

        private class ParserWorkItem : ProducerConsumerQueue.IWork
        {
            public ParserWorkItem(SledProjectFilesFileType file, SledLanguageParserVerbosity verbosity, object userData, SledUtil.BoolWrapper shouldCancel)
            {
                m_file = file;
                m_verbosity = verbosity;
                m_userData = userData;
                m_shouldCancel = shouldCancel;
                Results = new List<SledLanguageParserResult>();
            }

            public void WorkCallback()
            {
                if (m_shouldCancel.Value)
                    return;

                var results = (List<SledLanguageParserResult>)Results;
                var plugin = ((SledLuaVariableParserService)m_userData).m_luaLanguagePlugin;

                try
                {
                    using (var parser = SledLuaVariableParserFactory.Create())
                    {
                        try
                        {
                            parser.LogHandler = LuaLogHandler;

                            if (m_shouldCancel.Value)
                                return;

                            var success = parser.Parse(new Uri(m_file.AbsolutePath));
                            if (!success)
                            {
                                if (m_verbosity > SledLanguageParserVerbosity.Overall)
                                {
                                    var error = parser.Error;
                                    SledOutDevice.OutLine(
                                        SledMessageType.Info,
                                        "{0}: Parse error in \"{1}\": {2}",
                                        typeof(SledLuaVariableParserService), m_file.AbsolutePath, error);
                                }

                                return;
                            }

                            if (m_shouldCancel.Value)
                                return;

                            foreach (var result in parser.Globals)
                            {
                                results.Add(new VariableResult(plugin, m_file, result.Name, result.Line, result.Occurrence, VariableResultType.Global));
                            }

                            foreach (var result in parser.Locals)
                            {
                                results.Add(new VariableResult(plugin, m_file, result.Name, result.Line, result.Occurrence, VariableResultType.Local));
                            }

                            foreach (var result in parser.Upvalues)
                            {
                                results.Add(new VariableResult(plugin, m_file, result.Name, result.Line, result.Occurrence, VariableResultType.Upvalue));
                            }

                            foreach (var result in parser.Functions)
                            {
                                results.Add(new FunctionResult(plugin, m_file, result.Name, result.LineDefined, result.LastLineDefined));
                            }

                            foreach (var line in parser.ValidBreakpointLines)
                            {
                                results.Add(new BreakpointResult(plugin, m_file, line));
                            }
                        }
                        catch (Exception ex)
                        {
                            if (m_verbosity > SledLanguageParserVerbosity.None)
                            {
                                SledOutDevice.OutLine(
                                    SledMessageType.Error,
                                    "{0}: Exception parsing \"{1}\": {2}",
                                    this, m_file.AbsolutePath, ex.Message);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    SledOutDevice.OutLine(
                        SledMessageType.Error,
                        "{0}: Exception creating Lua parser: {1}",
                        this, ex.Message);
                }
            }

            public IEnumerable<SledLanguageParserResult> Results { get; private set; }

            private void LuaLogHandler(string message)
            {
                SledOutDevice.OutLine(SledMessageType.Info, "{0}: {1}", this, message);
            }

            private readonly SledProjectFilesFileType m_file;
            private readonly SledLanguageParserVerbosity m_verbosity;
            private readonly object m_userData;
            private readonly SledUtil.BoolWrapper m_shouldCancel;
        }

        private static class SledLuaVariableParserFactory
        {
            public static ILuaParser Create()
            {
                try
                {
                    if (s_luaVersionService == null)
                        s_luaVersionService = SledServiceInstance.TryGet<ISledLuaLuaVersionService>();

                    if (s_luaVersionService == null)
                        return new Sce.Lua.Utilities.Lua51.x86.LuaParser();

                    switch (s_luaVersionService.CurrentLuaVersion)
                    {
                        case LuaVersion.Lua51: return new Sce.Lua.Utilities.Lua51.x86.LuaParser();
                        case LuaVersion.Lua52: return new Sce.Lua.Utilities.Lua52.x86.LuaParser();
                        default: throw new NullReferenceException("Unknown Lua version!");
                    }
                }
                catch (Exception ex)
                {
                    SledOutDevice.OutLine(
                        SledMessageType.Error,
                        "{0}: Exception creating Lua variable parser: {1}",
                        typeof(SledLuaVariableParserFactory), ex.Message);

                    return null;
                }
            }

            private static ISledLuaLuaVersionService s_luaVersionService;
        }

        #endregion
    }

    internal class SledLuaVariableParserServiceEventArgs : EventArgs
    {
        public SledLuaVariableParserServiceEventArgs(SledProjectFilesFileType file, IEnumerable<SledLuaVariableParserService.Result> results)
        {
            File = file;
            Results = new List<SledLuaVariableParserService.Result>(results);
        }

        public SledProjectFilesFileType File { get; private set; }

        public IEnumerable<SledLuaVariableParserService.Result> Results { get; private set; }
    }

    interface ISledLuaVariableParserService
    {
        Dictionary<string, List<SledLuaVariableParserService.VariableResult>> ParsedGlobals { get; }

        Dictionary<string, List<SledLuaVariableParserService.VariableResult>> ParsedLocals { get; }

        Dictionary<string, List<SledLuaVariableParserService.VariableResult>> ParsedUpvalues { get; }

        IEnumerable<int> ValidBreakpointLineNumbers(SledProjectFilesFileType file);
        
        IEnumerable<SledProjectFilesFileType> AllParsedFiles { get; }
              
        event EventHandler ParsingFiles;

        event EventHandler<SledLuaVariableParserServiceEventArgs> ParsingFile;

        event EventHandler<SledLuaVariableParserServiceEventArgs> ParsedFile;

        event EventHandler ParsedFiles;
    }
}
