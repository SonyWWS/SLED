/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

using Sce.Atf.Dom;

namespace Sce.Sled.Lua.Dom
{
    public class SledLuaVarLocalListType : SledLuaVarBaseListType<SledLuaVarLocalType>
    {
        protected override string Description
        {
            get { return "Lua Local Variables"; }
        }

        protected override string[] TheColumnNames
        {
            get { return SledLuaVarBaseType.ColumnNames; }
        }

        protected override AttributeInfo NameAttributeInfo
        {
            get { return SledLuaSchema.SledLuaVarLocalListType.nameAttribute; }
        }

        protected override ChildInfo VariablesChildInfo
        {
            get { return SledLuaSchema.SledLuaVarLocalListType.LocalsChild; }
        }
    }
}
