/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Schema;

using Sce.Atf;
using Sce.Atf.Adaptation;
using Sce.Atf.Dom;

using Sce.Sled.Shared.Resources;
using Sce.Sled.Shared.Utilities;

namespace Sce.Sled.Shared.Dom
{
    /// <summary>
    /// SLED project reader class
    /// </summary>
    public sealed class SledSpfReader
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="typeLoader">Schema loader</param>
        public SledSpfReader(XmlSchemaTypeLoader typeLoader)
        {
            m_typeLoader = typeLoader;
        }

        /// <summary>
        /// Read a node tree from a stream</summary>
        /// <param name="uri">URI of stream</param>
        /// <param name="bReadTempSettings">Whether to additionally read from the project temporary settings file</param>
        /// <returns>Node tree, created from stream</returns>
        public DomNode Read(Uri uri, bool bReadTempSettings)
        {
            DomNode rootProject = null;

            try
            {
                const int attempts = 5;
                const int waitMs = 1000;

                // Read project settings file
                using (var stream = SledUtil.TryOpen(
                            uri.LocalPath,
                            FileMode.Open,
                            FileAccess.Read,
                            FileShare.Read,
                            attempts,
                            waitMs))
                {
                    rootProject = ReadInternal(stream);
                }
            }
            catch (Exception ex)
            {
                SledOutDevice.OutLine(
                    SledMessageType.Error,
                    "Encountered an error when " +
                    "loading project file! {0}",
                    ex.Message);
            }

            if (rootProject == null)
                return null;

            // Should we read the project
            // temporary settings file?
            if (!bReadTempSettings)
                return rootProject;

            // Read from project temporary settings file
            DomNode rootTemp = null;

            try
            {
                // Path to project file
                var szAbsProjPath = uri.LocalPath;
                // Path to hidden project temporary settings file
                var szAbsTempPath = Path.ChangeExtension(szAbsProjPath, ".sus");

                if (!File.Exists(szAbsTempPath))
                    return rootProject;

                // Read project temporary settings file
                using (var stream = new FileStream(szAbsTempPath, FileMode.Open, FileAccess.Read))
                {
                    rootTemp = ReadInternal(stream);
                }
            }
            catch (Exception ex)
            {
                SledOutDevice.OutLine(
                    SledMessageType.Error,
                    "Encountered an error when loading " +
                    "project temporary settings file! {0}",
                    ex.Message);
            }

            // If null then fall back to project file
            if (rootTemp == null)
                return rootProject;

            foreach (var copier in s_lstCopiers)
            {
                try
                {
                    copier.Run(rootProject, rootTemp);
                }
                catch (Exception ex)
                {
                    SledOutDevice.OutLine(
                        SledMessageType.Error,
                        "Encountered an error when syncing " + 
                        "user settings file to project file! {0}",
                        ex.Message);
                }
            }

            return rootProject;
        }

        private DomNode ReadInternal(Stream stream)
        {
            m_root = null;
            m_nodeDictionary.Clear();
            m_nodeReferences.Clear();

            var settings =
                new XmlReaderSettings {IgnoreComments = true, IgnoreProcessingInstructions = true};
            //settings.IgnoreWhitespace = true;

            using (var reader = XmlReader.Create(stream, settings))
            {
                reader.MoveToContent();

                var rootElement = CreateRootElement(reader);
                if (rootElement == null)
                    throw new InvalidOperationException("Unknown root element");

                m_root = ReadElement(rootElement, reader);

                ResolveReferences();
            }

            return m_root;
        }

        private ChildInfo CreateRootElement(XmlReader reader)
        {
            var ns = reader.NamespaceURI;
            if (string.IsNullOrEmpty(ns))
            {
                // no xmlns declaration in the file, so grab the first type collection's target namespace
                foreach (var typeCollection in m_typeLoader.GetTypeCollections())
                {
                    ns = typeCollection.DefaultNamespace;
                    break;
                }
            }

            var rootElement = m_typeLoader.GetRootElement(ns + ":" + reader.LocalName);
            return rootElement;
        }

