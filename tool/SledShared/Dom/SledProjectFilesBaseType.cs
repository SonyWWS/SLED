/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

using Sce.Atf.Dom;

namespace Sce.Sled.Shared.Dom
{
    /// <summary>
    /// Base type to derive project file objects from
    /// </summary>
    public abstract class SledProjectFilesBaseType : DomNodeAdapter
    {
        /// <summary>
        /// Get or set the name attribute
        /// </summary>
        public abstract string Name
        {
            get; // { return GetAttribute<string>(SledSchema.SledProjectFilesBaseType.nameAttribute); }
            set; // { SetAttribute(SledSchema.SledProjectFilesBaseType.nameAttribute, value); }
        }

        /// <summary>
        /// Get or set expanded attribute
        /// </summary>
        public abstract bool Expanded
        {
            get; // { return GetAttribute<bool>(SledSchema.SledProjectFilesBaseType.expandedAttribute); }
            set; // { SetAttribute(SledSchema.SledProjectFilesBaseType.expandedAttribute, value); }
        }

        /// <summary>
        /// Copy name and expanded state from one item to another
        /// </summary>
        /// <param name="target">Item to copy to</param>
        /// <param name="copyFrom">Item to copy from</param>
        protected static void SyncNameAndExpanded(SledProjectFilesBaseType target, SledProjectFilesBaseType copyFrom)
        {
            target.Name = copyFrom.Name;
            target.Expanded = copyFrom.Expanded;
        }
    }
}
