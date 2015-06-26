/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;

using Sce.Atf;

using Sce.Sled.Shared.Dom;
using Sce.Sled.Shared.Plugin;
using Sce.Sled.Shared.Services;

namespace Sce.Sled
{
    /// <summary>
    /// SledDebugBreakpointService Class
    /// </summary>
    [Export(typeof(IInitializable))]
    [Export(typeof(SledDebugBreakpointService))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    class SledDebugBreakpointService : IInitializable
    {
        #region IInitializable Interface

        void IInitializable.Initialize()
        {
            m_debugService =
                SledServiceInstance.Get<ISledDebugService>();

            m_debugService.PluginsReady += DebugService_PluginsReady;

            m_projectService =
                SledServiceInstance.Get<ISledProjectService>();

            m_breakpointService =
                SledServiceInstance.Get<ISledBreakpointService>();

            m_breakpointService.Added += BreakpointService_Added;
            m_breakpointService.Changing += BreakpointService_Changing;
            m_breakpointService.Changed += BreakpointService_Changed;
            m_breakpointService.Removing += BreakpointService_Removing;
        }

        #endregion

        #region ISledDebugService Events

        private void DebugService_PluginsReady(object sender, SledDebugServiceEventArgs e)
        {
            var dictBreakpoints =
                new Dictionary<ISledLanguagePlugin, List<SledProjectFilesBreakpointType>>();

            // Gather all breakpoints
            foreach (var projFile in m_projectService.AllFiles)
            {
                if (projFile.LanguagePlugin == null)
                    continue;

                // Add breakpoints under appropriate language plugin
                if (dictBreakpoints.ContainsKey(projFile.LanguagePlugin))
                {
                    // Add to existing
                    dictBreakpoints[projFile.LanguagePlugin].AddRange(projFile.Breakpoints);
                }
                else
                {
                    // Add to new
                    dictBreakpoints.Add(
                        projFile.LanguagePlugin,
                        new List<SledProjectFilesBreakpointType>(projFile.Breakpoints));
                }
            }

            var plugins =
                SledServiceInstance.GetAll<ISledBreakpointPlugin>();

            // Figure out if any breakpoints need to be removed due to hard limits in plugins
            foreach (var plugin in plugins)
            {
                // Iterate through checking with each language plugin
                foreach (var kv in dictBreakpoints)
                {
                    if (plugin != kv.Key)
                        continue;

                    // Get maximum number of breakpoints for this plugin
                    int maxBreakpoints;
                    if (plugin.TryGetMax(out maxBreakpoints))
                    {
                        // If over maximum number of breakpoints remove
                        // until equal to maximum number of breakpoints
                        if (kv.Value.Count > maxBreakpoints)
                        {
                            while (kv.Value.Count > maxBreakpoints)
                            {
                                if (kv.Value.Count <= 0)
                                    break;

                                // Arbitrarily removing breakpoint from the end of the list
                                var pos = kv.Value.Count - 1;
                                var bp = kv.Value[pos];

                                try
                                {
                                    // Remove breakpoint from file
                                    m_bRemoving = true;
                                    bp.File.Breakpoints.Remove(bp);
                                }
                                finally
                                {
                                    m_bRemoving = false;
                                }

                                // Remove breakpoint from list
                                kv.Value.RemoveAt(pos);
                            }
                        }
                    }
                }
            }

            // Send breakpoints
            foreach (var kv in dictBreakpoints)
            {
                var enabledBps =
                    kv.Value.Where(bp => bp.Enabled);

                foreach (var bp in enabledBps)
                    SendBreakpoint(bp);
            }
        }

        #endregion

        #region ISledBreakpointService Events

        private void BreakpointService_Added(object sender, SledBreakpointServiceBreakpointEventArgs e)
        {
            SendBreakpoint(e.Breakpoint);
        }

        private void BreakpointService_Changing(object sender, SledBreakpointServiceBreakpointChangingEventArgs e)
        {
            var bShouldSend = true;
            var bEnabled = e.Breakpoint.Enabled;

            switch (e.ChangeType)
            {
                // Send these if the breakpoint is enabled
                case SledBreakpointChangeType.Condition:
                    bShouldSend = bEnabled;
                    break;

                // Send these if the breakpoint is enabled
                case SledBreakpointChangeType.ConditionEnabled:
                case SledBreakpointChangeType.ConditionDisabled:
                    bShouldSend = bEnabled;
                    break;

                // Send these if the breakpoint is enabled
                case SledBreakpointChangeType.ConditionResultTrue:
                case SledBreakpointChangeType.ConditionResultFalse:
                    bShouldSend = bEnabled;
                    break;

                // Don't send breakpoint enabled/disabled change twice
                case SledBreakpointChangeType.Disabled:
                case SledBreakpointChangeType.Enabled:
                    bShouldSend = false;
                    break;

                // Send if breakpoint is enabled
                case SledBreakpointChangeType.LineNumber:
                    bShouldSend = bEnabled;
                    break;

                // Send if breakpoint is enabled
                case SledBreakpointChangeType.UseFunctionEnvironmentTrue:
                case SledBreakpointChangeType.UseFunctionEnvironmentFalse:
                    bShouldSend = bEnabled;
                    break;
            }

            if (bShouldSend)
            {
                SendBreakpoint(e.Breakpoint);
            }
        }

        private void BreakpointService_Changed(object sender, SledBreakpointServiceBreakpointChangingEventArgs e)
        {
            var bShouldSend = true;
            var bEnabled = e.Breakpoint.Enabled;

            switch (e.ChangeType)
            {
                // Send these if the breakpoint is enabled
                case SledBreakpointChangeType.Condition:
                    bShouldSend = bEnabled;
                    break;

                // Send if breakpoint is enabled
                case SledBreakpointChangeType.ConditionEnabled:
                case SledBreakpointChangeType.ConditionDisabled:
                    bShouldSend = bEnabled;
                    break;

                // Send if breakpoint is enabled
                case SledBreakpointChangeType.ConditionResultTrue:
                case SledBreakpointChangeType.ConditionResultFalse:
                    bShouldSend = bEnabled;
                    break;

                // Send if breakpoint is enabled
                case SledBreakpointChangeType.LineNumber:
                    bShouldSend = bEnabled;
                    break;

                // Send if breakpoint is enabled
                case SledBreakpointChangeType.UseFunctionEnvironmentTrue:
                case SledBreakpointChangeType.UseFunctionEnvironmentFalse:
                    bShouldSend = bEnabled;
                    break;
            }

            if (bShouldSend)
            {
                SendBreakpoint(e.Breakpoint);
            }
        }

        private void BreakpointService_Removing(object sender, SledBreakpointServiceBreakpointEventArgs e)
        {
            if (m_bRemoving)
                return;

            // Don't send if not enabled
            if (!e.Breakpoint.Enabled)
                return;

            SendBreakpoint(e.Breakpoint);
        }

        private void SendBreakpoint(SledProjectFilesBreakpointType bp)
        {
            if (!m_debugService.IsConnected)
                return;

            if (bp.File == null)
                return;

            if (bp.File.LanguagePlugin == null)
                return;

            //
            // Gather breakpoint details
            //

            // Language plugin that is setting the breakpoint
            var languageId = bp.File.LanguagePlugin.LanguageId;

            // Relative path to script file breakpoint is in
            var relPath = bp.File.Path.Replace(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

            // Condition if any (only send the condition if the condition is enabled and it isn't null/empty!)
            var condition = (bp.ConditionEnabled && !string.IsNullOrEmpty(bp.Condition)) ? bp.Condition : string.Empty;

            // Create network breakpoint
            var netBp =
                new Shared.Scmp.BreakpointDetails(languageId, relPath, bp.Line, condition, bp.ConditionResult, bp.UseFunctionEnvironment);

            // Send breakpoint to target
            m_debugService.SendScmp(netBp);
        }

        #endregion

        private bool m_bRemoving;

        private ISledDebugService m_debugService;
        private ISledProjectService m_projectService;
        private ISledBreakpointService m_breakpointService;
    }
}
