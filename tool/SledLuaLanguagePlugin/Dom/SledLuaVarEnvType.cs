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
    public class SledLuaVarEnvType : SledLuaVarBaseType
    {
        #region SledVarBaseType Overrides

        public override string Name
        {
            get { return GetAttribute<string>(SledLuaSchema.SledLuaVarEnvType.nameAttribute); }
            set { SetAttribute(SledLuaSchema.SledLuaVarEnvType.nameAttribute, value); SetSortKey(); }
        }

        public override IList<SledVarLocationType> Locations
        {
            get { return GetChildList<SledVarLocationType>(SledLuaSchema.SledLuaVarEnvType.LocationsChild); }
        }

        #endregion

        #region SledLuaVarBaseType Overrides

        public override string DisplayName
        {
            get { return GetAttribute<string>(SledLuaSchema.SledLuaVarEnvType.display_nameAttribute); }
            set { SetAttribute(SledLuaSchema.SledLuaVarEnvType.display_nameAttribute, value); }
        }

        public override string UniqueName
        {
            get { return GetAttribute<string>(SledLuaSchema.SledLuaVarEnvType.unique_nameAttribute); }
            set { SetAttribute(SledLuaSchema.SledLuaVarEnvType.unique_nameAttribute, value); }
        }

        public override string What
        {
            get { return GetAttribute<string>(SledLuaSchema.SledLuaVarEnvType.typeAttribute); }
            set { SetAttribute(SledLuaSchema.SledLuaVarEnvType.typeAttribute, value); SetWhatSortKey(); }
        }

        public override string Value
        {
            get { return GetAttribute<string>(SledLuaSchema.SledLuaVarEnvType.valueAttribute); }
            set { SetAttribute(SledLuaSchema.SledLuaVarEnvType.valueAttribute, value); SetValueSortKey(); }
        }

        public override int KeyType
        {
            get { return GetAttribute<int>(SledLuaSchema.SledLuaVarEnvType.keytypeAttribute); }
            set { SetAttribute(SledLuaSchema.SledLuaVarEnvType.keytypeAttribute, value); }
        }

        public override bool Expanded
        {
            get { return GetAttribute<bool>(SledLuaSchema.SledLuaVarEnvType.expandedAttribute); }
            set { SetAttribute(SledLuaSchema.SledLuaVarEnvType.expandedAttribute, value); }
        }

        public override bool Visible
        {
            get { return GetAttribute<bool>(SledLuaSchema.SledLuaVarEnvType.visibleAttribute); }
            set { SetAttribute(SledLuaSchema.SledLuaVarEnvType.visibleAttribute, value); }
        }

        public override IEnumerable<ISledLuaVarBaseType> Variables
        {
            get { return EnvVars; }
        }

        public override IList<SledLuaVarNameTypePairType> TargetHierarchy
        {
            get { return GetChildList<SledLuaVarNameTypePairType>(SledLuaSchema.SledLuaVarEnvType.TargetHierarchyChild); }
        }

        public override void GenerateUniqueName()
        {
            var uniqueName =
                Resource.Lua + Resource.Colon +
                Resource.E + Resource.Colon +
                Name + Resource.Colon +
                What + Resource.Colon +
                KeyType;

            UniqueName = uniqueName;
        }

        public override SledLuaVarScopeType Scope
        {
            get { return SledLuaVarScopeType.Environment; }
        }

        #endregion

        #region IITemView Interface

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
            var clone = (SledLuaVarEnvType)base.Clone();
            return clone;
        }

        #endregion

        public int Level
        {
            get { return GetAttribute<int>(SledLuaSchema.SledLuaVarEnvType.levelAttribute); }
            set { SetAttribute(SledLuaSchema.SledLuaVarEnvType.levelAttribute, value); }
        }

        public IList<SledLuaVarEnvType> EnvVars
        {
            get { return GetChildList<SledLuaVarEnvType>(SledLuaSchema.SledLuaVarEnvType.EnvVarsChild); }
        }
    }
}
