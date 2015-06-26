/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

using System;
using System.ComponentModel.Composition;
using System.Linq;
using System.Windows.Forms;

using Sce.Atf;
using Sce.Atf.Applications;
using Sce.Atf.Controls;
using Sce.Atf.Controls.PropertyEditing;

using Sce.Sled.Shared.Services;
using Sce.Sled.Shared.Utilities;

namespace Sce.Sled.Shared.Controls
{
    /// <summary>
    /// SledTreeListViewEditor Class
    /// </summary>
    [InheritedExport(typeof(IInitializable))]
    [InheritedExport(typeof(SledTreeListViewEditor))]
    [InheritedExport(typeof(IInstancingContext))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class SledTreeListViewEditor : TreeListViewEditor, IControlHostClient, IInitializable, IInstancingContext
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Name of the GUI</param>
        /// <param name="controlGroup">Image name to use for the GUI</param>
        /// <param name="columns">Column names</param>
        /// <param name="style">TreeListView.Style of the GUI</param>
        /// <param name="image">StandardControlGroup to place the GUI in</param>
        [ImportingConstructor]
        public SledTreeListViewEditor(
            string name,
            string image,
            string[] columns,
            TreeListView.Style style,
            StandardControlGroup controlGroup)
            : base(style)
        {
            TreeListView.Name = name;

            foreach (var column in columns)
                TreeListView.Columns.Add(new TreeListView.Column(column));

            {
                var theImage =
                    (!string.IsNullOrEmpty(image) &&
                    ResourceUtil.GetImageList16().Images.ContainsKey(image))
                        ? ResourceUtil.GetImageList16().Images[image]
                        : null;

                m_controlInfo =
                    new ControlInfo(
                        TreeListView.Name,
                        TreeListView.Name,
                        controlGroup,
                        theImage);
            }
        }

        #region IInitializable Interface

        /// <summary>
        /// Finishes initializing component by registering with settings service
        /// </summary>
        public virtual void Initialize()
        {
            {
                var owner =
                    string.Format(
                        "{0}-{1}-TreeListView-Settings",
                        this,
                        TreeListView.Name);

                SettingsService.RegisterSettings(
                    SledUtil.GuidFromString(owner),
                    new BoundPropertyDescriptor(
                        TreeListView,
                        () => TreeListView.PersistedSettings,
                        owner,
                        null,
                        owner));
            }

            ControlHostService.RegisterControl(TreeListView, m_controlInfo, this);

            StandardEditCommands.Copying += StandardEditCommandsCopying;
            StandardEditCommands.Copied += StandardEditCommandsCopied;

            if (!AllowDebugFreeze)
                return;

            DebugFreezeService.Freezing += DebugFreezeServiceFreezing;
            DebugFreezeService.Thawing += DebugFreezeServiceThawing;
        }

        #endregion

        #region IControlHostClient Interface

        /// <summary>
        /// Notifies the client that its Control has been activated. Activation occurs when
        /// the Control gets focus, or a parent "host" Control gets focus.</summary>
        /// <param name="control">Client Control that was activated</param>
        /// <remarks>This method is only called by IControlHostService if the Control was previously
        /// registered for this IControlHostClient.</remarks>
        public virtual void Activate(Control control)
        {
            if (!IsUs(control))
                return;

            ContextRegistry.ActiveContext = this;
        }

        /// <summary>
        /// Notifies the client that its Control has been deactivated. Deactivation occurs when
        /// another Control or "host" Control gets focus.</summary>
        /// <param name="control">Client Control that was deactivated</param>
        /// <remarks>This method is only called by IControlHostService if the Control was previously
        /// registered for this IControlHostClient.</remarks>
        public virtual void Deactivate(Control control)
        {
            if (!IsUs(control))
                return;

            ContextRegistry.RemoveContext(this);
        }

        /// <summary>
        /// Requests permission to close the client's Control</summary>
        /// <param name="control">Client Control to be closed</param>
        /// <returns>True if the Control can close, or false to cancel</returns>
        /// <remarks>
        /// 1. This method is only called by IControlHostService if the Control was previously
        /// registered for this IControlHostClient.
        /// 2. If true is returned, the IControlHostService calls its own
        /// UnregisterControl. The IControlHostClient has to call RegisterControl again
        /// if it wants to re-register this Control.
        /// 3. This method is NOT called when the user toggles the visibility using the Windows
        /// menu commands. To know if your Control is actually visible or not requires a bit
        /// of a hack, as the VisibleChanged event is only raised when this Control is made
        /// visible but not when it is hidden. This is a .NET bug. http://tracker.ship.scea.com/jira/browse/WWSATF-1335 </remarks>
        public virtual bool Close(Control control)
        {
            return true;
        }

        #endregion

        #region Implementation of IInstancingContext

        /// <summary>
        /// Returns whether the context can copy the selection</summary>
        /// <returns>True iff the context can copy</returns>
        public bool CanCopy()
        {
            return
                TreeListViewAdapter.Selection.Any() &&
                !string.IsNullOrEmpty(GetCopyText());
        }

        /// <summary>
        /// Copies the selection. Returns a data object representing the copied items.</summary>
        /// <returns>Data object representing the copied items; e.g., a
        /// System.Windows.Forms.IDataObject object</returns>
        public object Copy()
        {
            m_allowSystemClipboardUse = true;
            var text = GetCopyText();
            return new DataObject(DataFormats.Text, text);
        }

        /// <summary>
        /// Returns whether the context can insert the data object</summary>
        /// <param name="dataObject">Data to insert; e.g., System.Windows.Forms.IDataObject</param>
        /// <returns>True iff the context can insert the data object</returns>
        /// <remarks>ApplicationUtil calls this method in its CanInsert method, BUT
        /// if the context also implements IHierarchicalInsertionContext,
        /// IHierarchicalInsertionContext is preferred and the IInstancingContext
        /// implementation is ignored for insertion.</remarks>
        public bool CanInsert(object dataObject)
        {
            return false;
        }

        /// <summary>
        /// Inserts the data object into the context</summary>
        /// <param name="dataObject">Data to insert; e.g., System.Windows.Forms.IDataObject</param>
        /// <remarks>ApplicationUtil calls this method in its Insert method, BUT
        /// if the context also implements IHierarchicalInsertionContext,
        /// IHierarchicalInsertionContext is preferred and the IInstancingContext
        /// implementation is ignored for insertion.</remarks>
        public void Insert(object dataObject)
        {
        }

        /// <summary>
        /// Returns whether the context can delete the selection</summary>
        /// <returns>True iff the context can delete</returns>
        public bool CanDelete()
        {
            return false;
        }

        /// <summary>
        /// Deletes the selection</summary>
        public void Delete()
        {
        }

        #endregion

        #region StandardEditCommands Events

        private void StandardEditCommandsCopying(object sender, EventArgs e)
        {
            if (m_allowSystemClipboardUse)
                StandardEditCommands.UseSystemClipboard = true;
        }

        private void StandardEditCommandsCopied(object sender, EventArgs e)
        {
            try
            {
                if (m_allowSystemClipboardUse)
                    StandardEditCommands.UseSystemClipboard = false;
            }
            finally
            {
                m_allowSystemClipboardUse = false;
            }
        }

        #endregion

        #region ISledDebugFreezeService Events

        private void DebugFreezeServiceFreezing(object sender, EventArgs e)
        {
            Freeze();
        }

        private void DebugFreezeServiceThawing(object sender, EventArgs e)
        {
            Thaw();
        }

        #endregion

        /// <summary>
        /// Get or set the name
        /// </summary>
        public string Name
        {
            get { return TreeListView.Name; }
            set
            {
                TreeListView.Name = value;
                m_controlInfo.Name = value;
            }
        }

        /// <summary>
        /// Get or set whether to respond to ISledDebugFreezeService events
        /// </summary>
        public bool AllowDebugFreeze
        {
            get { return m_allowDebugFreeze; }
            set
            {
                // Value not changing
                if (value == m_allowDebugFreeze)
                    return;

                m_allowDebugFreeze = value;

                if (DebugFreezeService == null)
                    return;

                if (m_allowDebugFreeze)
                {
                    DebugFreezeService.Freezing += DebugFreezeServiceFreezing;
                    DebugFreezeService.Thawing += DebugFreezeServiceThawing;
                }
                else
                {
                    DebugFreezeService.Freezing -= DebugFreezeServiceFreezing;
                    DebugFreezeService.Thawing -= DebugFreezeServiceThawing;
                }

                // Make sure not to get stuck in frozen state
                if (!m_allowDebugFreeze && DebugFreezeService.Frozen)
                    Thaw();
            }
        }

        #region Member Methods

        /// <summary>
        /// Determine whether the control is us
        /// </summary>
        /// <param name="control">Control tested</param>
        /// <returns>Whether control is us</returns>
        protected bool IsUs(Control control)
        {
            if (ReferenceEquals(TreeListView.Control, control))
                return true;

            if (ReferenceEquals(TreeListView, control))
                return true;

            return false;
        }

        /// <summary>
        /// Return text to copy to the clipboard. Allows derived classes to supply their own text without all the extra clipboard machinery.
        /// </summary>
        /// <returns>Text to copy to clipboard</returns>
        protected virtual string GetCopyText()
        {
            return string.Empty;
        }

        private void Freeze()
        {
            if (TreeListView == null)
                return;

            TreeListView.Control.Enabled = false;
        }

        private void Thaw()
        {
            if (TreeListView == null)
                return;

            TreeListView.Control.Enabled = true;
        }

        #endregion

        /// <summary>
        /// IContextRegistry
        /// </summary>
        [Import]
        protected IContextRegistry ContextRegistry;

        /// <summary>
        /// ISettingsService
        /// </summary>
        [Import]
        protected ISettingsService SettingsService;

        /// <summary>
        /// IControlHostService
        /// </summary>
        [Import]
        protected IControlHostService ControlHostService;

        /// <summary>
        /// ISledDebugFreezeService
        /// </summary>
        [Import]
        protected ISledDebugFreezeService DebugFreezeService;

        private bool m_allowDebugFreeze;
        private bool m_allowSystemClipboardUse;

        private readonly ControlInfo m_controlInfo;
    }

