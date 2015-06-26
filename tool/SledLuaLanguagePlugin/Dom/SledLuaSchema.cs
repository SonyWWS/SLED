/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

using Sce.Atf.Dom;

namespace Sce.Sled.Lua.Dom
{
    public static class SledLuaSchema
    {
        public const string NS = "sled";

        public static void Initialize(XmlSchemaTypeCollection typeCollection)
        {
            SledLuaVarGlobalListType.Type = typeCollection.GetNodeType("SledLuaVarGlobalListType");
            SledLuaVarGlobalListType.nameAttribute = SledLuaVarGlobalListType.Type.GetAttributeInfo("name");
            SledLuaVarGlobalListType.GlobalsChild = SledLuaVarGlobalListType.Type.GetChildInfo("Globals");

            SledLuaVarBaseListType.Type = typeCollection.GetNodeType("SledLuaVarBaseListType");
            SledLuaVarBaseListType.nameAttribute = SledLuaVarBaseListType.Type.GetAttributeInfo("name");

            SledLuaVarGlobalType.Type = typeCollection.GetNodeType("SledLuaVarGlobalType");
            SledLuaVarGlobalType.display_nameAttribute = SledLuaVarGlobalType.Type.GetAttributeInfo("display_name");
            SledLuaVarGlobalType.nameAttribute = SledLuaVarGlobalType.Type.GetAttributeInfo("name");
            SledLuaVarGlobalType.unique_nameAttribute = SledLuaVarGlobalType.Type.GetAttributeInfo("unique_name");
            SledLuaVarGlobalType.typeAttribute = SledLuaVarGlobalType.Type.GetAttributeInfo("type");
            SledLuaVarGlobalType.valueAttribute = SledLuaVarGlobalType.Type.GetAttributeInfo("value");
            SledLuaVarGlobalType.keytypeAttribute = SledLuaVarGlobalType.Type.GetAttributeInfo("keytype");
            SledLuaVarGlobalType.expandedAttribute = SledLuaVarGlobalType.Type.GetAttributeInfo("expanded");
            SledLuaVarGlobalType.visibleAttribute = SledLuaVarGlobalType.Type.GetAttributeInfo("visible");
            SledLuaVarGlobalType.LocationsChild = SledLuaVarGlobalType.Type.GetChildInfo("Locations");
            SledLuaVarGlobalType.GlobalsChild = SledLuaVarGlobalType.Type.GetChildInfo("Globals");
            SledLuaVarGlobalType.TargetHierarchyChild = SledLuaVarGlobalType.Type.GetChildInfo("TargetHierarchy");

            SledLuaVarBaseType.Type = typeCollection.GetNodeType("SledLuaVarBaseType");
            SledLuaVarBaseType.nameAttribute = SledLuaVarBaseType.Type.GetAttributeInfo("name");
            SledLuaVarBaseType.display_nameAttribute = SledLuaVarBaseType.Type.GetAttributeInfo("display_name");
            SledLuaVarBaseType.unique_nameAttribute = SledLuaVarBaseType.Type.GetAttributeInfo("unique_name");
            SledLuaVarBaseType.typeAttribute = SledLuaVarBaseType.Type.GetAttributeInfo("type");
            SledLuaVarBaseType.valueAttribute = SledLuaVarBaseType.Type.GetAttributeInfo("value");
            SledLuaVarBaseType.keytypeAttribute = SledLuaVarBaseType.Type.GetAttributeInfo("keytype");
            SledLuaVarBaseType.expandedAttribute = SledLuaVarBaseType.Type.GetAttributeInfo("expanded");
            SledLuaVarBaseType.visibleAttribute = SledLuaVarBaseType.Type.GetAttributeInfo("visible");
            SledLuaVarBaseType.LocationsChild = SledLuaVarBaseType.Type.GetChildInfo("Locations");
            SledLuaVarBaseType.TargetHierarchyChild = SledLuaVarBaseType.Type.GetChildInfo("TargetHierarchy");

            SledLuaVarLocalListType.Type = typeCollection.GetNodeType("SledLuaVarLocalListType");
            SledLuaVarLocalListType.nameAttribute = SledLuaVarLocalListType.Type.GetAttributeInfo("name");
            SledLuaVarLocalListType.LocalsChild = SledLuaVarLocalListType.Type.GetChildInfo("Locals");

            SledLuaVarLocalType.Type = typeCollection.GetNodeType("SledLuaVarLocalType");
            SledLuaVarLocalType.nameAttribute = SledLuaVarLocalType.Type.GetAttributeInfo("name");
            SledLuaVarLocalType.display_nameAttribute = SledLuaVarLocalType.Type.GetAttributeInfo("display_name");
            SledLuaVarLocalType.unique_nameAttribute = SledLuaVarLocalType.Type.GetAttributeInfo("unique_name");
            SledLuaVarLocalType.typeAttribute = SledLuaVarLocalType.Type.GetAttributeInfo("type");
            SledLuaVarLocalType.valueAttribute = SledLuaVarLocalType.Type.GetAttributeInfo("value");
            SledLuaVarLocalType.keytypeAttribute = SledLuaVarLocalType.Type.GetAttributeInfo("keytype");
            SledLuaVarLocalType.expandedAttribute = SledLuaVarLocalType.Type.GetAttributeInfo("expanded");
            SledLuaVarLocalType.visibleAttribute = SledLuaVarLocalType.Type.GetAttributeInfo("visible");
            SledLuaVarLocalType.levelAttribute = SledLuaVarLocalType.Type.GetAttributeInfo("level");
            SledLuaVarLocalType.indexAttribute = SledLuaVarLocalType.Type.GetAttributeInfo("index");
            SledLuaVarLocalType.function_nameAttribute = SledLuaVarLocalType.Type.GetAttributeInfo("function_name");
            SledLuaVarLocalType.function_linedefinedAttribute = SledLuaVarLocalType.Type.GetAttributeInfo("function_linedefined");
            SledLuaVarLocalType.LocationsChild = SledLuaVarLocalType.Type.GetChildInfo("Locations");
            SledLuaVarLocalType.LocalsChild = SledLuaVarLocalType.Type.GetChildInfo("Locals");
            SledLuaVarLocalType.TargetHierarchyChild = SledLuaVarLocalType.Type.GetChildInfo("TargetHierarchy");

            SledLuaVarLocalUpvalueBaseType.Type = typeCollection.GetNodeType("SledLuaVarLocalUpvalueBaseType");
            SledLuaVarLocalUpvalueBaseType.nameAttribute = SledLuaVarLocalUpvalueBaseType.Type.GetAttributeInfo("name");
            SledLuaVarLocalUpvalueBaseType.display_nameAttribute = SledLuaVarLocalUpvalueBaseType.Type.GetAttributeInfo("display_name");
            SledLuaVarLocalUpvalueBaseType.unique_nameAttribute = SledLuaVarLocalUpvalueBaseType.Type.GetAttributeInfo("unique_name");
            SledLuaVarLocalUpvalueBaseType.typeAttribute = SledLuaVarLocalUpvalueBaseType.Type.GetAttributeInfo("type");
            SledLuaVarLocalUpvalueBaseType.valueAttribute = SledLuaVarLocalUpvalueBaseType.Type.GetAttributeInfo("value");
            SledLuaVarLocalUpvalueBaseType.keytypeAttribute = SledLuaVarLocalUpvalueBaseType.Type.GetAttributeInfo("keytype");
            SledLuaVarLocalUpvalueBaseType.expandedAttribute = SledLuaVarLocalUpvalueBaseType.Type.GetAttributeInfo("expanded");
            SledLuaVarLocalUpvalueBaseType.visibleAttribute = SledLuaVarLocalUpvalueBaseType.Type.GetAttributeInfo("visible");
            SledLuaVarLocalUpvalueBaseType.levelAttribute = SledLuaVarLocalUpvalueBaseType.Type.GetAttributeInfo("level");
            SledLuaVarLocalUpvalueBaseType.indexAttribute = SledLuaVarLocalUpvalueBaseType.Type.GetAttributeInfo("index");
            SledLuaVarLocalUpvalueBaseType.function_nameAttribute = SledLuaVarLocalUpvalueBaseType.Type.GetAttributeInfo("function_name");
            SledLuaVarLocalUpvalueBaseType.function_linedefinedAttribute = SledLuaVarLocalUpvalueBaseType.Type.GetAttributeInfo("function_linedefined");
            SledLuaVarLocalUpvalueBaseType.LocationsChild = SledLuaVarLocalUpvalueBaseType.Type.GetChildInfo("Locations");
            SledLuaVarLocalUpvalueBaseType.TargetHierarchyChild = SledLuaVarLocalUpvalueBaseType.Type.GetChildInfo("TargetHierarchy");

            SledLuaVarUpvalueListType.Type = typeCollection.GetNodeType("SledLuaVarUpvalueListType");
            SledLuaVarUpvalueListType.nameAttribute = SledLuaVarUpvalueListType.Type.GetAttributeInfo("name");
            SledLuaVarUpvalueListType.UpvaluesChild = SledLuaVarUpvalueListType.Type.GetChildInfo("Upvalues");

            SledLuaVarUpvalueType.Type = typeCollection.GetNodeType("SledLuaVarUpvalueType");
            SledLuaVarUpvalueType.nameAttribute = SledLuaVarUpvalueType.Type.GetAttributeInfo("name");
            SledLuaVarUpvalueType.display_nameAttribute = SledLuaVarUpvalueType.Type.GetAttributeInfo("display_name");
            SledLuaVarUpvalueType.unique_nameAttribute = SledLuaVarUpvalueType.Type.GetAttributeInfo("unique_name");
            SledLuaVarUpvalueType.typeAttribute = SledLuaVarUpvalueType.Type.GetAttributeInfo("type");
            SledLuaVarUpvalueType.valueAttribute = SledLuaVarUpvalueType.Type.GetAttributeInfo("value");
            SledLuaVarUpvalueType.keytypeAttribute = SledLuaVarUpvalueType.Type.GetAttributeInfo("keytype");
            SledLuaVarUpvalueType.expandedAttribute = SledLuaVarUpvalueType.Type.GetAttributeInfo("expanded");
            SledLuaVarUpvalueType.visibleAttribute = SledLuaVarUpvalueType.Type.GetAttributeInfo("visible");
            SledLuaVarUpvalueType.levelAttribute = SledLuaVarUpvalueType.Type.GetAttributeInfo("level");
            SledLuaVarUpvalueType.indexAttribute = SledLuaVarUpvalueType.Type.GetAttributeInfo("index");
            SledLuaVarUpvalueType.function_nameAttribute = SledLuaVarUpvalueType.Type.GetAttributeInfo("function_name");
            SledLuaVarUpvalueType.function_linedefinedAttribute = SledLuaVarUpvalueType.Type.GetAttributeInfo("function_linedefined");
            SledLuaVarUpvalueType.LocationsChild = SledLuaVarUpvalueType.Type.GetChildInfo("Locations");
            SledLuaVarUpvalueType.UpvaluesChild = SledLuaVarUpvalueType.Type.GetChildInfo("Upvalues");
            SledLuaVarUpvalueType.TargetHierarchyChild = SledLuaVarUpvalueType.Type.GetChildInfo("TargetHierarchy");

            SledLuaVarWatchListType.Type = typeCollection.GetNodeType("SledLuaVarWatchListType");
            SledLuaVarWatchListType.nameAttribute = SledLuaVarWatchListType.Type.GetAttributeInfo("name");
            SledLuaVarWatchListType.GlobalsChild = SledLuaVarWatchListType.Type.GetChildInfo("Globals");
            SledLuaVarWatchListType.LocalsChild = SledLuaVarWatchListType.Type.GetChildInfo("Locals");
            SledLuaVarWatchListType.UpvaluesChild = SledLuaVarWatchListType.Type.GetChildInfo("Upvalues");
            SledLuaVarWatchListType.EnvVarsChild = SledLuaVarWatchListType.Type.GetChildInfo("EnvVars");

            SledLuaVarEnvType.Type = typeCollection.GetNodeType("SledLuaVarEnvType");
            SledLuaVarEnvType.nameAttribute = SledLuaVarEnvType.Type.GetAttributeInfo("name");
            SledLuaVarEnvType.display_nameAttribute = SledLuaVarEnvType.Type.GetAttributeInfo("display_name");
            SledLuaVarEnvType.unique_nameAttribute = SledLuaVarEnvType.Type.GetAttributeInfo("unique_name");
            SledLuaVarEnvType.typeAttribute = SledLuaVarEnvType.Type.GetAttributeInfo("type");
            SledLuaVarEnvType.valueAttribute = SledLuaVarEnvType.Type.GetAttributeInfo("value");
            SledLuaVarEnvType.keytypeAttribute = SledLuaVarEnvType.Type.GetAttributeInfo("keytype");
            SledLuaVarEnvType.expandedAttribute = SledLuaVarEnvType.Type.GetAttributeInfo("expanded");
            SledLuaVarEnvType.visibleAttribute = SledLuaVarEnvType.Type.GetAttributeInfo("visible");
            SledLuaVarEnvType.levelAttribute = SledLuaVarEnvType.Type.GetAttributeInfo("level");
            SledLuaVarEnvType.LocationsChild = SledLuaVarEnvType.Type.GetChildInfo("Locations");
            SledLuaVarEnvType.EnvVarsChild = SledLuaVarEnvType.Type.GetChildInfo("EnvVars");
            SledLuaVarEnvType.TargetHierarchyChild = SledLuaVarEnvType.Type.GetChildInfo("TargetHierarchy");

            SledLuaVarEnvListType.Type = typeCollection.GetNodeType("SledLuaVarEnvListType");
            SledLuaVarEnvListType.nameAttribute = SledLuaVarEnvListType.Type.GetAttributeInfo("name");
            SledLuaVarEnvListType.EnvVarsChild = SledLuaVarEnvListType.Type.GetChildInfo("EnvVars");

            SledLuaStateListType.Type = typeCollection.GetNodeType("SledLuaStateListType");
            SledLuaStateListType.nameAttribute = SledLuaStateListType.Type.GetAttributeInfo("name");
            SledLuaStateListType.LuaStatesChild = SledLuaStateListType.Type.GetChildInfo("LuaStates");

            SledLuaStateType.Type = typeCollection.GetNodeType("SledLuaStateType");
            SledLuaStateType.nameAttribute = SledLuaStateType.Type.GetAttributeInfo("name");
            SledLuaStateType.addressAttribute = SledLuaStateType.Type.GetAttributeInfo("address");
            SledLuaStateType.checkedAttribute = SledLuaStateType.Type.GetAttributeInfo("checked");

            SledLuaVarFiltersType.Type = typeCollection.GetNodeType("SledLuaVarFiltersType");
            SledLuaVarFiltersType.nameAttribute = SledLuaVarFiltersType.Type.GetAttributeInfo("name");
            SledLuaVarFiltersType.expandedAttribute = SledLuaVarFiltersType.Type.GetAttributeInfo("expanded");
            SledLuaVarFiltersType.GlobalsChild = SledLuaVarFiltersType.Type.GetChildInfo("Globals");
            SledLuaVarFiltersType.LocalsChild = SledLuaVarFiltersType.Type.GetChildInfo("Locals");
            SledLuaVarFiltersType.UpvaluesChild = SledLuaVarFiltersType.Type.GetChildInfo("Upvalues");
            SledLuaVarFiltersType.EnvVarsChild = SledLuaVarFiltersType.Type.GetChildInfo("EnvVars");

            SledLuaVarFilterType.Type = typeCollection.GetNodeType("SledLuaVarFilterType");
            SledLuaVarFilterType.LocalTypesChild = SledLuaVarFilterType.Type.GetChildInfo("LocalTypes");
            SledLuaVarFilterType.LocalNamesChild = SledLuaVarFilterType.Type.GetChildInfo("LocalNames");
            SledLuaVarFilterType.TargetTypesChild = SledLuaVarFilterType.Type.GetChildInfo("TargetTypes");
            SledLuaVarFilterType.TargetNamesChild = SledLuaVarFilterType.Type.GetChildInfo("TargetNames");

            SledLuaVarFilterTypesType.Type = typeCollection.GetNodeType("SledLuaVarFilterTypesType");
            SledLuaVarFilterTypesType.lua_tnilAttribute = SledLuaVarFilterTypesType.Type.GetAttributeInfo("lua_tnil");
            SledLuaVarFilterTypesType.lua_tbooleanAttribute = SledLuaVarFilterTypesType.Type.GetAttributeInfo("lua_tboolean");
            SledLuaVarFilterTypesType.lua_tlightuserdataAttribute = SledLuaVarFilterTypesType.Type.GetAttributeInfo("lua_tlightuserdata");
            SledLuaVarFilterTypesType.lua_tnumberAttribute = SledLuaVarFilterTypesType.Type.GetAttributeInfo("lua_tnumber");
            SledLuaVarFilterTypesType.lua_tstringAttribute = SledLuaVarFilterTypesType.Type.GetAttributeInfo("lua_tstring");
            SledLuaVarFilterTypesType.lua_ttableAttribute = SledLuaVarFilterTypesType.Type.GetAttributeInfo("lua_ttable");
            SledLuaVarFilterTypesType.lua_tfunctionAttribute = SledLuaVarFilterTypesType.Type.GetAttributeInfo("lua_tfunction");
            SledLuaVarFilterTypesType.lua_tuserdataAttribute = SledLuaVarFilterTypesType.Type.GetAttributeInfo("lua_tuserdata");
            SledLuaVarFilterTypesType.lua_tthreadAttribute = SledLuaVarFilterTypesType.Type.GetAttributeInfo("lua_tthread");

            SledLuaVarFilterNamesType.Type = typeCollection.GetNodeType("SledLuaVarFilterNamesType");
            SledLuaVarFilterNamesType.NamesChild = SledLuaVarFilterNamesType.Type.GetChildInfo("Names");

            SledLuaVarFilterNameType.Type = typeCollection.GetNodeType("SledLuaVarFilterNameType");
            SledLuaVarFilterNameType.nameAttribute = SledLuaVarFilterNameType.Type.GetAttributeInfo("name");

            SledLuaFunctionType.Type = typeCollection.GetNodeType("SledLuaFunctionType");
            SledLuaFunctionType.nameAttribute = SledLuaFunctionType.Type.GetAttributeInfo("name");
            SledLuaFunctionType.line_definedAttribute = SledLuaFunctionType.Type.GetAttributeInfo("line_defined");
            SledLuaFunctionType.last_line_definedAttribute = SledLuaFunctionType.Type.GetAttributeInfo("last_line_defined");

            SledLuaCompileAttributeType.Type = typeCollection.GetNodeType("SledLuaCompileAttributeType");
            SledLuaCompileAttributeType.nameAttribute = SledLuaCompileAttributeType.Type.GetAttributeInfo("name");
            SledLuaCompileAttributeType.compileAttribute = SledLuaCompileAttributeType.Type.GetAttributeInfo("compile");

            SledLuaVarNameTypePairType.Type = typeCollection.GetNodeType("SledLuaVarNameTypePairType");
            SledLuaVarNameTypePairType.nameAttribute = SledLuaVarNameTypePairType.Type.GetAttributeInfo("name");
            SledLuaVarNameTypePairType.name_typeAttribute = SledLuaVarNameTypePairType.Type.GetAttributeInfo("name_type");

            SledLuaVarLookUpType.Type = typeCollection.GetNodeType("SledLuaVarLookUpType");
            SledLuaVarLookUpType.scopeAttribute = SledLuaVarLookUpType.Type.GetAttributeInfo("scope");
            SledLuaVarLookUpType.stack_levelAttribute = SledLuaVarLookUpType.Type.GetAttributeInfo("stack_level");
            SledLuaVarLookUpType.indexAttribute = SledLuaVarLookUpType.Type.GetAttributeInfo("index");
            SledLuaVarLookUpType.extraAttribute = SledLuaVarLookUpType.Type.GetAttributeInfo("extra");
            SledLuaVarLookUpType.contextAttribute = SledLuaVarLookUpType.Type.GetAttributeInfo("context");
            SledLuaVarLookUpType.NamesAndTypesChild = SledLuaVarLookUpType.Type.GetChildInfo("NamesAndTypes");

            SledLuaProjectFilesWatchType.Type = typeCollection.GetNodeType("SledLuaProjectFilesWatchType");
            SledLuaProjectFilesWatchType.nameAttribute = SledLuaProjectFilesWatchType.Type.GetAttributeInfo("name");
            SledLuaProjectFilesWatchType.expandedAttribute = SledLuaProjectFilesWatchType.Type.GetAttributeInfo("expanded");
            SledLuaProjectFilesWatchType.language_pluginAttribute = SledLuaProjectFilesWatchType.Type.GetAttributeInfo("language_plugin");
            SledLuaProjectFilesWatchType.scopeAttribute = SledLuaProjectFilesWatchType.Type.GetAttributeInfo("scope");
            SledLuaProjectFilesWatchType.contextAttribute = SledLuaProjectFilesWatchType.Type.GetAttributeInfo("context");
            SledLuaProjectFilesWatchType.guidAttribute = SledLuaProjectFilesWatchType.Type.GetAttributeInfo("guid");
            SledLuaProjectFilesWatchType.LookUpChild = SledLuaProjectFilesWatchType.Type.GetChildInfo("LookUp");

            SledLuaCompileConfigurationType.Type = typeCollection.GetNodeType("SledLuaCompileConfigurationType");
            SledLuaCompileConfigurationType.nameAttribute = SledLuaCompileConfigurationType.Type.GetAttributeInfo("name");
            SledLuaCompileConfigurationType.little_endianAttribute = SledLuaCompileConfigurationType.Type.GetAttributeInfo("little_endian");
            SledLuaCompileConfigurationType.strip_debugAttribute = SledLuaCompileConfigurationType.Type.GetAttributeInfo("strip_debug");
            SledLuaCompileConfigurationType.sizeof_intAttribute = SledLuaCompileConfigurationType.Type.GetAttributeInfo("sizeof_int");
            SledLuaCompileConfigurationType.sizeof_size_tAttribute = SledLuaCompileConfigurationType.Type.GetAttributeInfo("sizeof_size_t");
            SledLuaCompileConfigurationType.sizeof_lua_NumberAttribute = SledLuaCompileConfigurationType.Type.GetAttributeInfo("sizeof_lua_Number");
            SledLuaCompileConfigurationType.selectedAttribute = SledLuaCompileConfigurationType.Type.GetAttributeInfo("selected");
            SledLuaCompileConfigurationType.output_pathAttribute = SledLuaCompileConfigurationType.Type.GetAttributeInfo("output_path");
            SledLuaCompileConfigurationType.output_extensionAttribute = SledLuaCompileConfigurationType.Type.GetAttributeInfo("output_extension");
            SledLuaCompileConfigurationType.preserve_relative_path_infoAttribute = SledLuaCompileConfigurationType.Type.GetAttributeInfo("preserve_relative_path_info");

            SledLuaCompileSettingsType.Type = typeCollection.GetNodeType("SledLuaCompileSettingsType");
            SledLuaCompileSettingsType.nameAttribute = SledLuaCompileSettingsType.Type.GetAttributeInfo("name");
            SledLuaCompileSettingsType.expandedAttribute = SledLuaCompileSettingsType.Type.GetAttributeInfo("expanded");
            SledLuaCompileSettingsType.ConfigurationsChild = SledLuaCompileSettingsType.Type.GetChildInfo("Configurations");

            SledLuaProfileInfoRootElement = typeCollection.GetRootElement("SledLuaProfileInfo");
            SledLuaProfileFuncCallsRootElement = typeCollection.GetRootElement("SledLuaProfileFuncCalls");
            SledLuaMemoryTraceRootElement = typeCollection.GetRootElement("SledLuaMemoryTrace");
            SledLuaCallStackRootElement = typeCollection.GetRootElement("SledLuaCallStack");
            SledLuaVarGlobalsRootElement = typeCollection.GetRootElement("SledLuaVarGlobals");
            SledLuaVarLocalsRootElement = typeCollection.GetRootElement("SledLuaVarLocals");
            SledLuaVarUpvaluesRootElement = typeCollection.GetRootElement("SledLuaVarUpvalues");
            SledLuaVarWatchListRootElement = typeCollection.GetRootElement("SledLuaVarWatchList");
            SledLuaVarEnvListRootElement = typeCollection.GetRootElement("SledLuaVarEnvList");
            SledLuaStatesListRootElement = typeCollection.GetRootElement("SledLuaStatesList");
            SledLuaVarFiltersListRootElement = typeCollection.GetRootElement("SledLuaVarFiltersList");
        }

