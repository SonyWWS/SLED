/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

using System;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Windows.Forms;

using Sce.Atf;
using Sce.Atf.Applications;

using Sce.Sled.Resources;
using Sce.Sled.Shared.Document;
using Sce.Sled.Shared.Scmp;
using Sce.Sled.Shared.Services;
using Sce.Sled.Shared.Utilities;

namespace Sce.Sled
{
    /// <summary>
    /// SledDebugFileService Class
    /// </summary>
    [Export(typeof(IInitializable))]
    [Export(typeof(SledDebugFileService))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    class SledDebugFileService : IInitializable
    {
        [ImportingConstructor]
        public SledDebugFileService(MainForm mainForm)
        {
            m_mainForm = mainForm;
        }

        #region IInitializable Interface

        void IInitializable.Initialize()
        {
            m_debugService =
                SledServiceInstance.Get<ISledDebugService>();

            m_debugService.DebugConnect += DebugServiceDebugConnect;
            m_debugService.DebugDisconnect += DebugServiceDebugDisconnect;
            m_debugService.DebugStart += DebugServiceDebugStart;
            m_debugService.DebugCurrentStatement += DebugServiceDebugCurrentStatement;
            m_debugService.DebugStepInto += DebugServiceDebugStepInto;
            m_debugService.DebugStepOver += DebugServiceDebugStepOver;
            m_debugService.DebugStepOut += DebugServiceDebugStepOut;
            m_debugService.BreakpointHitting += DebugServiceBreakpointHitting;
            m_debugService.BreakpointHit += DebugServiceBreakpointHit;
            m_debugService.BreakpointContinue += DebugServiceBreakpointContinue;
            m_debugService.Disconnected += DebugServiceDisconnected;

            m_documentService =
                SledServiceInstance.Get<ISledDocumentService>();

            m_documentService.Opened += DocumentServiceOpened;
            m_documentService.Saving += DocumentServiceSaving;
            m_documentService.Saved += DocumentServiceSaved;
            m_documentService.Closing += DocumentServiceClosing;

            m_projectService =
                SledServiceInstance.Get<ISledProjectService>();

            m_projectService.FileAdded += ProjectServiceFileAdded;
            m_projectService.FileRemoving += ProjectServiceFileRemoving;
        }

        #endregion

        #region ISledDebugService Events

        private void DebugServiceDebugConnect(object sender, SledDebugServiceEventArgs e)
        {
            SaveOpenDocuments();
            MarkProjectDocsReadOnly(true);
        }

        private void DebugServiceDebugDisconnect(object sender, EventArgs e)
        {
            MarkProjectDocsReadOnly(false);
        }

        private void DebugServiceDebugStart(object sender, EventArgs e)
        {
            SaveOpenDocuments();
            MarkProjectDocsReadOnly(true);
        }

        private void DebugServiceDebugCurrentStatement(object sender, EventArgs e)
        {
            if (m_curBreakpoint == null)
                return;

            var szAbsPath = SledUtil.GetAbsolutePath(m_curBreakpoint.File, m_projectService.AssetDirectory);
            if (string.IsNullOrEmpty(szAbsPath))
                return;

            m_gotoService.Get.GotoLine(szAbsPath, m_curBreakpoint.Line, true);
        }

        private void DebugServiceDebugStepInto(object sender, EventArgs e)
        {
            SaveOpenDocuments();
            MarkProjectDocsReadOnly(true);
        }

        private void DebugServiceDebugStepOver(object sender, EventArgs e)
        {
            SaveOpenDocuments();
            MarkProjectDocsReadOnly(true);
        }

        private void DebugServiceDebugStepOut(object sender, EventArgs e)
        {
            SaveOpenDocuments();
            MarkProjectDocsReadOnly(true);
        }

        private void DebugServiceBreakpointHitting(object sender, SledDebugServiceBreakpointEventArgs e)
        {
            m_curBreakpoint = e.Breakpoint.Clone() as SledNetworkBreakpoint;

            if (m_curBreakpoint.IsUnknownFile())
                return;

            // Get absolute path to file
            var szAbsPath = SledUtil.GetAbsolutePath(e.Breakpoint.File, m_projectService.AssetDirectory);
            if (string.IsNullOrEmpty(szAbsPath) || !File.Exists(szAbsPath))
                return;

            // Check if file is in the project
            var projFile = m_projectFileFinderService.Get.Find(szAbsPath);

            // Try and add file to project
            if (projFile == null)
                m_projectService.AddFile(szAbsPath, out projFile);
        }

        private void DebugServiceBreakpointHit(object sender, SledDebugServiceBreakpointEventArgs e)
        {
            if (string.IsNullOrEmpty(e.Breakpoint.File))
            {
                // TODO: open a faux document saying something about unknown file?
                return;
            }

            // Get absolute path to file
            var szAbsPath = SledUtil.GetAbsolutePath(e.Breakpoint.File, m_projectService.AssetDirectory);
            if (!string.IsNullOrEmpty(szAbsPath) && File.Exists(szAbsPath))
            {
                ISledDocument sd;
                var uri = new Uri(szAbsPath);

                // If already open jump to line otherwise open the file
                if (m_documentService.IsOpen(uri, out sd))
                {
                    m_gotoService.Get.GotoLine(sd, m_curBreakpoint.Line, true);
                }
                else
                {
                    m_documentService.Open(uri, out sd);
                }
            }

            MarkProjectDocsReadOnly(false);
        }

        private void DebugServiceBreakpointContinue(object sender, SledDebugServiceBreakpointEventArgs e)
        {
            m_curBreakpoint = e.Breakpoint.Clone() as SledNetworkBreakpoint;

            RemoveCurrentStatementIndicators();
        }

        private void DebugServiceDisconnected(object sender, SledDebugServiceEventArgs e)
        {
            RemoveCurrentStatementIndicators();
            MarkProjectDocsReadOnly(false);
        }

        #endregion

        #region ISledDocumentService Events

        private void DocumentServiceOpened(object sender, SledDocumentServiceEventArgs e)
        {
            // We do a couple of things here:
            // 1) Draw current statement indicator if opened document matches breakpoint document
            // 2) Draw lock icon on documents opened while not stopped on a breakpoint

            var sd = e.Document;
            if (sd == null)
                return;

            // Don't care about non-project documents as they're not being debugged
            if (sd.SledProjectFile == null)
                return;

            // Don't care if disconnected
            if (m_debugService.IsDisconnected)
                return;

            if (m_debugService.IsDebugging)
            {
                // Not stopped on a breakpoint

                // Draw lock on document
                ((SledDocument)sd).IsReadOnly = true;
            }
            else
            {
                // Stopped on a breakpoint

                // Haven't hit a breakpoint yet so no where to jump to
                if (m_curBreakpoint == null)
                    return;

                // Check if the opened document is the breakpoint document
                // and if it is draw the current statement indicator and jump
                // to the breakpoint line
                if (string.Compare(sd.SledProjectFile.Path, m_curBreakpoint.File, true) == 0)
                {
                    // Jump to line & draw current statement indicator
                    m_gotoService.Get.GotoLine(e.Document, m_curBreakpoint.Line, true);
                }
            }
        }

        private void DocumentServiceSaving(object sender, SledDocumentServiceEventArgs e)
        {
            m_sd = null;

            // Not connected (so can't edit & continue)
            if (!m_debugService.IsConnected)
                return;

            // File not in project
            if (e.Document.SledProjectFile == null)
                return;

            // File has no changes so nothing to update
            if (!e.Document.Dirty)
                return;

            // Store for edit & continue
            m_sd = e.Document;
        }

        private void DocumentServiceSaved(object sender, SledDocumentServiceEventArgs e)
        {
            // No document to deal with
            if (m_sd == null)
                return;

            // Out of whack somehow?
            if (m_sd != e.Document)
                return;

            HandleEditAndContinue(m_sd);

            // Reset
            m_sd = null;
        }

        private void DocumentServiceClosing(object sender, SledDocumentServiceEventArgs e)
        {
            // Remove any current statement indicators
            RemoveCurrentStatementIndicators(e.Document);
        }

        #endregion

        #region ISledProjectService Events

        private void ProjectServiceFileRemoving(object sender, SledProjectServiceFileEventArgs e)
        {
            if (!m_debugService.IsDebugging)
                return;

            var sd = e.File.SledDocument;
            if (sd == null)
                return;

            ((SledDocument)sd).IsReadOnly = false;
        }

        private void ProjectServiceFileAdded(object sender, SledProjectServiceFileEventArgs e)
        {
            if (!m_debugService.IsDebugging)
                return;

            var sd = e.File.SledDocument;
            if (sd == null)
                return;

            ((SledDocument)sd).IsReadOnly = true;
        }

        #endregion

        #region Member Methods

        private void SaveOpenDocuments()
        {
            // Find any opened docs that are in the
            // project and also dirty
            var openedDirtyProjectDocs =
                from sd in m_documentService.OpenDocuments
                where sd.SledProjectFile != null && sd.Dirty
                select sd;
            
            foreach (var sd in openedDirtyProjectDocs)
                m_documentService.Save(sd);
        }

        private void MarkProjectDocsReadOnly(bool bReadOnly)
        {
            // Go through project documents telling them they need to
            // adjust their read only property and indicator(s)
            var openedProjectDocs =
                from sd in m_documentService.OpenDocuments
                where sd.SledProjectFile != null
                select sd;

            // Notify of read only state
            foreach (var sd in openedProjectDocs)
                ((SledDocument)sd).IsReadOnly = bReadOnly;
        }

        private void RemoveCurrentStatementIndicators()
        {
            // Remove all current statement indicators from open documents
            foreach (var sd in m_documentService.OpenDocuments)
            {
                RemoveCurrentStatementIndicators(sd);
            }
        }

        private static void RemoveCurrentStatementIndicators(ISledDocument sd)
        {
            if (sd == null)
                return;

            foreach (var line in sd.Editor.CurrentStatements)
            {
                sd.Editor.CurrentStatement(line, false);
            }
        }

        private void HandleEditAndContinue(ISledDocument sd)
        {
            var res =
                MessageBox.Show(
                    m_mainForm,
                    Localization.SledEditAndContinueReloadFileQuestion,
                    Localization.SledEditAndContinueTitle,
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

            if (res == DialogResult.Yes)
            {
                // Tell target to reload the file
                m_debugService.SendScmp(
                    new EditAndContinue(
                        sd.SledProjectFile.LanguagePlugin.LanguageId,
                        sd.SledProjectFile.Path.Replace(
                            Path.DirectorySeparatorChar,
                            Path.AltDirectorySeparatorChar)));
            }
            else
            {
                // Show message to user
                MessageBox.Show(
                    m_mainForm,
                    Localization.SledEditAndContinueFileNotSentError,
                    Localization.SledEditAndContinueTitle,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }
        }

        #endregion

        private ISledDocument m_sd;
        private SledNetworkBreakpoint m_curBreakpoint;

        private readonly MainForm m_mainForm;

        private ISledDebugService m_debugService;
        private ISledDocumentService m_documentService;
        private ISledProjectService m_projectService;

        private readonly SledServiceReference<ISledGotoService> m_gotoService =
            new SledServiceReference<ISledGotoService>();

        private readonly SledServiceReference<ISledProjectFileFinderService> m_projectFileFinderService =
            new SledServiceReference<ISledProjectFileFinderService>();
    }
}
