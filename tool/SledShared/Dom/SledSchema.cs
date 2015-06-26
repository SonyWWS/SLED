// -------------------------------------------------------------------------------------------------------------------
// Generated code, do not edit
// Command Line:  DomGen "D:\Sony\poleary\components\wws_sled\SLED\SledShared\Resources\Schemas\SledProjectFiles.xsd" "SledSchema.cs" "sled" "Sce.Sled.Shared.Dom"
// -------------------------------------------------------------------------------------------------------------------

using Sce.Atf.Dom;

namespace Sce.Sled.Shared.Dom
{
    /// <summary>
    /// SledSchema Class
    /// </summary>
    public static class SledSchema
    {
        /// <summary>
        /// Namespace
        /// </summary>
        public const string NS = "sled";

        /// <summary>
        /// Initialize
        /// </summary>
        /// <param name="typeCollection"></param>
        public static void Initialize(XmlSchemaTypeCollection typeCollection)
        {
            SledProjectFilesType.Type = typeCollection.GetNodeType("SledProjectFilesType");
            SledProjectFilesType.nameAttribute = SledProjectFilesType.Type.GetAttributeInfo("name");
            SledProjectFilesType.expandedAttribute = SledProjectFilesType.Type.GetAttributeInfo("expanded");
            SledProjectFilesType.assetdirectoryAttribute = SledProjectFilesType.Type.GetAttributeInfo("assetdirectory");
            SledProjectFilesType.guidAttribute = SledProjectFilesType.Type.GetAttributeInfo("guid");
            SledProjectFilesType.FilesChild = SledProjectFilesType.Type.GetChildInfo("Files");
            SledProjectFilesType.FoldersChild = SledProjectFilesType.Type.GetChildInfo("Folders");
            SledProjectFilesType.LanguagesChild = SledProjectFilesType.Type.GetChildInfo("Languages");
            SledProjectFilesType.WatchesChild = SledProjectFilesType.Type.GetChildInfo("Watches");
            SledProjectFilesType.RootsChild = SledProjectFilesType.Type.GetChildInfo("Roots");
            SledProjectFilesType.UserSettingsChild = SledProjectFilesType.Type.GetChildInfo("UserSettings");

            SledProjectFilesEmptyType.Type = typeCollection.GetNodeType("SledProjectFilesEmptyType");
            SledProjectFilesEmptyType.nameAttribute = SledProjectFilesEmptyType.Type.GetAttributeInfo("name");

            SledProjectFilesFolderType.Type = typeCollection.GetNodeType("SledProjectFilesFolderType");
            SledProjectFilesFolderType.nameAttribute = SledProjectFilesFolderType.Type.GetAttributeInfo("name");
            SledProjectFilesFolderType.expandedAttribute = SledProjectFilesFolderType.Type.GetAttributeInfo("expanded");
            SledProjectFilesFolderType.FilesChild = SledProjectFilesFolderType.Type.GetChildInfo("Files");
            SledProjectFilesFolderType.FoldersChild = SledProjectFilesFolderType.Type.GetChildInfo("Folders");

            SledProjectFilesBaseType.Type = typeCollection.GetNodeType("SledProjectFilesBaseType");
            SledProjectFilesBaseType.nameAttribute = SledProjectFilesBaseType.Type.GetAttributeInfo("name");
            SledProjectFilesBaseType.expandedAttribute = SledProjectFilesBaseType.Type.GetAttributeInfo("expanded");

            SledProjectFilesFileType.Type = typeCollection.GetNodeType("SledProjectFilesFileType");
            SledProjectFilesFileType.nameAttribute = SledProjectFilesFileType.Type.GetAttributeInfo("name");
            SledProjectFilesFileType.expandedAttribute = SledProjectFilesFileType.Type.GetAttributeInfo("expanded");
            SledProjectFilesFileType.pathAttribute = SledProjectFilesFileType.Type.GetAttributeInfo("path");
            SledProjectFilesFileType.guidAttribute = SledProjectFilesFileType.Type.GetAttributeInfo("guid");
            SledProjectFilesFileType.BreakpointsChild = SledProjectFilesFileType.Type.GetChildInfo("Breakpoints");
            SledProjectFilesFileType.FunctionsChild = SledProjectFilesFileType.Type.GetChildInfo("Functions");
            SledProjectFilesFileType.AttributesChild = SledProjectFilesFileType.Type.GetChildInfo("Attributes");

            SledProjectFilesBreakpointType.Type = typeCollection.GetNodeType("SledProjectFilesBreakpointType");
            SledProjectFilesBreakpointType.lineAttribute = SledProjectFilesBreakpointType.Type.GetAttributeInfo("line");
            SledProjectFilesBreakpointType.enabledAttribute = SledProjectFilesBreakpointType.Type.GetAttributeInfo("enabled");
            SledProjectFilesBreakpointType.conditionAttribute = SledProjectFilesBreakpointType.Type.GetAttributeInfo("condition");
            SledProjectFilesBreakpointType.conditionenabledAttribute = SledProjectFilesBreakpointType.Type.GetAttributeInfo("conditionenabled");
            SledProjectFilesBreakpointType.conditionresultAttribute = SledProjectFilesBreakpointType.Type.GetAttributeInfo("conditionresult");
            SledProjectFilesBreakpointType.usefunctionenvironmentAttribute = SledProjectFilesBreakpointType.Type.GetAttributeInfo("usefunctionenvironment");

            SledFunctionBaseType.Type = typeCollection.GetNodeType("SledFunctionBaseType");
            SledFunctionBaseType.nameAttribute = SledFunctionBaseType.Type.GetAttributeInfo("name");
            SledFunctionBaseType.line_definedAttribute = SledFunctionBaseType.Type.GetAttributeInfo("line_defined");

            SledAttributeBaseType.Type = typeCollection.GetNodeType("SledAttributeBaseType");
            SledAttributeBaseType.nameAttribute = SledAttributeBaseType.Type.GetAttributeInfo("name");

            SledProjectFilesLanguageType.Type = typeCollection.GetNodeType("SledProjectFilesLanguageType");
            SledProjectFilesLanguageType.languageAttribute = SledProjectFilesLanguageType.Type.GetAttributeInfo("language");
            SledProjectFilesLanguageType.versionAttribute = SledProjectFilesLanguageType.Type.GetAttributeInfo("version");

            SledProjectFilesWatchType.Type = typeCollection.GetNodeType("SledProjectFilesWatchType");
            SledProjectFilesWatchType.nameAttribute = SledProjectFilesWatchType.Type.GetAttributeInfo("name");
            SledProjectFilesWatchType.expandedAttribute = SledProjectFilesWatchType.Type.GetAttributeInfo("expanded");
            SledProjectFilesWatchType.language_pluginAttribute = SledProjectFilesWatchType.Type.GetAttributeInfo("language_plugin");

            SledProjectFilesRootType.Type = typeCollection.GetNodeType("SledProjectFilesRootType");
            SledProjectFilesRootType.directoryAttribute = SledProjectFilesRootType.Type.GetAttributeInfo("directory");

            SledProjectFilesUserSettingsType.Type = typeCollection.GetNodeType("SledProjectFilesUserSettingsType");
            SledProjectFilesUserSettingsType.nameAttribute = SledProjectFilesUserSettingsType.Type.GetAttributeInfo("name");
            SledProjectFilesUserSettingsType.expandedAttribute = SledProjectFilesUserSettingsType.Type.GetAttributeInfo("expanded");

            SledSyntaxErrorListType.Type = typeCollection.GetNodeType("SledSyntaxErrorListType");
            SledSyntaxErrorListType.nameAttribute = SledSyntaxErrorListType.Type.GetAttributeInfo("name");
            SledSyntaxErrorListType.ErrorsChild = SledSyntaxErrorListType.Type.GetChildInfo("Errors");

            SledSyntaxErrorType.Type = typeCollection.GetNodeType("SledSyntaxErrorType");
            SledSyntaxErrorType.lineAttribute = SledSyntaxErrorType.Type.GetAttributeInfo("line");
            SledSyntaxErrorType.errorAttribute = SledSyntaxErrorType.Type.GetAttributeInfo("error");

            SledFindResultsListType.Type = typeCollection.GetNodeType("SledFindResultsListType");
            SledFindResultsListType.nameAttribute = SledFindResultsListType.Type.GetAttributeInfo("name");
            SledFindResultsListType.FindResultsChild = SledFindResultsListType.Type.GetChildInfo("FindResults");

            SledFindResultsType.Type = typeCollection.GetNodeType("SledFindResultsType");
            SledFindResultsType.nameAttribute = SledFindResultsType.Type.GetAttributeInfo("name");
            SledFindResultsType.fileAttribute = SledFindResultsType.Type.GetAttributeInfo("file");
            SledFindResultsType.lineAttribute = SledFindResultsType.Type.GetAttributeInfo("line");
            SledFindResultsType.start_offsetAttribute = SledFindResultsType.Type.GetAttributeInfo("start_offset");
            SledFindResultsType.end_offsetAttribute = SledFindResultsType.Type.GetAttributeInfo("end_offset");
            SledFindResultsType.line_textAttribute = SledFindResultsType.Type.GetAttributeInfo("line_text");

            SledProfileInfoType.Type = typeCollection.GetNodeType("SledProfileInfoType");
            SledProfileInfoType.functionAttribute = SledProfileInfoType.Type.GetAttributeInfo("function");
            SledProfileInfoType.time_totalAttribute = SledProfileInfoType.Type.GetAttributeInfo("time_total");
            SledProfileInfoType.time_avgAttribute = SledProfileInfoType.Type.GetAttributeInfo("time_avg");
            SledProfileInfoType.time_minAttribute = SledProfileInfoType.Type.GetAttributeInfo("time_min");
            SledProfileInfoType.time_maxAttribute = SledProfileInfoType.Type.GetAttributeInfo("time_max");
            SledProfileInfoType.time_total_innerAttribute = SledProfileInfoType.Type.GetAttributeInfo("time_total_inner");
            SledProfileInfoType.time_avg_innerAttribute = SledProfileInfoType.Type.GetAttributeInfo("time_avg_inner");
            SledProfileInfoType.time_min_innerAttribute = SledProfileInfoType.Type.GetAttributeInfo("time_min_inner");
            SledProfileInfoType.time_max_innerAttribute = SledProfileInfoType.Type.GetAttributeInfo("time_max_inner");
            SledProfileInfoType.num_callsAttribute = SledProfileInfoType.Type.GetAttributeInfo("num_calls");
            SledProfileInfoType.lineAttribute = SledProfileInfoType.Type.GetAttributeInfo("line");
            SledProfileInfoType.fileAttribute = SledProfileInfoType.Type.GetAttributeInfo("file");
            SledProfileInfoType.num_funcs_calledAttribute = SledProfileInfoType.Type.GetAttributeInfo("num_funcs_called");
            SledProfileInfoType.ProfileInfoChild = SledProfileInfoType.Type.GetChildInfo("ProfileInfo");

            SledProfileInfoListType.Type = typeCollection.GetNodeType("SledProfileInfoListType");
            SledProfileInfoListType.nameAttribute = SledProfileInfoListType.Type.GetAttributeInfo("name");
            SledProfileInfoListType.ProfileInfoChild = SledProfileInfoListType.Type.GetChildInfo("ProfileInfo");

            SledMemoryTraceType.Type = typeCollection.GetNodeType("SledMemoryTraceType");
            SledMemoryTraceType.orderAttribute = SledMemoryTraceType.Type.GetAttributeInfo("order");
            SledMemoryTraceType.whatAttribute = SledMemoryTraceType.Type.GetAttributeInfo("what");
            SledMemoryTraceType.oldaddressAttribute = SledMemoryTraceType.Type.GetAttributeInfo("oldaddress");
            SledMemoryTraceType.newaddressAttribute = SledMemoryTraceType.Type.GetAttributeInfo("newaddress");
            SledMemoryTraceType.oldsizeAttribute = SledMemoryTraceType.Type.GetAttributeInfo("oldsize");
            SledMemoryTraceType.newsizeAttribute = SledMemoryTraceType.Type.GetAttributeInfo("newsize");

            SledMemoryTraceListType.Type = typeCollection.GetNodeType("SledMemoryTraceListType");
            SledMemoryTraceListType.nameAttribute = SledMemoryTraceListType.Type.GetAttributeInfo("name");
            SledMemoryTraceListType.MemoryTraceChild = SledMemoryTraceListType.Type.GetChildInfo("MemoryTrace");

            SledCallStackType.Type = typeCollection.GetNodeType("SledCallStackType");
            SledCallStackType.functionAttribute = SledCallStackType.Type.GetAttributeInfo("function");
            SledCallStackType.fileAttribute = SledCallStackType.Type.GetAttributeInfo("file");
            SledCallStackType.currentlineAttribute = SledCallStackType.Type.GetAttributeInfo("currentline");
            SledCallStackType.linedefinedAttribute = SledCallStackType.Type.GetAttributeInfo("linedefined");
            SledCallStackType.lineendAttribute = SledCallStackType.Type.GetAttributeInfo("lineend");
            SledCallStackType.levelAttribute = SledCallStackType.Type.GetAttributeInfo("level");

            SledCallStackListType.Type = typeCollection.GetNodeType("SledCallStackListType");
            SledCallStackListType.nameAttribute = SledCallStackListType.Type.GetAttributeInfo("name");
            SledCallStackListType.CallStackChild = SledCallStackListType.Type.GetChildInfo("CallStack");

            SledVarLocationType.Type = typeCollection.GetNodeType("SledVarLocationType");
            SledVarLocationType.fileAttribute = SledVarLocationType.Type.GetAttributeInfo("file");
            SledVarLocationType.lineAttribute = SledVarLocationType.Type.GetAttributeInfo("line");
            SledVarLocationType.occurenceAttribute = SledVarLocationType.Type.GetAttributeInfo("occurence");

            SledVarBaseWatchListType.Type = typeCollection.GetNodeType("SledVarBaseWatchListType");
            SledVarBaseWatchListType.nameAttribute = SledVarBaseWatchListType.Type.GetAttributeInfo("name");

            SledVarBaseType.Type = typeCollection.GetNodeType("SledVarBaseType");
            SledVarBaseType.nameAttribute = SledVarBaseType.Type.GetAttributeInfo("name");
            SledVarBaseType.unique_nameAttribute = SledVarBaseType.Type.GetAttributeInfo("unique_name");
            SledVarBaseType.LocationsChild = SledVarBaseType.Type.GetChildInfo("Locations");

            SledProjectFilesRootElement = typeCollection.GetRootElement("SledProjectFiles");
            SledProjectFilesEmptyRootElement = typeCollection.GetRootElement("SledProjectEmpty");
            SledSyntaxErrorsRootElement = typeCollection.GetRootElement("SledSyntaxErrors");
            SledFindResults1RootElement = typeCollection.GetRootElement("SledFindResults1");
            SledFindResults2RootElement = typeCollection.GetRootElement("SledFindResults2");
        }

#pragma warning disable 1591
        public static class SledProjectFilesType
        {
            public static DomNodeType Type;
            public static AttributeInfo nameAttribute;
            public static AttributeInfo expandedAttribute;
            public static AttributeInfo assetdirectoryAttribute;
            public static AttributeInfo guidAttribute;
            public static ChildInfo FilesChild;
            public static ChildInfo FoldersChild;
            public static ChildInfo LanguagesChild;
            public static ChildInfo WatchesChild;
            public static ChildInfo RootsChild;
            public static ChildInfo UserSettingsChild;
        }