        public static class SledLuaVarGlobalListType
        {
            public static DomNodeType Type;
            public static AttributeInfo nameAttribute;
            public static ChildInfo GlobalsChild;
        }

        public static class SledLuaVarBaseListType
        {
            public static DomNodeType Type;
            public static AttributeInfo nameAttribute;
        }

        public static class SledLuaVarGlobalType
        {
            public static DomNodeType Type;
            public static AttributeInfo nameAttribute;
            public static AttributeInfo display_nameAttribute;
            public static AttributeInfo unique_nameAttribute;
            public static AttributeInfo typeAttribute;
            public static AttributeInfo valueAttribute;
            public static AttributeInfo keytypeAttribute;
            public static AttributeInfo expandedAttribute;
            public static AttributeInfo visibleAttribute;
            public static ChildInfo LocationsChild;
            public static ChildInfo GlobalsChild;
            public static ChildInfo TargetHierarchyChild;
        }

        public static class SledLuaVarBaseType
        {
            public static DomNodeType Type;
            public static AttributeInfo nameAttribute;
            public static AttributeInfo display_nameAttribute;
            public static AttributeInfo unique_nameAttribute;
            public static AttributeInfo typeAttribute;
            public static AttributeInfo valueAttribute;
            public static AttributeInfo keytypeAttribute;
            public static AttributeInfo expandedAttribute;
            public static AttributeInfo visibleAttribute;
            public static ChildInfo LocationsChild;
            public static ChildInfo TargetHierarchyChild;
        }

