/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

using System.Collections.Generic;

using Sce.Atf.Adaptation;
using Sce.Atf.Applications;
using Sce.Atf.Dom;

using Sce.Sled.Shared.Controls;

namespace Sce.Sled.Shared.Dom
{
    /// <summary>
    /// Complex Type for a function
    /// </summary>
    public abstract class SledFunctionBaseType : DomNodeAdapter, IItemView, ISledProjectFilesTreeViewable
    {
        /// <summary>
        /// Get or set the name attribute
        /// </summary>
        public abstract string Name
        {
            get; // { return GetAttribute<string>(SledSchema.SledFunctionBaseType.nameAttribute); }
            set; // { SetAttribute(SledSchema.SledFunctionBaseType.nameAttribute, value); }
        }

        /// <summary>
        /// Get or set the line defined attribute
        /// </summary>
        public abstract int LineDefined
        {
            get; // { return GetAttribute<int>(SledSchema.SledFunctionBaseType.line_definedAttribute); }
            set; // { SetAttribute(SledSchema.SledFunctionBaseType.line_definedAttribute, value); }
        }

        #region ISledProjectFilesTreeViewable Interface

        /// <summary>
        /// Get or set whether the item is expanded</summary>
        bool ISledProjectFilesTreeViewable.Expanded { get; set; }

        /// <summary>
        /// Gets the parent</summary>
        ISledProjectFilesTreeViewable ISledProjectFilesTreeViewable.Parent
        {
            get { return DomNode.Parent.As<ISledProjectFilesTreeViewable>(); }
        }

        /// <summary>
        /// Gets any children</summary>
        IEnumerable<ISledProjectFilesTreeViewable> ISledProjectFilesTreeViewable.Children
        {
            get { yield break; }
        }

        #endregion

        #region IDisposable

        /// <summary>
        /// Dispose
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
            return copy[0].As<SledFunctionBaseType>();
        }

        #endregion

        #region IItemView

        /// <summary>
        /// Fill in contents for displaying on a GUI
        /// </summary>
        /// <param name="item">Item</param>
        /// <param name="info">Display info to update</param>
        public void GetInfo(object item, ItemInfo info)
        {
            info.Label = Name;
            info.IsLeaf = true;
            info.AllowLabelEdit = false;
            info.Description = "Function: " + Name + ", line: " + LineDefined;
            info.ImageIndex = info.GetImageIndex(Atf.Resources.DataImage);
        }

        #endregion

        /// <summary>
        /// Gets the file the function belongs to
        /// </summary>
        public SledProjectFilesFileType File
        {
            get { return DomNode.Parent == null ? null : DomNode.Parent.As<SledProjectFilesFileType>(); }
        }
    }
}
