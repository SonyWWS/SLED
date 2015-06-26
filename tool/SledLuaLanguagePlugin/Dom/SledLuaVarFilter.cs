/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

using System.Collections.Generic;

using Sce.Atf.Adaptation;
using Sce.Atf.Dom;

using Sce.Sled.Shared.Dom;

namespace Sce.Sled.Lua.Dom
{
    public class SledLuaVarFilterTypesType : DomNodeAdapter
    {
        /// <summary>
        /// Gets/sets enabled status for specific LUA_T&lt;type&gt;
        /// </summary>
        public bool LUA_TNIL
        {
            get { return GetAttribute<bool>(SledLuaSchema.SledLuaVarFilterTypesType.lua_tnilAttribute); }
            set { SetAttribute(SledLuaSchema.SledLuaVarFilterTypesType.lua_tnilAttribute, value); }
        }

        /// <summary>
        /// Gets/sets enabled status for specific LUA_T&lt;type&gt;
        /// </summary>
        public bool LUA_TBOOLEAN
        {
            get { return GetAttribute<bool>(SledLuaSchema.SledLuaVarFilterTypesType.lua_tbooleanAttribute); }
            set { SetAttribute(SledLuaSchema.SledLuaVarFilterTypesType.lua_tbooleanAttribute, value); }
        }

        /// <summary>
        /// Gets/sets enabled status for specific LUA_T&lt;type&gt;
        /// </summary>
        public bool LUA_TLIGHTUSERDATA
        {
            get { return GetAttribute<bool>(SledLuaSchema.SledLuaVarFilterTypesType.lua_tlightuserdataAttribute); }
            set { SetAttribute(SledLuaSchema.SledLuaVarFilterTypesType.lua_tlightuserdataAttribute, value); }
        }

        /// <summary>
        /// Gets/sets enabled status for specific LUA_T&lt;type&gt;
        /// </summary>
        public bool LUA_TNUMBER
        {
            get { return GetAttribute<bool>(SledLuaSchema.SledLuaVarFilterTypesType.lua_tnumberAttribute); }
            set { SetAttribute(SledLuaSchema.SledLuaVarFilterTypesType.lua_tnumberAttribute, value); }
        }

        /// <summary>
        /// Gets/sets enabled status for specific LUA_T&lt;type&gt;
        /// </summary>
        public bool LUA_TSTRING
        {
            get { return GetAttribute<bool>(SledLuaSchema.SledLuaVarFilterTypesType.lua_tstringAttribute); }
            set { SetAttribute(SledLuaSchema.SledLuaVarFilterTypesType.lua_tstringAttribute, value); }
        }

        /// <summary>
        /// Gets/sets enabled status for specific LUA_T&lt;type&gt;
        /// </summary>
        public bool LUA_TTABLE
        {
            get { return GetAttribute<bool>(SledLuaSchema.SledLuaVarFilterTypesType.lua_ttableAttribute); }
            set { SetAttribute(SledLuaSchema.SledLuaVarFilterTypesType.lua_ttableAttribute, value); }
        }

        /// <summary>
        /// Gets/sets enabled status for specific LUA_T&lt;type&gt;
        /// </summary>
        public bool LUA_TFUNCTION
        {
            get { return GetAttribute<bool>(SledLuaSchema.SledLuaVarFilterTypesType.lua_tfunctionAttribute); }
            set { SetAttribute(SledLuaSchema.SledLuaVarFilterTypesType.lua_tfunctionAttribute, value); }
        }

        /// <summary>
        /// Gets/sets enabled status for specific LUA_T&lt;type&gt;
        /// </summary>
        public bool LUA_TUSERDATA
        {
            get { return GetAttribute<bool>(SledLuaSchema.SledLuaVarFilterTypesType.lua_tuserdataAttribute); }
            set { SetAttribute(SledLuaSchema.SledLuaVarFilterTypesType.lua_tuserdataAttribute, value); }
        }

        /// <summary>
        /// Gets/sets enabled status for specific LUA_T&lt;type&gt;
        /// </summary>
        public bool LUA_TTHREAD
        {
            get { return GetAttribute<bool>(SledLuaSchema.SledLuaVarFilterTypesType.lua_tthreadAttribute); }
            set { SetAttribute(SledLuaSchema.SledLuaVarFilterTypesType.lua_tthreadAttribute, value); }
        }