        public static class SledProjectFilesEmptyType
        {
            public static DomNodeType Type;
            public static AttributeInfo nameAttribute;
        }

        public static class SledProjectFilesFolderType
        {
            public static DomNodeType Type;
            public static AttributeInfo nameAttribute;
            public static AttributeInfo expandedAttribute;
            public static ChildInfo FilesChild;
            public static ChildInfo FoldersChild;
        }

        public static class SledProjectFilesBaseType
        {
            public static DomNodeType Type;
            public static AttributeInfo nameAttribute;
            public static AttributeInfo expandedAttribute;
        }

        public static class SledProjectFilesFileType
        {
            public static DomNodeType Type;
            public static AttributeInfo nameAttribute;
            public static AttributeInfo expandedAttribute;
            public static AttributeInfo pathAttribute;
            public static AttributeInfo guidAttribute;
            public static ChildInfo BreakpointsChild;
            public static ChildInfo FunctionsChild;
            public static ChildInfo AttributesChild;
        }

        public static class SledProjectFilesBreakpointType
        {
            public static DomNodeType Type;
            public static AttributeInfo lineAttribute;
            public static AttributeInfo enabledAttribute;
            public static AttributeInfo conditionAttribute;
            public static AttributeInfo conditionenabledAttribute;
            public static AttributeInfo conditionresultAttribute;
            public static AttributeInfo usefunctionenvironmentAttribute;
        }

