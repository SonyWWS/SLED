/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

using Sce.Atf.Dom;

namespace Sce.Sled.Lua.Dom
{
    public class SledLuaVarGlobalListType : SledLuaVarBaseListType<SledLuaVarGlobalType>
    {
        protected override string Description
        {
            get { return "Lua Global Variables"; }
        }

        protected override string[] TheColumnNames
        {
            get { return SledLuaVarBaseType.ColumnNames; }
        }

        protected override AttributeInfo NameAttributeInfo
        {
            get { return SledLuaSchema.SledLuaVarGlobalListType.nameAttribute; }
        }

        protected override ChildInfo VariablesChildInfo
        {
            get { return SledLuaSchema.SledLuaVarGlobalListType.GlobalsChild; }
        }
    }
}
