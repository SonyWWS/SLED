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
    public class SledLuaVarGlobalType : SledLuaVarBaseType
    {
        #region SledVarBaseType Overrides

        public override string Name
        {
            get { return GetAttribute<string>(SledLuaSchema.SledLuaVarGlobalType.nameAttribute); }
            set { SetAttribute(SledLuaSchema.SledLuaVarGlobalType.nameAttribute, value); SetSortKey(); }
        }

        public override IList<SledVarLocationType> Locations
        {
            get { return GetChildList<SledVarLocationType>(SledLuaSchema.SledLuaVarGlobalType.LocationsChild); }
        }

        #endregion

        #region SledLuaVarBaseType Overrides

        public override string DisplayName
        {
            get { return GetAttribute<string>(SledLuaSchema.SledLuaVarGlobalType.display_nameAttribute); }
            set { SetAttribute(SledLuaSchema.SledLuaVarGlobalType.display_nameAttribute, value); }
        }

        public override string UniqueName
        {
            get { return GetAttribute<string>(SledLuaSchema.SledLuaVarGlobalType.unique_nameAttribute); }
            set { SetAttribute(SledLuaSchema.SledLuaVarGlobalType.unique_nameAttribute, value); }
        }

        public override string What
        {
            get { return GetAttribute<string>(SledLuaSchema.SledLuaVarGlobalType.typeAttribute); }
            set { SetAttribute(SledLuaSchema.SledLuaVarGlobalType.typeAttribute, value); SetWhatSortKey(); }
        }

        public override string Value
        {
            get { return GetAttribute<string>(SledLuaSchema.SledLuaVarGlobalType.valueAttribute); }
            set { SetAttribute(SledLuaSchema.SledLuaVarGlobalType.valueAttribute, value); SetValueSortKey(); }
        }

        public override int KeyType
        {
            get { return GetAttribute<int>(SledLuaSchema.SledLuaVarGlobalType.keytypeAttribute); }
            set { SetAttribute(SledLuaSchema.SledLuaVarGlobalType.keytypeAttribute, value); }
        }

        public override bool Expanded
        {
            get { return GetAttribute<bool>(SledLuaSchema.SledLuaVarGlobalType.expandedAttribute); }
            set { SetAttribute(SledLuaSchema.SledLuaVarGlobalType.expandedAttribute, value); }
        }

        public override bool Visible
        {
            get { return GetAttribute<bool>(SledLuaSchema.SledLuaVarGlobalType.visibleAttribute); }
            set { SetAttribute(SledLuaSchema.SledLuaVarGlobalType.visibleAttribute, value); }
        }

        public override IEnumerable<ISledLuaVarBaseType> Variables
        {
            get { return Globals; }
        }

        public override IList<SledLuaVarNameTypePairType> TargetHierarchy
        {
            get { return GetChildList<SledLuaVarNameTypePairType>(SledLuaSchema.SledLuaVarGlobalType.TargetHierarchyChild); }
        }

        public override void GenerateUniqueName()
        {
            var uniqueName =
                Resource.Lua + Resource.Colon +
                Resource.G + Resource.Colon +
                Name + Resource.Colon +
                What + Resource.Colon +
                KeyType;

            UniqueName = uniqueName;
        }

        public override SledLuaVarScopeType Scope
        {
            get { return SledLuaVarScopeType.Global; }
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
            var clone = (SledLuaVarGlobalType)base.Clone();
            return clone;
        }

        #endregion

        public IList<SledLuaVarGlobalType> Globals
        {
            get { return GetChildList<SledLuaVarGlobalType>(SledLuaSchema.SledLuaVarGlobalType.GlobalsChild); }
        }
    }
}