        public static class SledFunctionBaseType
        {
            public static DomNodeType Type;
            public static AttributeInfo nameAttribute;
            public static AttributeInfo line_definedAttribute;
        }

        public static class SledAttributeBaseType
        {
            public static DomNodeType Type;
            public static AttributeInfo nameAttribute;
        }

        public static class SledProjectFilesLanguageType
        {
            public static DomNodeType Type;
            public static AttributeInfo languageAttribute;
            public static AttributeInfo versionAttribute;
        }

        public static class SledProjectFilesWatchType
        {
            public static DomNodeType Type;
            public static AttributeInfo nameAttribute;
            public static AttributeInfo expandedAttribute;
            public static AttributeInfo language_pluginAttribute;
        }

        public static class SledProjectFilesRootType
        {
            public static DomNodeType Type;
            public static AttributeInfo directoryAttribute;
        }

        public static class SledProjectFilesUserSettingsType
        {
            public static DomNodeType Type;
            public static AttributeInfo nameAttribute;
            public static AttributeInfo expandedAttribute;
        }

        public static class SledSyntaxErrorListType
        {
            public static DomNodeType Type;
            public static AttributeInfo nameAttribute;
            public static ChildInfo ErrorsChild;
        }

        public static class SledSyntaxErrorType
        {
            public static DomNodeType Type;
            public static AttributeInfo lineAttribute;
            public static AttributeInfo errorAttribute;
        }