        private DomNode ReadElement(ChildInfo nodeInfo, XmlReader reader)
        {
            // handle polymorphism, if necessary
            var type = GetChildType(nodeInfo.Type, reader);
            var index = type.Name.LastIndexOf(':');
            var typeNs = type.Name.Substring(0, index);

            var node = new DomNode(type, nodeInfo);

            // read attributes
            while (reader.MoveToNextAttribute())
            {
                if (reader.Prefix == string.Empty ||
                    reader.LookupNamespace(reader.Prefix) == typeNs)
                {
                    var attributeInfo = type.GetAttributeInfo(reader.LocalName);
                    if (attributeInfo != null)
                    {
                        var valueString = reader.Value;
                        if (attributeInfo.Type.Type == AttributeTypes.Reference)
                        {
                            // save reference so it can be resolved after all nodes have been read
                            m_nodeReferences.Add(new XmlNodeReference(node, attributeInfo, valueString));
                        }
                        else
                        {
                            var value = attributeInfo.Type.Convert(valueString);
                            node.SetAttribute(attributeInfo, value);
                        }
                    }
                }
            }

            // add node to map if it has an id
            if (node.Type.IdAttribute != null)
            {
                var id = node.GetId();
                if (!string.IsNullOrEmpty(id))
                    m_nodeDictionary[id] = node; // don't Add, in case there are multiple DomNodes with the same id
            }

            reader.MoveToElement();

            if (!reader.IsEmptyElement)
            {
                // read child elements
                while (reader.Read())
                {
                    if (reader.NodeType == XmlNodeType.Element)
                    {
                        // look up metadata for this element
                        var childInfo = type.GetChildInfo(reader.LocalName);
                        if (childInfo != null)
                        {
                            var childNode = ReadElement(childInfo, reader);
                            if (childNode != null)
                            {
                                // childNode is fully populated sub-tree
                                if (childInfo.IsList)
                                {
                                    node.GetChildList(childInfo).Add(childNode);
                                }
                                else
                                {
                                    node.SetChild(childInfo, childNode);
                                }
                            }
                        }
                        else
                        {
                            // try reading as an attribute
                            var attributeInfo = type.GetAttributeInfo(reader.LocalName);
                            if (attributeInfo != null)
                            {
                                reader.MoveToElement();

                                if (!reader.IsEmptyElement)
                                {
                                    // read element text
                                    while (reader.Read())
                                    {
                                        if (reader.NodeType == XmlNodeType.Text)
                                        {
                                            var value = attributeInfo.Type.Convert(reader.Value);
                                            node.SetAttribute(attributeInfo, value);
                                            // skip child elements, as this is an attribute value
                                            reader.Skip();
                                            break;
                                        }
                                        if (reader.NodeType == XmlNodeType.EndElement)
                                        {
                                            break;
                                        }
                                    }

                                    reader.MoveToContent();
                                }
                            }
                            else
                            {
                                // skip unrecognized element
                                reader.Skip();
                                // if that takes us to the end of the enclosing element, break
                                if (reader.NodeType == XmlNodeType.EndElement)
                                    break;
                            }
                        }
                    }
                    else if (reader.NodeType == XmlNodeType.Text)
                    {
                        var attributeInfo = type.GetAttributeInfo(string.Empty);
                        if (attributeInfo != null)
                        {
                            var value = attributeInfo.Type.Convert(reader.Value);
                            node.SetAttribute(attributeInfo, value);
                        }
                    }
                    else if (reader.NodeType == XmlNodeType.EndElement)
                    {
                        break;
                    }
                }
            }

            reader.MoveToContent();

            return node;
        }

        private DomNodeType GetDerivedType(string ns, string typeName)
        {
            return m_typeLoader.GetNodeType(ns + ":" + typeName);
        }

        private void ResolveReferences()
        {
            var unresolved = new List<XmlNodeReference>();
            foreach (var nodeReference in m_nodeReferences)
            {
                DomNode refNode;
                if (m_nodeDictionary.TryGetValue(nodeReference.Value, out refNode))
                {
                    nodeReference.Node.SetAttribute(nodeReference.AttributeInfo, refNode);
                }
                else
                {
                    unresolved.Add(nodeReference);
                }
            }

            m_nodeReferences = unresolved;
        }

