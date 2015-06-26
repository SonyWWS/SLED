/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

using System.ComponentModel.Composition;

using Sce.Atf;

using Sce.Sled.Shared;
using Sce.Sled.Shared.Document;
using Sce.Sled.Shared.Plugin;
using Sce.Sled.Shared.Services;
using Sce.Sled.Shared.Utilities;

namespace Sce.Sled.Lua
{
    [Export(typeof(IInitializable))]
    [Export(typeof(ISledBreakpointPlugin))]
    [Export(typeof(SledLuaMaxBreakpointService))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    sealed class SledLuaMaxBreakpointService : IInitializable, ISledBreakpointPlugin
    {
        #region IInitializable Interface

        void IInitializable.Initialize()
        {
            m_luaLanguagePlugin = SledServiceInstance.Get<SledLuaLanguagePlugin>();

            m_debugService = SledServiceInstance.Get<ISledDebugService>();
            m_debugService.DataReady += DebugServiceDataReady;

            var projectService = SledServiceInstance.Get<ISledProjectService>();
            projectService.Closing += ProjectServiceClosing;
        }

        #endregion

        #region ISledBreakpointPlugin Interface

        /// <summary>
        /// Determines whether breakpoint can be added
        /// </summary>
        /// <param name="sd">Document (if any - might be null)</param>
        /// <param name="lineNumber">Line number of breakpoint</param>
        /// <param name="lineText">Text on line of breakpoint</param>
        /// <param name="numLanguagePluginBreakpoints">The current number of breakpoints that belong to the language plugin</param>
        /// <returns></returns>
        public bool CanAdd(ISledDocument sd, int lineNumber, string lineText, int numLanguagePluginBreakpoints)
        {
            // Allow all breakpoints to be added if disconnected
            if (m_debugService.IsDisconnected)
                return true;

            // Figure out if we can add the breakpoint & show message if not
            var bCanAdd = ((numLanguagePluginBreakpoints + 1) <= m_iMaxBreakpoints);
            if (!bCanAdd)
            {
                SledOutDevice.OutLine(
                    SledMessageType.Info,
                    SledUtil.TransSub(
                        "[%s0] Breakpoint cannot be added due to target setting " + 
                        "(will be over max allowed breakpoints: %s1).",
                        m_luaLanguagePlugin.LanguageName, m_iMaxBreakpoints));
            }

            return bCanAdd;
        }

        /// <summary>
        /// Get the maximum number of breakpoints allowed
        /// </summary>
        /// <param name="maxBreakpoints">Maximum number of breakpoints allowed</param>
        /// <returns>True if there's a maximum false if there is not</returns>
        public bool TryGetMax(out int maxBreakpoints)
        {
            maxBreakpoints = m_iMaxBreakpoints >= 1 ? m_iMaxBreakpoints : 0;
            return maxBreakpoints != 0;
        }

        #endregion

        #region ISledDebugService Events

        private void DebugServiceDataReady(object sender, SledDebugServiceEventArgs e)
        {
            var typeCode = (Scmp.LuaTypeCodes)e.Scmp.TypeCode;

            switch (typeCode)
            {
                case Scmp.LuaTypeCodes.LuaLimits:
                {
                    var scmp = m_debugService.GetScmpBlob<Scmp.LuaLimits>();

                    m_iMaxBreakpoints = scmp.MaxBreakpoints;
                    
                    SledOutDevice.OutLine(
                        SledMessageType.Info,
                        SledUtil.TransSub(
                            "[%s0] Maximum breakpoints set to %s1 on the target.",
                            m_luaLanguagePlugin.LanguageName, m_iMaxBreakpoints));
                }
                break;
            }
        }

        #endregion

        #region ISledProjectService Events

        private void ProjectServiceClosing(object sender, SledProjectServiceProjectEventArgs e)
        {
            m_iMaxBreakpoints = -1;
        }

        #endregion

        private ISledDebugService m_debugService;
        private SledLuaLanguagePlugin m_luaLanguagePlugin;

        private int m_iMaxBreakpoints = -1;
    }
}