        public static class SledFindResultsListType
        {
            public static DomNodeType Type;
            public static AttributeInfo nameAttribute;
            public static ChildInfo FindResultsChild;
        }

        public static class SledFindResultsType
        {
            public static DomNodeType Type;
            public static AttributeInfo nameAttribute;
            public static AttributeInfo fileAttribute;
            public static AttributeInfo lineAttribute;
            public static AttributeInfo start_offsetAttribute;
            public static AttributeInfo end_offsetAttribute;
            public static AttributeInfo line_textAttribute;
        }

        public static class SledProfileInfoType
        {
            public static DomNodeType Type;
            public static AttributeInfo functionAttribute;
            public static AttributeInfo time_totalAttribute;
            public static AttributeInfo time_avgAttribute;
            public static AttributeInfo time_minAttribute;
            public static AttributeInfo time_maxAttribute;
            public static AttributeInfo time_total_innerAttribute;
            public static AttributeInfo time_avg_innerAttribute;
            public static AttributeInfo time_min_innerAttribute;
            public static AttributeInfo time_max_innerAttribute;
            public static AttributeInfo num_callsAttribute;
            public static AttributeInfo lineAttribute;
            public static AttributeInfo fileAttribute;
            public static AttributeInfo num_funcs_calledAttribute;
            public static ChildInfo ProfileInfoChild;
        }