        private DomNodeType GetChildType(DomNodeType type, XmlReader reader)
        {
            var result = type;

            // check for xsi:type attribute, for polymorphic elements
            var typeName = reader.GetAttribute("xsi:type");
            if (typeName != null)
            {
                // check for qualified type name
                var prefix = string.Empty;
                var index = typeName.IndexOf(':');
                if (index >= 0)
                {
                    prefix = typeName.Substring(0, index);
                    index++;
                    typeName = typeName.Substring(index, typeName.Length - index);
                }
                var ns = reader.LookupNamespace(prefix);

                result = GetDerivedType(ns, typeName);

                if (result == null)
                    throw new InvalidOperationException("Unknown derived type");
            }

            return result;
        }

        #region Spf Copier Stuff

        /// <summary>
        /// ICopier Interface
        /// </summary>
        public interface ICopier
        {
            /// <summary>
            /// Run the copier
            /// </summary>
            /// <param name="domProjectTree">Tree to copy</param>
            /// <param name="domTempTree">Copied tree</param>
            void Run(DomNode domProjectTree, DomNode domTempTree);
        }

        /// <summary> 
        /// Copier base class
        /// </summary>
        public abstract class CopierBase : ICopier
        {
            /// <summary>
            /// Gather all DomNodes of a specific DomNodeType in a tree
            /// </summary>
            /// <param name="tree">DomNode tree whose DomNodes are gathered</param>
            /// <param name="nodeType">DomNodeType of DomNodes to gather</param>
            /// <param name="lstNodes">Collection of gathered DomNodes</param>
            protected static void GatherNodeTypes(DomNode tree, DomNodeType nodeType, ICollection<DomNode> lstNodes)
            {
                if (tree.Type == nodeType)
                    lstNodes.Add(tree);

                foreach (var child in tree.Children)
                {
                    GatherNodeTypes(child, nodeType, lstNodes);
                }
            }

            /// <summary>
            /// Run the copier
            /// </summary>
            /// <param name="domProjectTree">Tree to copy</param>
            /// <param name="domTempTree">Copied tree</param>
            public abstract void Run(DomNode domProjectTree, DomNode domTempTree);
        }

        /// <summary>
        /// DomNodeType synchronization class
        /// </summary>
        public class DomNodeTypeSync : CopierBase
        {
            /// <summary>
            /// Sync function delegate
            /// </summary>
            /// <param name="domNodeProjTree">Tree to synchronize with</param>
            /// <param name="domNodeTempTree">Synchronized tree</param>
            public delegate void SyncFunc(DomNode domNodeProjTree, DomNode domNodeTempTree);

            /// <summary>
            /// Sync up elements in the project files tree with their corresponding
            /// elements in the project temporary settings file tree
            /// </summary>
            /// <param name="nodeType">Type of elements to synchronize</param>
            /// <param name="comparer">Comparer function</param>
            /// <param name="syncFunc">Synchronizer function</param>
            public DomNodeTypeSync(DomNodeType nodeType, IEqualityComparer<DomNode> comparer, SyncFunc syncFunc)
            {
                m_nodeType = nodeType;
                m_comparer = comparer;
                m_syncFunc = syncFunc;
            }

            /// <summary>
            /// Run the copier
            /// </summary>
            /// <param name="domProjectTree">Tree to copy</param>
            /// <param name="domTempTree">Copied tree</param>
            public override void Run(DomNode domProjectTree, DomNode domTempTree)
            {
                var lstProjNodes = new List<DomNode>();
                GatherNodeTypes(domProjectTree, m_nodeType, lstProjNodes);

                var lstTempNodes = new List<DomNode>();
                GatherNodeTypes(domTempTree, m_nodeType, lstTempNodes);

                var lstPairs =
                    (from projNode in lstProjNodes
                     from tempNode in lstTempNodes
                     where m_comparer.Equals(projNode, tempNode)
                     select new Pair<DomNode, DomNode>(projNode, tempNode)).ToList();

                if (lstPairs.Count <= 0)
                    return;

                foreach (var pair in lstPairs)
                {
                    m_syncFunc(pair.First, pair.Second);
                }
            }

