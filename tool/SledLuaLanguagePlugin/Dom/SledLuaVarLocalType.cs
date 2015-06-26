/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

using System.Collections.Generic;

using Sce.Atf.Applications;

using Sce.Sled.Lua.Resources;
using Sce.Sled.Shared.Dom;

namespace Sce.Sled.Lua.Dom
{
    public class SledLuaVarLocalType : SledLuaVarLocalUpvalueBaseType
    {
        #region SledVarBaseType Overrides

        public override string Name
        {
            get { return GetAttribute<string>(SledLuaSchema.SledLuaVarLocalType.nameAttribute); }
            set { SetAttribute(SledLuaSchema.SledLuaVarLocalType.nameAttribute, value); SetSortKey(); }
        }

        public override IList<SledVarLocationType> Locations
        {
            get { return GetChildList<SledVarLocationType>(SledLuaSchema.SledLuaVarLocalType.LocationsChild); }
        }

        #endregion

        #region SledLuaVarBaseType Overrides

        public override string DisplayName
        {
            get { return GetAttribute<string>(SledLuaSchema.SledLuaVarLocalType.display_nameAttribute); }
            set { SetAttribute(SledLuaSchema.SledLuaVarLocalType.display_nameAttribute, value); }
        }

        public override string UniqueName
        {
            get { return GetAttribute<string>(SledLuaSchema.SledLuaVarLocalType.unique_nameAttribute); }
            set { SetAttribute(SledLuaSchema.SledLuaVarLocalType.unique_nameAttribute, value); }
        }

        public override string What
        {
            get { return GetAttribute<string>(SledLuaSchema.SledLuaVarLocalType.typeAttribute); }
            set { SetAttribute(SledLuaSchema.SledLuaVarLocalType.typeAttribute, value); SetWhatSortKey(); }
        }

        public override string Value
        {
            get { return GetAttribute<string>(SledLuaSchema.SledLuaVarLocalType.valueAttribute); }
            set { SetAttribute(SledLuaSchema.SledLuaVarLocalType.valueAttribute, value); SetValueSortKey(); }
        }

        public override int KeyType
        {
            get { return GetAttribute<int>(SledLuaSchema.SledLuaVarLocalType.keytypeAttribute); }
            set { SetAttribute(SledLuaSchema.SledLuaVarLocalType.keytypeAttribute, value); }
        }

        public override bool Expanded
        {
            get { return GetAttribute<bool>(SledLuaSchema.SledLuaVarLocalType.expandedAttribute); }
            set { SetAttribute(SledLuaSchema.SledLuaVarLocalType.expandedAttribute, value); }
        }

        public override bool Visible
        {
            get { return GetAttribute<bool>(SledLuaSchema.SledLuaVarLocalType.visibleAttribute); }
            set { SetAttribute(SledLuaSchema.SledLuaVarLocalType.visibleAttribute, value); }
        }

        public override IEnumerable<ISledLuaVarBaseType> Variables
        {
            get { return Locals; }
        }

        public override IList<SledLuaVarNameTypePairType> TargetHierarchy
        {
            get { return GetChildList<SledLuaVarNameTypePairType>(SledLuaSchema.SledLuaVarLocalType.TargetHierarchyChild); }
        }

        public override void GenerateUniqueName()
        {
            var uniqueName =
                Resource.Lua + Resource.Colon +
                Resource.L + Resource.Colon +
                Name + Resource.Colon +
                FunctionName + Resource.Colon +
                FunctionLineDefined + Resource.Colon +
                What + Resource.Colon +
                Index + Resource.Colon +
                KeyType;

            UniqueName = uniqueName;
        }

        public override SledLuaVarScopeType Scope
        {
            get { return SledLuaVarScopeType.Local; }
        }

        #endregion

        #region SledLuaVarLocalUpvalueBaseType Overrides

        public override int Level
        {
            get { return GetAttribute<int>(SledLuaSchema.SledLuaVarLocalType.levelAttribute); }
            set { SetAttribute(SledLuaSchema.SledLuaVarLocalType.levelAttribute, value); }
        }

        public override int Index
        {
            get { return GetAttribute<int>(SledLuaSchema.SledLuaVarLocalType.indexAttribute); }
            set { SetAttribute(SledLuaSchema.SledLuaVarLocalType.indexAttribute, value); }
        }

        public override string FunctionName
        {
            get { return GetAttribute<string>(SledLuaSchema.SledLuaVarLocalType.function_nameAttribute); }
            set { SetAttribute(SledLuaSchema.SledLuaVarLocalType.function_nameAttribute, value); }
        }

        public override int FunctionLineDefined
        {
            get { return GetAttribute<int>(SledLuaSchema.SledLuaVarLocalType.function_linedefinedAttribute); }
            set { SetAttribute(SledLuaSchema.SledLuaVarLocalType.function_linedefinedAttribute, value); }
        }

        #endregion

        #region IItemView Interface

        public override void GetInfo(object item, ItemInfo info)
        {
            info.Label = DisplayName;
            info.Properties = new[] { What, Value, SledLuaVarScopeTypeString.ToString(Scope) };
            info.IsLeaf = (LuaType != LuaType.LUA_TTABLE);
            info.ImageIndex = info.GetImageIndex(Atf.Resources.DataImage);
            info.Description =
                Name + Resource.Space + Resource.Colon + Resource.Space +
                What + Resource.Space + Resource.Colon + Resource.Space +
                Value;
            info.IsExpandedInView = Expanded;
        }

        #endregion

        #region ICloneable Interface

        public override object Clone()
        {
            var clone = (SledLuaVarLocalType)base.Clone();
            return clone;
        }

        #endregion

        public IList<SledLuaVarLocalType> Locals
        {
            get { return GetChildList<SledLuaVarLocalType>(SledLuaSchema.SledLuaVarLocalType.LocalsChild); }
        }
    }
}