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
using Sce.Atf.Applications;
using Sce.Atf.Controls.PropertyEditing;
using Sce.Atf.Dom;

using Sce.Sled.Lua.Dom;
using Sce.Sled.Shared;
using Sce.Sled.Shared.Dom;
using Sce.Sled.Shared.Services;

namespace Sce.Sled.Lua
{
    [Export(typeof(IInitializable))]
    [Export(typeof(ISledLuaFunctionParserService))]
    [Export(typeof(SledLuaFunctionParserService))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    sealed class SledLuaFunctionParserService : IInitializable, ISledLuaFunctionParserService
    {
        [ImportingConstructor]
        public SledLuaFunctionParserService(ISettingsService settingsService)
        {
            var verboseSetting =
                new BoundPropertyDescriptor(
                    this,
                    () => Verbose,
                    Resources.Resource.Verbose,
                    Resources.Resource.LuaFunctionParser,
                    Resources.Resource.Verbose);

            settingsService.RegisterSettings(this, verboseSetting);
            settingsService.RegisterUserSettings(SledLuaSettings.Category, verboseSetting);
        }

        #region IInitializable Interface

        void IInitializable.Initialize()
        {
            m_projectService.Closing += ProjectServiceClosing;

            m_luaVariableParserService.ParsingFile += LuaVariableParserServiceParsingFile;
            m_luaVariableParserService.ParsedFile += LuaVariableParserServiceParsedFile;
        }

        #endregion

        #region Persisted Settings

        public bool Verbose { get; set; }

        #endregion

        #region ISledLuaFunctionParserService Interface

        public string LookUpFunction(string funcName)
        {
            // Sometimes Lua can't find the name of a function and the runtime will tag
            // the function name it sends to SLED so that SLED can fill in the right name
            // 
            // The tag looks like:
            // ":<line>:<source>"

            if (string.IsNullOrEmpty(funcName))
                return string.Empty;

            if (funcName[0] != ':')
                return funcName;

            // Look up the function name from the cached list of functions
            try
            {
                var iPos = funcName.IndexOf(':', 1);

                // Some weird format encountered
                if (iPos == -1)
                    return Resources.Resource.Unknown;

                var szLineNumber = funcName.Substring(1, iPos - 1);
                var szSource = funcName.Substring(iPos + 1).Replace('/', '\\');

                int iLine;
                if (!int.TryParse(szLineNumber, out iLine))
                    return Resources.Resource.Unknown;

                foreach (var kv in m_dictFuncInfo)
                {
                    // Match source file
                    if (string.Compare(kv.Key.Path, szSource, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        // Line number
                        foreach (var func in kv.Value)
                        {
                            if (func.LineDefined == iLine)
                                return func.Name;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                SledOutDevice.OutLine(
                    SledMessageType.Error,
                    "{0}: Exception looking up cached function " + 
                    "name for string: {1}! Exception was: {2}",
                    this, funcName, ex.Message);
            }

            return Resources.Resource.Unknown;
        }

        public event EventHandler<SledLuaFunctionParserServiceEventArgs> ParsedFunctions;

        #endregion

        #region ISledProjectService Events

        private void ProjectServiceClosing(object sender, SledProjectServiceProjectEventArgs e)
        {
            m_dictFuncInfo.Clear();
        }

        #endregion

        #region ISledLuaVariableParserService Events

        private void LuaVariableParserServiceParsingFile(object sender, SledLuaVariableParserServiceEventArgs e)
        {
            // Remove existing entries for this file
            m_dictFuncInfo.Remove(e.File);
            
            var funcsToRemove = e.File.Functions
                .Where(f => f.Is<SledLuaFunctionType>())
                .ToList();

            // Remove Lua functions from this file
            foreach (var funcToRemove in funcsToRemove)
                e.File.Functions.Remove(funcToRemove);
        }

        private void LuaVariableParserServiceParsedFile(object sender, SledLuaVariableParserServiceEventArgs e)
        {
            var luaFunctions = e.Results
                .Where(r => r.Is<SledLuaVariableParserService.FunctionResult>())
                .Select(
                    result =>
                    {
                        var funcResult = result.As<SledLuaVariableParserService.FunctionResult>();

                        var function =
                            new DomNode(SledLuaSchema.SledLuaFunctionType.Type)
                            .As<SledLuaFunctionType>();

                        function.Name = funcResult.Name;
                        function.LineDefined = funcResult.LineDefined;
                        function.LastLineDefined = funcResult.LastLineDefined;

                        return function;
                    })
                .ToList();

            // Add new entries for this file
            m_dictFuncInfo.Add(e.File, luaFunctions);

            // Add Lua functions to this file
            foreach (var luaFunction in luaFunctions)
                e.File.Functions.Add(luaFunction);

            ParsedFunctions.Raise(
                this,
                new SledLuaFunctionParserServiceEventArgs(e.File, luaFunctions));
        }

        #endregion

#pragma warning disable 649 // Field is never assigned to and will always have its default value null

        [Import]
        private ISledProjectService m_projectService;

        [Import]
        private ISledLuaVariableParserService m_luaVariableParserService;

#pragma warning restore 649

        private readonly Dictionary<SledProjectFilesFileType, List<SledLuaFunctionType>> m_dictFuncInfo =
            new Dictionary<SledProjectFilesFileType, List<SledLuaFunctionType>>();
    }

    internal class SledLuaFunctionParserServiceEventArgs : EventArgs
    {
        public SledLuaFunctionParserServiceEventArgs(SledProjectFilesFileType file, IEnumerable<SledLuaFunctionType> functions)
        {
            File = file;
            Functions = new List<SledLuaFunctionType>(functions);
        }

        public SledProjectFilesFileType File { get; private set; }

        public IEnumerable<SledLuaFunctionType> Functions { get; private set; }
    }

    interface ISledLuaFunctionParserService
    {
        string LookUpFunction(string funcName);

        event EventHandler<SledLuaFunctionParserServiceEventArgs> ParsedFunctions;
    }
}
