/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

using System.Collections.Generic;

using Sce.Atf.Adaptation;
using Sce.Atf.Applications;
using Sce.Atf.Dom;

namespace Sce.Sled.Shared.Dom
{
    /// <summary>
    /// Complex Type for a syntax error list
    /// </summary>
    public class SledSyntaxErrorListType : DomNodeAdapter, IItemView, ITreeListView
    {
        /// <summary>
        /// Get or set the name attribute
        /// </summary>
        public string Name
        {
            get { return GetAttribute<string>(SledSchema.SledSyntaxErrorListType.nameAttribute); }
            set { SetAttribute(SledSchema.SledSyntaxErrorListType.nameAttribute, value); }
        }

        /// <summary>
        /// Get the Errors sequence
        /// </summary>
        public IList<SledSyntaxErrorType> Errors
        {
            get { return GetChildList<SledSyntaxErrorType>(SledSchema.SledSyntaxErrorListType.ErrorsChild); }
        }

        #region IItemView Interface

        /// <summary>
        /// Fills in or modifies the given display info for the item</summary>
        /// <param name="item">Item</param>
        /// <param name="info">Display info to update</param>
        public void GetInfo(object item, ItemInfo info)
        {
            if (item.Is<SledSyntaxErrorListType>())
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
            yield break;
        }

        /// <summary>
        /// Get root level items
        /// </summary>
        public IEnumerable<object> Roots
        {
            get { return Errors; }
        }

        /// <summary>
        /// Get column names
        /// </summary>
        public string[] ColumnNames
        {
            get { return SledSyntaxErrorType.TheColumnNames; }
        }

        #endregion
    }
}
