/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Schema;

using Scea.Dom;

using Sce.Sled.Shared.Resources;
using Sce.Sled.Shared.Utilities;


namespace Sce.Sled.Shared.Dom.Persisters
{
    /// <summary>
    /// A custom persister for saving SLED project files
    /// </summary>
    public class SledDomProjectFilesPersister : DomFilePersister
    {
        private const string Period = ".";
        private const string IndentChars = "\t";
        private const string NewLineChars = "\r\n";
        private const string DontSaveAnnotation = "sce.sled.shared.dom.attribute.dontsave";

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="extMain">Extension to use for the project file</param>
        /// <param name="extTemp">Extension to use for the temporary settings file</param>
        public SledDomProjectFilesPersister(string extMain, string extTemp)
            : base(extMain.Replace(Period, string.Empty))
        {
            m_extTmp = extTemp;

            if (!s_instances.Contains(this))
                s_instances.Add(this);
        }

        /// <summary>
        /// Register a copier on the persister
        /// </summary>
        /// <param name="copier">Copier</param>
        public void RegisterCopier(ITempSettingsCopier copier)
        {
            if (!m_lstCopiers.Contains(copier))
                m_lstCopiers.Add(copier);
        }

        /// <summary>
        /// Get instances of SledDomProjectFilesPersister
        /// </summary>
        public static IList<SledDomProjectFilesPersister> Instances
        {
            get { return s_instances; }
        }

        #region DomPersister Overrides

        /// <summary>
        /// Reads a DomObject from the given stream
        /// </summary>
        /// <param name="stream">Stream to read</param>
        /// <returns>Dom tree that was read</returns>
        public override DomObject Read(Stream stream)
        {
            // Read project settings
            DomObject rootProjDomObject = ReadInternal(stream);

            // Try and read temporary settings file
            DomObject rootTempDomObject = null;
            try
            {
                // Path to project file
                string szAbsProjPath = ((FileStream)stream).Name;
                // Path to hidden project temporary settings file
                string szAbsTempPath = Path.ChangeExtension(szAbsProjPath, m_extTmp);

                // Read from disk if file exists
                if (File.Exists(szAbsTempPath))
                {
                    using (FileStream file = new FileStream(szAbsTempPath, FileMode.Open, FileAccess.Read))
                    {
                        rootTempDomObject = ReadInternal(file);
                    }
                }
            }
            catch (Exception ex)
            {
                SledOutDevice.OutLine(SledMessageType.Error, SledUtil.TransSub(Localization.SledSharedLoadTempSettingsFileError, new string[] { ex.Message }));
            }

            // Combine project file with temporary settings file
            Combine(rootProjDomObject, rootTempDomObject);

            return rootProjDomObject;
        }

        /// <summary>
        /// Writes the DomObject to the given stream
        /// </summary>
        /// <param name="stream">Stream to write to</param>
        /// <param name="rootDomObject">Dom tree to write</param>
        public override void Write(Stream stream, DomObject rootDomObject)
        {
            // Write project settings
            WriteInternal(stream, rootDomObject, false);

            // Create stream to write the temporary settings file or if we can't then do nothing
            try
            {
                // Path to project file
                string szAbsProjPath = ((FileStream)stream).Name;
                // Path to hidden project temporary settings file
                string szAbsTempPath = Path.ChangeExtension(szAbsProjPath, m_extTmp);

                // Make it writeable if it already exists
                if (File.Exists(szAbsTempPath))
                    File.SetAttributes(szAbsTempPath, FileAttributes.Normal);

                // Write to disk
                using (FileStream file = new FileStream(szAbsTempPath, FileMode.Create, FileAccess.Write))
                {
                    WriteInternal(file, rootDomObject, true);
                }

                // Make hidden
                if (File.Exists(szAbsTempPath))
                    File.SetAttributes(szAbsTempPath, FileAttributes.Hidden);
            }
            catch (Exception ex)
            {
                SledOutDevice.OutLine(SledMessageType.Error, SledUtil.TransSub(Localization.SledSharedSaveTempSettingsFileError, new string[] { ex.Message }));
            }
        }

        #endregion