        public void SetAll(bool bValue)
        {
            LUA_TNIL = bValue;
            LUA_TBOOLEAN = bValue;
            LUA_TLIGHTUSERDATA = bValue;
            LUA_TNUMBER = bValue;
            LUA_TSTRING = bValue;
            LUA_TTABLE = bValue;
            LUA_TFUNCTION = bValue;
            LUA_TUSERDATA = bValue;
            LUA_TTHREAD = bValue;
        }
    }

    public class SledLuaVarFilterNameType : DomNodeAdapter
    {
        /// <summary>
        /// Gets/sets name attribute
        /// </summary>
        public string Name
        {
            get { return GetAttribute<string>(SledLuaSchema.SledLuaVarFilterNameType.nameAttribute); }
            set { SetAttribute(SledLuaSchema.SledLuaVarFilterNameType.nameAttribute, value); }
        }
    }

    public class SledLuaVarFilterNamesType : DomNodeAdapter
    {
        /// <summary>
        /// Gets names filters
        /// </summary>
        public IList<SledLuaVarFilterNameType> Names
        {
            get { return GetChildList<SledLuaVarFilterNameType>(SledLuaSchema.SledLuaVarFilterNamesType.NamesChild); }
        }
    }

    public class SledLuaVarFilterType : DomNodeAdapter
    {
        public SledLuaVarFilterTypesType LocalTypes
        {
            get { return GetChild<SledLuaVarFilterTypesType>(SledLuaSchema.SledLuaVarFilterType.LocalTypesChild); }
        }

        public SledLuaVarFilterNamesType LocalNames
        {
            get { return GetChild<SledLuaVarFilterNamesType>(SledLuaSchema.SledLuaVarFilterType.LocalNamesChild); }
        }

        public SledLuaVarFilterTypesType TargetTypes
        {
            get { return GetChild<SledLuaVarFilterTypesType>(SledLuaSchema.SledLuaVarFilterType.TargetTypesChild); }
        }

        public SledLuaVarFilterNamesType TargetNames
        {
            get { return GetChild<SledLuaVarFilterNamesType>(SledLuaSchema.SledLuaVarFilterType.TargetNamesChild); }
        }

        protected override void OnNodeSet()
        {
            if (LocalTypes == null)
            {
                var domNode =
                    new DomNode(SledLuaSchema.SledLuaVarFilterTypesType.Type);

                var localTypes =
                    domNode.As<SledLuaVarFilterTypesType>();

                SetChild(SledLuaSchema.SledLuaVarFilterType.LocalTypesChild, localTypes);

                if (LocalTypes != null)
                    LocalTypes.SetAll(false);
            }

            if (LocalNames == null)
            {
                var domNode =
                    new DomNode(SledLuaSchema.SledLuaVarFilterNamesType.Type);

                var localNames =
                    domNode.As<SledLuaVarFilterNamesType>();

                SetChild(SledLuaSchema.SledLuaVarFilterType.LocalNamesChild, localNames);
            }

            if (TargetTypes == null)
            {
                var domNode =
                    new DomNode(SledLuaSchema.SledLuaVarFilterTypesType.Type);

                var targetTypes =
                    domNode.As<SledLuaVarFilterTypesType>();

                SetChild(SledLuaSchema.SledLuaVarFilterType.TargetTypesChild, targetTypes);

                if (TargetTypes != null)
                    TargetTypes.SetAll(false);
            }

            if (TargetNames == null)
            {
                var domNode =
                    new DomNode(SledLuaSchema.SledLuaVarFilterNamesType.Type);

                var targetNames =
                    domNode.As<SledLuaVarFilterNamesType>();

                SetChild(SledLuaSchema.SledLuaVarFilterType.TargetNamesChild, targetNames);
            }

            base.OnNodeSet();
        }

