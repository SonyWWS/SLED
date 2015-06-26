/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

using System.Collections.Generic;
using System.Linq;

using Sce.Atf.Adaptation;
using Sce.Atf.Applications;
using Sce.Atf.Dom;

namespace Sce.Sled.Shared.Dom
{
    /// <summary>
    /// Complex Type for a call stack entry
    /// </summary>
    public class SledFindResultsListType : DomNodeAdapter, IItemView, ITreeListView
    {
        /// <summary>
        /// Get or set the name
        /// </summary>
        public string Name
        {
            get { return GetAttribute<string>(SledSchema.SledFindResultsListType.nameAttribute); }
            set { SetAttribute(SledSchema.SledFindResultsListType.nameAttribute, value); }
        }

        /// <summary>
        /// Gets FindResults sequence
        /// </summary>
        public IList<SledFindResultsType> FindResults
        {
            get { return GetChildList<SledFindResultsType>(SledSchema.SledFindResultsListType.FindResultsChild); }
        }

        #region IItemView Interface

        /// <summary>
        /// Fills in or modifies the given display info for the item
        /// </summary>
        /// <param name="item">Item</param>
        /// <param name="info">Display info to update</param>
        public void GetInfo(object item, ItemInfo info)
        {
            if (item.Is<SledFindResultsListType>())
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
        /// Get root level objects
        /// </summary>
        public IEnumerable<object> Roots
        {
            get { return FindResults; }
        }

        /// <summary>
        /// Get columns
        /// </summary>
        public string[] ColumnNames
        {
            get { return SledFindResultsType.TheColumnNames; }
        }

        #endregion
    }
}
