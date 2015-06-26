/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using Sce.Atf;
using Sce.Atf.Adaptation;
using Sce.Atf.Applications;
using Sce.Atf.Controls;
using Sce.Atf.Dom;

using Sce.Sled.Lua.Dom;
using Sce.Sled.Lua.Resources;
using Sce.Sled.Shared.Controls;
using Sce.Sled.Shared.Dom;
using Sce.Sled.Shared.Services;
using Sce.Sled.Shared.Utilities;

namespace Sce.Sled.Lua
{
    [Export(typeof(IInitializable))]
    [Export(typeof(ISledLuaCallStackService))]
    [Export(typeof(SledLuaCallStackService))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    sealed class SledLuaCallStackService : IInitializable, ISledLuaCallStackService
    {
        #region IInitializable Interface

        void IInitializable.Initialize()
        {
            PreviousStackLevel = -1;

            m_luaLanguagePlugin = SledServiceInstance.Get<SledLuaLanguagePlugin>();

            m_projectService.Created += ProjectServiceCreated;
            m_projectService.Opened += ProjectServiceOpened;
            m_projectService.Closing += ProjectServiceClosing;

            m_debugService = SledServiceInstance.Get<ISledDebugService>();
            m_debugService.UpdateBegin += DebugServiceUpdateBegin;
            m_debugService.DataReady += DebugServiceDataReady;
            m_debugService.UpdateEnd += DebugServiceUpdateEnd;
            m_debugService.Disconnected += DebugServiceDisconnected;

            m_luaFunctionCursorWatcherService = SledServiceInstance.Get<ISledLuaFunctionCursorWatcherService>();
            m_luaFunctionCursorWatcherService.CursorFunctionChanged += LuaFunctionCursorWatcherServiceCursorFunctionChanged;

            m_callStackEditor.MouseClick += CallStackEditorMouseClick;
            m_callStackEditor.MouseDoubleClick += CallStackEditorMouseDoubleClick;

            // Adjust to shortened name if Lua language plugin is the only one loaded
            if (m_languagePluginService.Value.Count == 1)
                m_callStackEditor.Name = Localization.SledLuaCallStackTitleShort;

            //m_liveConnectService = SledServiceInstance.TryGet<ISledLiveConnectService>();
            //if (m_liveConnectService != null)
            //{
            //}
        }

        #endregion

        #region ISledLuaCallStackService Interface

        public SledCallStackType this[int iStackLevel]
        {
            get
            {
                return
                    m_callStackCollection == null
                        ? null
                        : m_callStackCollection.CallStack[iStackLevel];
            }
        }

        public int CurrentStackLevel
        {
            get { return m_curStackLevel; }
            set { PreviousStackLevel = CurrentStackLevel; m_curStackLevel = value; }
        }

        public int PreviousStackLevel { get; private set; }

        /// <summary>
        /// Event fired when the call stack is about to be cleared
        /// </summary>
        public event EventHandler Clearing;

        /// <summary>
        /// Event fired when the call stack has been cleared
        /// </summary>
        public event EventHandler Cleared;

        /// <summary>
        /// Event fired when a new stack level is being added to the call stack
        /// </summary>
        public event EventHandler<SledLuaCallStackServiceEventArgs> LevelAdding;

        /// <summary>
        /// Event fired when a new stack level has finished being added to the call stack
        /// </summary>
        public event EventHandler<SledLuaCallStackServiceEventArgs> LevelAdded;

        /// <summary>
        /// Event fired when current stack level is changing
        /// </summary>
        public event EventHandler<SledLuaCallStackServiceEventArgs> StackLevelChanging;

        /// <summary>
        /// Event fired when current stack level has finished changing
        /// </summary>
        public event EventHandler<SledLuaCallStackServiceEventArgs> StackLevelChanged;

        /// <summary>
        /// Event fired when stack level needs to be looked up
        /// </summary>
        public event EventHandler<SledLuaCallStackServiceEventArgs> StackLevelLookingUp;

        /// <summary>
        /// Event fired when stack level has finished being looked up
        /// </summary>
        public event EventHandler<SledLuaCallStackServiceEventArgs> StackLevelLookedUp;

        #endregion

        #region ISledProjectService Events

        private void ProjectServiceCreated(object sender, SledProjectServiceProjectEventArgs e)
        {
            CreateCallStackCollection();
        }

        private void ProjectServiceOpened(object sender, SledProjectServiceProjectEventArgs e)
        {
            CreateCallStackCollection();
        }

        private void ProjectServiceClosing(object sender, SledProjectServiceProjectEventArgs e)
        {
            DestroyCallStackCollection();
        }

        #endregion

        #region ISledDebugService Events

        private void DebugServiceUpdateBegin(object sender, SledDebugServiceBreakpointEventArgs e)
        {
            //SendLiveConnectCallStackClear(m_liveConnectService);
        }

        private void DebugServiceDataReady(object sender, SledDebugServiceEventArgs e)
        {
            var typeCode = (Scmp.LuaTypeCodes)e.Scmp.TypeCode;

            switch (typeCode)
            {
                case Scmp.LuaTypeCodes.LuaCallStackBegin:
                    RemoteTargetCallStackClear();
                    break;

                case Scmp.LuaTypeCodes.LuaCallStack:
                {
                    var var = m_debugService.GetScmpBlob<Scmp.LuaCallStack>();

                    var node = new DomNode(SledSchema.SledCallStackType.Type);

                    var cs = node.As<SledCallStackType>();
                    cs.File = SledUtil.FixSlashes(var.RelScriptPath);
                    cs.CurrentLine = var.CurrentLine;
                    cs.LineDefined = var.LineDefined;
                    cs.LineEnd = var.LastLineDefined;
                    cs.Function = m_luaFunctionParserService.Value.LookUpFunction(var.FunctionName);
                    cs.Level = var.StackLevel;
                    cs.NodeCanBeLookedUp = true;
                    cs.NodeNeedsLookedUp = (cs.Level != 0);
                    cs.IsCursorInFunction = (cs.Level == 0);

                    RemoteTargetCallStackNew(cs);
                }
                break;

                case Scmp.LuaTypeCodes.LuaCallStackEnd:
                    break;

                case Scmp.LuaTypeCodes.LuaCallStackLookupBegin:
                    break;

                case Scmp.LuaTypeCodes.LuaCallStackLookup:
                {
                    var scmp = m_debugService.GetScmpBlob<Scmp.LuaCallStackLookup>();

                    int iLevel = scmp.StackLevel;

                    // Save this so the call stack GUI knows where it is
                    CurrentStackLevel = iLevel;

                    // Find existing callstack entry at iLevel and update
                    // it to not need to be looked up anymore
                    m_callStackCollection.CallStack[iLevel].NodeNeedsLookedUp = false;
                }
                break;

                case Scmp.LuaTypeCodes.LuaCallStackLookupEnd:
                {
                    var cssea = new SledLuaCallStackServiceEventArgs(PreviousStackLevel, CurrentStackLevel);

                    // Fire event indicating stack level has finished being looked up
                    StackLevelLookedUp.Raise(this, cssea);

                    // Fire event indicating stack level has changed
                    StackLevelChanged.Raise(this, cssea);
                }
                break;
            }
        }

        private void DebugServiceUpdateEnd(object sender, SledDebugServiceBreakpointEventArgs e)
        {
            m_callStackEditor.View = m_callStackCollection;
            m_callStackCollection.ValidationEnded();
        }

        private void DebugServiceDisconnected(object sender, SledDebugServiceEventArgs e)
        {
            RemoteTargetCallStackClear();
        }

        #endregion

        #region ISledLuaFunctionCursorWatcherService Events

        private void LuaFunctionCursorWatcherServiceCursorFunctionChanged(object sender, SledLuaFunctionCursorWatcherServiceEventArgs e)
        {
            // Don't care if not connected to a target
            if (m_debugService.IsDisconnected)
                return;

            if (m_callStackCollection == null)
                return;

            try
            {
                m_callStackCollection.ValidationBeginning();

                // Indicate to matching callstack function that the cursor is there
                foreach (var cs in m_callStackCollection.CallStack)
                {
                    cs.IsCursorInFunction = IsMatchingCallStackFunction(cs, e.File, e.Function);
                }

                m_callStackEditor.View = null;
                m_callStackEditor.View = m_callStackCollection;
            }
            finally
            {
                m_callStackCollection.ValidationEnded();
            }
        }

        #endregion

        #region Member Methods

        private void CreateCallStackCollection()
        {
            var root =
                new DomNode(
                    SledSchema.SledCallStackListType.Type,
                    SledLuaSchema.SledLuaCallStackRootElement);

            m_callStackCollection = root.As<SledCallStackListType>();

            m_callStackCollection.Name =
                m_projectService.ProjectName +
                Resources.Resource.Space +
                Resources.Resource.LuaCallStack;

            m_callStackEditor.View = m_callStackCollection;
        }

        private void DestroyCallStackCollection()
        {
            m_callStackEditor.View = null;

            m_callStackCollection.CallStack.Clear();
            m_callStackCollection = null;
        }

        private void RemoteTargetCallStackNew(SledCallStackType cs)
        {
            m_callStackCollection.ValidationBeginning();

            // Event args indicating new stack level #
            var cssea = new SledLuaCallStackServiceEventArgs(-1, cs.Level);

            // Fire event signalling a new stack level is being added
            LevelAdding.Raise(this, cssea);

            // Add new call stack level
            m_callStackCollection.CallStack.Add(cs);

            // Fire event signalling a new stack level has finished being added
            LevelAdded.Raise(this, cssea);
        }

        private void RemoteTargetCallStackClear()
        {
            // Fire clearing event
            Clearing.Raise(this, EventArgs.Empty);

            // Reset
            CurrentStackLevel = 0;

            // Clear call stack window
            m_callStackEditor.View = null;

            // Remote items from the collection
            if (m_callStackCollection != null)
                m_callStackCollection.CallStack.Clear();

            // Rebind with a context
            m_callStackEditor.View = m_callStackCollection;

            // Fire cleared event
            Cleared.Raise(this, EventArgs.Empty);
        }

        private void CallStackEditorMouseClick(object sender, MouseEventArgs e)
        {
            var editor = sender.As<SledLuaCallStackEditor>();
            if (editor == null)
                return;

            var obj = editor.GetItemAt(e.Location);
            if (obj == null)
                return;

            if (!obj.Is<SledCallStackType>())
                return;

            var cs = obj.As<SledCallStackType>();

            // Don't continue if already showing this level
            if (cs.Level == CurrentStackLevel)
                return;

            // Create event args (cs.Level is new level
            // and m_iCurrentStackLevel is old level)
            var cssea = 
                new SledLuaCallStackServiceEventArgs(
                    CurrentStackLevel,
                    cs.Level);

            // Fire event indicating stack level is changing
            StackLevelChanging.Raise(this, cssea);

            // See if we have the data or not and look it up if we don't
            if (cs.NodeCanBeLookedUp && cs.NodeNeedsLookedUp)
            {
                // Send message
                m_debugService.SendScmp(
                    new Scmp.LuaCallStackLookupPerform(
                        m_luaLanguagePlugin.LanguageId,
                        (Int16)cs.Level));

                // Fire event indicating the stack level has to be looked up
                StackLevelLookingUp.Raise(this, cssea);
            }
            else
            {
                // Store level since we switched
                CurrentStackLevel = cs.Level;

                // Fire event indicating stack level has changed
                StackLevelChanged.Raise(this, cssea);
            }
        }

        private void CallStackEditorMouseDoubleClick(object sender, MouseEventArgs e)
        {
            var editor = sender.As<SledLuaCallStackEditor>();
            if (editor == null)
                return;

            var obj = editor.GetItemAt(e.Location);
            if (obj == null)
                return;

            if (!obj.Is<SledCallStackType>())
                return;

            var cs = obj.As<SledCallStackType>();

            var szAbsPath =
                SledUtil.GetAbsolutePath(
                    cs.File,
                    m_projectService.AssetDirectory);

            if (!File.Exists(szAbsPath))
                return;

            // Jump to line
            m_gotoService.GotoLine(szAbsPath, cs.CurrentLine, false);
        }

        private static bool IsMatchingCallStackFunction(SledCallStackType cs, SledProjectFilesFileType file, SledLuaFunctionType function)
        {
            if (function == null)
                return false;

            return
                ((cs.LineDefined == function.LineDefined) &&
                (cs.LineEnd == function.LastLineDefined) &&
                (string.Compare(cs.File, file.Path, StringComparison.OrdinalIgnoreCase) == 0) &&
                (string.Compare(cs.Function, function.Name, StringComparison.OrdinalIgnoreCase) == 0));
        }

        #endregion

        private int m_curStackLevel;
        
        private ISledDebugService m_debugService;
        private SledLuaLanguagePlugin m_luaLanguagePlugin;
        private ISledLuaFunctionCursorWatcherService m_luaFunctionCursorWatcherService;

        private SledCallStackListType m_callStackCollection;

#pragma warning disable 649 // Field is never assigned

        [Import]
        private SledLuaCallStackEditor m_callStackEditor;

        [Import]
        private ISledGotoService m_gotoService;

        [Import]
        private ISledProjectService m_projectService;

        [Import]
        private Lazy<ISledLanguagePluginService> m_languagePluginService;

        [Import]
        private Lazy<ISledLuaFunctionParserService> m_luaFunctionParserService;

#pragma warning restore 649

        #region Private Classes

        [Export(typeof(SledLuaCallStackEditor))]
        [Export(typeof(IContextMenuCommandProvider))]
        [PartCreationPolicy(CreationPolicy.Shared)]
        private class SledLuaCallStackEditor : SledTreeListViewEditor, IContextMenuCommandProvider
        {
            [ImportingConstructor]
            public SledLuaCallStackEditor()
                : base(
                    Localization.SledLuaCallStackTitle,
                    null,
                    SledCallStackListType.TheColumnNames,
                    TreeListView.Style.List,
                    StandardControlGroup.Right)
            {
                AllowDebugFreeze = true;

                // Keep the items sorted in the order they were added
                TreeListView.NodeSorter = new CallstackNodeSorter();
            }

            #region Implementation of IContextMenuCommandProvider

            public IEnumerable<object> GetCommands(object context, object target)
            {
                if (!ReferenceEquals(context, TreeListViewAdapter.View))
                    yield break;

                var clicked = target.As<SledCallStackType>();
                if (clicked == null)
                    yield break;

                yield return StandardCommand.EditCopy;
            }

            #endregion

            #region SledTreeListViewEditor Overrides

            protected override string GetCopyText()
            {
                if ((TreeListViewAdapter == null) ||
                    (!TreeListViewAdapter.Selection.Any()))
                    return string.Empty;

                const string tab = "\t";
                const string indicator = ">";
                const string space = " ";

                var sb = new StringBuilder();
                var items = TreeListViewAdapter.Selection.AsIEnumerable<SledCallStackType>();

                foreach (var item in items)
                {
                    sb.Append(item.IsCursorInFunction ? indicator : space);
                    sb.Append(tab);
                    sb.Append(item.Function);
                    sb.Append(tab);
                    sb.Append(item.File);
                    sb.Append(tab);
                    sb.Append(item.CurrentLine);
                    sb.Append(Environment.NewLine);
                }

                return sb.ToString();
            }

            #endregion

            private class CallstackNodeSorter : IComparer<TreeListView.Node>
            {
                public int Compare(TreeListView.Node x, TreeListView.Node y)
                {
                    if ((x == null) && (y == null))
                        return 0;

                    if (x == null)
                        return 1;

                    if (y == null)
                        return -1;

                    if (ReferenceEquals(x, y))
                        return 0;

                    return -1;
                }
            }
        }

        #endregion
    }

    public class SledLuaCallStackServiceEventArgs : EventArgs
    {
        public SledLuaCallStackServiceEventArgs(int oldLevel, int newLevel)
        {
            OldLevel = oldLevel;
            NewLevel = newLevel;
        }

        public int OldLevel { get; private set; }

        public int NewLevel { get; private set; }
    }

    interface ISledLuaCallStackService
    {
        /// <summary>
        /// Access the call stack of a particular stack level
        /// </summary>
        /// <param name="iStackLevel">Stack level</param>
        /// <returns>Call stack of particular stack level</returns>
        SledCallStackType this[int iStackLevel] { get; }

        /// <summary>
        /// Obtain the current call stack level
        /// </summary>
        int CurrentStackLevel { get; }

        /// <summary>
        /// Event fired when the call stack is about to be cleared
        /// </summary>
        event EventHandler Clearing;

        /// <summary>
        /// Event fired when the call stack has been cleared
        /// </summary>
        event EventHandler Cleared;

        /// <summary>
        /// Event fired when a new stack level is being added to the call stack
        /// </summary>
        event EventHandler<SledLuaCallStackServiceEventArgs> LevelAdding;

        /// <summary>
        /// Event fired when a new stack level has finished being added to the call stack
        /// </summary>
        event EventHandler<SledLuaCallStackServiceEventArgs> LevelAdded;

        /// <summary>
        /// Event fired when current stack level is changing
        /// </summary>
        event EventHandler<SledLuaCallStackServiceEventArgs> StackLevelChanging;

        /// <summary>
        /// Event fired when current stack level has finished changing
        /// </summary>
        event EventHandler<SledLuaCallStackServiceEventArgs> StackLevelChanged;

        /// <summary>
        /// Event fired when stack level needs to be looked up
        /// </summary>
        event EventHandler<SledLuaCallStackServiceEventArgs> StackLevelLookingUp;

        /// <summary>
        /// Event fired when stack level has finished being looked up
        /// </summary>
        event EventHandler<SledLuaCallStackServiceEventArgs> StackLevelLookedUp;
    }
}