    /// <summary>
    /// Sled recursive checkboxes TreeView Class
    /// </summary>
    public class SledRecursiveCheckBoxesTreeView : TreeView
    {
        /// <summary>
        /// Get or set whether nodes are checked or unchecked recursively
        /// </summary>
        public bool RecursiveCheckBoxes { get; set; }

        /// <summary>
        /// Raises the System.Windows.Forms.TreeView.AfterCheck event
        /// </summary>
        /// <param name="e">TreeView event arguments</param>
        protected override void OnAfterCheck(TreeViewEventArgs e)
        {
            base.OnAfterCheck(e);

            if (!RecursiveCheckBoxes)
                return;

            if (e.Action == TreeViewAction.Unknown)
                return;

            if (e.Node.Nodes.Count > 0)
                CheckAllNodes(e.Node, e.Node.Checked);
        }

        /// <summary>
        /// Override System.Windows.Forms.Control.WndProc(System.Windows.Forms.Message@)
        /// </summary>
        /// <param name="m">Windows System.Windows.Forms.Message to process</param>
        protected override void WndProc(ref Message m)
        {
            if (RecursiveCheckBoxes && (m.Msg == User32.WM_LBUTTONDBLCLK))
                return;
            
            base.WndProc(ref m);
        }

        private static void CheckAllNodes(TreeNode rootNode, bool check)
        {
            foreach (TreeNode node in rootNode.Nodes)
            {
                node.Checked = check;

                if (node.Nodes.Count > 0)
                    CheckAllNodes(node, check);
            }
        }
    }
}