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

using Sce.Sled.Lua.Resources;

namespace Sce.Sled.Lua.Dom
{
    /// <summary>
    /// Complex Type for a list of Lua states
    /// </summary>
    public class SledLuaStateListType : DomNodeAdapter, IItemView, ITreeListView, IObservableContext, IValidationContext
    {
        /// <summary>
        /// Gets/sets the name attribute
        /// </summary>
        public string Name
        {
            get { return GetAttribute<string>(SledLuaSchema.SledLuaStateListType.nameAttribute); }
            set { SetAttribute(SledLuaSchema.SledLuaStateListType.nameAttribute, value); }
        }

        /// <summary>
        /// Gets the LuaStates sequence
        /// </summary>
        public IList<SledLuaStateType> LuaStates
        {
            get { return GetChildList<SledLuaStateType>(SledLuaSchema.SledLuaStateListType.LuaStatesChild); }
        }

        #region IItemView Interface

        /// <summary>
        /// Fill in items for GUI
        /// </summary>
        /// <param name="item"></param>
        /// <param name="info">item to fill in</param>
        public void GetInfo(object item, ItemInfo info)
        {
            if (item.Is<SledLuaStateListType>())
            {
                info.Label = Name;
                info.IsLeaf = false;
                info.Description = info.Label;
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

        public IEnumerable<object> GetChildren(object parent)
        {
            yield break;
        }

        public IEnumerable<object> Roots
        {
            get { return LuaStates; }
        }

        public string[] ColumnNames
        {
            get { return TheColumnNames; }
        }

        #endregion

        #region IObservableContext Interface

        public event EventHandler<ItemInsertedEventArgs<object>> ItemInserted;

        public event EventHandler<ItemRemovedEventArgs<object>> ItemRemoved;

        public event EventHandler<ItemChangedEventArgs<object>> ItemChanged;

        public event EventHandler Reloaded { add { } remove { } } // Cheap trick to avoid compiler warning

        #endregion

        #region IValidationContext Interface

        public event EventHandler Beginning;

        public event EventHandler Cancelled { add { } remove { } } // Cheap trick to avoid compiler warning

        public event EventHandler Ending;

        public event EventHandler Ended;

        #endregion

        public static readonly string[] TheColumnNames =
        {
            Localization.SledLuaLuaStateAddress,
            Localization.SledLuaLuaStateName
        };

        public void ValidationBeginning()
        {
            Beginning.Raise(this, EventArgs.Empty);
        }

        public void ValidationEnding()
        {
            Ending.Raise(this, EventArgs.Empty);
            Ended.Raise(this, EventArgs.Empty);
        }

        protected override void OnNodeSet()
        {
            DomNode.AttributeChanged += DomNodeAttributeChanged;
            DomNode.ChildInserted += DomNodeChildInserted;
            DomNode.ChildRemoving += DomNodeChildRemoving;

            base.OnNodeSet();
        }

        private void DomNodeAttributeChanged(object sender, AttributeEventArgs e)
        {
            if (!e.DomNode.Is<SledLuaStateType>())
                return;

            ItemChanged.Raise(
                this,
                new ItemChangedEventArgs<object>(e.DomNode.As<SledLuaStateType>()));
        }

        private void DomNodeChildInserted(object sender, ChildEventArgs e)
        {
            if (!e.Child.Is<SledLuaStateType>())
                return;

            ItemInserted.Raise(
                this,
                new ItemInsertedEventArgs<object>(e.Index, e.Child.As<SledLuaStateType>()));
        }

        private void DomNodeChildRemoving(object sender, ChildEventArgs e)
        {
            if (!e.Child.Is<SledLuaStateType>())
                return;

            ItemRemoved.Raise(
                this,
                new ItemRemovedEventArgs<object>(e.Index, e.Child.As<SledLuaStateType>()));
        }
    }
}