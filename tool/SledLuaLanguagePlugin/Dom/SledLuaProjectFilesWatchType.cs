/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

using System;
using System.Collections.Generic;

using Sce.Atf.Adaptation;
using Sce.Atf.Dom;

using Sce.Sled.Shared.Dom;
using Sce.Sled.Shared.Services;
using Sce.Sled.Shared.Utilities;

namespace Sce.Sled.Lua.Dom
{
    public class SledLuaProjectFilesWatchType : SledProjectFilesWatchType
    {
        public override string Name
        {
            get { return GetAttribute<string>(SledLuaSchema.SledLuaProjectFilesWatchType.nameAttribute); }
            set { SetAttribute(SledLuaSchema.SledLuaProjectFilesWatchType.nameAttribute, value); }
        }

        public override bool Expanded
        {
            get { return GetAttribute<bool>(SledLuaSchema.SledLuaProjectFilesWatchType.expandedAttribute); }
            set { SetAttribute(SledLuaSchema.SledLuaProjectFilesWatchType.expandedAttribute, value); }
        }

        public override string LanguagePluginString
        {
            get { return GetAttribute<string>(SledLuaSchema.SledLuaProjectFilesWatchType.language_pluginAttribute); }
            set { SetAttribute(SledLuaSchema.SledLuaProjectFilesWatchType.language_pluginAttribute, value); }
        }

        /// <summary>
        /// Gets/sets the scope attribute
        /// </summary>
        public SledLuaVarScopeType Scope
        {
            get { return this.GetEnumValue<SledLuaVarScopeType>(SledLuaSchema.SledLuaProjectFilesWatchType.scopeAttribute); }
            set { this.SetEnumValue(SledLuaSchema.SledLuaProjectFilesWatchType.scopeAttribute, value); }
        }

        /// <summary>
        /// Gets/sets the context attribute
        /// </summary>
        public SledLuaVarLookUpContextType Context
        {
            get { return this.GetEnumValue<SledLuaVarLookUpContextType>(SledLuaSchema.SledLuaProjectFilesWatchType.contextAttribute); }
            set { this.SetEnumValue(SledLuaSchema.SledLuaProjectFilesWatchType.contextAttribute, value); }
        }

        /// <summary>
        /// Gets/sets the guid attribute
        /// </summary>
        public Guid Guid
        {
            get
            {
                var stringGuid = GetAttribute<string>(SledLuaSchema.SledLuaProjectFilesWatchType.guidAttribute);
                return new Guid(stringGuid);
            }
            set { SetAttribute(SledLuaSchema.SledLuaProjectFilesWatchType.guidAttribute, value.ToString()); }
        }

        /// <summary>
        /// Gets/sets the lookup child
        /// </summary>
        public SledLuaVarLookUpType LookUp
        {
            get { return GetChild<SledLuaVarLookUpType>(SledLuaSchema.SledLuaProjectFilesWatchType.LookUpChild); }
            set { SetChild(SledLuaSchema.SledLuaProjectFilesWatchType.LookUpChild, value); }
        }

        /// <summary>
        /// Create project watch from Lua variable
        /// </summary>
        /// <param name="luaVar"></param>
        /// <returns></returns>
        public static SledLuaProjectFilesWatchType CreateFromLuaVar(ISledLuaVarBaseType luaVar)
        {
            var projectFilesWatch =
                new DomNode(SledLuaSchema.SledLuaProjectFilesWatchType.Type)
                    .As<SledLuaProjectFilesWatchType>();

            var luaLanguagePlugin = SledServiceInstance.TryGet<SledLuaLanguagePlugin>();

            projectFilesWatch.Name = luaVar.DisplayName;
            projectFilesWatch.LanguagePlugin = luaLanguagePlugin;
            projectFilesWatch.Scope = luaVar.Scope;
            projectFilesWatch.Context = SledLuaVarLookUpContextType.WatchProject;
            projectFilesWatch.LookUp = SledLuaVarLookUpType.FromLuaVar(luaVar, projectFilesWatch.Context);

            return projectFilesWatch;
        }

        /// <summary>
        /// Create project watch from custom values
        /// </summary>
        /// <param name="alias"></param>
        /// <param name="scope"></param>
        /// <param name="namesAndTypes"></param>
        /// <param name="guid"></param>
        /// <returns></returns>
        public static SledLuaProjectFilesWatchType CreateFromCustom(string alias, SledLuaVarScopeType scope, IList<SledLuaVarNameTypePairType> namesAndTypes, Guid guid)
        {
            var projectFilesWatch =
                new DomNode(SledLuaSchema.SledLuaProjectFilesWatchType.Type)
                    .As<SledLuaProjectFilesWatchType>();

            var luaLanguagePlugin = SledServiceInstance.TryGet<SledLuaLanguagePlugin>();

            projectFilesWatch.Name = alias;
            projectFilesWatch.LanguagePlugin = luaLanguagePlugin;
            projectFilesWatch.Scope = scope;
            projectFilesWatch.Context = SledLuaVarLookUpContextType.WatchCustom;
            projectFilesWatch.Guid = guid;
            projectFilesWatch.LookUp = SledLuaVarLookUpType.FromCustomValues(scope, SledLuaVarLookUpContextType.WatchCustom, namesAndTypes);

            return projectFilesWatch;
        }

        /// <summary>
        /// OnNodeSet
        /// </summary>
        protected override void OnNodeSet()
        {
            if (DomNode.IsAttributeDefault(SledLuaSchema.SledLuaProjectFilesWatchType.guidAttribute))
                Guid = SledUtil.MakeXmlSafeGuid();

            base.OnNodeSet();
        }
    }
}