        public static class SledLuaVarLocalListType
        {
            public static DomNodeType Type;
            public static AttributeInfo nameAttribute;
            public static ChildInfo LocalsChild;
        }

        public static class SledLuaVarLocalType
        {
            public static DomNodeType Type;
            public static AttributeInfo nameAttribute;
            public static AttributeInfo display_nameAttribute;
            public static AttributeInfo unique_nameAttribute;
            public static AttributeInfo typeAttribute;
            public static AttributeInfo valueAttribute;
            public static AttributeInfo keytypeAttribute;
            public static AttributeInfo expandedAttribute;
            public static AttributeInfo visibleAttribute;
            public static AttributeInfo levelAttribute;
            public static AttributeInfo indexAttribute;
            public static AttributeInfo function_nameAttribute;
            public static AttributeInfo function_linedefinedAttribute;
            public static ChildInfo LocationsChild;
            public static ChildInfo LocalsChild;
            public static ChildInfo TargetHierarchyChild;

        }

        public static class SledLuaVarLocalUpvalueBaseType
        {
            public static DomNodeType Type;
            public static AttributeInfo nameAttribute;
            public static AttributeInfo display_nameAttribute;
            public static AttributeInfo unique_nameAttribute;
            public static AttributeInfo typeAttribute;
            public static AttributeInfo valueAttribute;
            public static AttributeInfo keytypeAttribute;
            public static AttributeInfo expandedAttribute;
            public static AttributeInfo visibleAttribute;
            public static AttributeInfo levelAttribute;
            public static AttributeInfo indexAttribute;
            public static AttributeInfo function_nameAttribute;
            public static AttributeInfo function_linedefinedAttribute;
            public static ChildInfo LocationsChild;
            public static ChildInfo TargetHierarchyChild;
        }

