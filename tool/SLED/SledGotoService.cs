/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Windows.Forms;

using Sce.Atf;
using Sce.Atf.Applications;

using Sce.Sled.Resources;
using Sce.Sled.Shared;
using Sce.Sled.Shared.Document;
using Sce.Sled.Shared.Dom;
using Sce.Sled.Shared.Services;
using Sce.Sled.Shared.Utilities;
using Sce.Sled.SyntaxEditor;

namespace Sce.Sled
{
    /// <summary>
    /// SledGotoService Class
    /// </summary>
    [Export(typeof(IInitializable))]
    [Export(typeof(ISledGotoService))]
    [Export(typeof(SledGotoService))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    class SledGotoService : IInitializable, ISledGotoService
    {
        [ImportingConstructor]
        public SledGotoService(
            MainForm mainForm,
            IControlHostService controlHostService)
        {
            m_mainForm = mainForm;
            m_controlHostService = controlHostService;
        }

        #region IInitializable Interface

        void IInitializable.Initialize()
        {
            m_documentService =
                SledServiceInstance.Get<ISledDocumentService>();
        }

        #endregion

        #region ISledGotoService Interface

        /// <summary>
        /// Go to a specific line in a file
        /// </summary>
        /// <param name="sd">document</param>
        /// <param name="iLine">line in document</param>
        /// <param name="bUseCsi">whether to use a "current statement indicator" or not</param>
        public void GotoLine(ISledDocument sd, int iLine, bool bUseCsi)
        {
            GotoLineWord(sd, null, iLine, -1, bUseCsi);
        }

        /// <summary>
        /// Go to a specific word on a line in a file
        /// </summary>
        /// <param name="sd">document</param>
        /// <param name="szWord">word to find</param>
        /// <param name="iLine">line in document</param>
        /// <param name="iOccurence">if the word occurs multiple times on a line then this represents which occurence</param>
        /// <param name="bUseCsi">whether to use a "current statement indicator" or not</param>
        public void GotoLineWord(ISledDocument sd, string szWord, int iLine, int iOccurence, bool bUseCsi)
        {
            if (sd == null)
                return;

            // Bring this files tab to the front
            m_controlHostService.Show(sd.Control);

            try
            {
                if (iOccurence < 0)
                {
                    // Selecting whole line
                    sd.Editor.SelectLine(iLine);
                }
                else
                {
                    // Selecting part of line

                    // Try and select the word "name" on the line
                    var szLine = sd.Editor.GetLineText(iLine);

                    var iBeg = -1;
                    for (var i = 0; i < iOccurence; i++)
                        iBeg = szLine.IndexOf(szWord, iBeg + 1);

                    var iEnd = iBeg + szWord.Length - 1;

                    // Select
                    sd.Editor.SelectLine(iLine, iBeg, iEnd);
                }

                // Scroll to line
                sd.Editor.CurrentLineNumber = iLine;

                if (bUseCsi)
                    sd.Editor.CurrentStatement(iLine, true);
            }
            catch (Exception ex)
            {
                SledOutDevice.OutLine(
                    SledMessageType.Error,
                    SledUtil.TransSub(
                        Localization.SledGotoLineError1,
                        iLine, Path.GetFileName(sd.Uri.LocalPath), ex.Message));
            }

            // Now force focus to actually see the cursor on the newly selected line
            sd.Control.Focus();
        }

        /// <summary>
        /// Go to a specific line in a file
        /// </summary>
        /// <param name="szFile">absolute path to file</param>
        /// <param name="iLine">line in file</param>
        /// <param name="bUseCsi">whether to use a "current statement indicator" or not</param>
        public void GotoLine(string szFile, int iLine, bool bUseCsi)
        {
            ISledDocument sd;
            if (!m_documentService.Open(new Uri(szFile), out sd))
                return;

            GotoLine(sd, iLine, bUseCsi);
        }

        /// <summary>
        /// Go to a specific word on a line in a file
        /// </summary>
        /// <param name="szFile">absolute path to file</param>
        /// <param name="szWord">word to find</param>
        /// <param name="iLine">line in file</param>
        /// <param name="iOccurence">if the word occurs multiple times on a line then this represents which occurence</param>
        /// <param name="bUseCsi">whether to use a "current statement indicator" or not</param>
        public void GotoLineWord(string szFile, string szWord, int iLine, int iOccurence, bool bUseCsi)
        {
            ISledDocument sd;
            if (!m_documentService.Open(new Uri(szFile), out sd))
                return;

            GotoLineWord(sd, szWord, iLine, iOccurence, bUseCsi);
        }

        /// <summary>
        /// Go to a specific text range in a file and highlight the range
        /// <remarks>Used mainly for Find and Replace</remarks>
        /// </summary>
        /// <param name="szFile">file to goto</param>
        /// <param name="iStartOffset">start offset of text range</param>
        /// <param name="iEndOffset">end offset of text range</param>
        public void GotoFileAndHighlightRange(string szFile, int iStartOffset, int iEndOffset)
        {
            var uri = new Uri(szFile);

            ISledDocument sd;
            if (!m_documentService.Open(uri, out sd))
            {
                // Look in open documents
                if (!m_documentService.IsOpen(uri, out sd))
                    return;
            }

            // Bring this files tab to the front
            m_controlHostService.Show(sd.Control);

            try
            {
                // Try and highlight the text range
                sd.Editor.SelectRange = new SyntaxEditorTextRange(iStartOffset, iEndOffset);
            }
            catch (Exception ex)
            {
                ex.ToString();
            }
        }

        /// <summary>
        /// Go to a specific variable instance in a file
        /// </summary>
        /// <param name="var">variable to go to</param>
        public void GotoVariable(ISledVarBaseType var)
        {
            if (var == null)
                return;

            if (var.Locations.Count <= 0)
                return;

            if (var.Locations.Count == 1)
            {
                // Go to this specific one
                GotoLineWord(
                    var.Locations[0].File,
                    var.Name,
                    var.Locations[0].Line,
                    var.Locations[0].Occurence,
                    false);
            }
            else
            {
                var dictLocations =
                    new Dictionary<string, List<SledVarLocationType>>(StringComparer.CurrentCultureIgnoreCase);

                // Go through all locations grouping by file
                foreach (var loc in var.Locations)
                {
                    List<SledVarLocationType> lstLocs;
                    if (dictLocations.TryGetValue(loc.File, out lstLocs))
                    {
                        // Add to existing key
                        lstLocs.Add(loc);
                    }
                    else
                    {
                        // Create new key/value pair
                        lstLocs =
                            new List<SledVarLocationType> {loc};

                        dictLocations.Add(loc.File, lstLocs);
                    }
                }

                if (dictLocations.Count <= 0)
                    return;

                // Create variable goto form
                var form = new SledVarGotoForm();

                // Create one syntax editor for all the iterations
                using (var sec = TextEditorFactory.CreateSyntaxHighlightingEditor())
                {
                    // Go through each file pulling out locations
                    foreach (var kv in dictLocations)
                    {
                        StreamReader reader = null;

                        try
                        {
                            // Open the file
                            reader = new StreamReader(kv.Key, true);
                            if (reader != StreamReader.Null)
                            {
                                // Read entire file contents into SyntaxEditor
                                sec.Text = reader.ReadToEnd();

                                // Go through populating form
                                foreach (var loc in kv.Value)
                                {
                                    try
                                    {
                                        // Add location to the form
                                        form.AddLocation(loc, sec.GetLineText(loc.Line).Trim());
                                    }
                                    catch (Exception ex2)
                                    {
                                        SledOutDevice.OutLine(
                                            SledMessageType.Info,
                                            SledUtil.TransSub(Localization.SledGotoVariableError1, loc.Line, kv.Key, ex2.Message));
                                    }
                                }
                            }
                        }
                        catch (Exception ex1)
                        {
                            SledOutDevice.OutLine(
                                SledMessageType.Info,
                                SledUtil.TransSub(Localization.SledGotoVariableError2, kv.Key, ex1.Message));
                        }
                        finally
                        {
                            // Close up reader if everything went well
                            if ((reader != null) && (reader != StreamReader.Null))
                            {
                                reader.Close();
                                reader.Dispose();
                            }
                        }
                    }
                }

                if (form.ShowDialog(m_mainForm) == DialogResult.OK)
                {
                    // Go to this one
                    GotoLineWord(
                        form.SelectedLocation.File,
                        var.Name,
                        form.SelectedLocation.Line,
                        form.SelectedLocation.Occurence,
                        false);
                }

                form.Dispose();
            }
        }

        #endregion

        private readonly MainForm m_mainForm;
        private readonly IControlHostService m_controlHostService;

        private ISledDocumentService m_documentService;
    }
}