        public static class SledProfileInfoListType
        {
            public static DomNodeType Type;
            public static AttributeInfo nameAttribute;
            public static ChildInfo ProfileInfoChild;
        }

        public static class SledMemoryTraceType
        {
            public static DomNodeType Type;
            public static AttributeInfo orderAttribute;
            public static AttributeInfo whatAttribute;
            public static AttributeInfo oldaddressAttribute;
            public static AttributeInfo newaddressAttribute;
            public static AttributeInfo oldsizeAttribute;
            public static AttributeInfo newsizeAttribute;
        }

        public static class SledMemoryTraceListType
        {
            public static DomNodeType Type;
            public static AttributeInfo nameAttribute;
            public static ChildInfo MemoryTraceChild;
        }

        public static class SledCallStackType
        {
            public static DomNodeType Type;
            public static AttributeInfo functionAttribute;
            public static AttributeInfo fileAttribute;
            public static AttributeInfo currentlineAttribute;
            public static AttributeInfo linedefinedAttribute;
            public static AttributeInfo lineendAttribute;
            public static AttributeInfo levelAttribute;
        }

        public static class SledCallStackListType
        {
            public static DomNodeType Type;
            public static AttributeInfo nameAttribute;
            public static ChildInfo CallStackChild;
        }

        public static class SledVarLocationType
        {
            public static DomNodeType Type;
            public static AttributeInfo fileAttribute;
            public static AttributeInfo lineAttribute;
            public static AttributeInfo occurenceAttribute;
        }

        public static class SledVarBaseWatchListType
        {
            public static DomNodeType Type;
            public static AttributeInfo nameAttribute;
        }

        public static class SledVarBaseType
        {
            public static DomNodeType Type;
            public static AttributeInfo nameAttribute;
            public static AttributeInfo unique_nameAttribute;
            public static ChildInfo LocationsChild;
        }

        public static ChildInfo SledProjectFilesRootElement;

        public static ChildInfo SledProjectFilesEmptyRootElement;

        public static ChildInfo SledSyntaxErrorsRootElement;

        public static ChildInfo SledFindResults1RootElement;

        public static ChildInfo SledFindResults2RootElement;
#pragma warning restore 1591
    }
}
