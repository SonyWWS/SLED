/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

using System.ComponentModel.Composition;
using System.Reflection;
using System.Xml.Schema;

using Sce.Atf;
using Sce.Atf.Dom;

namespace Sce.Sled.Shared.Dom
{
    /// <summary>
    /// SledShared SchemaLoader Class
    /// </summary>
    [Export(typeof(IInitializable))]
    [Export(typeof(SledSharedSchemaLoader))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class SledSharedSchemaLoader : XmlSchemaTypeLoader, IInitializable
    {
        /// <summary>
        /// Constructor
        /// </summary>
        [ImportingConstructor]
        public SledSharedSchemaLoader()
        {
            var assembly = Assembly.GetAssembly(typeof(SledShared));
            var schemaSet = new XmlSchemaSet();

            var strm = assembly.GetManifestResourceStream(SledShared.SchemaPath);
            if (strm != null)
            {
                // Load schema from file
                var schema = XmlSchema.Read(strm, null);

                schemaSet.Add(schema);
            }

            Load(schemaSet);
        }

        #region IInitializable Interface

        /// <summary>
        /// Finishes initializing component</summary>
        void IInitializable.Initialize()
        {
            // Keep this here
        }

        #endregion

        #region XmlSchemaTypeLoader Overrides

        /// <summary>
        /// Method called after the schema set has been loaded and the DomNodeTypes have been created, but
        /// before the DomNodeTypes have been frozen. This means that DomNodeType.SetIdAttribute, for example, has
        /// not been called on the DomNodeTypes. Is called shortly before OnDomNodeTypesFrozen.</summary>
        /// <param name="schemaSet">XML schema sets being loaded</param>
        protected override void OnSchemaSetLoaded(XmlSchemaSet schemaSet)
        {
            var typeCollections =
                GetTypeCollections();

            foreach (var typeCollection in typeCollections)
            {
                Namespace = typeCollection.TargetNamespace;
                TypeCollection = typeCollection;

                SledSchema.Initialize(typeCollection);

                SledSchema.SledProjectFilesBreakpointType.Type.Define(new ExtensionInfo<SledProjectFilesBreakpointType>());
                SledSchema.SledProjectFilesLanguageType.Type.Define(new ExtensionInfo<SledProjectFilesLanguageType>());
                SledSchema.SledProjectFilesFileType.Type.Define(new ExtensionInfo<SledProjectFilesFileType>());
                SledSchema.SledProjectFilesFolderType.Type.Define(new ExtensionInfo<SledProjectFilesFolderType>());
                SledSchema.SledProjectFilesType.Type.Define(new ExtensionInfo<SledProjectFilesType>());
                SledSchema.SledProjectFilesEmptyType.Type.Define(new ExtensionInfo<SledProjectFilesEmptyType>());
                SledSchema.SledSyntaxErrorType.Type.Define(new ExtensionInfo<SledSyntaxErrorType>());
                SledSchema.SledSyntaxErrorListType.Type.Define(new ExtensionInfo<SledSyntaxErrorListType>());
                SledSchema.SledProfileInfoType.Type.Define(new ExtensionInfo<SledProfileInfoType>());
                SledSchema.SledProfileInfoListType.Type.Define(new ExtensionInfo<SledProfileInfoListType>());
                SledSchema.SledCallStackType.Type.Define(new ExtensionInfo<SledCallStackType>());
                SledSchema.SledCallStackListType.Type.Define(new ExtensionInfo<SledCallStackListType>());
                SledSchema.SledVarLocationType.Type.Define(new ExtensionInfo<SledVarLocationType>());
                SledSchema.SledFindResultsType.Type.Define(new ExtensionInfo<SledFindResultsType>());
                SledSchema.SledFindResultsListType.Type.Define(new ExtensionInfo<SledFindResultsListType>());
                SledSchema.SledProjectFilesRootType.Type.Define(new ExtensionInfo<SledProjectFilesRootType>());

                break; // Only one namespace
            }
        }

        #endregion

        /// <summary>
        /// Gets namespace
        /// </summary>
        public string Namespace { get; private set; }

        /// <summary>
        /// Gets type collection
        /// </summary>
        public XmlSchemaTypeCollection TypeCollection { get; private set; }
    }
}
