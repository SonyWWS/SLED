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

using Sce.Sled.Shared.Controls;

using Sce.Sled.Lua.Dom;
using Sce.Sled.Lua.Resources;
using Sce.Sled.Shared;
using Sce.Sled.Shared.Dom;
using Sce.Sled.Shared.Plugin;
using Sce.Sled.Shared.Services;
using Sce.Sled.Shared.Utilities;

namespace Sce.Sled.Lua
{
    [Export(typeof(IInitializable))]
    [Export(typeof(ISledProjectSettingsPlugin))]
    [Export(typeof(SledLuaProfilerService))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    sealed class SledLuaProfilerService : IInitializable, ICommandClient, ISledProjectSettingsPlugin
    {
        [ImportingConstructor]
        public SledLuaProfilerService(ICommandService commandService)
        {
            commandService.RegisterCommand(
                Command.Toggle,
                SledLuaMenuShared.MenuTag,
                SledLuaMenuShared.CommandGroupTag,
                Localization.SledLuaToggleLuaProfiler,
                Localization.SledLuaToggleLuaProfilerComment,
                Keys.None,
                SledIcon.ProjectToggleProfiler,
                CommandVisibility.All,
                this);
        }

        #region IInitializable Interface

        void IInitializable.Initialize()
        {
            m_luaLanguagePlugin =
                SledServiceInstance.Get<SledLuaLanguagePlugin>();

            m_debugService =
                SledServiceInstance.Get<ISledDebugService>();

            m_debugService.DataReady += DebugServiceDataReady;
            m_debugService.UpdateEnd += DebugServiceUpdateEnd;
            m_debugService.Disconnected += DebugServiceDisconnected;

            var projectService =
                SledServiceInstance.Get<ISledProjectService>();

            projectService.Created += ProjectServiceCreated;
            projectService.Opened += ProjectServiceOpened;
            projectService.Saved += ProjectServiceSaved;
            projectService.Closing += ProjectServiceClosing;

            m_editor.MouseDoubleClick += EditorMouseDoubleClick;

            m_funcCallsEditor.MouseDoubleClick += FuncCallsEditorMouseDoubleClick;
            m_funcCallsEditor.TreeListViewAdapter.ItemLazyLoad += FuncCallsEditorItemLazyLoad;

            // Adjust to shortened name if this language plugin is the only one loaded
            if (m_languagePluginService.Get.Count != 1)
                return;

            m_editor.Name = Localization.SledLuaProfileInfoTitleShort;
            m_funcCallsEditor.Name = Localization.SledLuaProfileFuncCallsTitleShort;
        }

        #endregion

        #region Commands

        enum Command
        {
            Toggle,
        }

        #endregion

        #region ICommandClient Interface

        public bool CanDoCommand(object commandTag)
        {
            var bEnabled = false;

            if (commandTag is Command)
            {
                switch ((Command)commandTag)
                {
                    case Command.Toggle:
                        bEnabled = m_bProfilerEnabled && m_debugService.IsConnected;
                        break;
                }
            }

            return bEnabled;
        }

        public void DoCommand(object commandTag)
        {
            if (!(commandTag is Command))
                return;

            switch ((Command)commandTag)
            {
                case Command.Toggle:
                    ToggleProfiler();
                    break;
            }
        }

        public void UpdateCommand(object commandTag, CommandState state)
        {
            if (!(commandTag is Command))
                return;

            switch ((Command)commandTag)
            {
                case Command.Toggle:
                    state.Check = m_bProfilerRunning;
                    break;
            }
        }

        #endregion

        #region ISledProjectSettingsPlugin Interface

        public bool NeedsSaving()
        {
            return
                m_bDirty &&
                (m_collection != null) &&
                (m_collection.ProfileInfo.Count > 0);
        }

        #endregion

        #region ISledDebugService Events

        private void DebugServiceDataReady(object sender, SledDebugServiceEventArgs e)
        {
            var typeCode = (Scmp.LuaTypeCodes)e.Scmp.TypeCode;

            switch (typeCode)
            {
                case Scmp.LuaTypeCodes.LuaProfileInfoBegin:
                    
                    RemoteTargetProfileInfoNew();
                    break;

                case Scmp.LuaTypeCodes.LuaProfileInfo:
                {
                    var scmp =
                        m_debugService.GetScmpBlob<Scmp.LuaProfileInfo>();

                    var domNode =
                        new DomNode(SledSchema.SledProfileInfoType.Type);

                    var pi = domNode.As<SledProfileInfoType>();
                    pi.Function = m_luaFunctionParserService.Get.LookUpFunction(scmp.FunctionName);
                    pi.TimeTotal = scmp.FnTimeElapsed;
                    pi.TimeAverage = scmp.FnTimeElapsedAvg;
                    pi.TimeMin = scmp.FnTimeElapsedShortest;
                    pi.TimeMax = scmp.FnTimeElapsedLongest;
                    pi.TimeTotalInner = scmp.FnTimeInnerElapsed;
                    pi.TimeAverageInner = scmp.FnTimeInnerElapsedAvg;
                    pi.TimeMinInner = scmp.FnTimeInnerElapsedShortest;
                    pi.TimeMaxInner = scmp.FnTimeInnerElapsedLongest;
                    pi.NumCalls = (int)scmp.FnCallCount;
                    pi.Line = scmp.FnLine;
                    pi.File = scmp.RelScriptPath.Replace(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar).ToLower();
                    pi.NumFuncsCalled = scmp.FnCalls;
                    pi.GenerateUniqueName();

                    RemoteTargetProfileInfoAdd(pi);
                    RemoteTargetProfileFuncCallAdd((SledProfileInfoType)pi.Clone());
                }
                break;

                case Scmp.LuaTypeCodes.LuaProfileInfoEnd:
                    RemoteTargetProfileFuncCallEnd();
                    break;

                //case Scmp.LuaTypeCodes.LuaProfileInfoLookupBegin:
                //    break;

                case Scmp.LuaTypeCodes.LuaProfileInfoLookup:
                {
                    var scmp =
                        m_debugService.GetScmpBlob<Scmp.LuaProfileInfoLookup>();

                    var domNode =
                        new DomNode(SledSchema.SledProfileInfoType.Type);

                    var pi = domNode.As<SledProfileInfoType>();
                    pi.Function = m_luaFunctionParserService.Get.LookUpFunction(scmp.FunctionName);
                    pi.TimeTotal = scmp.FnTimeElapsed;
                    pi.TimeAverage = scmp.FnTimeElapsedAvg;
                    pi.TimeMin = scmp.FnTimeElapsedShortest;
                    pi.TimeMax = scmp.FnTimeElapsedLongest;
                    pi.TimeTotalInner = scmp.FnTimeInnerElapsed;
                    pi.TimeAverageInner = scmp.FnTimeInnerElapsedAvg;
                    pi.TimeMinInner = scmp.FnTimeInnerElapsedShortest;
                    pi.TimeMaxInner = scmp.FnTimeInnerElapsedLongest;
                    pi.NumCalls = (int)scmp.FnCallCount;
                    pi.Line = scmp.FnLine;
                    pi.File = scmp.RelScriptPath.Replace(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar).ToLower();
                    pi.NumFuncsCalled = scmp.FnCalls;
                    pi.GenerateUniqueName();

                    RemoteTargetProfileFuncCallLookUpAdd(pi);
                }
                break;

                case Scmp.LuaTypeCodes.LuaProfileInfoLookupEnd:
                    RemoteTargetProfileLookUpFinished();
                    break;

                case Scmp.LuaTypeCodes.LuaLimits:
                {
                    var scmp = m_debugService.GetScmpBlob<Scmp.LuaLimits>();

                    m_bProfilerEnabled = scmp.ProfilerEnabled;
                    if (!m_bProfilerEnabled)
                    {
                        SledOutDevice.OutLine(
                            SledMessageType.Info,
                            "[Lua] Profiler is disabled due to target settings.");
                    }
                }
                break;
            }
        }

        private void DebugServiceUpdateEnd(object sender, SledDebugServiceBreakpointEventArgs e)
        {
            m_editor.View = m_collection;
            m_funcCallsEditor.View = m_funcCallsCollection;
        }

        private void DebugServiceDisconnected(object sender, SledDebugServiceEventArgs e)
        {
            if (m_bProfilerRunning)
                ToggleProfiler();

            m_bProfilerEnabled = false;
        }

        #endregion

        #region ISledProjectService Events

        private void ProjectServiceCreated(object sender, SledProjectServiceProjectEventArgs e)
        {
            CreateProfileInfoCollection();
            CreateProfileFuncCallCollection();
        }

        private void ProjectServiceOpened(object sender, SledProjectServiceProjectEventArgs e)
        {
            CreateProfileInfoCollection();
            CreateProfileFuncCallCollection();
        }

        private void ProjectServiceSaved(object sender, SledProjectServiceProjectEventArgs e)
        {
            WriteToDisk();
        }

        private void ProjectServiceClosing(object sender, SledProjectServiceProjectEventArgs e)
        {
            DestroyProfileInfoCollection();
            DestroyProfileFuncCallsCollection();
        }

        #endregion

        #region Member Methods

        private void ToggleProfiler()
        {
            m_debugService.SendScmp(
                new Scmp.LuaProfilerToggle(
                    m_luaLanguagePlugin.LanguageId));

            m_bProfilerRunning = !m_bProfilerRunning;

            if (!m_bProfilerRunning)
                return;

            m_editor.View = null;
            m_collection.ProfileInfo.Clear();
            m_editor.View = m_collection;

            m_funcCallsEditor.View = null;
            m_funcCallsCollection.ProfileInfo.Clear();
            m_funcCallsEditor.View = m_funcCallsCollection;
        }

        private void CreateProfileInfoCollection()
        {
            var root =
                new DomNode(
                    SledSchema.SledProfileInfoListType.Type,
                    SledLuaSchema.SledLuaProfileInfoRootElement);

            m_collection =
                root.As<SledProfileInfoListType>();

            m_collection.Name =
                string.Format(
                    "{0}{1}{2}",
                    m_projectService.Get.ProjectName,
                    Resources.Resource.Space,
                    Resources.Resource.LuaProfileInfo);

            m_collection.DisplayMode =
                SledProfileInfoListType.Display.Normal;

            m_bDirty = false;
        }

        private void CreateProfileFuncCallCollection()
        {
            var root =
                new DomNode(
                    SledSchema.SledProfileInfoListType.Type,
                    SledLuaSchema.SledLuaProfileFuncCallsRootElement);

            m_funcCallsCollection =
                root.As<SledProfileInfoListType>();

            m_funcCallsCollection.Name =
                string.Format(
                    "{0}{1}{2}",
                    m_projectService.Get.ProjectName,
                    Resources.Resource.Space,
                    Resources.Resource.LuaProfileFuncCalls);

            m_funcCallsCollection.DisplayMode =
                SledProfileInfoListType.Display.CallGraph;
        }

        private void DestroyProfileInfoCollection()
        {
            // Clear GUI
            m_editor.View = null;

            m_collection.ProfileInfo.Clear();
            m_collection = null;
        }

        private void DestroyProfileFuncCallsCollection()
        {
            // Clear GUI
            m_funcCallsEditor.View = null;

            m_funcCallsCollection.ProfileInfo.Clear();
            m_funcCallsCollection = null;
        }

        private void RemoteTargetProfileInfoNew()
        {
            m_editor.View = null;
            m_collection.ProfileInfo.Clear();
            m_editor.View = m_collection;

            m_funcCallsEditor.View = null;
            m_funcCallsCollection.ProfileInfo.Clear();
            m_funcCallsEditor.View = m_funcCallsCollection;
        }

        private void RemoteTargetProfileLookUpFinished()
        {
            if (m_lstQueuedProfileLookUps.Count <= 0)
                return;

            m_lstQueuedProfileLookUps.RemoveAt(0);
        }

        private void RemoteTargetProfileInfoAdd(SledProfileInfoType pi)
        {
            m_collection.ProfileInfo.Add(pi);
            m_bDirty = true;
        }

        private void RemoteTargetProfileFuncCallAdd(SledProfileInfoType pi)
        {
            m_funcCallsCollection.ProfileInfo.Add(pi);
        }

        private void RemoteTargetProfileFuncCallEnd()
        {
            m_funcCallsEditor.View = m_funcCallsCollection;
        }

        private void RemoteTargetProfileFuncCallLookUpAdd(SledProfileInfoType pi)
        {
            if (m_lstQueuedProfileLookUps.Count > 0)
                m_lstQueuedProfileLookUps[0].ProfileInfo.Add(pi);
        }

        private void EditorMouseDoubleClick(object sender, MouseEventArgs e)
        {
            var editor = sender.As<SledLuaProfileEditor>();
            if (editor == null)
                return;

            if (editor.LastHit == null)
                return;

            var pi = editor.LastHit.As<SledProfileInfoType>();
            if (pi == null)
                return;

            var szAbsPath =
                SledUtil.GetAbsolutePath(
                    pi.File,
                    m_projectService.Get.AssetDirectory);

            if (!File.Exists(szAbsPath))
                return;

            m_gotoService.Get.GotoLine(szAbsPath, pi.Line, false);
        }

        private void FuncCallsEditorMouseDoubleClick(object sender, MouseEventArgs e)
        {
            var editor = sender.As<SledLuaProfileFuncCallsEditor>();
            if (editor == null)
                return;

            if (editor.LastHit == null)
                return;

            var pi = editor.LastHit.As<SledProfileInfoType>();
            if (pi == null)
                return;

            var szAbsPath =
                SledUtil.GetAbsolutePath(
                    pi.File,
                    m_projectService.Get.AssetDirectory);

            if (!File.Exists(szAbsPath))
                return;

            m_gotoService.Get.GotoLine(szAbsPath, pi.Line, false);
        }

        private void FuncCallsEditorItemLazyLoad(object sender, TreeListViewAdapter.ItemLazyLoadEventArgs e)
        {
            if (!m_debugService.IsConnected)
                return;

            if (m_debugService.IsDebugging)
                return;

            if (e.Item == null)
                return;

            var pi = e.Item.As<SledProfileInfoType>();
            if (pi == null)
                return;

            if (m_lstQueuedProfileLookUps.Contains(pi))
                return;

            // TODO: FIX ME
            var lookup = string.Empty;
                //m_luaGenerateLookUpStringService.Get.GenerateLookUpString(pi);

            if (string.IsNullOrEmpty(lookup))
                return;

            m_lstQueuedProfileLookUps.Add(pi);

            //SledOutDevice.OutLine(
            //    SledMessageType.Error,
            //    string.Format("[Lookup] {0}", lookup));

            // Send off message
            m_debugService.SendScmp(
                new Scmp.LuaProfileInfoLookupPerform(
                    m_luaLanguagePlugin.LanguageId,
                    lookup));
        }

        private void WriteToDisk()
        {
            if (!m_bDirty)
                return;

            try
            {
                if (m_collection == null)
                    return;

                if (m_collection.ProfileInfo.Count <= 0)
                    return;

                var schemaLoader =
                    SledServiceInstance.Get<SledSharedSchemaLoader>();

                if (schemaLoader == null)
                    return;
            
                var projDir =
                    m_projectService.Get.ProjectDirectory;

                var filePath =
                    Path.Combine(
                        projDir + Path.DirectorySeparatorChar,
                        m_collection.Name + ".xml");

                var uri = new Uri(filePath);

                using (var stream =
                    new FileStream(uri.LocalPath, FileMode.Create, FileAccess.Write))
                {
                    var writer =
                        new DomXmlWriter(schemaLoader.TypeCollection);

                    writer.Write(m_collection.DomNode, stream, uri);
                }

            }
            catch (Exception ex)
            {
                SledOutDevice.OutLine(
                    SledMessageType.Error,
                    "[SledLuaProfilerService] Failure to " +
                    "save profiler data to disk: {0}",
                    ex.Message);
            }
            finally
            {
                m_bDirty = false;
            }
        }

        #endregion

        private ISledDebugService m_debugService;
        private SledLuaLanguagePlugin m_luaLanguagePlugin;

        private bool m_bDirty;
        private bool m_bProfilerEnabled;
        private bool m_bProfilerRunning;

#pragma warning disable 649 // Field is never assigned

        [Import]
        private SledLuaProfileEditor m_editor;

        [Import]
        private SledLuaProfileFuncCallsEditor m_funcCallsEditor;

#pragma warning restore 649

        private SledProfileInfoListType m_collection;
        private SledProfileInfoListType m_funcCallsCollection;

        private readonly List<SledProfileInfoType> m_lstQueuedProfileLookUps =
            new List<SledProfileInfoType>();

        private readonly SledServiceReference<ISledGotoService> m_gotoService =
            new SledServiceReference<ISledGotoService>();

        private readonly SledServiceReference<ISledProjectService> m_projectService =
            new SledServiceReference<ISledProjectService>();

        private readonly SledServiceReference<ISledLanguagePluginService> m_languagePluginService =
            new SledServiceReference<ISledLanguagePluginService>();

        private readonly SledServiceReference<ISledLuaFunctionParserService> m_luaFunctionParserService =
            new SledServiceReference<ISledLuaFunctionParserService>();

        #region Private Classes

        private class MySorter : IComparer<TreeListView.Node>
        {
            public MySorter(TreeListView control)
            {
                m_control = control;
            }

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

                if ((x.Tag.Is<SledProfileInfoType>()) &&
                    (y.Tag.Is<SledProfileInfoType>()))
                {
                    return
                        SledProfileInfoType.Compare(
                            x.Tag.As<SledProfileInfoType>(),
                            y.Tag.As<SledProfileInfoType>(),
                            m_control.SortColumn,
                            m_control.SortOrder);
                }

                return string.Compare(x.Label, y.Label, StringComparison.Ordinal);
            }

            private readonly TreeListView m_control;
        }

        [Export(typeof(SledLuaProfileEditor))]
        [Export(typeof(IContextMenuCommandProvider))]
        [PartCreationPolicy(CreationPolicy.Shared)]
        private class SledLuaProfileEditor : SledTreeListViewEditor, IContextMenuCommandProvider
        {
            [ImportingConstructor]
            public SledLuaProfileEditor()
                : base(
                    Localization.SledLuaProfileInfoTitle,
                    SledIcon.ProjectToggleProfiler,
                    SledProfileInfoType.NormalColumnNames,
                    TreeListView.Style.List,
                    StandardControlGroup.Right)
            {
                AllowDebugFreeze = true;
                TreeListView.NodeSorter = new MySorter(TreeListView);
            }

            #region Implementation of IContextMenuCommandProvider

            public IEnumerable<object> GetCommands(object context, object target)
            {
                if (!ReferenceEquals(context, TreeListViewAdapter.View))
                    yield break;

                var clicked = target.As<SledProfileInfoType>();
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

                var sb = new StringBuilder();
                var items = TreeListViewAdapter.Selection.AsIEnumerable<SledProfileInfoType>();

                foreach (var item in items)
                {
                    sb.Append(item.Function);
                    sb.Append(tab);
                    sb.Append(item.NumCalls);
                    sb.Append(tab);
                    sb.Append(item.TimeTotal);
                    sb.Append(tab);
                    sb.Append(item.TimeAverage);
                    sb.Append(tab);
                    sb.Append(item.TimeMin);
                    sb.Append(tab);
                    sb.Append(item.TimeMax);
                    sb.Append(tab);
                    sb.Append(item.TimeTotalInner);
                    sb.Append(tab);
                    sb.Append(item.TimeAverageInner);
                    sb.Append(tab);
                    sb.Append(item.TimeMinInner);
                    sb.Append(tab);
                    sb.Append(item.TimeMaxInner);
                    sb.Append(Environment.NewLine);
                }

                return sb.ToString();
            }

            #endregion
        }

        [Export(typeof(SledLuaProfileFuncCallsEditor))]
        [Export(typeof(IContextMenuCommandProvider))]
        [PartCreationPolicy(CreationPolicy.Shared)]
        private class SledLuaProfileFuncCallsEditor : SledTreeListViewEditor, IContextMenuCommandProvider
        {
            [ImportingConstructor]
            public SledLuaProfileFuncCallsEditor()
                : base(
                    Localization.SledLuaProfileFuncCallsTitle,
                    SledIcon.ProjectToggleProfiler,
                    SledProfileInfoType.CallGraphColumnNames,
                    TreeListView.Style.TreeList,
                    StandardControlGroup.Right)
            {
                AllowDebugFreeze = true;
                TreeListView.NodeSorter = new MySorter(TreeListView);
                //UseManualQueue = true;
                //ManualQueuePulsed += SledListTreeViewEditorManualQueuePulsed;
            }

            #region Implementation of IContextMenuCommandProvider

            public IEnumerable<object> GetCommands(object context, object target)
            {
                if (!ReferenceEquals(context, TreeListViewAdapter.View))
                    yield break;

                var clicked = target.As<SledProfileInfoType>();
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

                var sb = new StringBuilder();
                var items = TreeListViewAdapter.Selection.AsIEnumerable<SledProfileInfoType>();

                foreach (var item in items)
                {
                    sb.Append(item.Function);
                    sb.Append(Environment.NewLine);
                }

                return sb.ToString();
            }

            #endregion

            //public void Finish()
            //{
            //    PulseManualQueue();
            //}

            //private void SledListTreeViewEditorManualQueuePulsed(object sender, EventArgs e)
            //{
            //    FinishNotify();
            //}

            //[System.Diagnostics.Conditional("DEBUG")]
            //private static void FinishNotify()
            //{
            //    var szWhat = Resources.Resource.LuaProfileFuncCalls;
            //    SledOutDevice.OutLine(SledMessageType.Info, string.Format("[{0}] Finish() Called", szWhat));
            //}
        }

        //private class SledLuaProfileListTreeViewAdapter : SledListTreeViewAdapter
        //{
        //    public override IEnumerable<object> Items
        //    {
        //        get
        //        {
        //            if (Root == null)
        //                yield break;

        //            foreach (var child in Root.Subtree)
        //            {
        //                if (child.Is<SledProfileInfoType>())
        //                    yield return child.As<SledProfileInfoType>();
        //            }
        //        }
        //    }
        //}

        #endregion
    }
}
