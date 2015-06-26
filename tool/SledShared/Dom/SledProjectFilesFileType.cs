/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;

using Sce.Atf;
using Sce.Atf.Adaptation;
using Sce.Atf.Applications;
using Sce.Atf.Dom;

using Sce.Sled.Shared.Controls;
using Sce.Sled.Shared.Document;
using Sce.Sled.Shared.Plugin;
using Sce.Sled.Shared.Services;
using Sce.Sled.Shared.Utilities;

namespace Sce.Sled.Shared.Dom
{
    /// <summary>
    /// Project file representation
    /// </summary>
    public class SledProjectFilesFileType : SledProjectFilesBaseType, ISledProjectFilesTreeViewable, IItemView, IResource
    {
        #region Persisted Stuff

        /// <summary>
        /// Get or set the name attribute
        /// </summary>
        public override string Name
        {
            get { return GetAttribute<string>(SledSchema.SledProjectFilesFileType.nameAttribute); }
            set { SetAttribute(SledSchema.SledProjectFilesFileType.nameAttribute, value); }
        }

        /// <summary>
        /// Get or set expanded attribute
        /// </summary>
        public override bool Expanded
        {
            get { return GetAttribute<bool>(SledSchema.SledProjectFilesFileType.expandedAttribute); }
            set { SetAttribute(SledSchema.SledProjectFilesFileType.expandedAttribute, value); }
        }

        /// <summary>
        /// Gets the breakpoints element
        /// </summary>
        public IList<SledProjectFilesBreakpointType> Breakpoints
        {
            get { return GetChildList<SledProjectFilesBreakpointType>(SledSchema.SledProjectFilesFileType.BreakpointsChild); }
        }

        /// <summary>
        /// Gets the functions element
        /// </summary>
        public IList<SledFunctionBaseType> Functions
        {
            get { return GetChildList<SledFunctionBaseType>(SledSchema.SledProjectFilesFileType.FunctionsChild); }
        }

        /// <summary>
        /// Gets the attributes element
        /// </summary>
        public IList<SledAttributeBaseType> Attributes
        {
            get { return GetChildList<SledAttributeBaseType>(SledSchema.SledProjectFilesFileType.AttributesChild); }
        }

        /// <summary>
        /// Get or set the path attribute
        /// </summary>
        public string Path
        {
            get
            {
                // Chop off ".\" from relative paths
                var szPath = GetAttribute<string>(SledSchema.SledProjectFilesFileType.pathAttribute);
                if (szPath == null)
                    throw new NullReferenceException("File path is null!");

                if ((szPath.Length > 2) && (szPath[0] == '.') && (szPath[1] == System.IO.Path.DirectorySeparatorChar))
                    szPath = szPath.Substring(2, szPath.Length - 2);

                return szPath;
            }
            set
            {
                // Chop off ".\" from relative paths
                if ((value.Length > 2) && (value[0] == '.') && (value[1] == System.IO.Path.DirectorySeparatorChar))
                    value = value.Substring(2, value.Length - 2);

                SetAttribute(SledSchema.SledProjectFilesFileType.pathAttribute, value);
            }
        }

        /// <summary>
        /// Get or set GUID
        /// </summary>
        public Guid Guid
        {
            get
            {
                var szGuid = GetAttribute<string>(SledSchema.SledProjectFilesFileType.guidAttribute);
                
                var guid = 
                    string.IsNullOrEmpty(szGuid)
                        ? SledUtil.MakeXmlSafeGuid()
                        : new Guid(szGuid);

                return guid;
            }
            set { SetAttribute(SledSchema.SledProjectFilesFileType.guidAttribute, value.ToString()); }
        }

        /// <summary>
        /// Performs one-time initialization when this adapter's DomNode property is set.
        /// The DomNode property is only ever set once for the lifetime of this adapter.</summary>
        protected override void OnNodeSet()
        {
            if (DomNode.IsAttributeDefault(SledSchema.SledProjectFilesFileType.guidAttribute))
                Guid = SledUtil.MakeXmlSafeGuid();

            base.OnNodeSet();
        }

