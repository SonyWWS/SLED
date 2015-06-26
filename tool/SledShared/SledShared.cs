/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Xml.Schema;

using Sce.Atf;

using Sce.Sled.Shared.Utilities;

namespace Sce.Sled.Shared
{
    /// <summary>
    /// Static class containing some shared items
    /// </summary>
    public static class SledShared
    {
        /// <summary>
        /// SLED XML namespace
        /// </summary>
        public const string SledXmlNamespace = "sled";

        /// <summary>
        /// Path to the schema directory (schema embedded in .exe)
        /// </summary>
        public const string SchemaPath = "Sce.Sled.Shared.Resources.Schemas.SledProjectFiles.xsd";

        /// <summary>
        /// Path to icon directory (icons embedded in .exe)
        /// </summary>
        public const string IconPathBase = "Sce.Sled.Shared.Resources.Icons";

        /// <summary>
        /// Path to base directory of application
        /// <remarks>ie. it is Application.StartupPath</remarks>
        /// </summary>
        public static string DirectoryPath
        {
            get { return s_directoryPath; }
            set { if (s_directoryPath == null) s_directoryPath = value; }
        }
        private static string s_directoryPath;

        /// <summary>
        /// Gets or sets path to plugin directory
        /// <remarks>DirectoryPath + "Plugins\"</remarks>
        /// </summary>
        public static string PluginPath
        {
            get { return s_pluginPath; }
            set { if (s_pluginPath == null) s_pluginPath = value; }
        }
        private static string s_pluginPath;

        /// <summary>
        /// Gets or sets XmlSchema
        /// </summary>
        public static XmlSchema XmlSchema
        {
            get { return s_xmlSchema; }
            set { if (s_xmlSchema == null) s_xmlSchema = value; }
        }
        private static XmlSchema s_xmlSchema;

        /// <summary>
        /// Gets whether the running code is running inside SLED</summary>
        public static bool IsSled
        {
            get
            {
                // Check once
                if (!s_bIsSledCheck)
                {
                    var assembly = Assembly.GetEntryAssembly();
                    s_bIsSled = assembly.IsDefined(typeof(SledAssemblyAttribute), false);
                    s_bIsSledCheck = true;
                }

                return s_bIsSled;
            }
        }
        private static bool s_bIsSledCheck;
        private static bool s_bIsSled;

        /// <summary>
        /// Gets whether SLED is running as a 64 bit process or not
        /// </summary>
        public static bool Is64Bit
        {
            get { return IntPtr.Size == 8; }
        }

        /// <summary>
        /// Register images with ATF
        /// </summary>
        public static void RegisterImages()
        {
            if (s_registeredImages)
                return;

            var assembly = Assembly.GetAssembly(typeof(SledShared));

            const string pngExt = ".png";
            const string icoExt = ".ico";
            const string period = ".";

            var dictImages =
                new Dictionary<string, string>
                    {
                        {SledIcon.DebugConnect, "debug_connect" + pngExt},
                        {SledIcon.DebugDisconnect, "debug_disconnect" + pngExt},
                        {SledIcon.DebugManageTargets, "debug_manage_targets" + pngExt},
                        {SledIcon.DebugStart, "debug_start_debug" + pngExt},
                        {SledIcon.DebugStepOut, "debug_stepOut_debug" + pngExt},
                        {SledIcon.DebugStepInto, "debug_step_debug" + pngExt},
                        {SledIcon.DebugStepOver, "debug_step_over_debug" + pngExt},
                        {SledIcon.DebugStop, "debug_stop_debug" + pngExt},
                        {SledIcon.FileClose, "file_close" + pngExt},
                        {SledIcon.FileNewLua, "file_new_lua" + pngExt},
                        {SledIcon.FileNewTxt, "file_new_txt" + pngExt},
                        {SledIcon.FileOpen, "file_open" + pngExt},
                        {SledIcon.ProjectFileAdd, "file_project_add" + pngExt},
                        {SledIcon.ProjectClose, "file_project_close" + pngExt},
                        {SledIcon.ProjectNew, "file_project_new" + pngExt},
                        {SledIcon.ProjectOpen, "file_project_open" + pngExt},
                        {SledIcon.ProjectFileRemove, "file_project_remove" + pngExt},
                        {SledIcon.ProjectSave, "file_project_save" + pngExt},
                        {SledIcon.ProjectSaveAs, "file_project_save_as" + pngExt},
                        {SledIcon.ProjectParseVariables, "project_parse_globals" + pngExt},
                        {SledIcon.ProjectSyntaxCheck, "project_syntax_check" + pngExt},
                        {SledIcon.ProjectToggleBreakpoint, "project_toggle_breakpoints" + pngExt},
                        {SledIcon.ProjectToggleMemoryTracer, "project_toggle_memory_trc" + pngExt},
                        {SledIcon.ProjectToggleProfiler, "project_toggle_profiler" + pngExt},
                        {SledIcon.Breakpoint, "bp" + pngExt},
                        {SledIcon.BreakpointHit, "bp_hit" + pngExt},
                        {SledIcon.BreakpointCsi, "bp_csi" + pngExt},
                        {SledIcon.Go, "go" + pngExt},
                        {SledIcon.Stop, "stop" + pngExt}
                    };

            foreach (var kv in dictImages)
            {
                var iPos = kv.Value.IndexOf(pngExt, StringComparison.Ordinal);

                var sz16 = kv.Value.Insert(iPos, "16");
                var sz24 = kv.Value.Insert(iPos, "24");
                var sz32 = kv.Value.Insert(iPos, "32");

                ResourceUtil.RegisterImage(
                    kv.Key,
                    GdiUtil.GetImage(assembly, IconPathBase + period + sz16),
                    GdiUtil.GetImage(assembly, IconPathBase + period + sz24),
                    GdiUtil.GetImage(assembly, IconPathBase + period + sz32));
            }

            using (var icon = GdiUtil.GetIcon(assembly, IconPathBase + period + "Sled" + icoExt))
            {
                ResourceUtil.RegisterImage(SledIcon.Sled, icon.ToBitmap());
            }

            s_registeredImages = true;
        }

        private static bool s_registeredImages;
    }
}