            private readonly DomNodeType m_nodeType;
            private readonly IEqualityComparer<DomNode> m_comparer;
            private readonly SyncFunc m_syncFunc;
        }

        /// <summary>
        /// DomNodeType copier class
        /// </summary>
        public class DomNodeTypeRootCopier : CopierBase
        {
            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="nodeType">DomNodeType to copy</param>
            public DomNodeTypeRootCopier(DomNodeType nodeType)
            {
                m_nodeType = nodeType;
            }

            /// <summary>
            /// Run the copier
            /// </summary>
            /// <param name="domProjectTree">Tree to copy</param>
            /// <param name="domTempTree">Copied tree</param>
            public override void Run(DomNode domProjectTree, DomNode domTempTree)
            {
                var lstToCopy = new List<DomNode>();
                GatherNodeTypes(domTempTree, m_nodeType, lstToCopy);

                foreach (var domNode in lstToCopy)
                {
                    var info = domNode.ChildInfo;
                    if (info.IsList)
                    {
                        domProjectTree.GetChildList(info).Add(domNode);
                    }
                    else
                    {
                        domProjectTree.SetChild(info, domNode);
                    }
                }
            }

            private readonly DomNodeType m_nodeType;
        }

        #endregion

        /// <summary>
        /// Register a copier with the Spf reader
        /// </summary>
        /// <param name="copier">ICopier object</param>
        public static void RegisterCopier(ICopier copier)
        {
            s_lstCopiers.Add(copier);
        }

        private DomNode m_root;

        private List<XmlNodeReference> m_nodeReferences =
            new List<XmlNodeReference>();

        private readonly XmlSchemaTypeLoader m_typeLoader;

        private readonly Dictionary<string, DomNode> m_nodeDictionary =
            new Dictionary<string, DomNode>();