        #endregion

        #region Non-persisted Stuff

        /// <summary>
        /// Create SledProjectFilesFileType
        /// </summary>
        /// <param name="szAbsPath">Absolute path of file</param>
        /// <param name="project">Project details used to aid in creating the project file representation</param>
        /// <returns>SledProjectFilesFileType</returns>
        public static SledProjectFilesFileType Create(string szAbsPath, SledProjectFilesType project)
        {
            var node = new DomNode(SledSchema.SledProjectFilesFileType.Type);
            
            var file = node.As<SledProjectFilesFileType>();

            file.Uri = new Uri(szAbsPath);
            file.Name = System.IO.Path.GetFileName(szAbsPath);
            file.Path = SledUtil.GetRelativePath(szAbsPath, project.AssetDirectory);

            return file;
        }

        /// <summary>
        /// Get or set reference to an open SledDocument
        /// </summary>
        public ISledDocument SledDocument { get; set; }

        /// <summary>
        /// Get or set the language plugin that is responsible for this file
        /// </summary>
        public ISledLanguagePlugin LanguagePlugin { get; set; }

        /// <summary>
        /// Get absolute path to the file
        /// </summary>
        public string AbsolutePath
        {
            get { return m_uri.LocalPath; }
        }

        /// <summary>
        /// Gets the project the file is in
        /// </summary>
        public SledProjectFilesType Project
        {
            get { return DomNode.GetRoot().As<SledProjectFilesType>(); }
        }

        /// <summary>
        /// Gets the SledDocumentClient for the file
        /// </summary>
        public ISledDocumentClient DocumentClient
        {
            get
            {
                if (m_documentClient != null)
                    return m_documentClient;

                if (s_documentService == null)
                {
                    s_documentService = SledServiceInstance.Get<ISledDocumentService>();
                }

                m_documentClient = s_documentService.GetDocumentClient(Uri);

                return m_documentClient;
            }
        }

        #endregion

        #region ISledProjectFilesTreeViewable Interface

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
            get { return Functions.AsIEnumerable<ISledProjectFilesTreeViewable>(); }
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
            info.IsLeaf = (Functions.Count == 0);
            info.AllowLabelEdit = true;
            info.Description = Name;
            info.Properties = new[] { Path };

            var bFileExists = File.Exists(Uri.LocalPath);
            var bReadOnly = SledUtil.IsFileReadOnly(Uri.LocalPath);

            // Mark files that don't exist
            if (!bFileExists)
            {
                info.FontStyle = FontStyle.Strikeout;
                info.AllowLabelEdit = false;
            }

            // Don't allow renaming of read-only files
            if (bReadOnly)
                info.AllowLabelEdit = false;

            // Default
            var imageName =
                Atf.Resources.DocumentImage;

            // Try to grab other
            if ((DocumentClient != null) &&
                (DocumentClient.Info != null) &&
                !string.IsNullOrEmpty(DocumentClient.Info.OpenIconName))
            {
                imageName = DocumentClient.Info.OpenIconName;
            }

            // Set image
            info.ImageIndex = info.GetImageIndex(imageName);

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
        /// Get or set the resource URI. It should be an absolute path. (Uri.IsAbsoluteUri should be true.)</summary>
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
        private ISledDocumentClient m_documentClient;

        private static ISledDocumentService s_documentService;
        private static ISledSourceControlService s_sourceControlService;

        /// <summary>
        /// Generate hash code
        /// </summary>
        /// <returns>Hash code of the absolute path of the file</returns>
        public override int GetHashCode()
        {
            var hash = Guid.GetHashCode();
            return hash;
        }

        /// <summary>
        /// Compare SledProjectFilesFileType with item
        /// </summary>
        /// <param name="obj">Item to compare against</param>
        /// <returns>True iff equal to the item compared against</returns>
        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;

            if (ReferenceEquals(this, obj))
                return true;

            if (obj is SledProjectFilesFileType)
            {
                var s = obj as SledProjectFilesFileType;
                return Guid == s.Guid;
            }

