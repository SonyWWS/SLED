/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

using System;
using System.Collections.Generic;
using System.IO;

using Sce.Atf;
using Sce.Atf.Adaptation;
using Sce.Atf.Applications;
using Sce.Atf.Dom;

using Sce.Sled.Shared.Controls;
using Sce.Sled.Shared.Services;
using Sce.Sled.Shared.Utilities;

namespace Sce.Sled.Shared.Dom
{
    /// <summary>
    /// Complex Type for a project
    /// </summary>
    public class SledProjectFilesType : SledProjectFilesFolderType, IItemView, IResource, ISledProjectFilesTreeViewable
    {
        #region Persisted Stuff

        /// <summary>
        /// Get or set the asset directory attribute
        /// </summary>
        public string AssetDirectory
        {
            get
            {
                //
                // Want to return an absolute path
                //

                var assetDir = GetAttribute<string>(SledSchema.SledProjectFilesType.assetdirectoryAttribute);

                if (PathUtil.IsRelative(assetDir))
                    assetDir = SledUtil.GetAbsolutePath(assetDir, ProjectDirectory);

                return assetDir;
            }
            set
            {
                //
                // Try to store as a relative path
                // from the project directory
                //

                var assetDir =
                    PathUtil.IsRelative(value)
                        ? value
                        : SledUtil.GetRelativePath(value, ProjectDirectory);

                SetAttribute(SledSchema.SledProjectFilesType.assetdirectoryAttribute, assetDir);
            }
        }

        /// <summary>
        /// Get or set the project's GUID
        /// </summary>
        public Guid Guid
        {
            get
            {
                var szGuid = GetAttribute<string>(SledSchema.SledProjectFilesType.guidAttribute);
                var guid = string.IsNullOrEmpty(szGuid) ? Guid.NewGuid() : new Guid(szGuid);

                return guid;
            }

            set
            {
                var guid = value;

                if (guid == Guid.Empty)
                    guid = SledUtil.MakeXmlSafeGuid();

                SetAttribute(SledSchema.SledProjectFilesType.guidAttribute, guid.ToString());
            }
        }

        /// <summary>
        /// Gets the Languages sequence
        /// </summary>
        public IList<SledProjectFilesLanguageType> Languages
        {
            get { return GetChildList<SledProjectFilesLanguageType>(SledSchema.SledProjectFilesType.LanguagesChild); }
        }

        /// <summary>
        /// Gets the Watches sequence
        /// </summary>
        public IList<SledProjectFilesWatchType> Watches
        {
            get { return GetChildList<SledProjectFilesWatchType>(SledSchema.SledProjectFilesType.WatchesChild); }
        }

        /// <summary>
        /// Gets the Roots sequence
        /// </summary>
        public IList<SledProjectFilesRootType> Roots
        {
            get { return GetChildList<SledProjectFilesRootType>(SledSchema.SledProjectFilesType.RootsChild); }
        }

        /// <summary>
        /// Gets the UserSettings sequence
        /// </summary>
        public IList<SledProjectFilesUserSettingsType> UserSettings
        {
            get { return GetChildList<SledProjectFilesUserSettingsType>(SledSchema.SledProjectFilesType.UserSettingsChild); }
        }

        #endregion

        #region Non-Persisted Stuff

        /// <summary>
        /// Get or set dirty
        /// </summary>
        public bool Dirty { get; set; }

        /// <summary>
        /// Gets the directory that the project file lives in
        /// </summary>
        public string ProjectDirectory
        {
            get { return Path.GetDirectoryName(AbsolutePath); }
        }

        /// <summary>
        /// Gets the absolute path to the project file on disk
        /// </summary>
        public string AbsolutePath
        {
            get { return m_uri.LocalPath; }
        }

        /// <summary>
        /// Gets all files in the project (recursively gathers them all)
        /// </summary>
        public ICollection<SledProjectFilesFileType> AllFiles
        {
            get
            {
                var lstFiles = new List<SledProjectFilesFileType>();

                GatherFiles(this, ref lstFiles);

                return lstFiles;
            }
        }

