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
using System.Xml;

using Sce.Atf;
using Sce.Atf.Adaptation;
using Sce.Atf.Applications;
using Sce.Atf.Controls;

using Sce.Sled.Lua.Resources;
using Sce.Sled.Shared;
using Sce.Sled.Shared.Controls;
using Sce.Sled.Shared.Dom;
using Sce.Sled.Shared.Plugin;
using Sce.Sled.Shared.Services;
using Sce.Sled.Shared.Utilities;

namespace Sce.Sled.Lua
{
    [Export(typeof(IInitializable))]
    [Export(typeof(ISledProjectSettingsPlugin))]
    [Export(typeof(SledLuaMemoryTraceService))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    sealed class SledLuaMemoryTraceService : IInitializable, ISledProjectSettingsPlugin, ICommandClient, IItemView, ITreeListView
    {
        [ImportingConstructor]
        public SledLuaMemoryTraceService(ICommandService commandService)
        {
            commandService.RegisterCommand(
                Command.Toggle,
                SledLuaMenuShared.MenuTag,
                SledLuaMenuShared.CommandGroupTag,
                Localization.SledLuaToggleMemoryTracer,
                Localization.SledLuaToggleMemoryTracerComment,
                Keys.None,
                SledIcon.ProjectToggleMemoryTracer,
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

            m_editor.TreeListViewAdapter.RetrieveVirtualItem += TreeListViewAdapterRetrieveVirtualItem;

            // Adjust to shortened name if this language plugin is the only one loaded
            if (m_languagePluginService.Get.Count == 1)
                m_editor.Name = Localization.SledLuaMemoryTraceTitleShort;
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
                        bEnabled = m_bMemoryTracerEnabled && m_debugService.IsConnected;
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
                    ToggleMemoryTracer();
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
                {
                    state.Check = m_bMemoryTracerRunning;
                }
                    break;
            }
        }

        #endregion

        #region ISledProjectSettingsPlugin Interface

        public bool NeedsSaving()
        {
            return
                m_bDirty &&
                (m_trace.Count > 0);
        }

        #endregion

        #region IItemView Interface

        public void GetInfo(object item, ItemInfo info)
        {
            var itemView = item.As<IItemView>();
            if ((itemView == null) || (itemView == this))
                return;

            itemView.GetInfo(item, info);
        }

        #endregion

        #region ITreeListView Interface

        public IEnumerable<object> GetChildren(object parent)
        {
            yield break;
        }

        public IEnumerable<object> Roots
        {
            get { yield break; }
        }

        public string[] ColumnNames
        {
            get { return SledMemoryTraceStream.TheColumnNames; }
        }

        #endregion

        #region ISledDebugService Events

        private void DebugServiceDataReady(object sender, SledDebugServiceEventArgs e)
        {
            var typeCode = (Scmp.LuaTypeCodes)e.Scmp.TypeCode;

            switch (typeCode)
            {
                //case Scmp.LuaTypeCodes.LuaMemoryTraceBegin:
                //    break;

                case Scmp.LuaTypeCodes.LuaMemoryTrace:
                {
                    var scmp =
                        m_debugService.GetScmpBlob<Scmp.LuaMemoryTrace>();

                    var chWhat = (char)scmp.What;

                    RemoteTargetMemoryTraceStreamAdd(
                        Create(
                            chWhat,
                            scmp.OldSize,
                            scmp.NewSize,
                            scmp.OldPtr,
                            scmp.NewPtr));
                }
                break;

                //case Scmp.LuaTypeCodes.LuaMemoryTraceEnd:
                //    break;

                //case Scmp.LuaTypeCodes.LuaMemoryTraceStreamBegin:
                //    break;

                case Scmp.LuaTypeCodes.LuaMemoryTraceStream:
                {
                    var scmp =
                        m_debugService.GetScmpBlob<Scmp.LuaMemoryTraceStream>();

                    var chWhat = (char)scmp.What;
                    
                    RemoteTargetMemoryTraceStreamAdd(
                        Create(
                            chWhat,
                            scmp.OldSize,
                            scmp.NewSize,
                            scmp.OldPtr,
                            scmp.NewPtr));
                }
                break;

                //case Scmp.LuaTypeCodes.LuaMemoryTraceStreamEnd:
                //    break;

                case Scmp.LuaTypeCodes.LuaLimits:
                {
                    var scmp =
                        m_debugService.GetScmpBlob<Scmp.LuaLimits>();

                    m_bMemoryTracerEnabled = scmp.MemoryTracerEnabled;
                    if (!m_bMemoryTracerEnabled)
                    {
                        SledOutDevice.OutLine(
                            SledMessageType.Info,
                            "[Lua] Memory tracer is disabled due to target settings.");
                    }
                }
                break;
            }
        }

        private void DebugServiceUpdateEnd(object sender, SledDebugServiceBreakpointEventArgs e)
        {
            m_editor.TreeListViewAdapter.VirtualListSize = m_trace.Count;
        }

        private void DebugServiceDisconnected(object sender, SledDebugServiceEventArgs e)
        {
            if (m_bMemoryTracerRunning)
                ToggleMemoryTracer();

            m_bMemoryTracerEnabled = false;
        }

        #endregion

        #region ISledProjectService Events

        private void ProjectServiceCreated(object sender, SledProjectServiceProjectEventArgs e)
        {
            Reset();
        }

        private void ProjectServiceOpened(object sender, SledProjectServiceProjectEventArgs e)
        {
            Reset();
        }

        private void ProjectServiceSaved(object sender, SledProjectServiceProjectEventArgs e)
        {
            WriteToDisk();
        }

        private void ProjectServiceClosing(object sender, SledProjectServiceProjectEventArgs e)
        {
            Reset();
        }

        #endregion

        #region Member Methods

        private void Reset()
        {
            m_editor.View = null;
            m_trace.Clear();
            m_bDirty = false;
            s_iNumMemoryTrace = 0;
            m_editor.View = this;
        }

        private static SledMemoryTraceStream Create(char what, int oldSize, int newSize, string oldAddres, string newAddress)
        {
            var stream =
                new SledMemoryTraceStream
                    {
                        Order = s_iNumMemoryTrace++,
                        What = (what == 'a'
                                    ? Localization.SledLuaMemoryTraceAlloc
                                    : (what == 'r'
                                           ? Localization.SledLuaMemoryTraceRealloc
                                           : Localization.SledLuaMemoryTraceDealloc)),
                        OldSize = oldSize,
                        NewSize = newSize,
                        OldAddress = oldAddres,
                        NewAddress = newAddress
                    };

            return stream;
        }

        private void ToggleMemoryTracer()
        {
            m_debugService.SendScmp(new Scmp.LuaMemoryTraceToggle(m_luaLanguagePlugin.LanguageId));

            if (!m_bMemoryTracerRunning)
                Reset();

            m_bMemoryTracerRunning = !m_bMemoryTracerRunning;
        }

        private void RemoteTargetMemoryTraceStreamAdd(SledMemoryTraceStream mt)
        {
            m_trace.Add(mt.Order, mt);
            m_bDirty = true;
        }

        private void WriteToDisk()
        {
            if (!m_bDirty)
                return;

            try
            {
                if (m_trace.Count <= 0)
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
                        m_projectService.Get.ProjectName + " Lua Memory Trace.xml");

                var uri = new Uri(filePath);

                using (var stream = new FileStream(uri.LocalPath, FileMode.Create, FileAccess.Write))
                {
                    var settings =
                        new XmlWriterSettings
                            {
                                Indent = true,
                                IndentChars = "\t",
                                NewLineHandling = NewLineHandling.Replace,
                                NewLineChars = "\r\n"
                            };

                    using (var writer = XmlWriter.Create(stream, settings))
                    {
                        writer.WriteStartDocument();
                        writer.WriteStartElement("SledMemoryTraceStreams");

                        foreach (var kv in m_trace)
                        {
                            writer.WriteStartElement("SledMemoryTraceStream");
                            writer.WriteAttributeString("Order", kv.Value.Order.ToString());
                            writer.WriteAttributeString("What", kv.Value.What);
                            writer.WriteAttributeString("OldSize", kv.Value.OldSize.ToString());
                            writer.WriteAttributeString("NewSize", kv.Value.NewSize.ToString());
                            writer.WriteAttributeString("OldAddress", kv.Value.OldAddress);
                            writer.WriteAttributeString("NewAddress", kv.Value.NewAddress);
                            writer.WriteEndElement();
                        }

                        writer.WriteEndElement();
                        writer.WriteEndDocument();
                    }
                }
            }
            catch (Exception ex)
            {
                SledOutDevice.OutLine(
                    SledMessageType.Error,
                    "[SledLuaMemoryTraceService] Failure " +
                    "to save memory trace data to disk: {0}",
                    ex.Message);
            }
            finally
            {
                m_bDirty = false;
            }
        }

        private void TreeListViewAdapterRetrieveVirtualItem(object sender, TreeListViewAdapter.RetrieveVirtualNodeAdapter e)
        {
            if (e.ItemIndex >= m_trace.Count)
                return;

            e.Item = m_trace[(ulong)e.ItemIndex];
        }

        #endregion

        private ISledDebugService m_debugService;
        private SledLuaLanguagePlugin m_luaLanguagePlugin;

        private bool m_bDirty;
        private bool m_bMemoryTracerEnabled;
        private bool m_bMemoryTracerRunning;

#pragma warning disable 649 // Field is never assigned

        [Import]
        private SledLuaMemoryTraceEditor m_editor;

#pragma warning restore 649

        private static ulong s_iNumMemoryTrace;

        private readonly SortedList<ulong, SledMemoryTraceStream> m_trace =
            new SortedList<ulong, SledMemoryTraceStream>();

        private readonly SledServiceReference<ISledProjectService> m_projectService =
            new SledServiceReference<ISledProjectService>();

        private readonly SledServiceReference<ISledLanguagePluginService> m_languagePluginService =
            new SledServiceReference<ISledLanguagePluginService>();

        #region Private Classes

        [Export(typeof(SledLuaMemoryTraceEditor))]
        [Export(typeof(IContextMenuCommandProvider))]
        [PartCreationPolicy(CreationPolicy.Shared)]
        private class SledLuaMemoryTraceEditor : SledTreeListViewEditor, IContextMenuCommandProvider
        {
            [ImportingConstructor]
            public SledLuaMemoryTraceEditor()
                : base(
                    Localization.SledLuaMemoryTraceTitle,
                    SledIcon.ProjectToggleMemoryTracer,
                    SledMemoryTraceStream.TheColumnNames,
                    TreeListView.Style.VirtualList,
                    StandardControlGroup.Right)
            {
                AllowDebugFreeze = true;
            }

            #region Implementation of IContextMenuCommandProvider

            public IEnumerable<object> GetCommands(object context, object target)
            {
                if (!ReferenceEquals(context, TreeListViewAdapter.View))
                    yield break;

                var clicked = target.As<SledMemoryTraceStream>();
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
                var items = TreeListViewAdapter.Selection.AsIEnumerable<SledMemoryTraceStream>();

                foreach (var item in items)
                {
                    sb.Append(item.Order);
                    sb.Append(tab);
                    sb.Append(item.What);
                    sb.Append(tab);
                    sb.Append(item.OldAddress);
                    sb.Append(tab);
                    sb.Append(item.NewAddress);
                    sb.Append(tab);
                    sb.Append(item.OldSize);
                    sb.Append(tab);
                    sb.Append(item.NewSize);
                    sb.Append(Environment.NewLine);
                }

                return sb.ToString();
            }

            #endregion
        }

        #endregion
    }
}
