/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

using System.ComponentModel.Composition;

using Sce.Atf;
using Sce.Atf.Adaptation;
using Sce.Atf.Dom;

using Sce.Sled.Lua.Dom;
using Sce.Sled.Shared.Services;

namespace Sce.Sled.Lua
{
    [Export(typeof(IInitializable))]
    [Export(typeof(ISledLuaVarScmpService))]
    [Export(typeof(SledLuaVarScmpService))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    sealed class SledLuaVarScmpService : IInitializable, ISledLuaVarScmpService
    {
        #region IInitializable Interface

        void IInitializable.Initialize()
        {
            m_debugService = SledServiceInstance.Get<ISledDebugService>();
        }

        #endregion

        #region ISledLuaVarScmpService Interface

        public SledLuaVarGlobalType GetScmpBlobAsLuaGlobalVar()
        {
            var var = m_debugService.GetScmpBlob<Scmp.LuaVarGlobal>();
            var node = new DomNode(SledLuaSchema.SledLuaVarGlobalType.Type);

            var global = node.As<SledLuaVarGlobalType>();
            global.DisplayName = var.Name;
            global.Name = SledLuaVarBaseType.CreateFlattenedHierarchyName(var.Name, var.Hierarchy, HierarchySeparatorString);
            global.What = SledLuaUtil.LuaTypeIntToString(var.What);
            global.Value = var.Value;
            global.KeyType = var.KeyType;
            SledLuaVarBaseType.SetupTargetHierarchyFromRuntimeData(global, var.Hierarchy);
            global.GenerateUniqueName();

            return global;
        }

        public SledLuaVarLocalType GetScmpBlobAsLuaLocalVar()
        {
            var var = m_debugService.GetScmpBlob<Scmp.LuaVarLocal>();
            var node = new DomNode(SledLuaSchema.SledLuaVarLocalType.Type);

            var local = node.As<SledLuaVarLocalType>();
            local.DisplayName = var.Name;
            local.Name = SledLuaVarBaseType.CreateFlattenedHierarchyName(var.Name, var.Hierarchy, HierarchySeparatorString);
            local.What = SledLuaUtil.LuaTypeIntToString(var.What);
            local.Value = var.Value;
            local.Level = var.StackLevel;
            local.Index = var.Index;
            local.KeyType = var.KeyType;

            local.FunctionName = m_luaCallStackService.Get[local.Level].Function;
            local.FunctionLineDefined = m_luaCallStackService.Get[local.Level].LineDefined;

            SledLuaVarBaseType.SetupTargetHierarchyFromRuntimeData(local, var.Hierarchy);
            local.GenerateUniqueName();

            return local;
        }

        public SledLuaVarUpvalueType GetScmpBlobAsLuaUpvalueVar()
        {
            var var = m_debugService.GetScmpBlob<Scmp.LuaVarUpvalue>();
            var node = new DomNode(SledLuaSchema.SledLuaVarUpvalueType.Type);

            var upvalue = node.As<SledLuaVarUpvalueType>();
            upvalue.DisplayName = var.Name;
            upvalue.Name = SledLuaVarBaseType.CreateFlattenedHierarchyName(var.Name, var.Hierarchy, HierarchySeparatorString);
            upvalue.What = SledLuaUtil.LuaTypeIntToString(var.What);
            upvalue.Value = var.Value;
            upvalue.Level = var.StackLevel;
            upvalue.Index = var.Index;
            upvalue.KeyType = var.KeyType;

            upvalue.FunctionName = m_luaCallStackService.Get[upvalue.Level].Function;
            upvalue.FunctionLineDefined = m_luaCallStackService.Get[upvalue.Level].LineDefined;

            SledLuaVarBaseType.SetupTargetHierarchyFromRuntimeData(upvalue, var.Hierarchy);
            upvalue.GenerateUniqueName();

            return upvalue;
        }

        public SledLuaVarEnvType GetScmpBlobAsLuaEnvironmentVar()
        {
            var var = m_debugService.GetScmpBlob<Scmp.LuaVarEnvVar>();
            var node = new DomNode(SledLuaSchema.SledLuaVarEnvType.Type);

            // Uses similar formatting as Globals
            var envVar = node.As<SledLuaVarEnvType>();
            envVar.DisplayName = var.Name;
            envVar.Name = SledLuaVarBaseType.CreateFlattenedHierarchyName(var.Name, var.Hierarchy, HierarchySeparatorString);
            envVar.What = SledLuaUtil.LuaTypeIntToString(var.What);
            envVar.Value = var.Value;
            envVar.Level = var.StackLevel; // Extra non-global param
            envVar.KeyType = var.KeyType;
            SledLuaVarBaseType.SetupTargetHierarchyFromRuntimeData(envVar, var.Hierarchy);
            envVar.GenerateUniqueName();

            return envVar;
        }

        #endregion

        private ISledDebugService m_debugService;

        private readonly SledServiceReference<ISledLuaCallStackService> m_luaCallStackService =
            new SledServiceReference<ISledLuaCallStackService>();

        public const string HierarchySeparatorString = ".";
    }

    interface ISledLuaVarScmpService
    {
        SledLuaVarGlobalType GetScmpBlobAsLuaGlobalVar();
        SledLuaVarLocalType GetScmpBlobAsLuaLocalVar();
        SledLuaVarUpvalueType GetScmpBlobAsLuaUpvalueVar();
        SledLuaVarEnvType GetScmpBlobAsLuaEnvironmentVar();
    }
}