        public static class SledLuaVarUpvalueListType
        {
            public static DomNodeType Type;
            public static AttributeInfo nameAttribute;
            public static ChildInfo UpvaluesChild;
        }

        public static class SledLuaVarUpvalueType
        {
            public static DomNodeType Type;
            public static AttributeInfo nameAttribute;
            public static AttributeInfo display_nameAttribute;
            public static AttributeInfo unique_nameAttribute;
            public static AttributeInfo typeAttribute;
            public static AttributeInfo valueAttribute;
            public static AttributeInfo keytypeAttribute;
            public static AttributeInfo expandedAttribute;
            public static AttributeInfo visibleAttribute;
            public static AttributeInfo levelAttribute;
            public static AttributeInfo indexAttribute;
            public static AttributeInfo function_nameAttribute;
            public static AttributeInfo function_linedefinedAttribute;
            public static ChildInfo LocationsChild;
            public static ChildInfo UpvaluesChild;
            public static ChildInfo TargetHierarchyChild;
        }

        public static class SledLuaVarWatchListType
        {
            public static DomNodeType Type;
            public static AttributeInfo nameAttribute;
            public static ChildInfo GlobalsChild;
            public static ChildInfo LocalsChild;
            public static ChildInfo UpvaluesChild;
            public static ChildInfo EnvVarsChild;
        }

