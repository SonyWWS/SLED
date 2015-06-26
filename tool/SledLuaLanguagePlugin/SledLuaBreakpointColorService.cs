/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

using System;
using System.ComponentModel.Composition;
using System.Drawing;
using System.Linq;

using Sce.Atf;

using Sce.Sled.Shared.Dom;
using Sce.Sled.Shared.Document;
using Sce.Sled.Shared.Services;

namespace Sce.Sled.Lua
{
    [Export(typeof(IInitializable))]
    [Export(typeof(SledLuaBreakpointColorService))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    sealed class SledLuaBreakpointColorService : IInitializable
    {
        #region IInitializable Interface

        void IInitializable.Initialize()
        {
            m_debugService = SledServiceInstance.Get<ISledDebugService>();
            m_debugService.Connected += DebugServiceConnected;
            m_debugService.Disconnected += DebugServiceDisconnected;

            m_luaLanguagePlugin = SledServiceInstance.Get<SledLuaLanguagePlugin>();
            
            m_luaVariableParserService = SledServiceInstance.Get<ISledLuaVariableParserService>();
            m_luaVariableParserService.ParsedFiles += LuaVariableParserServiceParsedFiles;

            var breakpointService = SledServiceInstance.Get<ISledBreakpointService>();
            breakpointService.Added += BreakpointServiceAdded;
            breakpointService.SilentAdded += BreakpointServiceSilentAdded;
        }

        #endregion

        #region ISledDebugService Events

        private void DebugServiceConnected(object sender, SledDebugServiceEventArgs e)
        {
            ColorBreakpoints(true);
        }

        private void DebugServiceDisconnected(object sender, SledDebugServiceEventArgs e)
        {
            ColorBreakpoints(false);
        }

        #endregion

        #region ISledBreakpointService Events

        private void BreakpointServiceAdded(object sender, SledBreakpointServiceBreakpointEventArgs e)
        {
            if (!PassesBreakpointColorCheck(e.Breakpoint))
                return;

            ColorBreakpoint(e.Breakpoint.File.SledDocument, e.Breakpoint);
        }

        private void BreakpointServiceSilentAdded(object sender, SledBreakpointServiceBreakpointEventArgs e)
        {
            if (!PassesBreakpointColorCheck(e.Breakpoint))
                return;

            ColorBreakpoint(e.Breakpoint.File.SledDocument, e.Breakpoint);
        }

        #endregion

        #region ISledLuaVariableParserService Events

        private void LuaVariableParserServiceParsedFiles(object sender, EventArgs e)
        {
            ColorBreakpoints(m_debugService.IsConnected);
        }

        #endregion

        #region Member Methods

        private bool PassesBreakpointColorCheck(SledProjectFilesBreakpointType bp)
        {
            if (bp == null)
                return false;

            if (bp.File == null)
                return false;

            if (bp.File.LanguagePlugin != m_luaLanguagePlugin)
                return false;

            if (bp.File.SledDocument == null)
                return false;

            return true;
        }

        private void ColorBreakpoint(ISledDocument sd, SledProjectFilesBreakpointType bp)
        {
            if (!m_debugService.IsConnected)
                return;

            if (sd.SledProjectFile == null)
                return;

            // Grab parsed breakpoint line numbers associated with this file
            var parsedBreakpointsLineNumbers = m_luaVariableParserService.ValidBreakpointLineNumbers(sd.SledProjectFile);
            if (parsedBreakpointsLineNumbers == null)
                return;

            // If breakpoint line number not in list then mark it as invalid
            if (!parsedBreakpointsLineNumbers.Contains(bp.Line))
                bp.Breakpoint.BackColor = m_bpInvalidColor;
        }

        private void ColorBreakpoints(bool bConnected)
        {
            var files = m_luaVariableParserService.AllParsedFiles.ToList();
            if (!files.Any())
                return;

            foreach (var file in files)
            {
                // If corresponding document not open, skip
                if (file.SledDocument == null)
                    continue;

                // Get breakpoints from SledDocument
                var bps = file.SledDocument.Editor.GetBreakpoints();

                // If no breakpoints, skip
                if (bps.Length <= 0)
                    continue;

                // Pull out valid line numbers
                var parsedBpLineNumbers = m_luaVariableParserService.ValidBreakpointLineNumbers(file).ToList();
                if (!parsedBpLineNumbers.Any())
                    continue;

                foreach (var ibp in bps)
                {
                    if (bConnected && !parsedBpLineNumbers.Contains(ibp.LineNumber))
                        ibp.BackColor = m_bpInvalidColor;
                    else
                        ibp.BackColor = m_bpValidColor;
                }
            }
        }

        #endregion

        private ISledDebugService m_debugService;
        private SledLuaLanguagePlugin m_luaLanguagePlugin;
        private ISledLuaVariableParserService m_luaVariableParserService;

        private readonly Color m_bpInvalidColor = Color.Coral;
        private readonly Color m_bpValidColor = Color.Maroon;

        
    }
}
