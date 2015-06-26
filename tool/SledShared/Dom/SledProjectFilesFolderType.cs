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

using Sce.Sled.Shared.Controls;

namespace Sce.Sled.Shared.Dom
{
    /// <summary>
    /// Complex Type for a folder
    /// </summary>
    public class SledProjectFilesFolderType : SledProjectFilesBaseType, ISledProjectFilesTreeViewable, IItemView
    {
        /// <summary>
        /// Get or set name attribute
        /// </summary>
        public override string Name
        {
            get { return GetAttribute<string>(SledSchema.SledProjectFilesFolderType.nameAttribute); }
            set { SetAttribute(SledSchema.SledProjectFilesFolderType.nameAttribute, value); }
        }

        /// <summary>
        /// Get or set expanded attribute
        /// </summary>
        public override bool Expanded
        {
            get { return GetAttribute<bool>(SledSchema.SledProjectFilesFolderType.expandedAttribute); }
            set { SetAttribute(SledSchema.SledProjectFilesFolderType.expandedAttribute, value); }
        }

        /// <summary>
        /// Get the files element
        /// </summary>
        public IList<SledProjectFilesFileType> Files
        {
            get { return GetChildList<SledProjectFilesFileType>(SledSchema.SledProjectFilesFolderType.FilesChild); }
        }

        /// <summary>
        /// Get the folders element
        /// </summary>
        public IList<SledProjectFilesFolderType> Folders
        {
            get { return GetChildList<SledProjectFilesFolderType>(SledSchema.SledProjectFilesFolderType.FoldersChild); }
        }

        /// <summary>
        /// Get the project the folder is in
        /// </summary>
        public SledProjectFilesType Project
        {
            get { return DomNode.GetRoot().As<SledProjectFilesType>(); }
        }

        /// <summary>
        /// Get whether the folder has no files and no sub-folders
        /// </summary>
        public bool Empty
        {
            get { return (Files.Count <= 0) && (Folders.Count <= 0); }
        }

        #region ISledProjectFilesTreeViewable Interface

        /// <summary>
        /// Get the parent</summary>
        ISledProjectFilesTreeViewable ISledProjectFilesTreeViewable.Parent
        {
            get { return DomNode.Parent.As<ISledProjectFilesTreeViewable>(); }
        }

        /// <summary>
        /// Get any children</summary>
        IEnumerable<ISledProjectFilesTreeViewable> ISledProjectFilesTreeViewable.Children
        {
            get
            {
                var children = new List<ISledProjectFilesTreeViewable>();
                children.AddRange(Folders.AsIEnumerable<ISledProjectFilesTreeViewable>());
                children.AddRange(Files.AsIEnumerable<ISledProjectFilesTreeViewable>());
                return children;
            }
        }

        #endregion

        #region IItemView Interface

        /// <summary>
        /// Fills in or modifies the given display info for the item</summary>
        /// <param name="item">Item</param>
        /// <param name="info">Display info to update</param>
        public void GetInfo(object item, ItemInfo info)
        {
            info.Label = Name;
            info.IsLeaf = Empty;
            info.AllowLabelEdit = true;
            info.Description = Name;
            info.ImageIndex = info.GetImageIndex(Atf.Resources.FolderImage);
        }

        #endregion

        /// <summary>
        /// SpfCopier Class
        /// </summary>
        public class SpfCopier : SledSpfReader.ICopier
        {
            /// <summary>
            /// Run the copier
            /// </summary>
            /// <param name="domProjectTree">Tree to copy</param>
            /// <param name="domTempTree">Copied tree</param>
            public void Run(DomNode domProjectTree, DomNode domTempTree)
            {
                var lstProjFolders = new List<SledProjectFilesFolderType>();
                GatherNodeTypes(domProjectTree, lstProjFolders);

                var lstTempFolders = new List<SledProjectFilesFolderType>();
                GatherNodeTypes(domTempTree, lstTempFolders);

                var lstPairs = new List<Pair<SledProjectFilesFolderType, SledProjectFilesFolderType>>();
                foreach (var projFolder in lstProjFolders)
                {
                    foreach (var tempFolder in lstTempFolders)
                    {
                        if ((projFolder.Files.Count == tempFolder.Files.Count) &&
                            (projFolder.Folders.Count == tempFolder.Folders.Count) &&
                            (string.Compare(projFolder.Name, tempFolder.Name, StringComparison.Ordinal) == 0))
                        {
                            lstPairs.Add(new Pair<SledProjectFilesFolderType, SledProjectFilesFolderType>(projFolder, tempFolder));
                            break;
                        }
                    }
                }

                if (lstPairs.Count <= 0)
                    return;

                foreach (var pair in lstPairs)
                {
                    SyncNameAndExpanded(pair.First, pair.Second);
                }
            }

            private static void GatherNodeTypes(DomNode domNode, ICollection<SledProjectFilesFolderType> lstFolders)
            {
                if (domNode.Is<SledProjectFilesFolderType>())
                    lstFolders.Add(domNode.As<SledProjectFilesFolderType>());

                foreach (var child in domNode.Children)
                {
                    GatherNodeTypes(child, lstFolders);
                }
            }
        }
    }
}