        public static class SledLuaVarEnvType
        {
            public static DomNodeType Type;
            public static AttributeInfo nameAttribute;
            public static AttributeInfo display_nameAttribute;
            public static AttributeInfo unique_nameAttribute;
            public static AttributeInfo typeAttribute;
            public static AttributeInfo valueAttribute;
            public static AttributeInfo keytypeAttribute;
            public static AttributeInfo expandedAttribute;
            public static AttributeInfo visibleAttribute;
            public static AttributeInfo levelAttribute;
            public static ChildInfo LocationsChild;
            public static ChildInfo EnvVarsChild;
            public static ChildInfo TargetHierarchyChild;
        }

        public static class SledLuaVarEnvListType
        {
            public static DomNodeType Type;
            public static AttributeInfo nameAttribute;
            public static ChildInfo EnvVarsChild;
        }

        public static class SledLuaStateListType
        {
            public static DomNodeType Type;
            public static AttributeInfo nameAttribute;
            public static ChildInfo LuaStatesChild;
        }

        public static class SledLuaStateType
        {
            public static DomNodeType Type;
            public static AttributeInfo nameAttribute;
            public static AttributeInfo addressAttribute;
            public static AttributeInfo checkedAttribute;
        }

        public static class SledLuaVarFiltersType
        {
            public static DomNodeType Type;
            public static AttributeInfo nameAttribute;
            public static AttributeInfo expandedAttribute;
            public static ChildInfo GlobalsChild;
            public static ChildInfo LocalsChild;
            public static ChildInfo UpvaluesChild;
            public static ChildInfo EnvVarsChild;
        }

