/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

using Sce.Atf.Applications;
using Sce.Atf.Dom;

namespace Sce.Sled.Shared.Dom
{
    /// <summary>
    /// ComplexType of watched items list
    /// </summary>
    public abstract class SledVarBaseWatchListType : DomNodeAdapter, IItemView
    {
        /// <summary>
        /// Get or set name attribute
        /// </summary>
        public abstract string Name
        {
            get; // { return GetAttribute<string>(SledSchema.SledVarBaseWatchListType.nameAttribute); }
            set; // { SetAttribute(SledSchema.SledVarBaseWatchListType.nameAttribute, value); }
        }

        /// <summary>
        /// Fills in or modifies the given display info for the item</summary>
        /// <param name="item">Item</param>
        /// <param name="info">Display info to update</param>
        public abstract void GetInfo(object item, ItemInfo info);
    }
}
