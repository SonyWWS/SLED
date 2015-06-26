/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

using System;
using System.Collections.Generic;
using System.Linq;

using Sce.Atf;
using Sce.Atf.Adaptation;
using Sce.Atf.Applications;
using Sce.Atf.Dom;

namespace Sce.Sled.Shared.Dom
{
    /// <summary>
    /// Complex Type for a list of profile info items
    /// </summary>
    public class SledProfileInfoListType : DomNodeAdapter, IItemView, ITreeListView, IObservableContext
    {
        /// <summary>
        /// Types enumeration
        /// </summary>
        public enum Display
        {
            /// <summary>
            /// Function calls, times, etc.
            /// </summary>
            Normal,

            /// <summary>
            /// Function call graph
            /// </summary>
            CallGraph,
        }

        /// <summary>
        /// Get or set display mode
        /// </summary>
        public Display DisplayMode { get; set; }

        /// <summary>
        /// Get or set the name attribute
        /// </summary>
        public string Name
        {
            get { return GetAttribute<string>(SledSchema.SledProfileInfoListType.nameAttribute); }
            set { SetAttribute(SledSchema.SledProfileInfoListType.nameAttribute, value); }
        }

        /// <summary>
        /// Gets the ProfileInfo sequence
        /// </summary>
        public IList<SledProfileInfoType> ProfileInfo
        {
            get { return GetChildList<SledProfileInfoType>(SledSchema.SledProfileInfoListType.ProfileInfoChild); }
        }

        #region IItemView Interface

        /// <summary>
        /// Fills in or modifies the given display info for the item</summary>
        /// <param name="item">Item</param>
        /// <param name="info">Display info to update</param>
        public void GetInfo(object item, ItemInfo info)
        {
            if (item.Is<SledProfileInfoListType>())
            {
                info.Label = Name;
                info.ImageIndex = info.GetImageIndex(Atf.Resources.FolderImage);

                return;
            }

            var itemView = item.As<IItemView>();
            if ((itemView == null) || ReferenceEquals(itemView, this))
                return;

            itemView.GetInfo(item, info);
        }

        #endregion

        #region ITreeListView Interface

        /// <summary>
        /// Get children for a parent
        /// </summary>
        /// <param name="parent">Parent object</param>
        /// <returns>Enumeration of the children of the parent object</returns>
        public IEnumerable<object> GetChildren(object parent)
        {
            if (DisplayMode == Display.Normal)
                yield break;

            var pi = parent.As<SledProfileInfoType>();
            if (pi == null)
                yield break;

            foreach (var child in pi.ProfileInfo)
                yield return child;
        }

        /// <summary>
        /// Get root level items
        /// </summary>
        public IEnumerable<object> Roots
        {
            get { return ProfileInfo; }
        }

        /// <summary>
        /// Get columns
        /// </summary>
        public string[] ColumnNames
        {
            get
            {
                return
                    DisplayMode == Display.Normal
                        ? SledProfileInfoType.NormalColumnNames
                        : SledProfileInfoType.CallGraphColumnNames;
            }
        }

        #endregion

        #region IObservableContext Interface

        /// <summary>
        /// Event fired when item added
        /// </summary>
        public event EventHandler<ItemInsertedEventArgs<object>> ItemInserted;

        /// <summary>
        /// Event fired when item removed
        /// </summary>
        public event EventHandler<ItemRemovedEventArgs<object>> ItemRemoved;

        /// <summary>
        /// Event fired when item changed
        /// </summary>
        public event EventHandler<ItemChangedEventArgs<object>> ItemChanged;

        /// <summary>
        /// Event fired when reloaded
        /// </summary>
        public event EventHandler Reloaded { add { } remove { } } // Cheap way to stop compile warning

        #endregion

        /// <summary>
        /// Performs one-time initialization when this adapter's DomNode property is set.
        /// The DomNode property is only ever set once for the lifetime of this adapter.</summary>
        protected override void OnNodeSet()
        {
            DomNode.AttributeChanged += DomNodeAttributeChanged;
            DomNode.ChildInserted += DomNodeChildInserted;
            DomNode.ChildRemoving += DomNodeChildRemoving;

            base.OnNodeSet();
        }

        private void DomNodeAttributeChanged(object sender, AttributeEventArgs e)
        {
            if (!e.DomNode.Is<SledProfileInfoType>())
                return;

            ItemChanged.Raise(
                this,
                new ItemChangedEventArgs<object>(
                    e.DomNode.As<SledProfileInfoType>()));
        }

        private void DomNodeChildInserted(object sender, ChildEventArgs e)
        {
            if (!e.Child.Is<SledProfileInfoType>())
                return;

            ItemInserted.Raise(
                this,
                new ItemInsertedEventArgs<object>(
                    e.Index,
                    e.Child.As<SledProfileInfoType>(),
                    e.Parent.As<SledProfileInfoType>()));
        }

        private void DomNodeChildRemoving(object sender, ChildEventArgs e)
        {
            if (!e.Child.Is<SledProfileInfoType>())
                return;

            ItemRemoved.Raise(
                this,
                new ItemRemovedEventArgs<object>(
                    e.Index,
                    e.Child.As<SledProfileInfoType>(),
                    e.Parent.As<SledProfileInfoType>()));
        }
    }
}
