/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;

using Sce.Atf;

using Sce.Lua.Utilities;

using Sce.Sled.Shared;
using Sce.Sled.Shared.Dom;
using Sce.Sled.Shared.Services;
using Sce.Sled.Shared.Utilities;

namespace Sce.Sled.Lua
{
    [Export(typeof(IInitializable))]
    [Export(typeof(SledLuaSyntaxCheckerService))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    sealed class SledLuaSyntaxCheckerService : IInitializable
    {
        #region IInitializable Interface

        void IInitializable.Initialize()
        {
            m_syntaxCheckerService.RegisterFilesCheckFunction(m_luaLanguagePlugin, CheckFiles, this);
            m_syntaxCheckerService.RegisterStringCheckFunction(m_luaLanguagePlugin, CheckString, this);
        }

        #endregion

        internal static bool TestISledCanPluginBeInstantiated()
        {
            return !CheckString("function test() end", SledSyntaxCheckerVerbosity.None, null).Any();
        }

        private static IEnumerable<SledSyntaxCheckerEntry> CheckString(string value, SledSyntaxCheckerVerbosity verbosity, object userData)
        {
            var errors = new List<SledSyntaxCheckerEntry>();

            try
            {
                using (var syntaxChecker = SledLuaSyntaxCheckerFactory.Create())
                {
                    try
                    {
                        var success = syntaxChecker.CheckBuffer(value);
                        if (!success)
                        {
                            var errorString = syntaxChecker.Error;
                            if (!string.IsNullOrEmpty(errorString))
                            {
                                var plugin = ((SledLuaSyntaxCheckerService)userData).m_luaLanguagePlugin;

                                // Fix up error string
                                var colon = errorString.IndexOf(':');
                                if (colon != -1)
                                {
                                    colon = errorString.IndexOf(':', colon + 1);
                                    if (colon != -1)
                                        errorString = errorString.Substring(colon + 1).Trim();
                                }

                                var errorEntry = new SledSyntaxCheckerEntry(plugin, null, 1, errorString);
                                errors.Add(errorEntry);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        if (verbosity > SledSyntaxCheckerVerbosity.None)
                        {
                            SledOutDevice.OutLine(
                                SledMessageType.Error,
                                "{0}: Exception syntax checking string \"{1}\": {2}",
                                typeof(SledLuaSyntaxCheckerService), value, ex.Message);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                SledOutDevice.OutLine(
                    SledMessageType.Error,
                    "{0}: Exception creating syntax checker: {1}",
                    typeof(SledLuaSyntaxCheckerService), ex.Message);
            }

            return errors;
        }

        private static IEnumerable<SledSyntaxCheckerEntry> CheckFiles(IEnumerable<SledProjectFilesFileType> files, SledSyntaxCheckerVerbosity verbosity, object userData, SledUtil.BoolWrapper shouldCancel)
        {
            SledHiPerfTimer timer = null;

            var enumeratedFiles = new List<SledProjectFilesFileType>(files);
            
            var errors = new List<SledSyntaxCheckerEntry>();
            var fileCount = enumeratedFiles.Count;

            try
            {
                if (verbosity > SledSyntaxCheckerVerbosity.None)
                {
                    timer = new SledHiPerfTimer();
                    timer.Start();
                }
                
                var allWorkItems = new SyntaxCheckerWorkItem[fileCount];

                for (var i = 0; i < fileCount; ++i)
                    allWorkItems[i] = new SyntaxCheckerWorkItem(enumeratedFiles[i], verbosity, userData, shouldCancel);

                var workerCount = Math.Min(ProducerConsumerQueue.WorkerCount, fileCount);
                using (var pcQueue = new ProducerConsumerQueue(workerCount, shouldCancel))
                {
                    pcQueue.EnqueueWorkItems(allWorkItems);
                }

                if (shouldCancel.Value)
                    return EmptyEnumerable<SledSyntaxCheckerEntry>.Instance;

                // gather all results from all work items
                foreach (var workItem in allWorkItems)
                    errors.AddRange(workItem.Errors);
            }
            catch (Exception ex)
            {
                SledOutDevice.OutLine(
                    SledMessageType.Error,
                    "{0}: Exception syntax checking files: {1}",
                    typeof(SledLuaSyntaxCheckerService), ex.Message);
            }
            finally
            {
                if ((timer != null) && (!shouldCancel.Value))
                {
                    SledOutDevice.OutLine(
                        SledMessageType.Info,
                        "[Lua] Syntax checked {0} files in {1} seconds",
                        fileCount, timer.Elapsed);
                }
            }

            return errors;
        }

#pragma warning disable 649 // Field is never assigned to and will always have its default value null

        [Import]
        private ISledSyntaxCheckerService m_syntaxCheckerService;

        [Import]
        private SledLuaLanguagePlugin m_luaLanguagePlugin;

#pragma warning restore 649

        #region Private Classes

        private class SyntaxCheckerWorkItem : ProducerConsumerQueue.IWork
        {
            public SyntaxCheckerWorkItem(SledProjectFilesFileType file, SledSyntaxCheckerVerbosity verbosity, object userData, SledUtil.BoolWrapper shouldCancel)
            {
                m_file = file;
                m_verbosity = verbosity;
                m_userData = userData;
                m_shouldCancel = shouldCancel;
                Errors = new List<SledSyntaxCheckerEntry>();
            }

            public void WorkCallback()
            {
                if (m_shouldCancel.Value)
                    return;

                try
                {
                    using (var syntaxChecker = SledLuaSyntaxCheckerFactory.Create())
                    {
                        try
                        {
                            if (m_shouldCancel.Value)
                                return;

                            var success = syntaxChecker.CheckFile(new Uri(m_file.AbsolutePath));
                            if (success)
                                return;

                            // Grab error from actual syntax checker
                            var errorString = syntaxChecker.Error;
                            if (string.IsNullOrEmpty(errorString))
                                return;

                            var plugin = ((SledLuaSyntaxCheckerService)m_userData).m_luaLanguagePlugin;
                            var errors = (List<SledSyntaxCheckerEntry>)Errors;

                            var line = -1;

                            // Try and find the file name in the string. The full path
                            // can become truncated so looking for it may be faulty.
                            var iPos = errorString.IndexOf(m_file.Name, StringComparison.Ordinal);
                            if (iPos != -1)
                            {
                                // Now look for the first colon following the file name
                                var iColon = errorString.IndexOf(':', iPos + m_file.Name.Length);
                                if (iColon != -1)
                                {
                                    // Strip down to line number & error
                                    errorString = errorString.Remove(0, iColon + 1);

                                    // Find next colon to get line number from error
                                    iColon = errorString.IndexOf(':');
                                    if (iColon != -1)
                                    {
                                        int iLine;
                                        if (int.TryParse(errorString.Substring(0, iColon), out iLine))
                                            line = iLine;

                                        errorString = errorString.Substring(iColon + 1).Trim();
                                    }
                                }
                            }

                            errors.Add(new SledSyntaxCheckerEntry(plugin, m_file, line, errorString));

                            if (m_verbosity > SledSyntaxCheckerVerbosity.Overall)
                            {
                                SledOutDevice.OutLine(
                                    SledMessageType.Info,
                                    "[Lua] Syntax error in {0} on line {1}: {2}",
                                    m_file.Name, line, errorString);
                            }
                        }
                        catch (Exception ex)
                        {
                            if (m_verbosity > SledSyntaxCheckerVerbosity.None)
                            {
                                SledOutDevice.OutLine(
                                    SledMessageType.Error,
                                    "{0}: Exception syntax checking \"{1}\": {2}",
                                    this, m_file.AbsolutePath, ex.Message);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    SledOutDevice.OutLine(
                        SledMessageType.Error,
                        "{0}: Exception creating Lua syntax checker: {1}",
                        this, ex.Message);
                }
            }

            public IEnumerable<SledSyntaxCheckerEntry> Errors { get; private set; }

            private readonly SledProjectFilesFileType m_file;
            private readonly SledSyntaxCheckerVerbosity m_verbosity;
            private readonly object m_userData;
            private readonly SledUtil.BoolWrapper m_shouldCancel;
        }

        private static class SledLuaSyntaxCheckerFactory
        {
            public static ILuaSyntaxChecker Create()
            {
                try
                {
                    if (s_luaVersionService == null)
                        s_luaVersionService = SledServiceInstance.TryGet<ISledLuaLuaVersionService>();

                    if (s_luaVersionService == null)
                        return new Sce.Lua.Utilities.Lua51.x86.LuaSyntaxChecker();

                    switch (s_luaVersionService.CurrentLuaVersion)
                    {
                        case LuaVersion.Lua51: return new Sce.Lua.Utilities.Lua51.x86.LuaSyntaxChecker();
                        case LuaVersion.Lua52: return new Sce.Lua.Utilities.Lua52.x86.LuaSyntaxChecker();
                        default: throw new NullReferenceException("Unknown Lua version!");
                    }
                }
                catch (Exception ex)
                {
                    SledOutDevice.OutLine(
                        SledMessageType.Error,
                        "{0}: Exception creating Lua sytnax checker: {1}",
                        typeof(SledLuaSyntaxCheckerFactory), ex.Message);

                    return null;
                }
            }

            private static ISledLuaLuaVersionService s_luaVersionService;
        }

        #endregion
    }
}
