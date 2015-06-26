/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

using Sce.Sled.Shared.Dom;

namespace Sce.Sled.Lua.Dom
{
    public class SledLuaCompileAttributeType : SledAttributeBaseType
    {
        /// <summary>
        /// Gets/sets the name attribute
        /// </summary>
        public override string Name
        {
            get { return GetAttribute<string>(SledLuaSchema.SledLuaCompileAttributeType.nameAttribute); }
            set { SetAttribute(SledLuaSchema.SledLuaCompileAttributeType.nameAttribute, value); }
        }

        /// <summary>
        /// Gets/sets the compile attribute
        /// </summary>
        public bool Compile
        {
            get { return GetAttribute<bool>(SledLuaSchema.SledLuaCompileAttributeType.compileAttribute); }
            set { SetAttribute(SledLuaSchema.SledLuaCompileAttributeType.compileAttribute, value); }
        }
    }
}
