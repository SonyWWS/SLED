/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

using System;
using System.ComponentModel.Composition;
using System.IO;

using Sce.Atf;
using Sce.Atf.Applications;

using Sce.Sled.Lua.Dom;
using Sce.Sled.Lua.Resources;
using Sce.Sled.Shared.Document;
using Sce.Sled.Shared.Dom;
using Sce.Sled.Shared.Plugin;
using Sce.Sled.Shared.Services;
using Sce.Sled.SyntaxEditor;

namespace Sce.Sled.Lua
{
    [Export(typeof(IInitializable))]
    [Export(typeof(ISledLanguagePlugin))]
    [Export(typeof(SledLuaLanguagePlugin))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class SledLuaLanguagePlugin : IInitializable, ISledLanguagePlugin, ISledCanPluginBeInstantiated
    {
        public SledLuaLanguagePlugin()
        {
            // Keep this here for ISledCanPluginBeInstantiated logic
        }

        [ImportingConstructor]
        public SledLuaLanguagePlugin(MainForm mainForm)
        {
            RegisterImages();

            var embeddedTypeInfo = new SledDocumentEmbeddedTypeInfo(typeof(SledLuaFunctionToolbar), SledDocumentEmbeddedTypePosition.Top);

            m_luaDocumentClient =
                new SledDocumentClient(
                    "Lua",
                    Resources.Resource.LuaExtension,
                    Shared.Utilities.SledIcon.FileNewLua,
                    new SledLuaDocumentSyntaxHighlighter(),
                    embeddedTypeInfo);
        }

        #region IInitializable Interface

        void IInitializable.Initialize()
        {
            SledSpfWriter.ExcludeDomNodeType(SledLuaSchema.SledLuaCompileAttributeType.Type);
            SledSpfWriter.ExcludeDomNodeType(SledLuaSchema.SledLuaCompileSettingsType.Type);
            SledSpfWriter.ExcludeDomNodeType(SledLuaSchema.SledLuaFunctionType.Type);
            SledSpfWriter.ExcludeDomNodeType(SledLuaSchema.SledLuaProjectFilesWatchType.Type);
            SledSpfWriter.ExcludeDomNodeType(SledLuaSchema.SledLuaVarFiltersType.Type);
            SledSpfWriter.ExcludeDomNodeType(SledLuaSchema.SledLuaVarFilterType.Type);
            SledSpfWriter.ExcludeDomNodeType(SledLuaSchema.SledLuaVarFilterTypesType.Type);

            // Copy Lua compile settings from project temporary settings file to project file
            SledSpfReader.RegisterCopier(
                new SledSpfReader.DomNodeTypeRootCopier(
                    SledLuaSchema.SledLuaCompileSettingsType.Type));

            // Copy Lua watched variables from project temporary settings file to project file
            SledSpfReader.RegisterCopier(
                new SledSpfReader.DomNodeTypeRootCopier(
                    SledLuaSchema.SledLuaProjectFilesWatchType.Type));

            // Copy variable filters from project temporary settings file to project file
            SledSpfReader.RegisterCopier(
                new SledSpfReader.DomNodeTypeRootCopier(
                    SledLuaSchema.SledLuaVarFiltersType.Type));
        }

        #endregion

        #region ISledCanPluginBeInstantiated Interface

        public bool CanPluginBeInstantiated
        {
            get
            {
                bool result;
                try
                {
                    result = SledLuaSyntaxCheckerService.TestISledCanPluginBeInstantiated();
                }
                catch (Exception)
                {
                    result = false;
                }

                return result;
            }
        }

        #endregion

        private static void RegisterImages()
        {
            // Register images
            ResourceUtil.RegisterImage(
                SledLuaIcon.Compile,
                GdiUtil.GetImage(SledLuaUtil.LuaIconPath + Resources.Resource.Period + "lua_compile16" + Resources.Resource.PngExtension),
                GdiUtil.GetImage(SledLuaUtil.LuaIconPath + Resources.Resource.Period + "lua_compile24" + Resources.Resource.PngExtension),
                GdiUtil.GetImage(SledLuaUtil.LuaIconPath + Resources.Resource.Period + "lua_compile32" + Resources.Resource.PngExtension));

            var ids =
                new[]
                {
                    Resources.Resource.LUA_TNONE,
                    Resources.Resource.LUA_TNIL,
                    Resources.Resource.LUA_TBOOLEAN,
                    Resources.Resource.LUA_TLIGHTUSERDATA,
                    Resources.Resource.LUA_TNUMBER,
                    Resources.Resource.LUA_TSTRING,
                    Resources.Resource.LUA_TTABLE,
                    Resources.Resource.LUA_TFUNCTION,
                    Resources.Resource.LUA_TUSERDATA,
                    Resources.Resource.LUA_TTHREAD
                };

            foreach (var id in ids)
            {
                var path =
                    string.Format(
                        "{0}{1}{2}",
                        SledLuaUtil.LuaIconPath,
                        Resources.Resource.Period,
                        id);

                var imagePath =
                    string.Format(
                        "{0}{1}{2}",
                        path,
                        Resources.Resource._16x16,
                        Resources.Resource.PngExtension);

                ResourceUtil.RegisterImage(path, GdiUtil.GetImage(imagePath));
            }
        }

        #region ISledLanguagePlugin Interface

        public string LanguageName
        {
            get { return Resources.Resource.Lua; }
        }

        public string[] LanguageExtensions
        {
            get { return new[] { Resources.Resource.LuaExtension }; }
        }

        public string LanguageDescription
        {
            get { return Localization.SledLuaLuaLanguageDescription; }
        }

        public UInt16 LanguageId
        {
            get { return PluginId; }
        }

        #endregion

        #region SledLuaDocumentSyntaxHighlighter Class

        private class SledLuaDocumentSyntaxHighlighter : SledDocumentSyntaxHighlighter
        {
            public override object Highlighter
            {
                get
                {
                    CheckForOverride();

                    if (!m_bOverrideExists)
                        return Languages.Lua;

                    // Want to use custom language with custom
                    // outlining dynamic syntax language class
                    var pair =
                        new LanguageStreamPair(
                            Languages.Lua,
                            File.OpenRead(m_overridePath));

                    return pair;
                }
            }

            private void CheckForOverride()
            {
                if (m_bCheckedForOverride)
                    return;

                try
                {
                    var directoryInfoService =
                        new SledServiceReference<ISledDirectoryInfoService>();

                    m_overridePath =
                        Path.Combine(
                            directoryInfoService.Get.PluginDirectory,
                            SyntaxHighlightingFileOverride);

                    m_bOverrideExists =
                        File.Exists(m_overridePath);
                }
                catch (Exception)
                {
                    m_bOverrideExists = false;
                    m_overridePath = string.Empty;
                }
                finally
                {
                    m_bCheckedForOverride = true;
                }
            }

            private bool m_bOverrideExists;
            private bool m_bCheckedForOverride;

            private string m_overridePath;

            private const string SyntaxHighlightingFileOverride =
                "LuaDefinitionOverride.xml";
        }

        #endregion

        [Export(typeof(IDocumentClient))]
        [Export(typeof(ISledDocumentClient))]
        private readonly SledDocumentClient m_luaDocumentClient;

        private const UInt16 PluginId = 1;
    }

    internal static class SledLuaSettings
    {
        public const string Element = "LuaSettings";

        public const string Category = "Lua Settings";
    }

    // Right from lua.h
    public enum LuaType
    {
        LUA_TNONE = -1,
        LUA_TNIL = 0,
        LUA_TBOOLEAN = 1,
        LUA_TLIGHTUSERDATA = 2,
        LUA_TNUMBER = 3,
        LUA_TSTRING = 4,
        LUA_TTABLE = 5,
        LUA_TFUNCTION = 6,
        LUA_TUSERDATA = 7,
        LUA_TTHREAD = 8,
    }
}
