/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

using System.Collections.Generic;

using Sce.Atf.Applications;
using Sce.Atf.Dom;

namespace Sce.Sled.Shared.Dom
{
    /// <summary>
    /// Complex Type of a base variable
    /// </summary>
    public abstract class SledVarBaseType : DomNodeAdapter, ISledVarBaseType
    {
        /// <summary>
        /// Get or set the name attribute
        /// <remarks>Variable's name</remarks>
        /// </summary>
        public abstract string Name { get; set; }

        /// <summary>
        /// Get the locations attribute
        /// <remarks>Returns a list of all the places the variable is
        /// referenced in various script files</remarks>
        /// </summary>
        public abstract IList<SledVarLocationType> Locations { get; }

        #region IDisposable Interface

        /// <summary>
        /// Dispose
        /// </summary>
        public abstract void Dispose();

        #endregion

        #region IItemView Interface

        /// <summary>
        /// Fills in or modifies the given display info for the item</summary>
        /// <param name="item">Item</param>
        /// <param name="info">Display info to update</param>
        public abstract void GetInfo(object item, ItemInfo info);

        #endregion

        #region ICloneable Interface

        /// <summary>
        /// Clone the object
        /// </summary>
        /// <returns>New cloned object</returns>
        public abstract object Clone();
        
        #endregion
    }
}
