/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

using System;

using Sce.Atf.Adaptation;
using Sce.Atf.Applications;
using Sce.Atf.Dom;

namespace Sce.Sled.Shared.Dom
{
    /// <summary>
    /// Complex Type for an attribute
    /// </summary>
    public abstract class SledAttributeBaseType : DomNodeAdapter, IItemView, ICloneable
    {
        /// <summary>
        /// Get or set the name
        /// </summary>
        public abstract string Name
        {
            get; // { return GetAttribute<string>(SledSchema.SledAttributeBaseType.nameAttribute); }
            set; // { SetAttribute(SledSchema.SledAttributeBaseType.nameAttribute, value); }
        }

        #region IDisposable

        /// <summary>
        /// Dispose of resources
        /// </summary>
        public void Dispose()
        {
        }

        #endregion

        #region ICloneable

        /// <summary>
        /// Clone the object
        /// </summary>
        /// <returns>A new cloned object</returns>
        public object Clone()
        {
            var copy = DomNode.Copy(new[] { DomNode });
            copy[0].InitializeExtensions();
            return copy[0].As<SledAttributeBaseType>();
        }

        #endregion

        #region IItemView

        /// <summary>
        /// Fill in contents for displaying on a GUI
        /// </summary>
        /// <param name="item">Item</param>
        /// <param name="info">Display info to update</param>
        public virtual void GetInfo(object item, ItemInfo info)
        {
            info.Label = Name;
            info.IsLeaf = true;
            info.AllowLabelEdit = false;
            info.Description = "Name: " + Name;
            info.ImageIndex = info.GetImageIndex(Atf.Resources.DataImage);
        }

        #endregion
    }
}
