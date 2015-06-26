/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Windows.Forms;

using Sce.Atf;
using Sce.Atf.Adaptation;
using Sce.Atf.Applications;
using Sce.Atf.Controls;
using Sce.Atf.Dom;

using Sce.Sled.Resources;
using Sce.Sled.Shared.Controls;
using Sce.Sled.Shared.Dom;
using Sce.Sled.Shared.Services;
using Sce.Sled.Shared.Utilities;

namespace Sce.Sled
{
    [Export(typeof(SledBreakpointEditor))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    class SledBreakpointEditor : SledTreeListViewEditor, IItemView, ITreeListView, IObservableContext
    {
        [ImportingConstructor]
        public SledBreakpointEditor()
            : base(
                Localization.BreakpointWindowName,
                SledIcon.ProjectToggleBreakpoint,
                SledProjectFilesBreakpointType.TheColumnNames,
                TreeListView.Style.List,
                StandardControlGroup.Right)
        {
            TreeListView.NodeSorter = new BreakpointComparer(TreeListView);
        }

        #region IInitializable Interface

        public override void Initialize()
        {
            base.Initialize();

            m_projectService =
                SledServiceInstance.Get<ISledProjectService>();

            m_projectService.Created += ProjectServiceCreated;
            m_projectService.Opened += ProjectServiceOpened;
            m_projectService.Closing += ProjectServiceClosing;

            KeyUp += ControlKeyUp;
            MouseDoubleClick += ControlMouseDoubleClick;
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
            get
            {
                if (!m_projectService.Active)
                    yield break;

                // Convert the project tree to a flat list
                // of only breakpoints

                foreach (var file in m_projectService.AllFiles)
                {
                    foreach (var bp in file.Breakpoints)
                        yield return bp;
                }
            }
        }

        public string[] ColumnNames
        {
            get { return SledProjectFilesBreakpointType.TheColumnNames; }
        }

        #endregion

        #region IObservableContext Interface

        public event EventHandler<ItemInsertedEventArgs<object>> ItemInserted;

        public event EventHandler<ItemRemovedEventArgs<object>> ItemRemoved;

        public event EventHandler<ItemChangedEventArgs<object>> ItemChanged;

        public event EventHandler Reloaded;

        #endregion

        #region ISledProjectService Events

        private void ProjectServiceCreated(object sender, SledProjectServiceProjectEventArgs e)
        {
            View = this;
            SubscribeToEvents(e.Project.DomNode);
        }

        private void ProjectServiceOpened(object sender, SledProjectServiceProjectEventArgs e)
        {
            View = this;
            SubscribeToEvents(e.Project.DomNode);
        }

        private void ProjectServiceClosing(object sender, SledProjectServiceProjectEventArgs e)
        {
            UnsubscribeFromEvents(e.Project.DomNode);
            View = null;

            // Not actually used; just here to stop compiler warning
            Reloaded.Raise(this, EventArgs.Empty);
        }

        #endregion

        #region TreeListViewEditor Events

        private void ControlKeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Delete)
                return;
            
            foreach (object item in Selection)
            {
                var bp = item.As<SledProjectFilesBreakpointType>();
                if (bp == null)
                    continue;

                bp.File.Breakpoints.Remove(bp);
            }
        }