        private DomObject ReadInternal(Stream stream)
        {
            DomObject rootDomObject;

            using (XmlReader reader = XmlReader.Create(stream))
            {
                reader.MoveToContent();

                string ns = reader.NamespaceURI;
                XmlQualifiedName rootElementName = new XmlQualifiedName(reader.LocalName, ns);
                DomMetaElement rootElement = DomSchemaRegistry.GetRootElement(rootElementName);
                if (rootElement == null)
                    throw new InvalidOperationException("Unknown root element");

                rootDomObject = new DomObject(rootElement);
                ReadElement(rootDomObject, reader);

                reader.Close();
            }

            return rootDomObject;
        }

        private void WriteInternal(Stream stream, DomObject rootDomObject, bool bTemp)
        {
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;
            settings.IndentChars = IndentChars;
            settings.NewLineHandling = NewLineHandling.Replace;
            settings.NewLineChars = NewLineChars;

            using (XmlWriter writer = XmlWriter.Create(stream, settings))
            {
                if (writer == null)
                    return;

                writer.WriteStartDocument();

                DomSchema schema = rootDomObject.Collection.Schema;
                WriteElement(rootDomObject, writer, schema, bTemp);

                writer.WriteEndDocument();
                writer.Close();
            }
        }

        private void Combine(DomObject rootProjDomObject, DomObject rootTempDomObject)
        {
            if ((rootProjDomObject == null) || (rootTempDomObject == null))
                return;

            // Try and combine the two files
            foreach (ITempSettingsCopier copier in m_lstCopiers)
            {
                copier.Run(rootProjDomObject, rootTempDomObject);
            }
        }

        /// <summary>
        /// Read element
        /// </summary>
        /// <param name="domObject"></param>
        /// <param name="reader"></param>
        protected void ReadElement(DomObject domObject, XmlReader reader)
        {
            DomComplexType type = domObject.Type;

            // Read attributes
            int required = type.RequiredAttributes;
            while (reader.MoveToNextAttribute())
            {
                if (reader.Prefix == string.Empty ||
                    reader.LookupNamespace(reader.Prefix) == type.Name.Namespace)
                {
                    DomMetaAttribute metaAttribute = type.GetAttribute(reader.LocalName);
                    if (metaAttribute != null)
                    {
                        object value = metaAttribute.Type.Convert(reader.Value);
                        domObject.SetAttribute(metaAttribute.Name, value);

                        if (metaAttribute.Required)
                            required--;
                    }
                }
            }

            if (required != 0)
                throw new InvalidOperationException("Missing required attribute");

            reader.MoveToElement();

            if (!reader.IsEmptyElement)
            {
                // Read child elements
                while (reader.Read())
                {
                    if (reader.NodeType == XmlNodeType.Element)
                    {
                        // look up metadata for this element
                        DomMetaElement metaElement = type.GetChild(reader.LocalName);
                        if (metaElement != null)
                        {
                            DomObject child = CreateElement(reader, metaElement);
                            ReadElement(child, reader);
                            // At this point, child is a fully populated sub-tree

                            domObject.AddChild(metaElement, child);
                        }
                        else
                        {
                            reader.Skip();
                        }
                    }
                    else if (reader.NodeType == XmlNodeType.Text)
                    {
                        if (type.ValueType != null)
                        {
                            domObject.Value = type.ValueType.Convert(reader.Value);
                        }
                    }
                    else if (reader.NodeType == XmlNodeType.EndElement)
                    {
                        break;
                    }
                }
            }

            reader.MoveToContent();
        }

        private static DomObject CreateElement(XmlReader reader, DomMetaElement metaElement)
        {
            DomComplexType type = metaElement.Type;

            // Check for xsi:type attribute, indicating a derived type
            string typeName = reader.GetAttribute("xsi:type");
            if (typeName != null)
            {
                // check for qualified type name
                string prefix = string.Empty;
                int index = typeName.IndexOf(':');
                if (index >= 0)
                {
                    prefix = typeName.Substring(0, index);
                    index++;
                    typeName = typeName.Substring(index, typeName.Length - index);
                }

                string ns = reader.LookupNamespace(prefix);
                XmlQualifiedName qualifiedName = new XmlQualifiedName(typeName, ns);
                type = DomSchemaRegistry.GetComplexType(qualifiedName);

                if (type == null)
                    throw new InvalidOperationException("Unknown derived type");
            }

            return new DomObject(type);
        }

