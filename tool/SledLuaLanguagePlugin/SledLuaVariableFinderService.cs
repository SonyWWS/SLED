/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

using System.Collections.Generic;
using System.ComponentModel.Composition;

using Sce.Atf;
using Sce.Atf.Dom;

using Sce.Sled.Lua.Dom;

namespace Sce.Sled.Lua
{
    [Export(typeof(IInitializable))]
    [Export(typeof(ISledLuaVariableFinderService))]
    [Export(typeof(SledLuaVariableFinderService))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    sealed class SledLuaVariableFinderService : IInitializable, ISledLuaVariableFinderService
    {
        #region IInitializable Interface

        void IInitializable.Initialize()
        {
            m_dictVarServices.Add(SledLuaSchema.SledLuaVarGlobalType.Type, m_globalVariableService);
            m_dictVarServices.Add(SledLuaSchema.SledLuaVarLocalType.Type, m_localVariableService);
            m_dictVarServices.Add(SledLuaSchema.SledLuaVarUpvalueType.Type, m_upvalueVariableService);
            m_dictVarServices.Add(SledLuaSchema.SledLuaVarEnvType.Type, m_environmentVariableService);
        }

        #endregion

        #region ISledLuaVariableFinderService Interface

        public bool TryGetVariable(string name, DomNodeType nodeType, out ISledLuaVarBaseType luaVar)
        {
            luaVar = null;

            if (string.IsNullOrEmpty(name))
                return false;

            if (nodeType == null)
                return false;

            if (m_dictVarServices.Count <= 0)
                return false;

            ISledLuaVariableService variableService;
            return
                m_dictVarServices.TryGetValue(nodeType, out variableService) &&
                variableService.TryGetVariable(name, out luaVar);
        }

        public bool TryGetGlobalVariable(string name, out SledLuaVarGlobalType global)
        {
            global = null;

            ISledLuaVarBaseType luaVar;
            if (!TryGetVariable(name, SledLuaSchema.SledLuaVarGlobalType.Type, out luaVar))
                return false;

            global = (SledLuaVarGlobalType)luaVar;

            return true;
        }

        public bool TryGetLocalVariable(string name, out SledLuaVarLocalType local)
        {
            local = null;

            ISledLuaVarBaseType luaVar;
            if (!TryGetVariable(name, SledLuaSchema.SledLuaVarLocalType.Type, out luaVar))
                return false;

            local = (SledLuaVarLocalType)luaVar;

            return true;
        }

        public bool TryGetUpvalueVariable(string name, out SledLuaVarUpvalueType upvalue)
        {
            upvalue = null;

            ISledLuaVarBaseType luaVar;
            if (!TryGetVariable(name, SledLuaSchema.SledLuaVarUpvalueType.Type, out luaVar))
                return false;

            upvalue = (SledLuaVarUpvalueType)luaVar;

            return true;
        }

        public bool TryGetEnvironmentVariable(string name, out SledLuaVarEnvType environment)
        {
            environment = null;

            ISledLuaVarBaseType luaVar;
            if (!TryGetVariable(name, SledLuaSchema.SledLuaVarEnvType.Type, out luaVar))
                return false;

            environment = (SledLuaVarEnvType)luaVar;

            return true;
        }

        #endregion

#pragma warning disable 649 // Field is never assigned

        [Import]
        private SledLuaGlobalVariableService m_globalVariableService;

        [Import]
        private SledLuaLocalVariableService m_localVariableService;

        [Import]
        private SledLuaUpvalueVariableService m_upvalueVariableService;

        [Import]
        private SledLuaEnvironmentVariableService m_environmentVariableService;

#pragma warning restore 649

        private readonly Dictionary<DomNodeType, ISledLuaVariableService> m_dictVarServices =
            new Dictionary<DomNodeType, ISledLuaVariableService>();
    }

    interface ISledLuaVariableFinderService
    {
        bool TryGetVariable(string name, DomNodeType nodeType, out ISledLuaVarBaseType luaVar);
        bool TryGetGlobalVariable(string name, out SledLuaVarGlobalType global);
        bool TryGetLocalVariable(string name, out SledLuaVarLocalType local);
        bool TryGetUpvalueVariable(string name, out SledLuaVarUpvalueType upvalue);
        bool TryGetEnvironmentVariable(string name, out SledLuaVarEnvType environment);
    }

    interface ISledLuaVariableService
    {
        bool TryGetVariable(string name, out ISledLuaVarBaseType luaVar);
    }
}