        private static readonly List<ICopier> s_lstCopiers =
            new List<ICopier>();
    }

    /// <summary>
    /// SLED project writer class
    /// </summary>
    public sealed class SledSpfWriter
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="typeCollection">Schema type collection</param>
        public SledSpfWriter(XmlSchemaTypeCollection typeCollection)
        {
            m_typeCollection = typeCollection;
        }

        /// <summary>
        /// Gets Schema type collection
        /// </summary>
        public XmlSchemaTypeCollection TypeCollection
        {
            get { return m_typeCollection; }
        }

        /// <summary>
        /// Write collection to disk
        /// </summary>
        /// <param name="root">Root DomNode</param>
        /// <param name="uri">URI for project settings file</param>
        /// <param name="bWriteTempSettings">Whether to write project temporary settings file (.sus)</param>
        public void Write(DomNode root, Uri uri, bool bWriteTempSettings)
        {
            try
            {
                // Write project settings
                using (var file = new FileStream(uri.LocalPath, FileMode.Create, FileAccess.Write))
                {
                    WriteInternal(root, file);
                }
            }
            catch (Exception ex)
            {
                SledOutDevice.OutLine(
                    SledMessageType.Error,
                    "Encountered an error when " + 
                    "saving project file! {0}",
                    ex.Message);
            }

            if (!bWriteTempSettings)
                return;

            try
            {
                m_bWritingUserSettings = true;

                // Path to project file
                var szAbsProjPath = uri.LocalPath;
                // Path to hidden project temporary settings file
                var szAbsTempPath = Path.ChangeExtension(szAbsProjPath, ".sus");

                // Make it writable if it exists already
                if (File.Exists(szAbsTempPath))
                    File.SetAttributes(szAbsTempPath, FileAttributes.Normal);

                // Write project temporary settings file
                using (var file = new FileStream(szAbsTempPath, FileMode.Create, FileAccess.Write))
                {
                    WriteInternal(root, file);
                }

                // Make it hidden
                if (File.Exists(szAbsTempPath))
                    File.SetAttributes(szAbsTempPath, FileAttributes.Hidden);
            }
            catch (Exception ex)
            {
                SledOutDevice.OutLine(
                    SledMessageType.Error,
                    SledUtil.TransSub(Localization.SledSharedSaveTempSettingsFileError, ex.Message));
            }
            finally
            {
                m_bWritingUserSettings = false;
            }
        }

        private void WriteInternal(DomNode root, Stream stream)
        {
            try
            {
                m_root = root;
                m_inlinePrefixes.Clear();

                var settings =
                    new XmlWriterSettings
                        {
                            Indent = true,
                            IndentChars = "\t",
                            NewLineHandling = NewLineHandling.Replace,
                            NewLineChars = "\r\n"
                        };

                using (var writer = XmlWriter.Create(stream, settings))
                {
                    writer.WriteStartDocument();
                    WriteElement(root, writer);
                    writer.WriteEndDocument();
                }
            }
            finally
            {
                m_root = null;
                m_inlinePrefixes.Clear();
            }
        }

        private void WriteElement(DomNode node, XmlWriter writer)
        {
            // If writing the project settings file...
            if (!m_bWritingUserSettings)
            {
                // Don't save types that are not supposed to be saved
                if (s_lstExcludeDomNodeTypes.Contains(node.Type))
                    return;
            }

            var elementNs = m_typeCollection.TargetNamespace;
            var index = node.ChildInfo.Name.LastIndexOf(':');
            if (index >= 0)
                elementNs = node.ChildInfo.Name.Substring(0, index);

            string elementPrefix;

            // is this the root DomNode (the one passed Write, above)?
            if (node == m_root)
            {
                elementPrefix = m_typeCollection.GetPrefix(elementNs) ?? GeneratePrefix(elementNs);

                writer.WriteStartElement(elementPrefix, node.ChildInfo.Name, elementNs);

                // define the xsi namespace
                writer.WriteAttributeString("xmlns", "xsi", null, XmlSchema.InstanceNamespace);

                // define schema namespaces
                foreach (var name in m_typeCollection.Namespaces)
                {
                    if (string.Compare(name.Name, elementPrefix) != 0)
                        writer.WriteAttributeString("xmlns", name.Name, null, name.Namespace);
                }
            }
            else
            {
                // not the root, so all schema namespaces have been defined
                elementPrefix = writer.LookupPrefix(elementNs) ?? GeneratePrefix(elementNs);

                writer.WriteStartElement(elementPrefix, node.ChildInfo.Name, elementNs);
            }

            // write type name if this is a polymorphic type
            var type = node.Type;
            if (node.ChildInfo.Type != type)
            {
                var name = type.Name;
                index = name.LastIndexOf(':');
                if (index >= 0)
                {
                    var typeName = name.Substring(index + 1, type.Name.Length - index - 1);
                    var typeNs = name.Substring(0, index);
                    var typePrefix = writer.LookupPrefix(typeNs);
                    if (typePrefix == null)
                    {
                        typePrefix = GeneratePrefix(typeNs);
                        writer.WriteAttributeString("xmlns", typePrefix, null, typeNs);
                    }

                    name = typeName;
                    if (typePrefix != string.Empty)
                        name = typePrefix + ":" + typeName;
                }

                writer.WriteAttributeString("xsi", "type", XmlSchema.InstanceNamespace, name);
            }

            // write attributes
            AttributeInfo valueAttribute = null;
            foreach (var attributeInfo in type.Attributes)
            {
                // if attribute is required, or not the default, write it
                if (/*attributeInfo.Required ||*/ !node.IsAttributeDefault(attributeInfo))
                {
                    if (attributeInfo.Name == string.Empty)
                    {
                        valueAttribute = attributeInfo;
                    }
                    else
                    {
                        var value = node.GetAttribute(attributeInfo);
                        string valueString = null;
                        if (attributeInfo.Type.Type == AttributeTypes.Reference)
                        {
                            // if reference is a valid node, convert to string
                            var refNode = value as DomNode;
                            if (refNode != null)
                                valueString = GetNodeReferenceString(refNode, m_root);
                        }
                        if (valueString == null)
                            valueString = attributeInfo.Type.Convert(value);

                        var bWriteAttribute = true;
                        if (!m_bWritingUserSettings)
                            bWriteAttribute = !s_lstExcludeAttributes.Contains(attributeInfo.Name);

                        if (bWriteAttribute)
                            writer.WriteAttributeString(attributeInfo.Name, valueString);
                    }
                }
            }

            // write value if not the default
            if (valueAttribute != null)
            {
                var value = node.GetAttribute(valueAttribute);
                writer.WriteString(valueAttribute.Type.Convert(value));
            }

            // write child elements
            foreach (var childInfo in type.Children)
            {
                if (childInfo.IsList)
                {
                    foreach (var child in node.GetChildList(childInfo))
                        WriteElement(child, writer);
                }
                else
                {
                    var child = node.GetChild(childInfo);
                    if (child != null)
                        WriteElement(child, writer);
                }
            }

            writer.WriteEndElement();
        }

        private static string GetNodeReferenceString(DomNode refNode, DomNode root)
        {
            var id = refNode.GetId();

            // if referenced node is in another resource, prepend URI
            if (!refNode.IsDescendantOf(root))
            {
                var nodeRoot = refNode.GetRoot();
                var resource = nodeRoot.As<IResource>();
                if (resource != null)
                    id = resource.Uri.LocalPath + "#" + id;
            }

            return id;
        }

        private string GeneratePrefix(string ns)
        {
            string prefix = null;
            if (!string.IsNullOrEmpty(ns))
            {
                if (!m_inlinePrefixes.TryGetValue(ns, out prefix))
                {
                    var suffix = m_inlinePrefixes.Count;
                    prefix = "_p" + suffix;
                    m_inlinePrefixes.Add(ns, prefix);
                }
            }

            return prefix;
        }

        /// <summary>
        /// Exclude an attribute from being saved to the project settings file
        /// </summary>
        /// <param name="attribute">Name of attribute</param>
        public static void ExcludeAttribute(string attribute)
        {
            if (!s_lstExcludeAttributes.Contains(attribute))
                s_lstExcludeAttributes.Add(attribute);
        }

        /// <summary>
        /// Exclude a DomNodeType from being saved to the project settings file
        /// </summary>
        /// <param name="nodeType">DomNodeType to exclude</param>
        public static void ExcludeDomNodeType(DomNodeType nodeType)
        {
            if (!s_lstExcludeDomNodeTypes.Contains(nodeType))
                s_lstExcludeDomNodeTypes.Add(nodeType);
        }

        private DomNode m_root;
        private bool m_bWritingUserSettings;

        private readonly XmlSchemaTypeCollection m_typeCollection;
        
        private readonly Dictionary<string, string> m_inlinePrefixes =
            new Dictionary<string, string>();

        private static readonly List<string> s_lstExcludeAttributes =
            new List<string>();

        private static readonly List<DomNodeType> s_lstExcludeDomNodeTypes =
            new List<DomNodeType>();
    }

    ///// <summary>
    ///// SledSpfComparers Class
    ///// </summary>
    //public static class SledSpfComparers
    //{
    //    /// <summary>
    //    /// Add a comparer to be used when comparing the project settings file to
    //    /// the project temporary settings file
    //    /// </summary>
    //    /// <param name="nodeType">DomNodeType</param>
    //    /// <param name="comparer">Compare to use on matching DomNodeTypes</param>
    //    public static void RegisterComparer(DomNodeType nodeType, IEqualityComparer<DomNode> comparer)
    //    {
    //        if (s_dictComparers.ContainsKey(nodeType))
    //            return;

    //        s_dictComparers.Add(nodeType, comparer);
    //    }

    //    /// <summary>
    //    /// Comparers
    //    /// </summary>
    //    public static Dictionary<DomNodeType, IEqualityComparer<DomNode>> Comparers
    //    {
    //        get { return s_dictComparers; }
    //    }

    //    private static readonly Dictionary<DomNodeType, IEqualityComparer<DomNode>> s_dictComparers =
    //        new Dictionary<DomNodeType, IEqualityComparer<DomNode>>();
    //}
}