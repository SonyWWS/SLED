/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

using Sce.Atf.Adaptation;

using Sce.Sled.Lua.Dom;
using Sce.Sled.Shared.Document;
using Sce.Sled.Shared.Services;

namespace Sce.Sled.Lua
{
    class SledLuaFunctionToolbar : ComboBox, ISledDocumentEmbeddedType
    {
        public SledLuaFunctionToolbar()
        {
            DropDownStyle = ComboBoxStyle.DropDownList;
        }

        #region ISledDocumentEmbeddedType Interface

        public void Initialize(ISledDocument sd)
        {
            m_sd = sd;

            // Default state
            m_bChangingSelection = true;
            Items.Add(NotProjectFile);
            SelectedItem = NotProjectFile;
            Enabled = false;
            m_bChangingSelection = false;

            // Follow user interaction
            SelectedIndexChanged += SledLuaFunctionToolbarSelectedIndexChanged;

            m_gotoService = SledServiceInstance.Get<ISledGotoService>();

            m_projectService = SledServiceInstance.Get<ISledProjectService>();

            m_projectService.FileOpened += ProjectServiceFileOpened;
            m_projectService.Opened += ProjectServiceOpened;
            m_projectService.FileAdded += ProjectServiceFileAdded;
            m_projectService.FileRemoving += ProjectServiceFileRemoving;

            m_luaFunctionParserService = SledServiceInstance.Get<ISledLuaFunctionParserService>();
            m_luaFunctionParserService.ParsedFunctions += LuaFunctionParserServiceParsedFunctions;

            m_luaFunctionCursorWatcherService = SledServiceInstance.Get<ISledLuaFunctionCursorWatcherService>();
            m_luaFunctionCursorWatcherService.CursorFunctionChanged += LuaFunctionCursorWatcherServiceCursorFunctionChanged;
        }

        void ISledDocumentEmbeddedType.Shown()
        {
        }

        void ISledDocumentEmbeddedType.Closing()
        {
        }

        #endregion

        #region ISledProjectService Events

        private void ProjectServiceFileOpened(object sender, SledProjectServiceFileEventArgs e)
        {
            // Check if we're opening our file or not
            if (e.File.SledDocument != m_sd)
                return;

            UpdateFunctions();
        }

        private void ProjectServiceOpened(object sender, SledProjectServiceProjectEventArgs e)
        {
            // If file is already opened when the project
            // is opened we need to fill in function list
            
            // File not in project
            if (m_sd.SledProjectFile == null)
                return;

            UpdateFunctions();
            SelectFunction();
        }

        private void ProjectServiceFileAdded(object sender, SledProjectServiceFileEventArgs e)
        {
            if (e.File.SledDocument != m_sd)
                return;

            UpdateFunctions();
            SelectFunction();
        }

        private void ProjectServiceFileRemoving(object sender, SledProjectServiceFileEventArgs e)
        {
            if (e.File.SledDocument != m_sd)
                return;

            m_bChangingSelection = true;
            Items.Clear();
            Items.Add(NotProjectFile);
            SelectedItem = NotProjectFile;
            Enabled = false;
            m_bChangingSelection = false;
        }

        #endregion

        #region ISledLuaFunctionParserService Events

        private void LuaFunctionParserServiceParsedFunctions(object sender, SledLuaFunctionParserServiceEventArgs e)
        {
            if (m_sd == null)
                return;

            if (m_sd.SledProjectFile == null)
                return;

            // Only care about our particular project file
            if (!m_sd.SledProjectFile.Equals(e.File))
                return;

            UpdateFunctions(e.Functions);
            SelectFunction();
        }

        #endregion

        #region ISledLuaFunctionCursorWatcherService Events

