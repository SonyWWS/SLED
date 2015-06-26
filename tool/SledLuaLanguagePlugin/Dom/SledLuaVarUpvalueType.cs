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
    public class SledLuaVarUpvalueType : SledLuaVarLocalUpvalueBaseType
    {
        #region SledVarBaseType Overrides

        public override string Name
        {
            get { return GetAttribute<string>(SledLuaSchema.SledLuaVarUpvalueType.nameAttribute); }
            set { SetAttribute(SledLuaSchema.SledLuaVarUpvalueType.nameAttribute, value); SetSortKey(); }
        }

        public override IList<SledVarLocationType> Locations
        {
            get { return GetChildList<SledVarLocationType>(SledLuaSchema.SledLuaVarUpvalueType.LocationsChild); }
        }

        #endregion

        #region SledLuaVarBaseType Overrides

        public override string DisplayName
        {
            get { return GetAttribute<string>(SledLuaSchema.SledLuaVarUpvalueType.display_nameAttribute); }
            set { SetAttribute(SledLuaSchema.SledLuaVarUpvalueType.display_nameAttribute, value); }
        }

        public override string UniqueName
        {
            get { return GetAttribute<string>(SledLuaSchema.SledLuaVarUpvalueType.unique_nameAttribute); }
            set { SetAttribute(SledLuaSchema.SledLuaVarUpvalueType.unique_nameAttribute, value); }
        }

        public override string What
        {
            get { return GetAttribute<string>(SledLuaSchema.SledLuaVarUpvalueType.typeAttribute); }
            set { SetAttribute(SledLuaSchema.SledLuaVarUpvalueType.typeAttribute, value); SetWhatSortKey(); }
        }

        public override string Value
        {
            get { return GetAttribute<string>(SledLuaSchema.SledLuaVarUpvalueType.valueAttribute); }
            set { SetAttribute(SledLuaSchema.SledLuaVarUpvalueType.valueAttribute, value); SetValueSortKey(); }
        }

        public override int KeyType
        {
            get { return GetAttribute<int>(SledLuaSchema.SledLuaVarUpvalueType.keytypeAttribute); }
            set { SetAttribute(SledLuaSchema.SledLuaVarUpvalueType.keytypeAttribute, value); }
        }

        public override bool Expanded
        {
            get { return GetAttribute<bool>(SledLuaSchema.SledLuaVarUpvalueType.expandedAttribute); }
            set { SetAttribute(SledLuaSchema.SledLuaVarUpvalueType.expandedAttribute, value); }
        }

        public override bool Visible
        {
            get { return GetAttribute<bool>(SledLuaSchema.SledLuaVarUpvalueType.visibleAttribute); }
            set { SetAttribute(SledLuaSchema.SledLuaVarUpvalueType.visibleAttribute, value); }
        }

        public override IEnumerable<ISledLuaVarBaseType> Variables
        {
            get { return Upvalues; }
        }

        public override IList<SledLuaVarNameTypePairType> TargetHierarchy
        {
            get { return GetChildList<SledLuaVarNameTypePairType>(SledLuaSchema.SledLuaVarUpvalueType.TargetHierarchyChild); }
        }

        public override void GenerateUniqueName()
        {
            var uniqueName =
                Resource.Lua + Resource.Colon +
                Resource.U + Resource.Colon +
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
            get { return SledLuaVarScopeType.Upvalue; }
        }

        #endregion

        #region SledLuaVarLocalUpvalueBaseType Overrides

        public override int Level
        {
            get { return GetAttribute<int>(SledLuaSchema.SledLuaVarUpvalueType.levelAttribute); }
            set { SetAttribute(SledLuaSchema.SledLuaVarUpvalueType.levelAttribute, value); }
        }

        public override int Index
        {
            get { return GetAttribute<int>(SledLuaSchema.SledLuaVarUpvalueType.indexAttribute); }
            set { SetAttribute(SledLuaSchema.SledLuaVarUpvalueType.indexAttribute, value); }
        }

        public override string FunctionName
        {
            get { return GetAttribute<string>(SledLuaSchema.SledLuaVarUpvalueType.function_nameAttribute); }
            set { SetAttribute(SledLuaSchema.SledLuaVarUpvalueType.function_nameAttribute, value); }
        }

        public override int FunctionLineDefined
        {
            get { return GetAttribute<int>(SledLuaSchema.SledLuaVarUpvalueType.function_linedefinedAttribute); }
            set { SetAttribute(SledLuaSchema.SledLuaVarUpvalueType.function_linedefinedAttribute, value); }
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
            var clone = (SledLuaVarUpvalueType)base.Clone();
            return clone;
        }

        #endregion

        public IList<SledLuaVarUpvalueType> Upvalues
        {
            get { return GetChildList<SledLuaVarUpvalueType>(SledLuaSchema.SledLuaVarUpvalueType.UpvaluesChild); }
        }
    }
}