        /// <summary>
        /// Write element
        /// </summary>
        /// <param name="element"></param>
        /// <param name="writer"></param>
        /// <param name="schema"></param>
        /// <param name="bTemp"></param>
        protected void WriteElement(DomObject element, XmlWriter writer, DomSchema schema, bool bTemp)
        {
            // Test if the element should be saved or not
            if (!bTemp && !element.CanCreateInterface<ISledProjectFilesSaveableType>())
                return;

            string elemNs = element.MetaElement.QualifiedName.Namespace;
            string elemPrefix;

            // Is this the root DomObject?
            if (schema != null)
            {
                elemPrefix = schema.GetPrefix(elemNs) ?? GeneratePrefix(elemNs);

                writer.WriteStartElement(elemPrefix, element.MetaElement.Name, elemNs);

                // Define the xsi namespace
                writer.WriteAttributeString("xmlns", "xsi", null, XmlSchema.InstanceNamespace);

                // Define schema namespaces
                foreach (XmlQualifiedName name in schema.Namespaces)
                    if (name.Name != elemPrefix) // don't redefine the element namespace
                        writer.WriteAttributeString("xmlns", name.Name, null, name.Namespace);
            }
            else
            {
                // Not the root, so all schema namespaces have been defined
                elemPrefix = writer.LookupPrefix(elemNs) ?? GeneratePrefix(elemNs);

                writer.WriteStartElement(elemPrefix, element.MetaElement.Name, elemNs);
            }

            // Write type name if this is a polymorphic type
            DomComplexType elementType = element.Type;
            if (element.MetaElement.Type != elementType)
            {
                string typeName = elementType.Name.Name;
                string typeNs = elementType.Name.Namespace;
                string typePrefix = writer.LookupPrefix(typeNs);
                if (typePrefix == null)
                {
                    typePrefix = GeneratePrefix(typeNs);
                    writer.WriteAttributeString("xmlns", typePrefix, null, typeNs);
                }

                if (typePrefix != string.Empty)
                    typeName = typePrefix + ":" + typeName;

                writer.WriteAttributeString("xsi", "type", XmlSchema.InstanceNamespace, typeName);
            }

            // When writing project file we build a list of attributes to NOT save
            List<string> lstAttrsDontSave = new List<string>();

            // Do some stuff if writing project file
            if (!bTemp)
            {
                // Get all annotations (including base type annotations [and their base type annotations etc.])
                List<XmlNode> lstAnnotations = new List<XmlNode>();
                GetAllAnnotations(element.Type, ref lstAnnotations);

                // Go through pulling out which attributes to not save based on the annotation
                foreach (XmlNode annotation in lstAnnotations)
                {
                    // Do something if it's a "dont save" annotation!
                    if (string.Compare(annotation.Name, DontSaveAnnotation, true) == 0)
                    {
                        XmlNode node = annotation.Attributes.GetNamedItem("name");
                        if (node != null)
                        {
                            string szAttribute = node.Value;
                            if (!lstAttrsDontSave.Contains(szAttribute))
                                lstAttrsDontSave.Add(szAttribute);
                        }
                    }
                }
            }

            // Write normal attributes
            foreach (DomMetaAttribute attribute in elementType.Attributes)
            {
                // Do some stuff if writing project file
                if (!bTemp && (lstAttrsDontSave.Count > 0) && !string.IsNullOrEmpty(attribute.Name))
                {
                    if (lstAttrsDontSave.Contains(attribute.Name))
                        continue;
                }

                object value = element.GetAttribute(attribute);
                if (attribute.Required)
                {
                    if (value == null)
                        throw new InvalidOperationException("missing attribute");

                    writer.WriteAttributeString(attribute.Name, attribute.Type.Convert(value));
                }
                else if (value != null && value != attribute.Type.GetDefault())
                {
                    writer.WriteAttributeString(attribute.Name, attribute.Type.Convert(value));
                }
            }

            // Write element value
            if (element.Value != null &&
                element.Value != elementType.ValueType.GetDefault())
            {
                writer.WriteString(elementType.ValueType.Convert(element.Value));
            }

            // write child elements
            foreach (DomMetaElement elem in elementType.Children)
                foreach (DomObject child in element.GetChildren(elem.Name))
                    WriteElement(child, writer, null, bTemp);

            writer.WriteEndElement();
        }