        private static void GatherFiles(SledProjectFilesFolderType rootFolder, ref List<SledProjectFilesFileType> lstFiles)
        {
            // Add files from this folder
            lstFiles.AddRange(rootFolder.Files);

            // Search sub-folders
            foreach (var folder in rootFolder.Folders)
                GatherFiles(folder, ref lstFiles);
        }

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
        public new void GetInfo(object item, ItemInfo info)
        {
            info.Label = Name;
            info.IsLeaf = false;
            info.AllowLabelEdit = false;
            info.Description = "Project: " + Name;
            info.ImageIndex = info.GetImageIndex(Atf.Resources.FolderImage);

            // Set source control status
            {
                if (s_sourceControlService == null)
                    s_sourceControlService = SledServiceInstance.TryGet<ISledSourceControlService>();

                if (s_sourceControlService == null)
                    return;

                if (!s_sourceControlService.CanUseSourceControl)
                    return;

                var sourceControlStatus = s_sourceControlService.GetStatus(this);
                switch (sourceControlStatus)
                {
                    case SourceControlStatus.CheckedOut:
                        info.StateImageIndex = info.GetImageIndex(Atf.Resources.DocumentCheckOutImage);
                        break;

                    default:
                        info.StateImageIndex = Atf.Controls.TreeListView.InvalidImageIndex;
                        break;
                }
            }
        }

        #endregion

        #region IResource Interface

        /// <summary>
        /// Gets a string identifying the type of the resource to the end-user</summary>
        public string Type
        {
            get { return ToString(); }
        }

        /// <summary>
        /// Get or set the path to the project file on disk
        /// </summary>
        public Uri Uri
        {
            get { return m_uri; }
            set
            {
                var oldUri = m_uri;

                m_uri = value;

                if (oldUri != m_uri)
                    UriChanged.Raise(this, new UriChangedEventArgs(oldUri));
            }
        }

        /// <summary>
        /// Event that is raised after the resource's URI changes</summary>
        public event EventHandler<UriChangedEventArgs> UriChanged;

        #endregion

        private Uri m_uri;

        private static ISledSourceControlService s_sourceControlService;
    }

    /// <summary>
    /// Sled project files type equality comparer class
    /// </summary>
    public class SledProjectFilesTypeEqualityComparer : IEqualityComparer<SledProjectFilesType>
    {
        /// <summary>
        /// Check if two items are equal
        /// </summary>
        /// <param name="projFile1">Project file to be compared</param>
        /// <param name="projFile2">Project file to be compared</param>
        /// <returns>Less than zero: projFile1 is less than projFile2. Zero: projFile1 equals projFile2. Greater than zero: projFile1 is greater than projFile2.</returns>
        public bool Equals(SledProjectFilesType projFile1, SledProjectFilesType projFile2)
        {
            return
                ((projFile1.Guid == projFile2.Guid) &&
                (string.Compare(projFile1.Name, projFile2.Name, StringComparison.OrdinalIgnoreCase) == 0));
        }

        /// <summary>
        /// Get a hash code for an item
        /// </summary>
        /// <param name="projFile">Base object to get hash code from</param>
        /// <returns>Hash code of project file</returns>
        public int GetHashCode(SledProjectFilesType projFile)
        {
            return projFile.GetHashCode();
        }
    }

    /// <summary>
    /// Complex Type for an empty project
    /// </summary>
    public class SledProjectFilesEmptyType : DomNodeAdapter, IItemView
    {
        #region Implementation of IItemView

        /// <summary>
        /// Fills in or modifies the given display info for the item</summary>
        /// <param name="item">Item</param>
        /// <param name="info">Display info to update</param>
        public void GetInfo(object item, ItemInfo info)
        {
            info.AllowLabelEdit = false;
            info.IsLeaf = true;
            info.Label = Name;
        }

        #endregion

        /// <summary>
        /// Get or set the name
        /// </summary>
        public string Name
        {
            get { return GetAttribute<string>(SledSchema.SledProjectFilesEmptyType.nameAttribute); }
            set { SetAttribute(SledSchema.SledProjectFilesEmptyType.nameAttribute, value); }
        }
    }
}
