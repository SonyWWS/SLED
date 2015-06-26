/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

using System;
using System.Collections.Generic;

using Sce.Atf;
using Sce.Atf.Adaptation;
using Sce.Atf.Applications;
using Sce.Atf.Dom;

using Sce.Sled.Shared.Resources;

namespace Sce.Sled.Shared.Dom
{
    /// <summary>
    /// Complex Type for a call stack entry
    /// </summary>
    public class SledCallStackListType : DomNodeAdapter, IItemView, ITreeListView, IObservableContext, IValidationContext
    {
        /// <summary>
        /// Get or set the name
        /// </summary>
        public string Name
        {
            get { return GetAttribute<string>(SledSchema.SledCallStackListType.nameAttribute); }
            set { SetAttribute(SledSchema.SledCallStackListType.nameAttribute, value); }
        }

        /// <summary>
        /// Gets CallStack sequence
        /// </summary>
        public IList<SledCallStackType> CallStack
        {
            get { return GetChildList<SledCallStackType>(SledSchema.SledCallStackListType.CallStackChild); }
        }

        #region IItemView Interface

        /// <summary>
        /// For IItemView
        /// </summary>
        /// <param name="item">Item</param>
        /// <param name="info">Display info to update</param>
        public void GetInfo(object item, ItemInfo info)
        {
            if (item.Is<SledCallStackListType>())
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
        /// Get children for a parent item
        /// </summary>
        /// <param name="parent">Parent object</param>
        /// <returns>Enumeration of the children of the parent object</returns>
        public IEnumerable<object> GetChildren(object parent)
        {
            yield break;
        }

        /// <summary>
        /// Get root level items
        /// </summary>
        public IEnumerable<object> Roots
        {
            get { return CallStack; }
        }

        /// <summary>
        /// Get column names
        /// </summary>
        public string[] ColumnNames
        {
            get { return TheColumnNames; }
        }

        #endregion

        #region IObservableContext Interface

        /// <summary>
        /// IObservableContext ItemInserted Event
        /// </summary>
        public virtual event EventHandler<ItemInsertedEventArgs<object>> ItemInserted;

        /// <summary>
        /// IObservableContext ItemRemoved Event
        /// </summary>
        public virtual event EventHandler<ItemRemovedEventArgs<object>> ItemRemoved;

        /// <summary>
        /// IObservableContext ItemChanged Event
        /// </summary>
        public virtual event EventHandler<ItemChangedEventArgs<object>> ItemChanged;

        /// <summary>
        /// IObservableContext Reloaded Event
        /// </summary>
        public virtual event EventHandler Reloaded { add { } remove { } } // Cheap trick to avoid compiler warning

        #endregion

        #region IValidationContext Interface

        /// <summary>
        /// IValidationContext Beginning Event
        /// </summary>
        public event EventHandler Beginning;

        /// <summary>
        /// IValidationContext Cancelled Event
        /// </summary>
        public event EventHandler Cancelled { add { } remove { } } // Cheap trick to avoid compiler warning

        /// <summary>
        /// IValidationContext Ending Event
        /// </summary>
        public event EventHandler Ending;

        /// <summary>
        /// IValidationContext Ended Event
        /// </summary>
        public event EventHandler Ended;

        #endregion

        /// <summary>
        /// Begin validation
        /// </summary>
        public void ValidationBeginning()
        {
            if (m_validating)
                return;

            try
            {
                Beginning.Raise(this, EventArgs.Empty);
            }
            finally
            {
                m_validating = true;
            }
        }

        /// <summary>
        /// End validation
        /// </summary>
        public void ValidationEnded()
        {
            if (!m_validating)
                return;

            try
            {
                Ending.Raise(this, EventArgs.Empty);
                Ended.Raise(this, EventArgs.Empty);
            }
            finally
            {
                m_validating = false;
            }
        }

        /// <summary>
        /// Perform one-time initialization when this adapter's DomNode property is set.
        /// The DomNode property is only ever set once for the lifetime of this adapter.</summary>
        protected override void OnNodeSet()
        {
            DomNode.AttributeChanged += DomNodeAttributeChanged;
            DomNode.ChildInserted += DomNodeChildInserted;
            DomNode.ChildRemoving += DomNodeChildRemoving;

            base.OnNodeSet();
        }

        /// <summary>
        /// Column names to use with SledTreeListView control
        /// </summary>
        public readonly static string[] TheColumnNames =
        {
            "*",
            Localization.SledSharedFunction,
            Localization.SledSharedFile,
            Localization.SledSharedLine
        };

        private void DomNodeAttributeChanged(object sender, AttributeEventArgs e)
        {
            if (!e.DomNode.Is<SledCallStackType>())
                return;

            ItemChanged.Raise(
                this,
                new ItemChangedEventArgs<object>(e.DomNode.As<SledCallStackType>()));
        }

        private void DomNodeChildInserted(object sender, ChildEventArgs e)
        {
            if (!e.Child.Is<SledCallStackType>())
                return;

            ItemInserted.Raise(
                this,
                new ItemInsertedEventArgs<object>(e.Index, e.Child.As<SledCallStackType>()));
        }

        private void DomNodeChildRemoving(object sender, ChildEventArgs e)
        {
            if (!e.Child.Is<SledCallStackType>())
                return;

            ItemRemoved.Raise(
                this,
                new ItemRemovedEventArgs<object>(e.Index, e.Child.As<SledCallStackType>()));
        }

        private bool m_validating;
    }
}
