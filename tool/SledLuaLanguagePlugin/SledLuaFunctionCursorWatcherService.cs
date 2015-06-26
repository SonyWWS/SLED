/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Windows.Forms;

using Sce.Atf;

using Sce.Sled.Lua.Dom;
using Sce.Sled.Shared.Document;
using Sce.Sled.Shared.Dom;
using Sce.Sled.Shared.Services;

namespace Sce.Sled.Lua
{
    [Export(typeof(IInitializable))]
    [Export(typeof(ISledLuaFunctionCursorWatcherService))]
    [Export(typeof(SledLuaFunctionCursorWatcherService))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    sealed class SledLuaFunctionCursorWatcherService : IInitializable, ISledLuaFunctionCursorWatcherService
    {
        #region IInitializable Interface

        void IInitializable.Initialize()
        {
            m_luaLanguagePlugin = SledServiceInstance.Get<SledLuaLanguagePlugin>();

            m_documentService = SledServiceInstance.Get<ISledDocumentService>();
            m_documentService.Opened += DocumentServiceOpened;
            m_documentService.ActiveDocumentChanged += DocumentServiceActiveDocumentChanged;
            m_documentService.Closing += DocumentServiceClosing;

            m_projectService = SledServiceInstance.Get<ISledProjectService>();
            m_projectService.FileOpened += ProjectServiceFileOpened;
            m_projectService.FileRemoving += ProjectServiceFileRemoving;

            m_luaFunctionParserService = SledServiceInstance.Get<ISledLuaFunctionParserService>();
            m_luaFunctionParserService.ParsedFunctions += LuaFunctionParserServiceParsedFunctions;
        }

        #endregion

        #region ISledLuaFunctionCursorWatcherService Interface

        public event EventHandler<SledLuaFunctionCursorWatcherServiceEventArgs> CursorFunctionChanged;

        public SledLuaFunctionType GetCurrentCursorFunction(ISledDocument sd)
        {
            if (sd == null)
                return null;

            // No project file to contain functions
            if (sd.SledProjectFile == null)
                return null;

            // No functions parsed
            if (m_dictFunctions.Count <= 0)
                return null;

            // No parsed functions for project file
            List<SledLuaFunctionType> lstLuaFuncs;
            if (!m_dictFunctions.TryGetValue(sd.SledProjectFile, out lstLuaFuncs))
                return null;

            var lineNumber = sd.Editor.CurrentLineNumber;

            // Figure out which function the cursor is in
            var lstPotentialFuncs = GetPotentialFunctions(lineNumber, lstLuaFuncs).ToList();

            var cursorFunc = GetClosestFromPotentialFunctions(lineNumber, lstPotentialFuncs);

            return cursorFunc;
        }

        #endregion

        #region ISledDocumentService Events

        private void DocumentServiceOpened(object sender, SledDocumentServiceEventArgs e)
        {
            RegisterDocument(e.Document);
        }

        private void DocumentServiceActiveDocumentChanged(object sender, EventArgs e)
        {
            var sd = m_documentService.ActiveDocument;

            // When closing a document and no others are open
            // m_documentService.ActiveDocument will be null.
            //
            // There's an issue when the following occurs:
            // * SLED hits a breakpoint in a specific file and on a specific line
            // * The user closes the file (and it's the only file open)
            // * The user continues (F5) and SLED hits the same breakpoint as previously hit
            // * The GUI shows no function in the function toolbar because FindFunction
            //   sees that the last function is equal to the current function (it
            //   doesn't know that the file was closed/re-opened)
            // To get around this we set last function to null when all documents are closed
            if (sd == null)
                m_lastFunc = null;

            if (!IsValidSledDocument(sd))
                return;

            // ReSharper disable PossibleNullReferenceException
            FindFunction(sd, sd.Editor.CurrentLineNumber);
            // ReSharper restore PossibleNullReferenceException
        }

        private void DocumentServiceClosing(object sender, SledDocumentServiceEventArgs e)
        {
            UnregisterDocument(e.Document);
        }

        #endregion

        #region ISledProjectService Events

        private void ProjectServiceFileOpened(object sender, SledProjectServiceFileEventArgs e)
        {
            RegisterDocument(e.File.SledDocument);
        }

        private void ProjectServiceFileRemoving(object sender, SledProjectServiceFileEventArgs e)
        {
            if (m_dictFunctions.Count <= 0)
                return;

            if (!m_dictFunctions.ContainsKey(e.File))
                return;

            m_dictFunctions.Remove(e.File);
        }

        #endregion

        #region ISledLuaFunctionParserService Events

        private void LuaFunctionParserServiceParsedFunctions(object sender, SledLuaFunctionParserServiceEventArgs e)
        {
            // Only refresh what's been updated

            // Remove existing functions for items
            m_dictFunctions.Remove(e.File);

            // Add items and functions back
            m_dictFunctions.Add(e.File, e.Functions.ToList());
        }

        #endregion

        #region Member Methods

        private void RegisterDocument(ISledDocument sd)
        {
            if (!IsValidSledDocument(sd))
                return;

            if (m_registeredDocs.Contains(sd))
                return;

            try
            {
                sd.Editor.CurrentLineNumberChanged += EditorCurrentLineNumberChanged;
            }
            finally
            {
                m_registeredDocs.Add(sd);
            }
        }

        private void UnregisterDocument(ISledDocument sd)
        {
            if (!IsValidSledDocument(sd))
                return;

            if (!m_registeredDocs.Contains(sd))
                return;

            try
            {
                sd.Editor.CurrentLineNumberChanged -= EditorCurrentLineNumberChanged;
            }
            finally
            {
                m_registeredDocs.Remove(sd);
            }
        }

        private bool IsValidSledDocument(ISledDocument sd)
        {
            return
                ((sd != null) &&
                (sd.Editor != null) &&
                (sd.LanguagePlugin != null) &&
                (sd.LanguagePlugin.LanguageId == m_luaLanguagePlugin.LanguageId));
        }

        private void EditorCurrentLineNumberChanged(object sender, EventArgs e)
        {
            var cntrl = sender as Control;
            if (cntrl == null)
                return;

            var sd = cntrl.Tag as ISledDocument;
            if (sd == null)
                return;

            if (!IsValidSledDocument(sd))
                return;

            FindFunction(sd, sd.Editor.CurrentLineNumber);
        }

        private void FindFunction(ISledDocument sd, int lineNumber)
        {
            if (m_dictFunctions.Count <= 0)
                return;

            // Skip non-project files
            if (sd.SledProjectFile == null)
                return;

            List<SledLuaFunctionType> lstLuaFuncs;
            if (!m_dictFunctions.TryGetValue(sd.SledProjectFile, out lstLuaFuncs))
                return;

            // Figure out which function the cursor is in
            var lstPotentialFuncs = GetPotentialFunctions(lineNumber, lstLuaFuncs).ToList();

            var cursorFunc = GetClosestFromPotentialFunctions(lineNumber, lstPotentialFuncs);

            // Fire event if cursor function is different
            if (!ReferenceEquals(cursorFunc, m_lastFunc))
            {
                CursorFunctionChanged.Raise(
                    this,
                    new SledLuaFunctionCursorWatcherServiceEventArgs(
                        sd.SledProjectFile,
                        cursorFunc));
            }

            // Save last function
            m_lastFunc = cursorFunc;
        }

        private static IEnumerable<SledLuaFunctionType> GetPotentialFunctions(int lineNumber, IEnumerable<SledLuaFunctionType> lstFunctions)
        {
            return
                from func in lstFunctions
                where ((lineNumber >= func.LineDefined) &&
                       (lineNumber <= func.LastLineDefined))
                select func;
        }

        private static SledLuaFunctionType GetClosestFromPotentialFunctions(int lineNumber, IList<SledLuaFunctionType> lstFunctions)
        {
            SledLuaFunctionType cursorFunc = null;

            if (lstFunctions.Count == 1)
            {
                cursorFunc = lstFunctions[0];
            }
            else if (lstFunctions.Count > 1)
            {
                cursorFunc = lstFunctions[0];
                var dist = lineNumber - cursorFunc.LineDefined;

                foreach (var luaFunc in lstFunctions)
                {
                    var newDist = lineNumber - luaFunc.LineDefined;
                    if (newDist > dist)
                        continue;

                    cursorFunc = luaFunc;
                    dist = newDist;
                }
            }

            return cursorFunc;
        }

        #endregion

        private ISledDocumentService m_documentService;
        private ISledProjectService m_projectService;
        private SledLuaLanguagePlugin m_luaLanguagePlugin;
        private ISledLuaFunctionParserService m_luaFunctionParserService;

        private SledLuaFunctionType m_lastFunc;

        private readonly List<ISledDocument> m_registeredDocs =
            new List<ISledDocument>();

        private readonly Dictionary<SledProjectFilesFileType, List<SledLuaFunctionType>> m_dictFunctions =
            new Dictionary<SledProjectFilesFileType, List<SledLuaFunctionType>>();
    }

    internal class SledLuaFunctionCursorWatcherServiceEventArgs : EventArgs
    {
        public SledLuaFunctionCursorWatcherServiceEventArgs(SledProjectFilesFileType file, SledLuaFunctionType function)
        {
            File = file;
            Function = function;
        }

        public readonly SledProjectFilesFileType File;
        public readonly SledLuaFunctionType Function;
    }

    internal interface ISledLuaFunctionCursorWatcherService
    {
        SledLuaFunctionType GetCurrentCursorFunction(ISledDocument sd);
        event EventHandler<SledLuaFunctionCursorWatcherServiceEventArgs> CursorFunctionChanged;
    }
}