        private void ControlMouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left)
                return;

            var lastHit = LastHit;
            if (lastHit == null)
                return;

            var bp = lastHit.As<SledProjectFilesBreakpointType>();
            if (bp == null)
                return;

            // Jump to breakpoint in file
            m_gotoService.Get.GotoLine(bp.File.AbsolutePath, bp.Line, false);
        }

        #endregion

        #region DomNode Events

        private void DomNodeAttributeChanged(object sender, AttributeEventArgs e)
        {
            if (!e.DomNode.Is<SledProjectFilesBreakpointType>())
                return;

            ItemChanged.Raise(
                this,
                new ItemChangedEventArgs<object>(
                    e.DomNode.As<SledProjectFilesBreakpointType>()));
        }

        private void DomNodeChildInserted(object sender, ChildEventArgs e)
        {
            if (!e.Child.Is<SledProjectFilesBreakpointType>())
                return;

            ItemInserted.Raise(
                this,
                new ItemInsertedEventArgs<object>(
                    e.Index,
                    e.Child.As<SledProjectFilesBreakpointType>()));
        }

        private void DomNodeChildRemoving(object sender, ChildEventArgs e)
        {
            if (!e.Child.Is<SledProjectFilesBreakpointType>())
                return;

            ItemRemoved.Raise(
                this,
                new ItemRemovedEventArgs<object>(
                    e.Index,
                    e.Child.As<SledProjectFilesBreakpointType>()));
        }

        #endregion

        #region Member Methods

        private void SubscribeToEvents(DomNode root)
        {
            root.AttributeChanged += DomNodeAttributeChanged;
            root.ChildInserted += DomNodeChildInserted;
            root.ChildRemoving += DomNodeChildRemoving;
        }

        private void UnsubscribeFromEvents(DomNode root)
        {
            root.AttributeChanged -= DomNodeAttributeChanged;
            root.ChildInserted -= DomNodeChildInserted;
            root.ChildRemoving -= DomNodeChildRemoving;
        }

        #endregion

        #region Private Classes

        private class BreakpointComparer : IComparer<TreeListView.Node>
        {
            public BreakpointComparer(TreeListView owner)
            {
                Owner = owner;
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

                var bpX = x.Tag.As<SledProjectFilesBreakpointType>();
                var bpY = y.Tag.As<SledProjectFilesBreakpointType>();

                var result = 0;
                SortFunction[] sortFuncs;

                switch (Owner.SortColumn)
                {
                    default: sortFuncs = s_sortColumn0; break;
                    case 1: sortFuncs = s_sortColumn1; break;
                    case 2: sortFuncs = s_sortColumn2; break;
                    case 3: sortFuncs = s_sortColumn3; break;
                    case 4: sortFuncs = s_sortColumn4; break;
                    case 5: sortFuncs = s_sortColumn5; break;
                    case 6: sortFuncs = s_sortColumn6; break;
                }

                for (var i = 0; i < sortFuncs.Length; i++)
                {
                    result = sortFuncs[i](bpX, bpY);
                    if (result != 0)
                        break;
                }

                if (Owner.SortOrder == SortOrder.Descending)
                    result *= -1;

                return result;
            }

            private TreeListView Owner { get; set; }
        }

        #endregion

        #region Breakpoint Sorting

        private delegate int SortFunction(SledProjectFilesBreakpointType x, SledProjectFilesBreakpointType y);

        private static int CompareEnabled(SledProjectFilesBreakpointType x, SledProjectFilesBreakpointType y)
        {
            if (x.Enabled && y.Enabled)
                return 0;

            return x.Enabled ? -1 : 1;
        }

        private static int CompareFile(SledProjectFilesBreakpointType x, SledProjectFilesBreakpointType y)
        {
            return string.Compare(x.File.Path, y.File.Path, StringComparison.CurrentCultureIgnoreCase);
        }

        private static int CompareLine(SledProjectFilesBreakpointType x, SledProjectFilesBreakpointType y)
        {
            if (x.Line == y.Line)
                return 0;

            return x.Line < y.Line ? -1 : 1;
        }

        private static int CompareCondition(SledProjectFilesBreakpointType x, SledProjectFilesBreakpointType y)
        {
            return string.Compare(x.Condition, y.Condition, StringComparison.CurrentCultureIgnoreCase);
        }

        private static int CompareConditionEnabled(SledProjectFilesBreakpointType x, SledProjectFilesBreakpointType y)
        {
            if (x.ConditionEnabled && y.ConditionEnabled)
                return 0;

            return x.ConditionEnabled ? -1 : 1;
        }

        private static int CompareConditionResult(SledProjectFilesBreakpointType x, SledProjectFilesBreakpointType y)
        {
            if (x.ConditionResult == y.ConditionResult)
                return 0;

            return x.ConditionResult ? -1 : 1;
        }

        private static int CompareEnvironment(SledProjectFilesBreakpointType x, SledProjectFilesBreakpointType y)
        {
            if (x.UseFunctionEnvironment == y.UseFunctionEnvironment)
                return 0;

            return x.UseFunctionEnvironment ? -1 : 1;
        }

        private static readonly SortFunction[] s_sortColumn0 =
            new SortFunction[]
            {
                CompareEnabled,
                CompareFile,
                CompareLine,
            };

        private static readonly SortFunction[] s_sortColumn1 =
            new SortFunction[]
            {
                CompareFile,
                CompareLine,
            };

        private static readonly SortFunction[] s_sortColumn2 =
            new SortFunction[]
            {
                CompareLine,
                CompareFile,
            };

        private static readonly SortFunction[] s_sortColumn3 =
            new SortFunction[]
            {
                CompareCondition,
                CompareFile,
                CompareLine,
            };

        private static readonly SortFunction[] s_sortColumn4 =
            new SortFunction[]
            {
                CompareConditionEnabled,
                CompareFile,
                CompareLine,
            };

        private static readonly SortFunction[] s_sortColumn5 =
            new SortFunction[]
            {
                CompareConditionResult,
                CompareFile,
                CompareLine,
            };

        private static readonly SortFunction[] s_sortColumn6 =
            new SortFunction[]
            {
                CompareEnvironment,
                CompareFile,
                CompareLine,
            };

        #endregion

        private ISledProjectService m_projectService;

        private readonly SledServiceReference<ISledGotoService> m_gotoService =
            new SledServiceReference<ISledGotoService>();
    }
}
