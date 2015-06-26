/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

using Sce.Atf.Dom;

namespace Sce.Sled.Lua.Dom
{
    public class SledLuaCompileConfigurationType : DomNodeAdapter
    {
        /// <summary>
        /// Gets/sets the name attribute
        /// </summary>
        public string Name
        {
            get { return GetAttribute<string>(SledLuaSchema.SledLuaCompileConfigurationType.nameAttribute); }
            set { SetAttribute(SledLuaSchema.SledLuaCompileConfigurationType.nameAttribute, value); }
        }

        /// <summary>
        /// Gets/sets little endian attribute (true if LE false if BE)
        /// </summary>
        public bool LittleEndian
        {
            get { return GetAttribute<bool>(SledLuaSchema.SledLuaCompileConfigurationType.little_endianAttribute); }
            set { SetAttribute(SledLuaSchema.SledLuaCompileConfigurationType.little_endianAttribute, value); }
        }

        /// <summary>
        /// Gets/sets the strip debug info attribute
        /// </summary>
        public bool StripDebugInfo
        {
            get { return GetAttribute<bool>(SledLuaSchema.SledLuaCompileConfigurationType.strip_debugAttribute); }
            set { SetAttribute(SledLuaSchema.SledLuaCompileConfigurationType.strip_debugAttribute, value); }
        }

        /// <summary>
        /// Gets/sets the sizeof int attribute
        /// </summary>
        public int SizeOfInt
        {
            get { return GetAttribute<int>(SledLuaSchema.SledLuaCompileConfigurationType.sizeof_intAttribute); }
            set { SetAttribute(SledLuaSchema.SledLuaCompileConfigurationType.sizeof_intAttribute, value); }
        }

        /// <summary>
        /// Gets/sets the sizeof size_t attribute
        /// </summary>
        public int SizeOfSizeT
        {
            get { return GetAttribute<int>(SledLuaSchema.SledLuaCompileConfigurationType.sizeof_size_tAttribute); }
            set { SetAttribute(SledLuaSchema.SledLuaCompileConfigurationType.sizeof_size_tAttribute, value); }
        }

        /// <summary>
        /// Gets/sets the sizeof lua_Number attribute
        /// </summary>
        public int SizeOfLuaNumber
        {
            get { return GetAttribute<int>(SledLuaSchema.SledLuaCompileConfigurationType.sizeof_lua_NumberAttribute); }
            set { SetAttribute(SledLuaSchema.SledLuaCompileConfigurationType.sizeof_lua_NumberAttribute, value); }
        }

        /// <summary>
        /// Gets/sets the selected attribute
        /// </summary>
        public bool Selected
        {
            get { return GetAttribute<bool>(SledLuaSchema.SledLuaCompileConfigurationType.selectedAttribute); }
            set { SetAttribute(SledLuaSchema.SledLuaCompileConfigurationType.selectedAttribute, value); }
        }

        /// <summary>
        /// Gets/sets the output path attribute
        /// </summary>
        public string OutputPath
        {
            get { return GetAttribute<string>(SledLuaSchema.SledLuaCompileConfigurationType.output_pathAttribute); }
            set { SetAttribute(SledLuaSchema.SledLuaCompileConfigurationType.output_pathAttribute, value); }
        }

        /// <summary>
        /// Gets/sets the output extension attribute
        /// </summary>
        public string OutputExtension
        {
            get { return GetAttribute<string>(SledLuaSchema.SledLuaCompileConfigurationType.output_extensionAttribute); }
            set { SetAttribute(SledLuaSchema.SledLuaCompileConfigurationType.output_extensionAttribute, value); }
        }

        /// <summary>
        /// Gets/sets preserve relative path info attribute
        /// </summary>
        public bool PreserveRelativePathInfo
        {
            get { return GetAttribute<bool>(SledLuaSchema.SledLuaCompileConfigurationType.preserve_relative_path_infoAttribute); }
            set { SetAttribute(SledLuaSchema.SledLuaCompileConfigurationType.preserve_relative_path_infoAttribute, value); }
        }
    }
}
