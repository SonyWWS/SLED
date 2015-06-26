/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

using System;
using System.ComponentModel.Composition;
using System.Reflection;
using System.Xml.Schema;

using Sce.Atf;
using Sce.Atf.Dom;

using Sce.Sled.Shared;
using Sce.Sled.Shared.Dom;

namespace Sce.Sled.Lua.Dom
{
    [Export(typeof(IInitializable))]
    [Export(typeof(SledLuaDomLoader))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    class SledLuaDomLoader : XmlSchemaTypeLoader, IInitializable
    {
        [ImportingConstructor]
        public SledLuaDomLoader()
        {
            // Stop a compiler warning
            if (m_sledSchemaLoader == null)
                m_sledSchemaLoader = null;

            var assembly = Assembly.GetAssembly(typeof(SledLuaDomLoader));
            var schemaSet = new XmlSchemaSet();

            var sledSharedSchemaSet = GetSledSharedSchemaSet();
            if (sledSharedSchemaSet != null)
                schemaSet.Add(sledSharedSchemaSet);

            var strm = assembly.GetManifestResourceStream(SledLuaUtil.LuaSchemaPath);
            if (strm != null)
            {
                // Load schema from file
                var schema = XmlSchema.Read(strm, null);

                schemaSet.Add(schema);
            }

            Load(schemaSet);
        }

        #region IInitializable Interface

        void IInitializable.Initialize()
        {
            // What's the correct way to get the base loader to contain
            // all the types from all the loaded schemas?

            // This is a temp solution to get items from Lua 
            // schema loader into base schema loader
            foreach (var nodeType in TypeCollection.GetNodeTypes())
            {
                var baseNodeType = m_sledSchemaLoader.GetNodeType(nodeType.Name);

                // If not null then base schema loader already contains
                // the node type and we don't want to add it
                if (baseNodeType != null)
                    continue;

                m_sledSchemaLoader.AddNodeType(nodeType.Name, nodeType);
            }
        }

        #endregion

        #region XmlSchemaTypeLoader Overrides

        protected override void OnSchemaSetLoaded(XmlSchemaSet schemaSet)
        {
            var typeCollections = GetTypeCollections();
            foreach (var typeCollection in typeCollections)
            {
                Namespace = typeCollection.TargetNamespace;
                TypeCollection = typeCollection;

                SledLuaSchema.Initialize(TypeCollection);

                SledLuaSchema.SledLuaCompileAttributeType.Type.Define(new ExtensionInfo<SledLuaCompileAttributeType>());
                SledLuaSchema.SledLuaCompileConfigurationType.Type.Define(new ExtensionInfo<SledLuaCompileConfigurationType>());
                SledLuaSchema.SledLuaCompileSettingsType.Type.Define(new ExtensionInfo<SledLuaCompileSettingsType>());
                SledLuaSchema.SledLuaFunctionType.Type.Define(new ExtensionInfo<SledLuaFunctionType>());
                SledLuaSchema.SledLuaVarNameTypePairType.Type.Define(new ExtensionInfo<SledLuaVarNameTypePairType>());
                SledLuaSchema.SledLuaVarLookUpType.Type.Define(new ExtensionInfo<SledLuaVarLookUpType>());
                SledLuaSchema.SledLuaProjectFilesWatchType.Type.Define(new ExtensionInfo<SledLuaProjectFilesWatchType>());
                SledLuaSchema.SledLuaStateListType.Type.Define(new ExtensionInfo<SledLuaStateListType>());
                SledLuaSchema.SledLuaStateType.Type.Define(new ExtensionInfo<SledLuaStateType>());
                SledLuaSchema.SledLuaVarEnvListType.Type.Define(new ExtensionInfo<SledLuaVarEnvListType>());
                SledLuaSchema.SledLuaVarEnvType.Type.Define(new ExtensionInfo<SledLuaVarEnvType>());
                SledLuaSchema.SledLuaVarFilterNameType.Type.Define(new ExtensionInfo<SledLuaVarFilterNameType>());
                SledLuaSchema.SledLuaVarFilterNamesType.Type.Define(new ExtensionInfo<SledLuaVarFilterNamesType>());
                SledLuaSchema.SledLuaVarFiltersType.Type.Define(new ExtensionInfo<SledLuaVarFiltersType>());
                SledLuaSchema.SledLuaVarFilterType.Type.Define(new ExtensionInfo<SledLuaVarFilterType>());
                SledLuaSchema.SledLuaVarFilterTypesType.Type.Define(new ExtensionInfo<SledLuaVarFilterTypesType>());
                SledLuaSchema.SledLuaVarGlobalType.Type.Define(new ExtensionInfo<SledLuaVarGlobalType>());
                SledLuaSchema.SledLuaVarGlobalListType.Type.Define(new ExtensionInfo<SledLuaVarGlobalListType>());
                SledLuaSchema.SledLuaVarLocalListType.Type.Define(new ExtensionInfo<SledLuaVarLocalListType>());
                SledLuaSchema.SledLuaVarLocalType.Type.Define(new ExtensionInfo<SledLuaVarLocalType>());
                SledLuaSchema.SledLuaVarUpvalueListType.Type.Define(new ExtensionInfo<SledLuaVarUpvalueListType>());
                SledLuaSchema.SledLuaVarUpvalueType.Type.Define(new ExtensionInfo<SledLuaVarUpvalueType>());
                SledLuaSchema.SledLuaVarWatchListType.Type.Define(new ExtensionInfo<SledLuaVarWatchListType>());

                SetupStringBasedEnumeration(SledLuaSchema.SledLuaVarLookUpType.scopeAttribute, typeof(SledLuaVarScopeType));
                SetupStringBasedEnumeration(SledLuaSchema.SledLuaVarLookUpType.contextAttribute, typeof(SledLuaVarLookUpContextType));
                SetupStringBasedEnumeration(SledLuaSchema.SledLuaProjectFilesWatchType.scopeAttribute, typeof(SledLuaVarScopeType));
                SetupStringBasedEnumeration(SledLuaSchema.SledLuaProjectFilesWatchType.contextAttribute, typeof(SledLuaVarLookUpContextType));

                break; // Only one namespace
            }
        }

        #endregion

        public string Namespace { get; private set; }

        public XmlSchemaTypeCollection TypeCollection { get; private set; }

        /// <summary>
        /// Set up any enumerations whose values are stored as strings
        /// </summary>
        /// <param name="attribute"></param>
        /// <param name="enumType"></param>
        public static void SetupStringBasedEnumeration(AttributeInfo attribute, Type enumType)
        {
            var strings = Enum.GetNames(enumType);
            AttributeRule rule = new StringEnumRule(strings);
            attribute.AddRule(rule);
            attribute.DefaultValue = strings[0];
        }

        private static XmlSchemaSet GetSledSharedSchemaSet()
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

            return schemaSet;
        }

        [Import]
        private SledSharedSchemaLoader m_sledSchemaLoader;
    }
}