            if (obj is DomNode)
            {
                var node = obj as DomNode;
                if (node.Is<SledProjectFilesFileType>())
                {
                    var projFile = node.As<SledProjectFilesFileType>();
                    return Guid == projFile.Guid;
                }
            }

            return false;
        }

        /// <summary>
        /// Synchronize two files
        /// </summary>
        /// <param name="domNodeProjTree">First file</param>
        /// <param name="domNodeTempTree">Second file</param>
        public static void SyncFiles(DomNode domNodeProjTree, DomNode domNodeTempTree)
        {
            var parent = domNodeProjTree.Parent;
            domNodeProjTree.RemoveFromParent();

            var clone = DomNode.Copy(new[] { domNodeTempTree });
            clone[0].InitializeExtensions();
            
            var info = domNodeTempTree.ChildInfo;
            if (info.IsList)
            {
                parent.GetChildList(info).Add(clone[0]);
            }
            else
            {
                parent.SetChild(info, clone[0]);
            }
        }

        /// <summary>
        /// Comparer that can be used to compare two SledProjectFilesFileType items
        /// </summary>
        public class Comparer : IComparer<SledProjectFilesFileType>
        {
            /// <summary>
            /// Compare two items
            /// </summary>
            /// <remarks>Default comparison method compares the absolute path of each item</remarks>
            /// <param name="file1">Item to compare</param>
            /// <param name="file2">Item to compare</param>
            /// <returns>Less than zero: file1 is less than file2. Zero: file1 equals file2. Greater than zero: file1 is greater than file2.</returns>
            public int Compare(SledProjectFilesFileType file1, SledProjectFilesFileType file2)
            {
                if ((file1 == null) && (file2 == null))
                    return 0;

                if (file1 == null)
                    return 1;

                if (file2 == null)
                    return -1;

                if (ReferenceEquals(file1, file2))
                    return 0;

                return
                    TheComparer == null
                        ? string.Compare(file1.AbsolutePath, file2.AbsolutePath, StringComparison.Ordinal)
                        : TheComparer(file1, file2);
            }

            /// <summary>
            /// Comparison delegate to let users easily plug in how the items are compared
            /// </summary>
            /// <param name="file1">Item to compare</param>
            /// <param name="file2">Item to compare</param>
            /// <returns>Less than zero: file1 is less than file2. Zero: file1 equals file2. Greater than zero: file1 is greater than file2.</returns>
            public delegate int CompareDelegate(SledProjectFilesFileType file1, SledProjectFilesFileType file2);

            /// <summary>
            /// Get or set the user comparison function to use
            /// </summary>
            public CompareDelegate TheComparer { get; set; }
        }

        /// <summary>
        /// Project file copier class
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
                var dictProjFiles = new Dictionary<Guid, SledProjectFilesFileType>();
                GatherNodeTypes(domProjectTree, dictProjFiles);

                var dictTempFiles = new Dictionary<Guid, SledProjectFilesFileType>();
                GatherNodeTypes(domTempTree, dictTempFiles);

                var lstPairs = new List<Pair<DomNode, DomNode>>();
                foreach (var kv in dictProjFiles)
                {
                    SledProjectFilesFileType tempFile;
                    if (dictTempFiles.TryGetValue(kv.Key, out tempFile))
                        lstPairs.Add(new Pair<DomNode, DomNode>(kv.Value.DomNode, tempFile.DomNode));
                }

                if (lstPairs.Count <= 0)
                    return;

                foreach (var pair in lstPairs)
                {
                    SyncFiles(pair.First, pair.Second);
                }
            }

            private static void GatherNodeTypes(DomNode tree, IDictionary<Guid, SledProjectFilesFileType> dictNodes)
            {
                if (tree.Is<SledProjectFilesFileType>())
                {
                    var projFile = tree.As<SledProjectFilesFileType>();
                    if (!dictNodes.ContainsKey(projFile.Guid))
                        dictNodes.Add(projFile.Guid, projFile);
                }

                foreach (var child in tree.Children)
                {
                    GatherNodeTypes(child, dictNodes);
                }
            }
        }
    }
}
