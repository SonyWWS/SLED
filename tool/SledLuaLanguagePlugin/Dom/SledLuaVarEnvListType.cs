/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

using Sce.Atf.Dom;

namespace Sce.Sled.Lua.Dom
{
    public class SledLuaVarEnvListType : SledLuaVarBaseListType<SledLuaVarEnvType>
    {
        protected override string Description
        {
            get { return "Lua Environment Variables"; }
        }

        protected override string[] TheColumnNames
        {
            get { return SledLuaVarBaseType.ColumnNames; }
        }

        protected override AttributeInfo NameAttributeInfo
        {
            get { return SledLuaSchema.SledLuaVarEnvListType.nameAttribute; }
        }

        protected override ChildInfo VariablesChildInfo
        {
            get { return SledLuaSchema.SledLuaVarEnvListType.EnvVarsChild; }
        }
    }
}
