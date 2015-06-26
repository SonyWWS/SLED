/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

using System;
using System.Collections.Generic;
using System.Linq;

using Sce.Atf.Adaptation;
using Sce.Atf.Dom;

using Sce.Sled.Shared;
using Sce.Sled.Shared.Services;

namespace Sce.Sled.Lua.Dom
{
    public sealed class SledLuaVarWatchListType : SledLuaVarBaseListType<ISledLuaVarBaseType>
    {
        protected override string Description
        {
            get { return "Lua Watched Variables"; }
        }

        protected override string[] TheColumnNames
        {
            get { return SledLuaVarBaseType.WatchColumnNames; }
        }

        protected override AttributeInfo NameAttributeInfo
        {
            get { return SledLuaSchema.SledLuaVarWatchListType.nameAttribute; }
        }

        protected override ChildInfo VariablesChildInfo
        {
            // Intentional
            get { throw new NotImplementedException(); }
        }

        public override IList<ISledLuaVarBaseType> Variables
        {
            get
            {
                var variables = new List<ISledLuaVarBaseType>();

                variables.AddRange(Globals);
                variables.AddRange(Locals);
                variables.AddRange(Upvalues);
                variables.AddRange(EnvVars);

                return variables;
            }
        }

        private IList<SledLuaVarGlobalType> Globals
        {
            get { return GetChildList<SledLuaVarGlobalType>(SledLuaSchema.SledLuaVarWatchListType.GlobalsChild); }
        }

        private IList<SledLuaVarLocalType> Locals
        {
            get { return GetChildList<SledLuaVarLocalType>(SledLuaSchema.SledLuaVarWatchListType.LocalsChild); }
        }

        private IList<SledLuaVarUpvalueType> Upvalues
        {
            get { return GetChildList<SledLuaVarUpvalueType>(SledLuaSchema.SledLuaVarWatchListType.UpvaluesChild); }
        }

        private IList<SledLuaVarEnvType> EnvVars
        {
            get { return GetChildList<SledLuaVarEnvType>(SledLuaSchema.SledLuaVarWatchListType.EnvVarsChild); }
        }

        public void Clear()
        {
            Globals.Clear();
            Locals.Clear();
            Upvalues.Clear();
            EnvVars.Clear();
            m_dictCustomWatches.Clear();
        }

        public void Add(ISledLuaVarBaseType luaVar)
        {
            if (luaVar == null)
                return;

            m_dictCustomWatches[luaVar] = WatchedVariableService.ReceivingWatchedCustomVariables;

            if (luaVar.DomNode.Is<SledLuaVarGlobalType>())
                Globals.Add(luaVar.DomNode.As<SledLuaVarGlobalType>());
            else if (luaVar.DomNode.Is<SledLuaVarLocalType>())
                Locals.Add(luaVar.DomNode.As<SledLuaVarLocalType>());
            else if (luaVar.DomNode.Is<SledLuaVarUpvalueType>())
                Upvalues.Add(luaVar.DomNode.As<SledLuaVarUpvalueType>());
            else if (luaVar.DomNode.Is<SledLuaVarEnvType>())
                EnvVars.Add(luaVar.DomNode.As<SledLuaVarEnvType>());
        }

        public void AddToItem(ISledLuaVarBaseType root, ISledLuaVarBaseType luaVar, bool bHierarchical)
        {
            if (!bHierarchical)
            {
                m_dictCustomWatches[luaVar] = WatchedVariableService.ReceivingWatchedCustomVariables;
                AddTo(root, luaVar);
                return;
            }

            // Figure out where to place the item
            var insert = FindInsertion(root, luaVar);
            if (insert == null)
                return;

            m_dictCustomWatches[luaVar] = WatchedVariableService.ReceivingWatchedCustomVariables;
            AddTo(insert, luaVar);
        }

        public void Remove(ISledLuaVarBaseType luaVar)
        {
            if (luaVar == null)
                return;

            m_dictCustomWatches.Remove(luaVar);
            luaVar.DomNode.RemoveFromParent();
        }

        public bool IsCustomWatchedVariable(ISledLuaVarBaseType luaVar)
        {
            bool result;
            m_dictCustomWatches.TryGetValue(luaVar, out result);
            return result;
        }