        public static class SledLuaVarFilterType
        {
            public static DomNodeType Type;
            public static ChildInfo LocalTypesChild;
            public static ChildInfo LocalNamesChild;
            public static ChildInfo TargetTypesChild;
            public static ChildInfo TargetNamesChild;
        }

        public static class SledLuaVarFilterTypesType
        {
            public static DomNodeType Type;
            public static AttributeInfo lua_tnilAttribute;
            public static AttributeInfo lua_tbooleanAttribute;
            public static AttributeInfo lua_tlightuserdataAttribute;
            public static AttributeInfo lua_tnumberAttribute;
            public static AttributeInfo lua_tstringAttribute;
            public static AttributeInfo lua_ttableAttribute;
            public static AttributeInfo lua_tfunctionAttribute;
            public static AttributeInfo lua_tuserdataAttribute;
            public static AttributeInfo lua_tthreadAttribute;
        }

        public static class SledLuaVarFilterNamesType
        {
            public static DomNodeType Type;
            public static ChildInfo NamesChild;
        }

        public static class SledLuaVarFilterNameType
        {
            public static DomNodeType Type;
            public static AttributeInfo nameAttribute;
        }

        public static class SledLuaFunctionType
        {
            public static DomNodeType Type;
            public static AttributeInfo nameAttribute;
            public static AttributeInfo line_definedAttribute;
            public static AttributeInfo last_line_definedAttribute;
        }

