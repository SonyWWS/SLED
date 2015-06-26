/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

using System;
using System.Collections.Generic;

using Sce.Atf.Adaptation;
using Sce.Atf.Dom;

using Sce.Sled.Shared;
using Sce.Sled.Shared.Dom;

namespace Sce.Sled.Lua.Dom
{
    public class SledLuaVarLookUpType : DomNodeAdapter, ICloneable
    {
        #region Implementation of ICloneable

        public object Clone()
        {
            var copy = DomNode.Copy(new[] { DomNode });
            copy[0].InitializeExtensions();

            return copy[0].As<SledLuaVarLookUpType>();
        }

        #endregion

        /// <summary>
        /// Gets/sets the scope attribute
        /// </summary>
        public SledLuaVarScopeType Scope
        {
            get { return this.GetEnumValue<SledLuaVarScopeType>(SledLuaSchema.SledLuaVarLookUpType.scopeAttribute); }
            set { this.SetEnumValue(SledLuaSchema.SledLuaVarLookUpType.scopeAttribute, value); }
        }

        /// <summary>
        /// Gets/sets the stack level attribute
        /// </summary>
        public int StackLevel
        {
            get { return GetAttribute<int>(SledLuaSchema.SledLuaVarLookUpType.stack_levelAttribute); }
            set { SetAttribute(SledLuaSchema.SledLuaVarLookUpType.stack_levelAttribute, value); }
        }

        /// <summary>
        /// Gets/sets the index attribute
        /// </summary>
        public int Index
        {
            get { return GetAttribute<int>(SledLuaSchema.SledLuaVarLookUpType.indexAttribute); }
            set { SetAttribute(SledLuaSchema.SledLuaVarLookUpType.indexAttribute, value); }
        }

        /// <summary>
        /// Gets/sets the extra attribute
        /// </summary>
        public bool Extra
        {
            get { return GetAttribute<bool>(SledLuaSchema.SledLuaVarLookUpType.extraAttribute); }
            set { SetAttribute(SledLuaSchema.SledLuaVarLookUpType.extraAttribute, value); }
        }

        /// <summary>
        /// Gets/sets the context attribute
        /// </summary>
        public SledLuaVarLookUpContextType Context
        {
            get { return this.GetEnumValue<SledLuaVarLookUpContextType>(SledLuaSchema.SledLuaVarLookUpType.contextAttribute); }
            set { this.SetEnumValue(SledLuaSchema.SledLuaVarLookUpType.contextAttribute, value); }
        }

        /// <summary>
        /// Gets the names and types child
        /// </summary>
        public IList<SledLuaVarNameTypePairType> NamesAndTypes
        {
            get { return GetChildList<SledLuaVarNameTypePairType>(SledLuaSchema.SledLuaVarLookUpType.NamesAndTypesChild); }
        }

        /// <summary>
        /// Create lookup from a Lua variable
        /// </summary>
        /// <param name="luaVar"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public static SledLuaVarLookUpType FromLuaVar(ISledLuaVarBaseType luaVar, SledLuaVarLookUpContextType context)
        {
            try
            {
                var lookUp =
                    new DomNode(SledLuaSchema.SledLuaVarLookUpType.Type)
                    .As<SledLuaVarLookUpType>();

                lookUp.Scope = luaVar.Scope;
                lookUp.Context = context;

                // Go through and generate a list of key/value pairs of the hierarchy so we can
                // look up and find the variable on the target no matter where it may be
                foreach (var nameAndType in luaVar.TargetHierarchy)
                    lookUp.NamesAndTypes.Add((SledLuaVarNameTypePairType)nameAndType.Clone());

                lookUp.NamesAndTypes.Add(SledLuaVarNameTypePairType.Create(luaVar.DisplayName, luaVar.KeyType));

                var index = 0;
                var stackLevel = 0;

                // Some special processing if a local or upvalue variable
                if (luaVar.DomNode.Is<SledLuaVarLocalUpvalueBaseType>())
                {
                    var varTmp = luaVar.DomNode.As<SledLuaVarLocalUpvalueBaseType>();

                    stackLevel = varTmp.Level;
                    index = varTmp.Index;
                }

                // Some special processing if an environment variable
                if (luaVar.DomNode.Is<SledLuaVarEnvType>())
                {
                    var varTmp = luaVar.DomNode.As<SledLuaVarEnvType>();

                    stackLevel = varTmp.Level;
                }

                lookUp.StackLevel = stackLevel;
                lookUp.Index = index;

                return lookUp;
            }
            catch (Exception ex)
            {
                SledOutDevice.OutLineDebug(
                    SledMessageType.Error,
                    "{0}: Exception in FromLuaVar: {1}",
                    typeof(SledLuaVarLookUpType), ex.Message);

                return null;
            }
        }

        public static SledLuaVarLookUpType FromCustomValues(SledLuaVarScopeType scope, SledLuaVarLookUpContextType context, IList<SledLuaVarNameTypePairType> namesAndTypes)
        {
            try
            {
                var lookUp =
                    new DomNode(SledLuaSchema.SledLuaVarLookUpType.Type)
                    .As<SledLuaVarLookUpType>();

                lookUp.Scope = scope;
                lookUp.Context = context;

                foreach (var nameAndType in namesAndTypes)
                    lookUp.NamesAndTypes.Add((SledLuaVarNameTypePairType)nameAndType.Clone());

                lookUp.Index = 0;
                lookUp.StackLevel = 0;

                return lookUp;
            }
            catch (Exception ex)
            {
                SledOutDevice.OutLineDebug(
                    SledMessageType.Error,
                    "{0}: Exception in FromCustomValues: {1}",
                    typeof(SledLuaVarLookUpType), ex.Message);

                return null;
            }
        }
    }
}