        private string GeneratePrefix(string ns)
        {
            string prefix = null;
            if (!string.IsNullOrEmpty(ns))
            {
                if (!m_inlinePrefixes.TryGetValue(ns, out prefix))
                {
                    int suffix = m_inlinePrefixes.Count;
                    prefix = "_p" + suffix;
                }
            }

            return prefix;
        }

        private static void GetAllAnnotations(DomComplexType complexType, ref List<XmlNode> lstAnnotations)
        {
            lstAnnotations.AddRange(complexType.Annotation);

            if (complexType.BaseType != null)
                GetAllAnnotations(complexType.BaseType, ref lstAnnotations);
        }

        private readonly Dictionary<string, string> m_inlinePrefixes = new Dictionary<string, string>();
        private readonly string m_extTmp;
        private readonly IList<ITempSettingsCopier> m_lstCopiers = new List<ITempSettingsCopier>();

        private readonly static IList<SledDomProjectFilesPersister> s_instances = new List<SledDomProjectFilesPersister>();

        #region Temporary Settings Copying Stuff

        /// <summary>
        /// Interface ITempSettingsCopier
        /// </summary>
        public interface ITempSettingsCopier
        {
            /// <summary>
            /// Run the logic of the copier
            /// </summary>
            /// <param name="rootProjDomObject">Project file DomObject tree</param>
            /// <param name="rootTempDomObject">Temporary settings file DomObject tree</param>
            void Run(DomObject rootProjDomObject, DomObject rootTempDomObject);
        }

        /// <summary>
        /// Abstract class TempSettingsCopier
        /// </summary>
        public abstract class TempSettingsCopier : ITempSettingsCopier
        {
            /// <summary>
            /// Run the logic of the copier
            /// </summary>
            /// <param name="rootProjDomObject">Project file DomObject tree</param>
            /// <param name="rootTempDomObject">Temporary settings file DomObject tree</param>
            public abstract void Run(DomObject rootProjDomObject, DomObject rootTempDomObject);

            /// <summary>
            /// Go through a DomObject tree gathering objects that can create an interface to type T
            /// </summary>
            /// <typeparam name="T">Type to create interface to using DomObject.CreateInterface()</typeparam>
            /// <param name="rootDomObject">DomObject tree to iterate through</param>
            /// <param name="lstTypes">List to store results in</param>
            protected virtual void GatherTypes<T>(DomObject rootDomObject, ref List<T> lstTypes) where T : class
            {
                if (rootDomObject.CanCreateInterface<T>())
                    lstTypes.Add(rootDomObject.CreateInterface<T>());

                if (rootDomObject.HasChildren)
                {
                    foreach (DomObject domObject in rootDomObject.Children)
                    {
                        GatherTypes(domObject, ref lstTypes);
                    }
                }
            }
        }

        /// <summary>
        /// Goes through project file DomObject tree and temporary settings file DomObject tree finding all DomObjects from each file that 
        /// can create interfaces to type T.
        /// 
        /// It then compares the two lists using a comparer and if matches are found tries to copy the specified attribute from the 
        /// temporary settings file DomObject to the matching project file DomObject.
        /// </summary>
        /// <typeparam name="T">Type to search for</typeparam>
        public class TempSettingsTypeAttributeCopier<T> : TempSettingsCopier
            where T : class, IDomObjectInterface
        {
            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="attribute">Attribute from DomObject to copy from temporary setting file to project file</param>
            /// <param name="comparer">Comparer to use to determine if two DomObject's from each file are equal</param>
            public TempSettingsTypeAttributeCopier(string attribute, IEqualityComparer<T> comparer)
            {
                m_attribute = attribute;
                m_comparer = comparer;
            }

            /// <summary>
            /// Run the logic of the copier
            /// </summary>
            /// <param name="rootProjDomObject">Project file DomObject tree</param>
            /// <param name="rootTempDomObject">Temporary settings file DomObject tree</param>
            public override void Run(DomObject rootProjDomObject, DomObject rootTempDomObject)
            {
                List<T> lstProjFiles = new List<T>();
                GatherTypes(rootProjDomObject, ref lstProjFiles);

                List<T> lstTempFiles = new List<T>();
                GatherTypes(rootTempDomObject, ref lstTempFiles);

                foreach (T projFile in lstProjFiles)
                {
                    foreach (T tempFile in lstTempFiles)
                    {
                        if (m_comparer.Equals(projFile, tempFile))
                        {
                            object projAttr = projFile.InternalObject.TryGetAttribute(m_attribute);
                            object tempAttr = tempFile.InternalObject.TryGetAttribute(m_attribute);

                            if ((projAttr != null) && (tempAttr != null))
                                projFile.InternalObject.SetAttribute(m_attribute, tempAttr);
                        }
                    }
                }
            }

            private readonly string m_attribute;
            private readonly IEqualityComparer<T> m_comparer;
        }

