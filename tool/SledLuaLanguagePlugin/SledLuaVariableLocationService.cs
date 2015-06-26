/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

using System.Collections.Generic;
using System.ComponentModel.Composition;

using Sce.Atf;
using Sce.Atf.Adaptation;
using Sce.Atf.Dom;

using Sce.Sled.Lua.Dom;
using Sce.Sled.Shared.Dom;

namespace Sce.Sled.Lua
{
    [Export(typeof(IInitializable))]
    [Export(typeof(ISledLuaVariableLocationService))]
    [Export(typeof(SledLuaVariableLocationService))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    sealed class SledLuaVariableLocationService : IInitializable, ISledLuaVariableLocationService
    {
        #region IInitializable Interface

        void IInitializable.Initialize()
        {
            // Keep this here
        }

        #endregion

        #region ISledLuaVariableLocationService Interface

        public void AddLocations(ISledLuaVarBaseType luaVar)
        {
            if (luaVar == null)
                return;

            Dictionary<string, List<SledLuaVariableParserService.VariableResult>> parsedVars;

            {
                var nodeType = luaVar.DomNode.Type;
                if (nodeType == SledLuaSchema.SledLuaVarLocalType.Type)
                    parsedVars = m_luaVariableParserService.ParsedLocals;
                else if (nodeType == SledLuaSchema.SledLuaVarUpvalueType.Type)
                    parsedVars = m_luaVariableParserService.ParsedUpvalues;
                else
                    parsedVars = m_luaVariableParserService.ParsedGlobals;
            }

            if (parsedVars == null)
                return;

            // No locations for variable
            List<SledLuaVariableParserService.VariableResult> items;
            if (!parsedVars.TryGetValue(luaVar.Name, out items))
                return;

            // Iterate through all values belonging to this key
            foreach (var result in items)
            {
                var domNode = new DomNode(SledSchema.SledVarLocationType.Type);

                // Generate location data
                var loc = domNode.As<SledVarLocationType>();

                loc.File = result.File.AbsolutePath;
                loc.Line = result.Line;
                loc.Occurence = result.Occurence;

                // Add to list
                luaVar.Locations.Add(loc);
            }
        }

        #endregion

#pragma warning disable 649 // Field is never assigned to and will always have its default value null

        [Import]
        private ISledLuaVariableParserService m_luaVariableParserService;

#pragma warning restore 649

    }

    interface ISledLuaVariableLocationService
    {
        void AddLocations(ISledLuaVarBaseType luaVar);
    }
}