        private void LuaFunctionCursorWatcherServiceCursorFunctionChanged(object sender, SledLuaFunctionCursorWatcherServiceEventArgs e)
        {
            try
            {
                m_bChangingSelection = true;

                // Not a project file
                if (e.File == null)
                    return;

                if (e.File.SledDocument == null)
                    return;

                // Not a document we care about
                if (e.File.SledDocument != m_sd)
                    return;

                // Grab any functions in the list
                var items = Items.OfType<FunctionAssoc>().ToList();

                // No functions in the list
                if (!items.Any())
                {
                    SelectedItem = null;
                    return;
                }

                // Want to make e.Function the selection so
                // find e.Function in the list of functions
                var cursor = items.FirstOrDefault(func => ReferenceEquals(func.Function, e.Function));

                // Item already selected
                if (SelectedItem == cursor)
                    return;

                // Update selected item to the proper function (or null)
                SelectedItem = cursor;
            }
            finally
            {
                m_bChangingSelection = false;
            }
        }

        #endregion

        #region Private Classes

        private class FunctionAssoc
        {
            public FunctionAssoc(SledLuaFunctionType function)
            {
                Function = function;
            }

            public override string ToString()
            {
                return Function.Name;
            }

            public SledLuaFunctionType Function { get; private set; }
        }

        private class FunctionComparer : IComparer<SledLuaFunctionType>
        {
            public int Compare(SledLuaFunctionType func1, SledLuaFunctionType func2)
            {
                if (func1.LineDefined == func2.LineDefined)
                    return string.Compare(func1.Name, func2.Name, StringComparison.Ordinal);
                if (func1.LineDefined < func2.LineDefined)
                    return -1;
                return 1;
            }
        }

        #endregion

        #region Member Methods

        private void UpdateFunctions()
        {
            UpdateFunctions(null);
        }

        private void UpdateFunctions(IEnumerable<SledLuaFunctionType> functions)
        {
            Items.Clear();
            Enabled = true;

            var luaFuncs = new List<SledLuaFunctionType>();

            if (functions == null)
            {
                luaFuncs.AddRange(
                    m_sd.SledProjectFile.Functions
                    .Where(f => f.Is<SledLuaFunctionType>())
                    .Select(f => f.As<SledLuaFunctionType>()));
            }
            else
            {
                luaFuncs.AddRange(functions);
            }

            // Sort
            luaFuncs.Sort(m_comparer);

            // Add functions to GUI
            foreach (var luaFunc in luaFuncs)
                Items.Add(new FunctionAssoc(luaFunc));
        }

        private void SelectFunction()
        {
            //
            // Try and select a function based
            // on the current cursor position
            //

            try
            {
                m_bChangingSelection = true;

                var cursorFunc = m_luaFunctionCursorWatcherService.GetCurrentCursorFunction(m_sd);
                if (cursorFunc == null)
                    return;

                var assocs = Items.OfType<FunctionAssoc>().ToList();
                if (!assocs.Any())
                    return;

                var assoc = assocs.FirstOrDefault(func => ReferenceEquals(func.Function, cursorFunc));

                SelectedItem = assoc;
            }
            finally
            {
                m_bChangingSelection = false;
            }
        }

        private void SledLuaFunctionToolbarSelectedIndexChanged(object sender, EventArgs e)
        {
            if (m_bChangingSelection)
                return;

            var selection = SelectedItem;
            if (selection == null)
                return;

            var assoc = selection.As<FunctionAssoc>();
            if (assoc == null)
                return;

            // Jump to function
            m_gotoService.GotoLine(m_sd, assoc.Function.LineDefined, false);

            // Take focus off of toolbar
            m_sd.Editor.Control.Focus();
        }

        #endregion
        
        private ISledDocument m_sd;
        private bool m_bChangingSelection;

        private readonly FunctionComparer m_comparer =
            new FunctionComparer();

        private const string NotProjectFile = "Not a project file";

        private ISledGotoService m_gotoService;
        private ISledProjectService m_projectService;
        private ISledLuaFunctionParserService m_luaFunctionParserService;
        private ISledLuaFunctionCursorWatcherService m_luaFunctionCursorWatcherService;
    }
}