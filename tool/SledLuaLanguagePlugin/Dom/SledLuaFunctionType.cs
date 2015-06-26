/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

using Sce.Sled.Shared.Dom;

namespace Sce.Sled.Lua.Dom
{
    public class SledLuaFunctionType : SledFunctionBaseType
    {
        public override string Name
        {
            get { return GetAttribute<string>(SledLuaSchema.SledLuaFunctionType.nameAttribute); }
            set { SetAttribute(SledLuaSchema.SledLuaFunctionType.nameAttribute, value); }
        }

        /// <summary>
        /// Gets/sets the line defined attribute
        /// </summary>
        public override int LineDefined
        {
            get { return GetAttribute<int>(SledLuaSchema.SledLuaFunctionType.line_definedAttribute); }
            set { SetAttribute(SledLuaSchema.SledLuaFunctionType.line_definedAttribute, value); }
        }

        /// <summary>
        /// Gets/sets the last line defined attribute
        /// </summary>
        public int LastLineDefined
        {
            get { return GetAttribute<int>(SledLuaSchema.SledLuaFunctionType.last_line_definedAttribute); }
            set { SetAttribute(SledLuaSchema.SledLuaFunctionType.last_line_definedAttribute, value); }
        }
    }
}