        #region Add/Insertion Helpers

        private static void AddTo(ISledLuaVarBaseType adder, ISledLuaVarBaseType addee)
        {
            if (addee.DomNode.Type == SledLuaSchema.SledLuaVarGlobalType.Type)
                AddGlobal(adder, addee);
            else if (addee.DomNode.Type == SledLuaSchema.SledLuaVarLocalType.Type)
                AddLocal(adder, addee);
            else if (addee.DomNode.Type == SledLuaSchema.SledLuaVarUpvalueType.Type)
                AddUpvalue(adder, addee);
            else if (addee.DomNode.Type == SledLuaSchema.SledLuaVarEnvType.Type)
                AddEnvironment(adder, addee);
        }

        private static void AddGlobal(ISledLuaVarBaseType adder, ISledLuaVarBaseType addee)
        {
            var parent = adder.DomNode.As<SledLuaVarGlobalType>();
            parent.Globals.Add(addee.DomNode.As<SledLuaVarGlobalType>());
        }

        private static void AddLocal(ISledLuaVarBaseType adder, ISledLuaVarBaseType addee)
        {
            var parent = adder.DomNode.As<SledLuaVarLocalType>();
            parent.Locals.Add(addee.DomNode.As<SledLuaVarLocalType>());
        }

        private static void AddUpvalue(ISledLuaVarBaseType adder, ISledLuaVarBaseType addee)
        {
            var parent = adder.DomNode.As<SledLuaVarUpvalueType>();
            parent.Upvalues.Add(addee.DomNode.As<SledLuaVarUpvalueType>());
        }

        private static void AddEnvironment(ISledLuaVarBaseType adder, ISledLuaVarBaseType addee)
        {
            var parent = adder.DomNode.As<SledLuaVarEnvType>();
            parent.EnvVars.Add(addee.DomNode.As<SledLuaVarEnvType>());
        }

        private static ISledLuaVarBaseType FindInsertion(ISledLuaVarBaseType adder, ISledLuaVarBaseType addee)
        {
            try
            {
                var adderPiecesCount = adder.TargetHierarchy.Count + 1;

                var pieces = new List<string>();
                {
                    pieces.AddRange(addee.TargetHierarchy.Select(kv => kv.Name));
                    pieces.Add(addee.DisplayName);
                }

                // Do we even need to try and find this item?
                return
                    pieces.Count <= (adderPiecesCount + 1)
                        ? adder
                        : FindInsertionHelper(adder, adderPiecesCount, pieces);
            }
            catch (Exception ex)
            {
                SledOutDevice.OutLine(
                    SledMessageType.Error,
                    "[SledLuaVarWatchList] Exception finding insertion " +
                    "point with adder \"{0}\" and addee \"{1}\": {2}",
                    adder.Name, addee.Name, ex.Message);

                return null;
            }
        }

        private static ISledLuaVarBaseType FindInsertionHelper(ISledLuaVarBaseType adder, int adderPiecesCount, List<string> pieces)
        {
            ISledLuaVarBaseType insert = null;

            var lstVariables = adder.Variables.ToList();
            for (var i = adderPiecesCount; i < (pieces.Count - 1); ++i)
            {
                var iPos = -1;

                var count = lstVariables.Count;
                for (var j = 0; j < count; j++)
                {
                    if (string.Compare(lstVariables[j].DisplayName, pieces[i], StringComparison.Ordinal) != 0)
                        continue;

                    iPos = j;
                    break;
                }

                if (iPos == -1)
                    return null;

                insert = lstVariables[iPos];
                lstVariables = insert.Variables.ToList();
            }

            return insert;
        }

        #endregion

        private ISledLuaWatchedVariableService WatchedVariableService
        {
            get { return m_watchedVariableService ?? (m_watchedVariableService = SledServiceInstance.Get<ISledLuaWatchedVariableService>()); }
        }

        private ISledLuaWatchedVariableService m_watchedVariableService;

        private readonly Dictionary<ISledLuaVarBaseType, bool> m_dictCustomWatches =
            new Dictionary<ISledLuaVarBaseType, bool>();
    }
}