        public void Load(SledLuaVariableFilterState varFilter)
        {
            varFilter.LocalFilterNames.Clear();
            varFilter.NetFilterNames.Clear();

            var i = 0;
            varFilter.LocalFilterTypes[i++] = LocalTypes.LUA_TNIL;
            varFilter.LocalFilterTypes[i++] = LocalTypes.LUA_TBOOLEAN;
            varFilter.LocalFilterTypes[i++] = LocalTypes.LUA_TLIGHTUSERDATA;
            varFilter.LocalFilterTypes[i++] = LocalTypes.LUA_TNUMBER;
            varFilter.LocalFilterTypes[i++] = LocalTypes.LUA_TSTRING;
            varFilter.LocalFilterTypes[i++] = LocalTypes.LUA_TTABLE;
            varFilter.LocalFilterTypes[i++] = LocalTypes.LUA_TFUNCTION;
            varFilter.LocalFilterTypes[i++] = LocalTypes.LUA_TUSERDATA;
            varFilter.LocalFilterTypes[i] = LocalTypes.LUA_TTHREAD;

            foreach (var name in LocalNames.Names)
            {
                varFilter.LocalFilterNames.Add(name.Name);
            }

            i = 0;
            varFilter.NetFilterTypes[i++] = TargetTypes.LUA_TNIL;
            varFilter.NetFilterTypes[i++] = TargetTypes.LUA_TBOOLEAN;
            varFilter.NetFilterTypes[i++] = TargetTypes.LUA_TLIGHTUSERDATA;
            varFilter.NetFilterTypes[i++] = TargetTypes.LUA_TNUMBER;
            varFilter.NetFilterTypes[i++] = TargetTypes.LUA_TSTRING;
            varFilter.NetFilterTypes[i++] = TargetTypes.LUA_TTABLE;
            varFilter.NetFilterTypes[i++] = TargetTypes.LUA_TFUNCTION;
            varFilter.NetFilterTypes[i++] = TargetTypes.LUA_TUSERDATA;
            varFilter.NetFilterTypes[i] = TargetTypes.LUA_TTHREAD;

            foreach (var name in TargetNames.Names)
            {
                varFilter.NetFilterNames.Add(name.Name);
            }
        }

        public void Setup(SledLuaVariableFilterState varFilter)
        {
            var i = 0;
            LocalTypes.LUA_TNIL = varFilter.LocalFilterTypes[i++];
            LocalTypes.LUA_TBOOLEAN = varFilter.LocalFilterTypes[i++];
            LocalTypes.LUA_TLIGHTUSERDATA = varFilter.LocalFilterTypes[i++];
            LocalTypes.LUA_TNUMBER = varFilter.LocalFilterTypes[i++];
            LocalTypes.LUA_TSTRING = varFilter.LocalFilterTypes[i++];
            LocalTypes.LUA_TTABLE = varFilter.LocalFilterTypes[i++];
            LocalTypes.LUA_TFUNCTION = varFilter.LocalFilterTypes[i++];
            LocalTypes.LUA_TUSERDATA = varFilter.LocalFilterTypes[i++];
            LocalTypes.LUA_TTHREAD = varFilter.LocalFilterTypes[i];

            LocalNames.Names.Clear();
            foreach (var name in varFilter.LocalFilterNames)
            {
                var domNode =
                    new DomNode(SledLuaSchema.SledLuaVarFilterNameType.Type);

                var temp =
                    domNode.As<SledLuaVarFilterNameType>();

                temp.Name = name;
                LocalNames.Names.Add(temp);
            }

            i = 0;
            TargetTypes.LUA_TNIL = varFilter.NetFilterTypes[i++];
            TargetTypes.LUA_TBOOLEAN = varFilter.NetFilterTypes[i++];
            TargetTypes.LUA_TLIGHTUSERDATA = varFilter.NetFilterTypes[i++];
            TargetTypes.LUA_TNUMBER = varFilter.NetFilterTypes[i++];
            TargetTypes.LUA_TSTRING = varFilter.NetFilterTypes[i++];
            TargetTypes.LUA_TTABLE = varFilter.NetFilterTypes[i++];
            TargetTypes.LUA_TFUNCTION = varFilter.NetFilterTypes[i++];
            TargetTypes.LUA_TUSERDATA = varFilter.NetFilterTypes[i++];
            TargetTypes.LUA_TTHREAD = varFilter.NetFilterTypes[i];

            TargetNames.Names.Clear();
            foreach (var name in varFilter.NetFilterNames)
            {
                var domNode =
                    new DomNode(SledLuaSchema.SledLuaVarFilterNameType.Type);

                var temp =
                    domNode.As<SledLuaVarFilterNameType>();

                temp.Name = name;
                TargetNames.Names.Add(temp);
            }
        }
    }