        /// <summary>
        /// Goes through project file DomObject tree and temporary settings file DomObject tree finding all DomObjects from each file that
        /// can create interfaces to type T.
        /// 
        /// It then compares the two lists using a comparer and if matches are found tries to copy the specified type U from the
        /// temporary settings file DomObject to the matching project file DomObject.
        /// </summary>
        /// <typeparam name="T">Type to search for</typeparam>
        /// <typeparam name="TU">Type of children under type T to copy</typeparam>
        public class TempSettingsTypeChildCopier<T, TU> : TempSettingsCopier
            where T : class, IDomObjectInterface
            where TU : class
        {
            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="comparer">Comparer to use to determine if two DomObject's from each file are equal</param>
            /// <param name="duplicateComparer">Comparer to use to check for duplicates. If duplicates allowed then just use null</param>
            public TempSettingsTypeChildCopier(IEqualityComparer<T> comparer, IEqualityComparer<TU> duplicateComparer)
            {
                m_comparer = comparer;
                m_dupComparer = duplicateComparer;
            }

            /// <summary>
            /// Run the logic of the copier
            /// </summary>
            /// <param name="rootProjDomObject">Project file DomObject tree</param>
            /// <param name="rootTempDomObject">Temporary settings file DomObject tree</param>
            public override void Run(DomObject rootProjDomObject, DomObject rootTempDomObject)
            {
                List<T> lstProjFiles = new List<T>();
                GatherTypes(rootProjDomObject, ref lstProjFiles);

                List<T> lstTempFiles = new List<T>();
                GatherTypes(rootTempDomObject, ref lstTempFiles);

                foreach (T projFile in lstProjFiles)
                {
                    foreach (T tempFile in lstTempFiles)
                    {
                        if (m_comparer.Equals(projFile, tempFile))
                        {
                            foreach (DomObject domObject in tempFile.InternalObject.Children)
                            {
                                if (domObject.CanCreateInterface<TU>())
                                {
                                    // Search for element to add child to
                                    DomMetaElement metaElement = null;
                                    foreach (DomMetaElement elem in projFile.InternalObject.Type.Children)
                                    {
                                        if (elem.Type == domObject.Type)
                                        {
                                            metaElement = elem;
                                            break;
                                        }

                                        if (!elem.Type.IsAssignableFrom(domObject.Type))
                                            continue;

                                        metaElement = elem;
                                        break;
                                    }

                                    if (metaElement != null)
                                    {
                                        bool bAdd = true;

                                        if (m_dupComparer != null)
                                        {
                                            // Check for duplicates first!
                                            bool bDuplicate = false;

                                            foreach (DomObject d in projFile.InternalObject.Children)
                                            {
                                                if (!d.CanCreateInterface<TU>())
                                                    continue;

                                                bool bEquals =
                                                    m_dupComparer.Equals(
                                                        d.CreateInterface<TU>(),
                                                        domObject.CreateInterface<TU>());

                                                if (!bEquals)
                                                    continue;

                                                bDuplicate = true;
                                                break;
                                            }

                                            // Don't add duplicates
                                            bAdd = !bDuplicate;
                                        }

                                        // Copy over to project (if it isn't already there)
                                        if (bAdd && !domObject.CanCreateInterface<ISledProjectFilesSaveableSingletonType>())
                                        {
                                            DomObject domClone = domObject.Clone();
                                            projFile.InternalObject.AddChild(metaElement, domClone);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            private readonly IEqualityComparer<T> m_comparer;
            private readonly IEqualityComparer<TU> m_dupComparer;
        }

        #endregion
    }

    /// <summary>
    /// Interface ISledProjectFilesSaveableType
    /// </summary>
    public interface ISledProjectFilesSaveableType
    {
    }

    /// <summary>
    /// Interface ISledProjectFilesSaveableSingletonType
    /// </summary>
    public interface ISledProjectFilesSaveableSingletonType
    {
    }
}