        public static class SledLuaCompileAttributeType
        {
            public static DomNodeType Type;
            public static AttributeInfo nameAttribute;
            public static AttributeInfo compileAttribute;
        }

        public static class SledLuaVarNameTypePairType
        {
            public static DomNodeType Type;
            public static AttributeInfo nameAttribute;
            public static AttributeInfo name_typeAttribute;
        }

        public static class SledLuaVarLookUpType
        {
            public static DomNodeType Type;
            public static AttributeInfo scopeAttribute;
            public static AttributeInfo stack_levelAttribute;
            public static AttributeInfo indexAttribute;
            public static AttributeInfo extraAttribute;
            public static AttributeInfo contextAttribute;
            public static ChildInfo NamesAndTypesChild;
        }

        public static class SledLuaProjectFilesWatchType
        {
            public static DomNodeType Type;
            public static AttributeInfo nameAttribute;
            public static AttributeInfo expandedAttribute;
            public static AttributeInfo language_pluginAttribute;
            public static AttributeInfo scopeAttribute;
            public static AttributeInfo contextAttribute;
            public static AttributeInfo guidAttribute;
            public static ChildInfo LookUpChild;
        }

        public static class SledLuaCompileConfigurationType
        {
            public static DomNodeType Type;
            public static AttributeInfo nameAttribute;
            public static AttributeInfo little_endianAttribute;
            public static AttributeInfo strip_debugAttribute;
            public static AttributeInfo sizeof_intAttribute;
            public static AttributeInfo sizeof_size_tAttribute;
            public static AttributeInfo sizeof_lua_NumberAttribute;
            public static AttributeInfo selectedAttribute;
            public static AttributeInfo output_pathAttribute;
            public static AttributeInfo output_extensionAttribute;
            public static AttributeInfo preserve_relative_path_infoAttribute;
        }

        public static class SledLuaCompileSettingsType
        {
            public static DomNodeType Type;
            public static AttributeInfo nameAttribute;
            public static AttributeInfo expandedAttribute;
            public static ChildInfo ConfigurationsChild;
        }

        public static ChildInfo SledProjectFilesRootElement;

        public static ChildInfo SledSyntaxErrorsRootElement;

        public static ChildInfo SledFindResults1RootElement;

        public static ChildInfo SledFindResults2RootElement;

        public static ChildInfo SledLuaProfileInfoRootElement;

        public static ChildInfo SledLuaProfileFuncCallsRootElement;

        public static ChildInfo SledLuaMemoryTraceRootElement;

        public static ChildInfo SledLuaCallStackRootElement;

        public static ChildInfo SledLuaVarGlobalsRootElement;

        public static ChildInfo SledLuaVarLocalsRootElement;

        public static ChildInfo SledLuaVarUpvaluesRootElement;

        public static ChildInfo SledLuaVarWatchListRootElement;

        public static ChildInfo SledLuaVarEnvListRootElement;

        public static ChildInfo SledLuaStatesListRootElement;

        public static ChildInfo SledLuaVarFiltersListRootElement;
    }
}