    public class SledLuaVarFiltersType : SledProjectFilesUserSettingsType
    {
        public override string Name
        {
            get { return GetAttribute<string>(SledLuaSchema.SledLuaVarFiltersType.nameAttribute); }
            set { SetAttribute(SledLuaSchema.SledLuaVarFiltersType.nameAttribute, value); }
        }

        public override bool Expanded
        {
            get { return GetAttribute<bool>(SledLuaSchema.SledLuaVarFiltersType.expandedAttribute); }
            set { SetAttribute(SledLuaSchema.SledLuaVarFiltersType.expandedAttribute, value); }
        }

        public SledLuaVarFilterType Globals
        {
            get { return GetChild<SledLuaVarFilterType>(SledLuaSchema.SledLuaVarFiltersType.GlobalsChild); }
        }

        public SledLuaVarFilterType Locals
        {
            get { return GetChild<SledLuaVarFilterType>(SledLuaSchema.SledLuaVarFiltersType.LocalsChild); }
        }

        public SledLuaVarFilterType Upvalues
        {
            get { return GetChild<SledLuaVarFilterType>(SledLuaSchema.SledLuaVarFiltersType.UpvaluesChild); }
        }

        public SledLuaVarFilterType EnvVars
        {
            get { return GetChild<SledLuaVarFilterType>(SledLuaSchema.SledLuaVarFiltersType.EnvVarsChild); }
        }

        protected override void  OnNodeSet()
        {
            if (Globals == null)
            {
                var domNode =
                    new DomNode(SledLuaSchema.SledLuaVarFilterType.Type);

                var global =
                    domNode.As<SledLuaVarFilterType>();

                SetChild(SledLuaSchema.SledLuaVarFiltersType.GlobalsChild, global);
            }

            if (Locals == null)
            {
                var domNode =
                    new DomNode(SledLuaSchema.SledLuaVarFilterType.Type);

                var locals =
                    domNode.As<SledLuaVarFilterType>();

                SetChild(SledLuaSchema.SledLuaVarFiltersType.LocalsChild, locals);
            }

            if (Upvalues == null)
            {
                var domNode =
                    new DomNode(SledLuaSchema.SledLuaVarFilterType.Type);

                var upvalues =
                    domNode.As<SledLuaVarFilterType>();

                SetChild(SledLuaSchema.SledLuaVarFiltersType.UpvaluesChild, upvalues);
            }

            if (EnvVars == null)
            {
                var domNode =
                    new DomNode(SledLuaSchema.SledLuaVarFilterType.Type);

                var envvars =
                    domNode.As<SledLuaVarFilterType>();

                SetChild(SledLuaSchema.SledLuaVarFiltersType.EnvVarsChild, envvars);
            }

            base.OnNodeSet();
        }

        public void Load(SledLuaVariableFilterState varFilterGlobals,
            SledLuaVariableFilterState varFilterLocals,
            SledLuaVariableFilterState varFilterUpvalues,
            SledLuaVariableFilterState varFilterEnvVars)
        {
            Globals.Load(varFilterGlobals);
            Locals.Load(varFilterLocals);
            Upvalues.Load(varFilterUpvalues);
            EnvVars.Load(varFilterEnvVars);
        }

        public void Setup(SledLuaVariableFilterState varFilterGlobals,
            SledLuaVariableFilterState varFilterLocals,
            SledLuaVariableFilterState varFilterUpvalues,
            SledLuaVariableFilterState varFilterEnvVars)
        {
            Globals.Setup(varFilterGlobals);
            Locals.Setup(varFilterLocals);
            Upvalues.Setup(varFilterUpvalues);
            EnvVars.Setup(varFilterEnvVars);
        }
    }
}
