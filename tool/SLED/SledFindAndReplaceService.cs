/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml;

using Sce.Atf;
using Sce.Atf.Adaptation;
using Sce.Atf.Applications;
using Sce.Atf.Controls;
using Sce.Atf.Controls.PropertyEditing;
using Sce.Atf.Dom;

using Sce.Sled.Resources;
using Sce.Sled.Shared;
using Sce.Sled.Shared.Controls;
using Sce.Sled.Shared.Document;
using Sce.Sled.Shared.Dom;
using Sce.Sled.Shared.Plugin;
using Sce.Sled.Shared.Services;
using Sce.Sled.Shared.Utilities;
using Sce.Sled.SyntaxEditor;

namespace Sce.Sled
{
    [Export(typeof(IInitializable))]
    [Export(typeof(ISledDocumentPlugin))]
    [Export(typeof(ISledFindAndReplaceService))]
    [Export(typeof(SledFindAndReplaceService))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    class SledFindAndReplaceService : IPartImportsSatisfiedNotification, IInitializable, ISledFindAndReplaceService, ISledDocumentPlugin, ICommandClient
    {
        [ImportingConstructor]
        public SledFindAndReplaceService(
            MainForm mainForm,
            ICommandService commandService,
            ISettingsService settingsService,
            IControlHostService controlHostService)
        {
            m_mainForm = mainForm;
            m_mainForm.Shown += MainFormShown;
            m_mainForm.FormClosing += MainFormFormClosing;

            // Register find and replace -> quick find command
            commandService.RegisterCommand(
                Command.QuickFind,
                StandardMenu.Edit,
                CommandGroup.FindAndReplace,
                Localization.SledCommandFindReplace + Resources.Resource.MenuSeparator + Localization.SledCommandFindReplaceQuickFind,
                Localization.SledCommandFindReplaceQuickFindComment,
                Keys.Control | Keys.F,
                Atf.Resources.FindImage,
                CommandVisibility.Menu,
                this);

            // Register find and replace -> find in files command
            commandService.RegisterCommand(
                Command.FindInFiles,
                StandardMenu.Edit,
                CommandGroup.FindAndReplace,
                Localization.SledCommandFindReplace + Resources.Resource.MenuSeparator + Localization.SledCommandFindReplaceFindInFiles,
                Localization.SledCommandFindReplaceFindInFilesComment,
                Keys.Control | Keys.Shift | Keys.F,
                Atf.Resources.FindImage,
                CommandVisibility.Menu,
                this);

            // Register find and replace -> quick replace command
            commandService.RegisterCommand(
                Command.QuickReplace,
                StandardMenu.Edit,
                CommandGroup.FindAndReplace,
                Localization.SledCommandFindReplace + Resources.Resource.MenuSeparator + Localization.SledCommandFindReplaceQuickReplace,
                Localization.SledCommandFindReplaceQuickReplaceComment,
                Keys.Control | Keys.H,
                Atf.Resources.FindImage,
                CommandVisibility.Menu,
                this);

            // Register find and replace -> replace in files command
            commandService.RegisterCommand(
                Command.ReplaceInFiles,
                StandardMenu.Edit,
                CommandGroup.FindAndReplace,
                Localization.SledCommandFindReplace + Resources.Resource.MenuSeparator + Localization.SledCommandFindReplaceReplaceInFiles,
                Localization.SledCommandFindReplaceReplaceInFilesComment,
                Keys.Control | Keys.Shift | Keys.H,
                Atf.Resources.FindImage,
                CommandVisibility.Menu,
                this);

            // Register F3 to repeat the last search [w/o showing the GUI]
            commandService.RegisterCommand(
                Command.RepeatLastSearchWithoutGui,
                StandardMenu.Edit,
                CommandGroup.FindAndReplace,
                Localization.SledCommandFindReplace + Resources.Resource.MenuSeparator + Localization.SledCommandFindReplaceRepeatLastSearchWithoutGui,
                Localization.SledCommandFindReplaceRepeatLastSearchWithoutGuiComment,
                Keys.F3,
                null,
                CommandVisibility.None,
                this);

            // Register Ctrl+F3 to search for next occurrence of the current selected text [w/o showing the GUI]
            commandService.RegisterCommand(
                Command.FindSelectedTextWithoutGui,
                StandardMenu.Edit,
                CommandGroup.FindAndReplace,
                Localization.SledCommandFindReplace + Resources.Resource.MenuSeparator + Localization.SledCommandFindReplaceFindNextInstanceOfSelectedTextWithoutGUI,
                Localization.SledCommandFindReplaceFindNextInstanceOfSelectedTextWithoutGUIComment,
                Keys.Control | Keys.F3,
                null,
                CommandVisibility.None,
                this);

            // Register Shift+F3 to repeat the last search up [w/o showing the GUI]
            commandService.RegisterCommand(
                Command.RepeatLastSearchUpWithoutGui,
                StandardMenu.Edit,
                CommandGroup.FindAndReplace,
                Localization.SledCommandFindReplace + Resources.Resource.MenuSeparator + Localization.SledCommandFindReplaceRepeatLastSearchUpWithoutGui,
                Localization.SledCommandFindReplaceRepeatLastSearchUpWithoutGuiComment,
                Keys.Shift | Keys.F3,
                null,
                CommandVisibility.None,
                this);

            // Register Shift+Ctrl+F3 to search for next occurrence up of the current selected text [w/o showing the GUI]
            commandService.RegisterCommand(
                Command.FindSelectedTextUpWithoutGui,
                StandardMenu.Edit,
                CommandGroup.FindAndReplace,
                Localization.SledCommandFindReplace + Resources.Resource.MenuSeparator + Localization.SledCommandFindReplaceFindNextInstanceOfSelectedTextUpWithoutGUI,
                Localization.SledCommandFindReplaceFindNextInstanceOfSelectedTextUpWithoutGUIComment,
                Keys.Shift | Keys.Control | Keys.F3,
                null,
                CommandVisibility.None,
                this);

            // Save find & replace stuff
            settingsService.RegisterSettings(
                this,
                new BoundPropertyDescriptor(
                    this,
                    () => FindAndReplaceSettings,
                    Resources.Resource.FindAndReplaceSettingsTitle,
                    Resources.Resource.Project,
                    Resources.Resource.FindAndReplaceSettingsComment));
        }

        #region IPartImportsSatisfiedNotification Interface

        void IPartImportsSatisfiedNotification.OnImportsSatisfied()
        {
            m_quickFind = new QuickFind { GotoService = m_gotoService, DocumentService = m_documentService };

            m_findInFiles = new FindInFiles();

            m_quickReplace = new QuickReplace { GotoService = m_gotoService, DocumentService = m_documentService };

            m_replaceInFiles = new ReplaceInFiles { DocumentService = m_documentService };

            DocumentList.DocumentService = m_documentService;
            DocumentList.ProjectService = m_projectService;

            DocumentStateAssociation.DocumentService = m_documentService;
        }

        #endregion

        #region IInitializable Interface

        void IInitializable.Initialize()
        {
        }

        #endregion

        #region Commands

        enum Command
        {
            QuickFind,
            FindInFiles,
            QuickReplace,
            ReplaceInFiles,

            RepeatLastSearchWithoutGui,
            FindSelectedTextWithoutGui,

            RepeatLastSearchUpWithoutGui,
            FindSelectedTextUpWithoutGui,
        }

        enum CommandGroup
        {
            FindAndReplace,
        }

        #endregion

        #region ICommandClient Interface

        public bool CanDoCommand(object commandTag)
        {
            if (!(commandTag is Command))
                return false;

            var bEnabled = false;

            var cmd = (Command)commandTag;
            switch (cmd)
            {
                case Command.QuickFind:
                case Command.FindInFiles:
                case Command.QuickReplace:
                case Command.ReplaceInFiles:
                    bEnabled = true;
                    break;

                case Command.RepeatLastSearchWithoutGui:
                case Command.RepeatLastSearchUpWithoutGui:
                    bEnabled = (s_lastSearchEventArgs != null);
                    break;

                case Command.FindSelectedTextWithoutGui:
                case Command.FindSelectedTextUpWithoutGui:
                    bEnabled = m_documentService.Active && m_documentService.ActiveDocument.HasSelection;
                    break;
            }

            return bEnabled;
        }

        public void DoCommand(object commandTag)
        {
            if (!(commandTag is Command))
                return;

            var initialText =
                (m_documentService.Active) &&
                (m_documentService.ActiveDocument.HasSelection)
                    ? m_documentService.ActiveDocument.Selection
                    : string.Empty;

            var cmd = (Command)commandTag;
            switch (cmd)
            {
                case Command.QuickFind:
                    ShowDialog(SledFindAndReplaceModes.QuickFind, initialText);
                    break;

                case Command.FindInFiles:
                    ShowDialog(SledFindAndReplaceModes.FindInFiles, initialText);
                    break;

                case Command.QuickReplace:
                    ShowDialog(SledFindAndReplaceModes.QuickReplace, initialText);
                    break;

                case Command.ReplaceInFiles:
                    ShowDialog(SledFindAndReplaceModes.ReplaceInFiles, initialText);
                    break;

                case Command.RepeatLastSearchWithoutGui:
                    ChangeSearchDirection(ref s_lastSearchEventArgs, false);
                    Run(s_lastSearchEventArgs);
                    break;

                case Command.RepeatLastSearchUpWithoutGui:
                    ChangeSearchDirection(ref s_lastSearchEventArgs, true);
                    Run(s_lastSearchEventArgs);
                    break;

                case Command.FindSelectedTextWithoutGui:
                case Command.FindSelectedTextUpWithoutGui:
                {
                    // Construct a QuickFind search object and pass it off to Run()

                    var ea =
                        new SledFindAndReplaceEventArgs.QuickFind(
                            initialText,
                            m_quickFind.LastLookIn,
                            true,
                            false,
                            cmd == Command.FindSelectedTextWithoutGui ? false : true,
                            SledFindAndReplaceSearchType.Normal);

                    Run(ea);
                }
                break;
            }
        }

        public void UpdateCommand(object commandTag, CommandState state)
        {
        }

        #endregion

        #region ISledFindAndReplaceService Interface

        /// <summary>
        /// Show a particular find and replace dialog
        /// </summary>
        /// <param name="dlg">Dialog to show</param>
        public void ShowDialog(SledFindAndReplaceModes dlg)
        {
            ShowDialog(dlg, string.Empty);
        }

        /// <summary>
        /// Show a particular find and replace dialog
        /// </summary>
        /// <param name="dlg">Dialog to show</param>
        /// <param name="initialText">Initial text to add to the find or replace box</param>
        public void ShowDialog(SledFindAndReplaceModes dlg, string initialText)
        {
            SledFindAndReplaceForm.Instance.Mode = dlg;
            SledFindAndReplaceForm.Instance.InitialText =
                string.IsNullOrEmpty(initialText)
                    ? string.Empty
                    : initialText;
            SledFindAndReplaceForm.Instance.Show(m_mainForm);
        }

        #endregion

        #region Persisted Settings

        /// <summary>
        /// Persist find & replace settings
        /// </summary>
        public string FindAndReplaceSettings
        {
            get
            {
                var xmlDoc = new XmlDocument();
                xmlDoc.AppendChild(xmlDoc.CreateXmlDeclaration(Resources.Resource.OnePointZero, Resources.Resource.UtfDashEight, Resources.Resource.YesLower));
                var root = xmlDoc.CreateElement(Resources.Resource.FindAndReplaceSettings);
                xmlDoc.AppendChild(root);

                try
                {
                    // Save form starting position
                    if (!SledFindAndReplaceForm.StartLocation.IsEmpty)
                    {
                        var elem = xmlDoc.CreateElement("StartLocation");
                        elem.SetAttribute("X", SledFindAndReplaceForm.StartLocation.X.ToString());
                        elem.SetAttribute("Y", SledFindAndReplaceForm.StartLocation.Y.ToString());
                        root.AppendChild(elem);
                    }

                    foreach (var findWhat in SledFindAndReplaceSettings.GlobalFindWhat.Items)
                    {
                        var elem = xmlDoc.CreateElement("GlobalFindWhat");
                        elem.SetAttribute("FindWhat", findWhat);
                        root.AppendChild(elem);
                    }

                    foreach (var replaceWith in SledFindAndReplaceSettings.GlobalReplaceWith.Items)
                    {
                        var elem = xmlDoc.CreateElement("GlobalReplaceWith");
                        elem.SetAttribute("ReplaceWith", replaceWith);
                        root.AppendChild(elem);
                    }

                    // Quick Find Settings
                    var elemQuickFind = xmlDoc.CreateElement("QuickFindSettings");
                    elemQuickFind.SetAttribute("LookInIndex", SledFindAndReplaceSettings.QuickFind.LookInIndex.ToString());
                    elemQuickFind.SetAttribute("FindOptionsExpanded", SledFindAndReplaceSettings.QuickFind.FindOptionsExpanded.ToString());
                    elemQuickFind.SetAttribute("MatchCaseChecked", SledFindAndReplaceSettings.QuickFind.MatchCaseChecked.ToString());
                    elemQuickFind.SetAttribute("MatchWholeWordChecked", SledFindAndReplaceSettings.QuickFind.MatchWholeWordChecked.ToString());
                    elemQuickFind.SetAttribute("SearchUpChecked", SledFindAndReplaceSettings.QuickFind.SearchUpChecked.ToString());
                    elemQuickFind.SetAttribute("UseChecked", SledFindAndReplaceSettings.QuickFind.UseChecked.ToString());
                    elemQuickFind.SetAttribute("UseIndex", SledFindAndReplaceSettings.QuickFind.UseIndex.ToString());
                    root.AppendChild(elemQuickFind);

                    // Find in Files settings
                    var elemFindInFiles = xmlDoc.CreateElement("FindInFilesSettings");
                    elemFindInFiles.SetAttribute("LookInIndex", SledFindAndReplaceSettings.FindInFiles.LookInIndex.ToString());
                    elemFindInFiles.SetAttribute("IncludeSubFolders", SledFindAndReplaceSettings.FindInFiles.IncludeSubFolders.ToString());
                    elemFindInFiles.SetAttribute("FindOptionsExpanded", SledFindAndReplaceSettings.FindInFiles.FindOptionsExpanded.ToString());
                    elemFindInFiles.SetAttribute("MatchCaseChecked", SledFindAndReplaceSettings.FindInFiles.MatchCaseChecked.ToString());
                    elemFindInFiles.SetAttribute("MatchWholeWordChecked", SledFindAndReplaceSettings.FindInFiles.MatchWholeWordChecked.ToString());
                    elemFindInFiles.SetAttribute("UseChecked", SledFindAndReplaceSettings.FindInFiles.UseChecked.ToString());
                    elemFindInFiles.SetAttribute("UseIndex", SledFindAndReplaceSettings.FindInFiles.UseIndex.ToString());
                    elemFindInFiles.SetAttribute("ResultsOptionsExpanded", SledFindAndReplaceSettings.FindInFiles.ResultOptionsExpanded.ToString());
                    elemFindInFiles.SetAttribute("Results1WindowChecked", SledFindAndReplaceSettings.FindInFiles.Results1WindowChecked.ToString());
                    elemFindInFiles.SetAttribute("DisplayFileNamesOnlyChecked", SledFindAndReplaceSettings.FindInFiles.DisplayFileNamesOnlyChecked.ToString());
                    root.AppendChild(elemFindInFiles);

                    // Quick Replace Settings
                    var elemQuickReplace = xmlDoc.CreateElement("QuickReplaceSettings");
                    elemQuickReplace.SetAttribute("LookInIndex", SledFindAndReplaceSettings.QuickReplace.LookInIndex.ToString());
                    elemQuickReplace.SetAttribute("FindOptionsExpanded", SledFindAndReplaceSettings.QuickReplace.FindOptionsExpanded.ToString());
                    elemQuickReplace.SetAttribute("MatchCaseChecked", SledFindAndReplaceSettings.QuickReplace.MatchCaseChecked.ToString());
                    elemQuickReplace.SetAttribute("MatchWholeWordChecked", SledFindAndReplaceSettings.QuickReplace.MatchWholeWordChecked.ToString());
                    elemQuickReplace.SetAttribute("SearchUpChecked", SledFindAndReplaceSettings.QuickReplace.SearchUpChecked.ToString());
                    elemQuickReplace.SetAttribute("UseChecked", SledFindAndReplaceSettings.QuickReplace.UseChecked.ToString());
                    elemQuickReplace.SetAttribute("UseIndex", SledFindAndReplaceSettings.QuickReplace.UseIndex.ToString());
                    root.AppendChild(elemQuickReplace);

                    // Replace in Files settings
                    var elemReplaceInFiles = xmlDoc.CreateElement("ReplaceInFilesSettings");
                    elemReplaceInFiles.SetAttribute("LookInIndex", SledFindAndReplaceSettings.ReplaceInFiles.LookInIndex.ToString());
                    elemReplaceInFiles.SetAttribute("IncludeSubFolders", SledFindAndReplaceSettings.ReplaceInFiles.IncludeSubFolders.ToString());
                    elemReplaceInFiles.SetAttribute("FindOptionsExpanded", SledFindAndReplaceSettings.ReplaceInFiles.FindOptionsExpanded.ToString());
                    elemReplaceInFiles.SetAttribute("MatchCaseChecked", SledFindAndReplaceSettings.ReplaceInFiles.MatchCaseChecked.ToString());
                    elemReplaceInFiles.SetAttribute("MatchWholeWordChecked", SledFindAndReplaceSettings.ReplaceInFiles.MatchWholeWordChecked.ToString());
                    elemReplaceInFiles.SetAttribute("UseChecked", SledFindAndReplaceSettings.ReplaceInFiles.UseChecked.ToString());
                    elemReplaceInFiles.SetAttribute("UseIndex", SledFindAndReplaceSettings.ReplaceInFiles.UseIndex.ToString());
                    elemReplaceInFiles.SetAttribute("ResultsOptionsExpanded", SledFindAndReplaceSettings.ReplaceInFiles.ResultOptionsExpanded.ToString());
                    elemReplaceInFiles.SetAttribute("Results1WindowChecked", SledFindAndReplaceSettings.ReplaceInFiles.Results1WindowChecked.ToString());
                    elemReplaceInFiles.SetAttribute("DisplayFileNamesOnlyChecked", SledFindAndReplaceSettings.ReplaceInFiles.DisplayFileNamesOnlyChecked.ToString());
                    elemReplaceInFiles.SetAttribute("KeepModifiedFilesOpen", SledFindAndReplaceSettings.ReplaceInFiles.KeepModifiedFilesOpen.ToString());
                    root.AppendChild(elemReplaceInFiles);

                    if (xmlDoc.DocumentElement == null)
                        xmlDoc.RemoveAll();
                    else if (xmlDoc.DocumentElement.ChildNodes.Count == 0)
                        xmlDoc.RemoveAll();
                }
                catch (Exception ex)
                {
                    xmlDoc.RemoveAll();

                    var szSetting = Resources.Resource.FindAndReplaceSettings;
                    SledOutDevice.OutLine(
                        SledMessageType.Info,
                        SledUtil.TransSub(Localization.SledSettingsErrorExceptionSavingSetting, szSetting, ex.Message));
                }

                return xmlDoc.InnerXml.Trim();
            }

            set
            {
                try
                {
                    var xmlDoc = new XmlDocument();
                    xmlDoc.LoadXml(value);

                    if (xmlDoc.DocumentElement == null)
                        return;

                    // Pull out form starting position
                    var nodesStartLocation = xmlDoc.DocumentElement.SelectNodes("StartLocation");
                    if ((nodesStartLocation != null) && (nodesStartLocation.Count > 0))
                    {
                        foreach (XmlElement elem in nodesStartLocation)
                        {
                            var posX = elem.GetAttribute("X");
                            var posY = elem.GetAttribute("Y");
                            int x, y;
                            if (!string.IsNullOrEmpty(posX) &&
                                !string.IsNullOrEmpty(posY) &&
                                int.TryParse(posX, out x) &&
                                int.TryParse(posY, out y))
                            {
                                SledFindAndReplaceForm.StartLocation = new Point(x, y);
                            }

                            break;
                        }
                    }

                    // Fill in Global Find What list
                    var nodesFindWhat = xmlDoc.DocumentElement.SelectNodes("GlobalFindWhat");
                    if ((nodesFindWhat != null) && (nodesFindWhat.Count > 0))
                    {
                        var lstItems =
                            (from XmlElement elem in nodesFindWhat
                             select elem.GetAttribute("FindWhat")).ToList();

                        lstItems.Reverse();

                        SledFindAndReplaceSettings.GlobalFindWhat.Clear();
                        foreach (var findWhat in lstItems)
                            SledFindAndReplaceSettings.GlobalFindWhat.Add(findWhat);
                    }

                    // Fill in Global Replace With list
                    var nodesReplaceWith = xmlDoc.DocumentElement.SelectNodes("GlobalReplaceWith");
                    if ((nodesReplaceWith != null) && (nodesReplaceWith.Count > 0))
                    {
                        var lstItems =
                            (from XmlElement elem in nodesReplaceWith
                             select elem.GetAttribute("ReplaceWith")).ToList();

                        lstItems.Reverse();

                        SledFindAndReplaceSettings.GlobalReplaceWith.Clear();
                        foreach (var replaceWith in lstItems)
                            SledFindAndReplaceSettings.GlobalReplaceWith.Add(replaceWith);
                    }

                    // Grab Quick Find specific settings
                    var nodesQuickFind = xmlDoc.DocumentElement.SelectNodes("QuickFindSettings");
                    if ((nodesQuickFind != null) && (nodesQuickFind.Count > 0))
                    {
                        foreach (XmlElement elem in nodesQuickFind)
                        {
                            int iLookInIdx;
                            if (int.TryParse(elem.GetAttribute("LookInIndex"), out iLookInIdx))
                                SledFindAndReplaceSettings.QuickFind.LookInIndex = iLookInIdx;
                            SledFindAndReplaceSettings.QuickFind.FindOptionsExpanded =
                                (string.Compare(elem.GetAttribute("FindOptionsExpanded"), "true", true) == 0);
                            SledFindAndReplaceSettings.QuickFind.MatchCaseChecked =
                                (string.Compare(elem.GetAttribute("MatchCaseChecked"), "true", true) == 0);
                            SledFindAndReplaceSettings.QuickFind.MatchWholeWordChecked =
                                (string.Compare(elem.GetAttribute("MatchWholeWordChecked"), "true", true) == 0);
                            SledFindAndReplaceSettings.QuickFind.SearchUpChecked =
                                (string.Compare(elem.GetAttribute("SearchUpChecked"), "true", true) == 0);
                            SledFindAndReplaceSettings.QuickFind.UseChecked =
                                (string.Compare(elem.GetAttribute("UseChecked"), "true", true) == 0);
                            int iUseIdx;
                            if (int.TryParse(elem.GetAttribute("UseIndex"), out iUseIdx))
                                SledFindAndReplaceSettings.QuickFind.UseIndex = iUseIdx;
                        }
                    }

                    // Grab Find in Files specific settings
                    var nodesFindInFiles = xmlDoc.DocumentElement.SelectNodes("FindInFilesSettings");
                    if ((nodesFindInFiles != null) && (nodesFindInFiles.Count > 0))
                    {
                        foreach (XmlElement elem in nodesFindInFiles)
                        {
                            int iLookInIdx;
                            if (int.TryParse(elem.GetAttribute("LookInIndex"), out iLookInIdx))
                                SledFindAndReplaceSettings.FindInFiles.LookInIndex = iLookInIdx;
                            SledFindAndReplaceSettings.FindInFiles.IncludeSubFolders =
                                (string.Compare(elem.GetAttribute("IncludeSubFolders"), "true", true) == 0);
                            SledFindAndReplaceSettings.FindInFiles.FindOptionsExpanded =
                                (string.Compare(elem.GetAttribute("FindOptionsExpanded"), "true", true) == 0);
                            SledFindAndReplaceSettings.FindInFiles.MatchCaseChecked =
                                (string.Compare(elem.GetAttribute("MatchCaseChecked"), "true", true) == 0);
                            SledFindAndReplaceSettings.FindInFiles.MatchWholeWordChecked =
                                (string.Compare(elem.GetAttribute("MatchWholeWordChecked"), "true", true) == 0);
                            SledFindAndReplaceSettings.FindInFiles.UseChecked =
                                (string.Compare(elem.GetAttribute("UseChecked"), "true", true) == 0);
                            int iUseIdx;
                            if (int.TryParse(elem.GetAttribute("UseIndex"), out iUseIdx))
                                SledFindAndReplaceSettings.FindInFiles.UseIndex = iUseIdx;
                            SledFindAndReplaceSettings.FindInFiles.ResultOptionsExpanded =
                                (string.Compare(elem.GetAttribute("ResultsOptionsExpanded"), "true", true) == 0);
                            SledFindAndReplaceSettings.FindInFiles.Results1WindowChecked =
                                (string.Compare(elem.GetAttribute("Results1WindowChecked"), "true", true) == 0);
                            SledFindAndReplaceSettings.FindInFiles.DisplayFileNamesOnlyChecked =
                                (string.Compare(elem.GetAttribute("DisplayFileNamesOnlyChecked"), "true", true) == 0);
                        }
                    }

                    // Grab Quick Replace settings
                    var nodesQuickReplace = xmlDoc.DocumentElement.SelectNodes("QuickReplaceSettings");
                    if ((nodesQuickReplace != null) && (nodesQuickReplace.Count > 0))
                    {
                        foreach (XmlElement elem in nodesQuickReplace)
                        {
                            int iLookInIdx;
                            if (int.TryParse(elem.GetAttribute("LookInIndex"), out iLookInIdx))
                                SledFindAndReplaceSettings.QuickReplace.LookInIndex = iLookInIdx;
                            SledFindAndReplaceSettings.QuickReplace.FindOptionsExpanded =
                                (string.Compare(elem.GetAttribute("FindOptionsExpanded"), "true", true) == 0);
                            SledFindAndReplaceSettings.QuickReplace.MatchCaseChecked =
                                (string.Compare(elem.GetAttribute("MatchCaseChecked"), "true", true) == 0);
                            SledFindAndReplaceSettings.QuickReplace.MatchWholeWordChecked =
                                (string.Compare(elem.GetAttribute("MatchWholeWordChecked"), "true", true) == 0);
                            SledFindAndReplaceSettings.QuickReplace.SearchUpChecked =
                                (string.Compare(elem.GetAttribute("SearchUpChecked"), "true", true) == 0);
                            SledFindAndReplaceSettings.QuickReplace.UseChecked =
                                (string.Compare(elem.GetAttribute("UseChecked"), "true", true) == 0);
                            int iUseIdx;
                            if (int.TryParse(elem.GetAttribute("UseIndex"), out iUseIdx))
                                SledFindAndReplaceSettings.QuickReplace.UseIndex = iUseIdx;
                        }
                    }

                    // Grab Replace in Files specific settings
                    var nodesReplaceInFiles = xmlDoc.DocumentElement.SelectNodes("ReplaceInFilesSettings");
                    if ((nodesReplaceInFiles != null) && (nodesReplaceInFiles.Count > 0))
                    {
                        foreach (XmlElement elem in nodesReplaceInFiles)
                        {
                            int iLookInIdx;
                            if (int.TryParse(elem.GetAttribute("LookInIndex"), out iLookInIdx))
                                SledFindAndReplaceSettings.ReplaceInFiles.LookInIndex = iLookInIdx;
                            SledFindAndReplaceSettings.ReplaceInFiles.IncludeSubFolders =
                                (string.Compare(elem.GetAttribute("IncludeSubFolders"), "true", true) == 0);
                            SledFindAndReplaceSettings.ReplaceInFiles.FindOptionsExpanded =
                                (string.Compare(elem.GetAttribute("FindOptionsExpanded"), "true", true) == 0);
                            SledFindAndReplaceSettings.ReplaceInFiles.MatchCaseChecked =
                                (string.Compare(elem.GetAttribute("MatchCaseChecked"), "true", true) == 0);
                            SledFindAndReplaceSettings.ReplaceInFiles.MatchWholeWordChecked =
                                (string.Compare(elem.GetAttribute("MatchWholeWordChecked"), "true", true) == 0);
                            SledFindAndReplaceSettings.ReplaceInFiles.UseChecked =
                                (string.Compare(elem.GetAttribute("UseChecked"), "true", true) == 0);
                            int iUseIdx;
                            if (int.TryParse(elem.GetAttribute("UseIndex"), out iUseIdx))
                                SledFindAndReplaceSettings.ReplaceInFiles.UseIndex = iUseIdx;
                            SledFindAndReplaceSettings.ReplaceInFiles.ResultOptionsExpanded =
                                (string.Compare(elem.GetAttribute("ResultsOptionsExpanded"), "true", true) == 0);
                            SledFindAndReplaceSettings.ReplaceInFiles.Results1WindowChecked =
                                (string.Compare(elem.GetAttribute("Results1WindowChecked"), "true", true) == 0);
                            SledFindAndReplaceSettings.ReplaceInFiles.DisplayFileNamesOnlyChecked =
                                (string.Compare(elem.GetAttribute("DisplayFileNamesOnlyChecked"), "true", true) == 0);
                            SledFindAndReplaceSettings.ReplaceInFiles.KeepModifiedFilesOpen =
                                (string.Compare(elem.GetAttribute("KeepModifiedFilesOpen"), "true", true) == 0);
                        }
                    }
                }
                catch (Exception ex)
                {
                    var szSetting = Resources.Resource.FindAndReplaceSettings;
                    SledOutDevice.OutLine(
                        SledMessageType.Info,
                        SledUtil.TransSub(Localization.SledSettingsErrorExceptionLoadingSetting, szSetting, ex.Message));
                }
            }
        }

        #endregion

        #region ISledDocumentPlugin Interface

        /// <summary>
        /// Gets context menu command tags for the target SledDocument
        /// </summary>
        /// <param name="args">Arguments (document, region clicked, line number clicked)</param>
        /// <returns>List of context menu command tags for the target SledDocument</returns>
        public IList<object> GetPopupCommandTags(SledDocumentContextMenuArgs args)
        {
            var commands =
                new List<object>
                    {
                        Command.QuickFind,
                        Command.FindInFiles,
                        Command.QuickReplace,
                        Command.ReplaceInFiles
                    };
            return commands;
        }

        /// <summary>
        /// Gets values for hovered over tokens
        /// </summary>
        /// <param name="args">Arguments (document, token, line number)</param>
        /// <returns>List of strings representing possible values for the hovered over token</returns>
        public IList<string> GetMouseHoverOverTokenValues(SledDocumentHoverOverTokenArgs args)
        {
            return null;
        }

        #endregion

        #region FindResultsCollection Stuff
        
        private void CreateFindResultsCollections()
        {
            var root1 =
                new DomNode(
                    SledSchema.SledFindResultsListType.Type,
                    SledSchema.SledFindResults1RootElement);

            m_findResults1Collection = 
                root1.As<SledFindResultsListType>();

            m_findResults1Collection.Name =
                Localization.SledFindResults1Title;

            var root2 =
                new DomNode(
                    SledSchema.SledFindResultsListType.Type,
                    SledSchema.SledFindResults2RootElement);

            m_findResults2Collection =
                root2.As<SledFindResultsListType>();

            m_findResults2Collection.Name =
                Localization.SledFindResults2Title;
        }
        
        private void CloseFindResultsCollections()
        {
            // Reset GUIs
            m_findResultsEditor1.View = null;
            m_findResultsEditor2.View = null;

            // Clear out find results
            if (m_findResults1Collection != null)
                m_findResults1Collection.FindResults.Clear();

            m_findResults1Collection = null;

            if (m_findResults2Collection != null)
                m_findResults2Collection.FindResults.Clear();

            m_findResults2Collection = null;
        }

        private void SledFindAndReplaceServiceFindResultsStartEvent(SledFindAndReplaceResultsWindow window)
        {
            SledFindResultsBaseEditor editor = null;
            SledFindResultsListType collection = null;

            switch (window)
            {
                case SledFindAndReplaceResultsWindow.One: editor = m_findResultsEditor1; collection = m_findResults1Collection; break;
                case SledFindAndReplaceResultsWindow.Two: editor = m_findResultsEditor2; collection = m_findResults2Collection; break;
            }

            if ((editor == null) || (collection == null))
                return;

            editor.View = null;
            collection.FindResults.Clear();
        }

        private void SledFindAndReplaceServiceFindResultsAddItemEvent(SledFindAndReplaceResultsWindow window, SledFindResultsType item)
        {
            SledFindResultsListType collection = null;

            switch (window)
            {
                case SledFindAndReplaceResultsWindow.One: collection = m_findResults1Collection; break;
                case SledFindAndReplaceResultsWindow.Two: collection = m_findResults2Collection; break;
            }

            if (collection == null)
                return;

            collection.FindResults.Add(item);
        }

        private void SledFindAndReplaceServiceFindResultsFinishEvent(SledFindAndReplaceResultsWindow window)
        {
            SledFindResultsBaseEditor editor = null;
            SledFindResultsListType collection = null;

            switch (window)
            {
                case SledFindAndReplaceResultsWindow.One: editor = m_findResultsEditor1; collection = m_findResults1Collection; break;
                case SledFindAndReplaceResultsWindow.Two: editor = m_findResultsEditor2; collection = m_findResults2Collection; break;
            }

            if ((editor == null) || (collection == null))
                return;

            editor.View = collection;
        }

        #endregion

        #region MainForm Events

        private void MainFormShown(object sender, EventArgs e)
        {
            CreateFindResultsCollections();

            // Subscribe to events
            FindResultsStartEvent += SledFindAndReplaceServiceFindResultsStartEvent;
            FindResultsAddItemEvent += SledFindAndReplaceServiceFindResultsAddItemEvent;
            FindResultsFinishEvent += SledFindAndReplaceServiceFindResultsFinishEvent;
        }

        private void MainFormFormClosing(object sender, FormClosingEventArgs e)
        {
            // Unsubscribe from events
            FindResultsStartEvent -= SledFindAndReplaceServiceFindResultsStartEvent;
            FindResultsAddItemEvent -= SledFindAndReplaceServiceFindResultsAddItemEvent;
            FindResultsFinishEvent -= SledFindAndReplaceServiceFindResultsFinishEvent;

            CloseFindResultsCollections();
        }

        #endregion

        /// <summary>
        /// Run find and replace event
        /// </summary>
        /// <param name="e">event arguments</param>
        public void Run(SledFindAndReplaceEventArgs e)
        {
            // Run the appropriate find or replace method
            switch (e.Mode)
            {
                case SledFindAndReplaceModes.QuickFind: m_quickFind.Run((SledFindAndReplaceEventArgs.QuickFind)e); break;
                case SledFindAndReplaceModes.FindInFiles: m_findInFiles.Run((SledFindAndReplaceEventArgs.FindInFiles)e); break;
                case SledFindAndReplaceModes.QuickReplace: m_quickReplace.Run((SledFindAndReplaceEventArgs.QuickReplace)e); break;
                case SledFindAndReplaceModes.ReplaceInFiles: m_replaceInFiles.Run((SledFindAndReplaceEventArgs.ReplaceInFiles)e); break;
            }

            // Store the last performed search
            s_lastSearchEventArgs = (SledFindAndReplaceEventArgs)e.Clone();

            // Put attention back on the app and not the find box
            m_mainForm.Focus();
        }

        /// <summary>
        /// Convert SledFindAndReplaceSearchType to SyntaxEditorFindReplaceSearchType
        /// </summary>
        /// <param name="searchType">search type</param>
        /// <returns>SyntaxEditorFindReplaceSearchType</returns>
        private static SyntaxEditorFindReplaceSearchType ToActiproFindReplaceSearchType(SledFindAndReplaceSearchType searchType)
        {
            if (searchType == SledFindAndReplaceSearchType.RegularExpressions)
                return SyntaxEditorFindReplaceSearchType.RegularExpression;
            
            if (searchType == SledFindAndReplaceSearchType.WildCards)
                return SyntaxEditorFindReplaceSearchType.Wildcard;

            return SyntaxEditorFindReplaceSearchType.Normal;
        }

        private static void ChangeSearchDirection(ref SledFindAndReplaceEventArgs e, bool searchUp)
        {
            // Don't care about the *InFiles event arguments
            // since they don't care about search direction

            if (e is SledFindAndReplaceEventArgs.QuickFind)
            {
                var args = (SledFindAndReplaceEventArgs.QuickFind)e;
                if (args.SearchUp == searchUp)
                    return;

                e = new SledFindAndReplaceEventArgs.QuickFind(
                    args.FindWhat,
                    args.LookIn,
                    args.MatchCase,
                    args.MatchWholeWord,
                    !args.SearchUp,
                    args.SearchType);
            }
            else if (e is SledFindAndReplaceEventArgs.QuickReplace)
            {
                var args = (SledFindAndReplaceEventArgs.QuickReplace)e;
                if (args.SearchUp == searchUp)
                    return;

                e = new SledFindAndReplaceEventArgs.QuickReplace(
                    args.FindWhat,
                    args.ReplaceWith,
                    args.LookIn,
                    args.MatchCase,
                    args.MatchWholeWord,
                    !args.SearchUp,
                    args.SearchType);
            }
        }

#pragma warning disable 649 // Field is never assigned

        [Import]
        private ISledDocumentService m_documentService;

        [Import]
        private ISledProjectService m_projectService;

        [Import]
        private ISledGotoService m_gotoService;

        [Import]
        private SledFindResultsEditor1 m_findResultsEditor1;

        [Import]
        private SledFindResultsEditor2 m_findResultsEditor2;

#pragma warning restore 649

        private readonly MainForm m_mainForm;

        private SledFindResultsListType m_findResults1Collection;
        private SledFindResultsListType m_findResults2Collection;

        private QuickFind m_quickFind;
        private FindInFiles m_findInFiles;
        private QuickReplace m_quickReplace;
        private ReplaceInFiles m_replaceInFiles;

        public const string AllExtension = "*.*";
        public const string TxtExtension = ".txt";

        private delegate void FindResultsHandler(SledFindAndReplaceResultsWindow window);
        private delegate void FindResultsAddItemHandler(SledFindAndReplaceResultsWindow window, SledFindResultsType item);

        private static event FindResultsHandler FindResultsStartEvent;
        private static event FindResultsAddItemHandler FindResultsAddItemEvent;
        private static event FindResultsHandler FindResultsFinishEvent;

        private static SledFindAndReplaceEventArgs s_lastSearchEventArgs;

        private static bool TryGetActiveDocument(ISledDocumentService documentService, out ISledDocument sd)
        {
            sd = documentService == null ? null : documentService.ActiveDocument;
            return sd != null;
        }

        private static bool TryGetOpenDocuments(ISledDocumentService documentService, out ISledDocument[] sdocs)
        {
            sdocs = documentService == null ? EmptyArray<ISledDocument>.Instance : documentService.OpenDocuments.ToArray();
            return sdocs.Length > 0;
        }

        #region Private SledFindResultsEditor

        [InheritedExport(typeof(IContextMenuCommandProvider))]
        public abstract class SledFindResultsBaseEditor : SledTreeListViewEditor, IContextMenuCommandProvider
        {
            [ImportingConstructor]
            protected SledFindResultsBaseEditor(
                string name,
                string image,
                string[] columns,
                TreeListView.Style style,
                StandardControlGroup controlGroup)
                : base(
                    name,
                    image,
                    columns,
                    style,
                    controlGroup)
            {
                MouseDoubleClick += ControlMouseDoubleClick;

                TreeListView.NodeSorter = new MySorter(TreeListView);
            }

            public override void Initialize()
            {
                base.Initialize();

                m_gotoService = SledServiceInstance.Get<ISledGotoService>();
            }

            #region Implementation of IContextMenuCommandProvider

            public IEnumerable<object> GetCommands(object context, object target)
            {
                if (!ReferenceEquals(context, TreeListViewAdapter.View))
                    yield break;

                var clicked = target.As<SledFindResultsType>();
                if (clicked == null)
                    yield break;

                yield return StandardCommand.EditCopy;
            }

            #endregion

            #region SledTreeListViewEditor Overrides

            protected override string GetCopyText()
            {
                if ((TreeListViewAdapter == null) ||
                    (!TreeListViewAdapter.Selection.Any()))
                    return string.Empty;

                const string tab = "\t";

                var sb = new StringBuilder();
                var items = TreeListViewAdapter.Selection.AsIEnumerable<SledFindResultsType>();

                foreach (var item in items)
                {
                    sb.Append(item.File);
                    sb.Append(tab);
                    sb.Append(item.Line);
                    sb.Append(tab);
                    sb.Append(item.LineText);
                    sb.Append(Environment.NewLine);
                }

                return sb.ToString();
            }

            #endregion

            private void ControlMouseDoubleClick(object sender, MouseEventArgs e)
            {
                if (LastHit == null)
                    return;

                var findResult = LastHit.As<SledFindResultsType>();
                if (findResult == null)
                    return;

                m_gotoService.GotoFileAndHighlightRange(
                    findResult.File,
                    findResult.StartOffset,
                    findResult.EndOffset);
            }

            private class MySorter : IComparer<TreeListView.Node>
            {
                public MySorter(TreeListView control)
                {
                    m_control = control;
                }

                public int Compare(TreeListView.Node x, TreeListView.Node y)
                {
                    return
                        SledFindResultsType.Compare(
                            x.Tag.As<SledFindResultsType>(),
                            y.Tag.As<SledFindResultsType>(),
                            m_control.SortColumn,
                            m_control.SortOrder);
                }

                private readonly TreeListView m_control;
            }

            private ISledGotoService m_gotoService;
        }

        [Export(typeof(SledFindResultsEditor1))]
        [PartCreationPolicy(CreationPolicy.Shared)]
        public class SledFindResultsEditor1 : SledFindResultsBaseEditor
        {
            [ImportingConstructor]
            public SledFindResultsEditor1()
                : base(
                    Localization.SledFindResults1Title,
                    Atf.Resources.FindImage,
                    SledFindResultsType.TheColumnNames,
                    TreeListView.Style.List,
                    StandardControlGroup.Bottom)
            {
            }
        }

        [Export(typeof(SledFindResultsEditor2))]
        [PartCreationPolicy(CreationPolicy.Shared)]
        public class SledFindResultsEditor2 : SledFindResultsBaseEditor
        {
            [ImportingConstructor]
            public SledFindResultsEditor2()
                : base(
                    Localization.SledFindResults2Title,
                    Atf.Resources.FindImage,
                    SledFindResultsType.TheColumnNames,
                    TreeListView.Style.List,
                    StandardControlGroup.Bottom)
            {
            }
        }

        #endregion

        #region QuickFind Specific Stuff

        private class QuickFind
        {
            public QuickFind()
            {
                LastLookIn = SledFindAndReplaceLookIn.CurrentDocument;
            }

            public void Run(SledFindAndReplaceEventArgs.QuickFind e)
            {
                // Update value
                LastLookIn = e.LookIn;

                // Update options
                Options.FindText = e.FindWhat;
                Options.MatchCase = e.MatchCase;
                Options.MatchWholeWord = e.MatchWholeWord;
                Options.SearchUp = e.SearchUp;
                Options.SearchType = ToActiproFindReplaceSearchType(e.SearchType);

                // Gather & reconcile list of documents to search
                m_docList.GatherAndReconcile(e.LookIn, Options.Modified);

                //
                // Run the search
                //

                var iCount = m_docList.Count;
                var assoc = m_docList.Get();

                // Nothing to search in if doc list is empty or assoc is null
                if ((iCount == 0) || (assoc == null))
                {
                    e.Result = SledFindAndReplaceResult.NothingToSearch;
                    return;
                }

                var iResultIndex = 0;
                var lstResults = new List<ResultSet>();
                
                // Search
                var bFound = false;
                for (var i = 0; ((i < iCount) && !bFound && (assoc != null)); i++)
                {
                    var bGrabNextDoc = false;

                    bFound = RunInternal(assoc, ref lstResults);

                    if (bFound)
                    {
                        if (e.LookIn != SledFindAndReplaceLookIn.CurrentDocument)
                        {
                            // If we're doing a multi-document search and we went past
                            // the document end and have only on result so far we need
                            // to keep searching.
                            var resultSet = lstResults[lstResults.Count - 1];
                            if (resultSet.Results.PastDocumentEnd && (lstResults.Count == 1))
                            {
                                // Keep trying to search
                                bFound = false;
                                bGrabNextDoc = true;
                            }
                            else
                            {
                                // Use the latest find result
                                iResultIndex = lstResults.Count - 1;
                            }
                        }
                    }

                    if (!bFound)
                        bGrabNextDoc = true;

                    if (bGrabNextDoc)
                    {
                        // Update doc 'pointer' to search in 'next' document
                        m_docList.Update();
                        assoc = m_docList.Get();
                    }
                }

                if (!bFound && (lstResults.Count > 0))
                {
                    iResultIndex = lstResults.Count - 1;
                    bFound = true;
                }

                if (bFound)
                {
                    var resultSet = lstResults[iResultIndex];

                    // Save this result
                    LastResult = resultSet;

                    // Jump to the correct file and highlight the result
                    GotoService.GotoFileAndHighlightRange(resultSet.PathName, resultSet.Results[0].StartOffset, resultSet.Results[0].EndOffset);

                    // Update last caret position
                    ISledDocument sdCurDoc;
                    if (TryGetActiveDocument(DocumentService, out sdCurDoc))
                        LastCaretOffset = sdCurDoc.Editor.CurrentOffset;

                    e.Result = SledFindAndReplaceResult.Success;
                }

                // Nothing found
                if (!bFound)
                    e.Result = SledFindAndReplaceResult.NothingFound;
            }

            public ISledGotoService GotoService { protected get; set; }

            public ISledDocumentService DocumentService { protected get; set; }

            public SledFindAndReplaceLookIn LastLookIn { get; private set; }

            private bool RunInternal(DocumentStateAssociation assoc, ref List<ResultSet> lstResults)
            {
                if (assoc == null)
                    return false;

                ISledDocument sd;

                if (assoc.State == DocumentState.Opened)
                {
                    // Use existing opened document
                    sd = assoc.OpenedDocument;
                }
                else
                {
                    // Open a new document
                    sd = SledDocument.CreateHidden(new Uri(assoc.UnopenedDocument), null);
                    try
                    {
                        sd.Read();
                    }
                    catch (Exception)
                    {
                        sd.Editor.Dispose();
                        return false;
                    }
                }

                // Are we searching in the active doc or not?
                var bSearchingActiveDoc = (DocumentService.Active && (sd == DocumentService.ActiveDocument));

                // Grab caret position
                var iCaretOffset = bSearchingActiveDoc ? sd.Editor.CurrentOffset : 0;

                // Potentially adjust the caret if searching up and the caret position has
                // not changed since the last search and the caret isn't at position 0
                if (Options.SearchUp && (iCaretOffset == LastCaretOffset) && (iCaretOffset != 0))
                    iCaretOffset -= 1;

                // Run the find command
                var results = sd.Editor.Find(Options, iCaretOffset);

                // Release editor
                if (assoc.State == DocumentState.Unopened)
                    sd.Editor.Dispose();

                // No results so abort
                if (results.Count <= 0)
                    return false;

                lstResults.Add(new ResultSet(results, sd.Uri.LocalPath));

                return true;
            }

            protected int LastCaretOffset;
            protected ResultSet LastResult;

            private readonly DocumentList m_docList = new DocumentList();
            
            protected readonly ISyntaxEditorFindReplaceOptions Options =
                TextEditorFactory.CreateSyntaxEditorFindReplaceOptions();
        }

        #endregion

        #region FindInFiles Specific Stuff

        private class FindInFiles
        {
            public void Run(SledFindAndReplaceEventArgs.FindInFiles e)
            {
                // Update options
                m_options.FindText = e.FindWhat;
                m_options.MatchCase = e.MatchCase;
                m_options.MatchWholeWord = e.MatchWholeWord;
                m_options.SearchUp = false;
                m_options.SearchType = ToActiproFindReplaceSearchType(e.SearchType);

                // Reset
                m_bCancelled = false;

                // Show a progress bar because it could take a while
                var progressBar =
                    new ThreadSafeProgressDialog(true, true)
                        {
                            Description = Localization.SledFindAndReplaceGatheringFiles
                        };

                progressBar.Cancelled += ProgressBarCancelled;

                // Check if e.FileExts contains *.* then use null later instead of e.FileExts
                var bAllExtension =
                    e.FileExts == null
                        ? true
                        : e.FileExts.Any(ext => string.Compare(AllExtension, ext, true) == 0);

                // Gather & reconcile list of documents to search
                if (e.LookIn != SledFindAndReplaceLookIn.Custom)
                    m_docList.GatherAndReconcile(e.LookIn, m_options.Modified);
                else
                    m_docList.GatherAndReconcileDirs(e.LookInFolders, e.IncludeSubFolders, bAllExtension ? null : e.FileExts, progressBar);

                //
                // Run the search
                //

                int iCount = m_docList.Count, i;
                var assoc = m_docList.Get();

                // Nothing to search in if doc list is empty or assoc is null
                if ((iCount == 0) || (assoc == null))
                {
                    e.Result = SledFindAndReplaceResult.NothingToSearch;

                    // Kill the progress bar
                    progressBar.Close();
                    return;
                }

                var lstResults = new List<ResultSet>();

                // Update progress bar
                progressBar.Percent = 0;

                // Search
                for (i = 0; ((i < iCount) && (assoc != null) && !m_bCancelled); i++)
                {
                    var fileName = Path.GetFileName(assoc.UnopenedDocument);
                    progressBar.Description = SledUtil.TransSub(Localization.SledFindAndReplaceSearching, fileName);

                    RunInternal(assoc, ref lstResults);

                    // Update doc 'pointer' to search in 'next' document
                    m_docList.Update();
                    assoc = m_docList.Get();

                    var flPercent = ((((float)(i + 1)) / ((float)iCount)) * 100.0f);
                    progressBar.Percent = SledUtil.Clamp((int)flPercent, 0, 100);
                }

                // Update progress bar
                progressBar.Percent = 0;
                progressBar.Description = Localization.SledFindAndReplaceDisplaying;

                i = 0;
                iCount = lstResults.Count;
                if (iCount > 0)
                {
                    // Found results so populate the proper results window
                    var window =
                        e.UseResults1Window
                            ? SledFindAndReplaceResultsWindow.One
                            : SledFindAndReplaceResultsWindow.Two;

                    // Update before showing new items
                    SledFindResultsType.ShowFileNamesOnly = SledFindAndReplaceSettings.FindInFiles.DisplayFileNamesOnlyChecked;

                    // Send start event
                    if (FindResultsStartEvent != null)
                        FindResultsStartEvent(window);

                    foreach (var resultSet in lstResults)
                    {
                        var name = Path.GetFileName(resultSet.PathName);

                        for (var j = 0; j < resultSet.Results.Count; j++)
                        {
                            var result = resultSet.Results[j];

                            var node = new DomNode(SledSchema.SledFindResultsType.Type);
                            var resultType = node.As<SledFindResultsType>();

                            resultType.Name = name;
                            resultType.File = resultSet.PathName;
                            resultType.StartOffset = result.StartOffset;
                            resultType.EndOffset = result.EndOffset;
                            resultType.Line = resultSet.Lines[j];
                            resultType.LineText = resultSet.Texts[j];

                            // Send add event
                            if (FindResultsAddItemEvent != null)
                                FindResultsAddItemEvent(window, resultType);
                        }

                        var flPercent = ((((float)(++i)) / ((float)iCount)) * 100.0f);
                        progressBar.Percent = SledUtil.Clamp((int)flPercent, 0, 100);
                    }

                    // Send finish event
                    if (FindResultsFinishEvent != null)
                        FindResultsFinishEvent(window);

                    // Success!
                    e.Result = SledFindAndReplaceResult.Success;

                    // Kill progress bar
                    progressBar.Close();
                }
                else
                {
                    // Nothing found
                    e.Result = SledFindAndReplaceResult.NothingFound;

                    // Kill progress bar
                    progressBar.Close();
                }
            }

            private void RunInternal(DocumentStateAssociation assoc, ref List<ResultSet> lstResults)
            {
                if (assoc == null)
                    return;

                ISledDocument sd;

                if (assoc.State == DocumentState.Opened)
                {
                    // Use existing opened document
                    sd = assoc.OpenedDocument;
                }
                else
                {
                    // Open a new document
                    sd = SledDocument.CreateHidden(new Uri(assoc.UnopenedDocument), null);
                    try
                    {
                        sd.Read();
                    }
                    catch (Exception)
                    {
                        sd.Editor.Dispose();
                        return;
                    }
                }
                
                // Run the find command
                var results = sd.Editor.FindAll(m_options);

                var lstLines = new List<int>();
                var lstTexts = new List<string>();

                // Grab line numbers and line text for each result in results
                if (results.Count > 0)
                {
                    foreach (ISyntaxEditorFindReplaceResult result in results)
                    {
                        var iLine = sd.Editor.GetLineFromOffset(result.StartOffset);
                        lstLines.Add(iLine);
                        var text = sd.Editor.GetLineText(iLine).Trim();
                        lstTexts.Add(text);
                    }
                }

                // Release editor
                if (assoc.State == DocumentState.Unopened)
                    sd.Editor.Dispose();

                // No results so abort
                if (results.Count <= 0)
                    return;

                lstResults.Add(new ResultSet(results, sd.Uri.LocalPath, lstLines, lstTexts));

                return;
            }

            private void ProgressBarCancelled(object sender, EventArgs e)
            {
                m_bCancelled = true;
            }

            private bool m_bCancelled;

            private readonly DocumentList m_docList = new DocumentList();

            private readonly ISyntaxEditorFindReplaceOptions m_options =
                TextEditorFactory.CreateSyntaxEditorFindReplaceOptions();
        }

        #endregion

        #region QuickReplace Specific Stuff

        private class QuickReplace : QuickFind
        {
            public void Run(SledFindAndReplaceEventArgs.QuickReplace e)
            {
                // Update find options so we can see if they've changed
                Options.FindText = e.FindWhat;
                Options.MatchCase = e.MatchCase;
                Options.MatchWholeWord = e.MatchWholeWord;
                Options.SearchUp = e.SearchUp;
                Options.SearchType = ToActiproFindReplaceSearchType(e.SearchType);

                // If e.ReplaceWith == null then we're just doing a 'QuickFind'

                if (!string.IsNullOrEmpty(e.ReplaceWith))
                {
                    // Doing a 'Replace' which means two things:
                    // 1) take the current result and replace it and then 'QuickFind' or
                    // 2) if no current result or search has modified then just 'QuickFind.'

                    // Try to see if we're doing a replace before doing the next find
                    ISledDocument sd;
                    if (TryGetActiveDocument(DocumentService, out sd))
                    {
                        // Find correct path for document
                        var path = sd.Uri.LocalPath;

                        // Got an active document so lets see if it was part of the last 'QuickFind' result...
                        if ((LastResult != null) && (string.Compare(path, LastResult.PathName, true) == 0) && (!Options.Modified))
                        {
                            // Add in replace text
                            Options.ReplaceText = e.ReplaceWith;

                            // Run the replace
                            var results = sd.Editor.Replace(Options, LastResult.Results[0]);

                            if (results.Count > 0)
                            {
                                // Jump to the correct file and highlight the result
                                GotoService.GotoFileAndHighlightRange(path, results[0].StartOffset, results[0].EndOffset);

                                // Update last caret position
                                LastCaretOffset = sd.Editor.CurrentOffset;
                            }
                        }
                    }
                }

                // Always run a 'QuickFind' so Re-package and run base class search
                var eq = new SledFindAndReplaceEventArgs.QuickFind(
                    e.FindWhat,
                    e.LookIn,
                    e.MatchCase,
                    e.MatchWholeWord,
                    e.SearchUp,
                    e.SearchType);

                Run(eq);

                // Propagate result status
                e.Result = eq.Result;
            }
        }

        #endregion

        #region ReplaceInFiles Specific Stuff

        private class ReplaceInFiles
        {
            public void Run(SledFindAndReplaceEventArgs.ReplaceInFiles e)
            {
                // Update options
                m_options.FindText = e.FindWhat;
                m_options.ReplaceText = e.ReplaceWith;
                m_options.MatchCase = e.MatchCase;
                m_options.MatchWholeWord = e.MatchWholeWord;
                m_options.SearchUp = false;
                m_options.SearchType = ToActiproFindReplaceSearchType(e.SearchType);

                // Reset
                m_bCancelled = false;

                // Show a progress bar because it could take a while
                var progressBar =
                    new ThreadSafeProgressDialog(true, true)
                        {
                            Description = Localization.SledFindAndReplaceGatheringFiles
                        };

                progressBar.Cancelled += ProgressBarCancelled;

                // Check if e.FileExts contains *.* then use null later instead of e.FileExts
                var bAllExtension =
                    e.FileExts == null
                        ? true
                        : e.FileExts.Any(ext => string.Compare(AllExtension, ext, true) == 0);

                // Gather & reconcile list of documents to search
                if (e.LookIn != SledFindAndReplaceLookIn.Custom)
                    m_docList.GatherAndReconcile(e.LookIn, true);
                else
                    m_docList.GatherAndReconcileDirs(e.LookInFolders, e.IncludeSubFolders, bAllExtension ? null : e.FileExts, progressBar);

                //
                // Run the replace
                //

                int iCount = m_docList.Count, i;
                var assoc = m_docList.Get();

                // Nothing to search in if doc list is empty or assoc is null
                if ((iCount == 0) || (assoc == null))
                {
                    progressBar.Close();
                    e.Result = SledFindAndReplaceResult.NothingToSearch;
                    return;
                }

                var lstResults = new List<ResultSet>();

                // Update progress bar
                progressBar.Percent = 0;

                // Replace
                for (i = 0; ((i < iCount) && (assoc != null) && !m_bCancelled); i++)
                {
                    var fileName = Path.GetFileName(assoc.UnopenedDocument);
                    progressBar.Description = SledUtil.TransSub(Localization.SledFindAndReplaceReplacing, fileName);

                    RunInternal(assoc, ref lstResults, e.KeepModifiedDocsOpen, (e.ResultsWindow == SledFindAndReplaceResultsWindow.None ? false : true));

                    // Update doc 'pointer' to search in 'next' document
                    m_docList.Update();
                    assoc = m_docList.Get();

                    var flPercent = ((((float)(i + 1)) / ((float)iCount)) * 100.0f);
                    progressBar.Percent = SledUtil.Clamp((int)flPercent, 0, 100);
                }

                // If no results found we can bail
                if (lstResults.Count <= 0)
                {
                    progressBar.Close();
                    e.Result = SledFindAndReplaceResult.NothingFound;
                    return;
                }

                // If we're not showing any results then we can bail
                if (e.ResultsWindow == SledFindAndReplaceResultsWindow.None)
                {
                    progressBar.Close();
                    e.Result = SledFindAndReplaceResult.Success;
                    return;
                }

                // Update progress bar
                progressBar.Percent = 0;
                progressBar.Description = Localization.SledFindAndReplaceDisplaying;

                i = 0;
                iCount = lstResults.Count;
                 
                // Found results so populate the proper results window
                var window = e.ResultsWindow;

                // Update before showing new items
                SledFindResultsType.ShowFileNamesOnly = SledFindAndReplaceSettings.ReplaceInFiles.DisplayFileNamesOnlyChecked;

                // Send start event
                if (FindResultsStartEvent != null)
                    FindResultsStartEvent(window);

                foreach (var resultSet in lstResults)
                {
                    var name = Path.GetFileName(resultSet.PathName);

                    for (var j = 0; j < resultSet.Results.Count; j++)
                    {
                        var result = resultSet.Results[j];

                        var node = new DomNode(SledSchema.SledFindResultsType.Type);
                        var resultType = node.As<SledFindResultsType>();

                        resultType.Name = name;
                        resultType.File = resultSet.PathName;
                        resultType.StartOffset = result.StartOffset;
                        resultType.EndOffset = result.EndOffset;
                        resultType.Line = resultSet.Lines[j];
                        resultType.LineText = resultSet.Texts[j];

                        // Send add event
                        if (FindResultsAddItemEvent != null)
                            FindResultsAddItemEvent(window, resultType);
                    }

                    var flPercent = ((((float)(++i)) / ((float)iCount)) * 100.0f);
                    progressBar.Percent = SledUtil.Clamp((int)flPercent, 0, 100);
                }

                // Send finish event
                if (FindResultsFinishEvent != null)
                    FindResultsFinishEvent(window);

                // Success!
                e.Result = SledFindAndReplaceResult.Success;

                // Kill progress bar
                progressBar.Close();
            }

            public ISledDocumentService DocumentService { private get; set; }

            private void RunInternal(DocumentStateAssociation assoc, ref List<ResultSet> lstResults, bool bKeepModifiedDocsOpen, bool bUseWindow)
            {
                if (assoc == null)
                    return;

                ISledDocument sd;

                if (assoc.State == DocumentState.Opened)
                {
                    // Use existing opened document
                    sd = assoc.OpenedDocument;
                }
                else
                {
                    // Open a new document
                    sd = SledDocument.CreateHidden(new Uri(assoc.UnopenedDocument), null);
                    try
                    {
                        sd.Read();
                    }
                    catch (Exception)
                    {
                        sd.Editor.Dispose();
                        return;
                    }
                }

                // Run the find
                var results = sd.Editor.Find(m_options, 0);

                // No results so there's nothing to replace in this file so we can bail
                if (results.Count <= 0)
                {
                    // Release editor if needed
                    if (assoc.State == DocumentState.Unopened)
                        sd.Editor.Dispose();

                    return;
                }

                // Now that we have some results to replace lets see if we need to
                // keep modified documents open or not. If we do have to keep modified
                // documents open then we have to show the file first so that changes
                // can be undone.
                if (bKeepModifiedDocsOpen)
                {
                    // Open the real doc, get a ISledDocument reference to it, and
                    // release the temporary doc we're using.
                    if (assoc.State == DocumentState.Unopened)
                    {
                        // Release current editor
                        sd.Editor.Dispose();

                        // Grab newly opened doc
                        DocumentService.Open(new Uri(assoc.UnopenedDocument), out sd);
                        if (sd == null)
                            return;
                    }
                }

                // Now we have the proper document to do the replace on so do it.
                results = sd.Editor.ReplaceAll(m_options);

                var lstLines = new List<int>();
                var lstTexts = new List<string>();

                // If the replaced text is to be shown in one of the results windows then
                // we need to do some more work grabbing the replaced text & line numbers
                if (bUseWindow)
                {
                    // Grab line numbers and line text for each result in results
                    if (results.Count > 0)
                    {
                        foreach (ISyntaxEditorFindReplaceResult result in results)
                        {
                            var iLine = sd.Editor.GetLineFromOffset(result.StartOffset);
                            lstLines.Add(iLine);
                            var text = sd.Editor.GetLineText(iLine).Trim();
                            lstTexts.Add(text);
                        }
                    }
                }

                // If we didn't need to keep modified docs open then we need to save the changes
                // to disk (and also free the SyntaxEditorControl)
                if (!bKeepModifiedDocsOpen && (assoc.State == DocumentState.Unopened))
                {
                    sd.Write();
                    sd.Editor.Dispose();
                }

                lstResults.Add(new ResultSet(results, sd.Uri.LocalPath, lstLines, lstTexts));

                return;
            }

            private void ProgressBarCancelled(object sender, EventArgs e)
            {
                m_bCancelled = true;
            }

            private bool m_bCancelled;

            private readonly DocumentList m_docList = new DocumentList();

            private readonly ISyntaxEditorFindReplaceOptions m_options =
                TextEditorFactory.CreateSyntaxEditorFindReplaceOptions();
        }

        #endregion

        #region DocumentStateAssociation Class

        private class DocumentStateAssociation
        {
            public DocumentStateAssociation(ISledDocument openedDoc)
            {
                State = DocumentState.Opened;
                OpenedDocument = openedDoc;
                UnopenedDocument = openedDoc.Uri.LocalPath;
            }

            public DocumentStateAssociation(string absPathToUnopenedDoc)
            {
                State = DocumentState.Unopened;
                OpenedDocument = null;
                UnopenedDocument = absPathToUnopenedDoc;
            }

            public DocumentState State { get; private set; }

            public ISledDocument OpenedDocument { get; private set; }

            public string UnopenedDocument { get; private set; }

            public void UpdateReferences()
            {
                ISledDocument sd;
                DocumentService.IsOpen(new Uri(UnopenedDocument), out sd);

                if (sd == null)
                {
                    State = DocumentState.Unopened;
                    OpenedDocument = null;
                }
                else
                {
                    State = DocumentState.Opened;
                    OpenedDocument = sd;
                }
            }

            public override bool Equals(object obj)
            {
                var assoc = obj as DocumentStateAssociation;

                if (assoc == null)
                    return base.Equals(obj);

                return (string.Compare(UnopenedDocument, assoc.UnopenedDocument, true) == 0);
            }

            public override int GetHashCode()
            {
                return
                    !string.IsNullOrEmpty(UnopenedDocument)
                        ? UnopenedDocument.GetHashCode()
                        : base.GetHashCode();
            }

            public static ISledDocumentService DocumentService { private get; set; }
        }

        #endregion

        #region DocumentList Class

        private class DocumentList
        {
            public void GatherAndReconcile(SledFindAndReplaceLookIn lookIn, bool bFindReplaceOptionsModified)
            {
                GatherAndReconcile(lookIn, bFindReplaceOptionsModified, null);
            }

            private void GatherAndReconcile(SledFindAndReplaceLookIn lookIn, bool bFindReplaceOptionsModified, IEnumerable<string> fileExts)
            {
                // Check if sources changed since last gather
                var bNeedsReconciling = ((lookIn != m_lastLookIn) || bFindReplaceOptionsModified);

                // Gather new current docs list
                var lstDocs = GatherDocs(lookIn, fileExts);

                // Compare last docs list with new current docs list
                if (bNeedsReconciling || AreDocsListDifferent(ref m_docList, ref lstDocs))
                {
                    // Start back @ beginning of doc list
                    m_iSearchDoc = 0;

                    // Use the new list
                    m_docList = lstDocs;
                }

                // Make sure any ISledDocument references in lstDocs are valid
                CheckStaleReferences(ref m_docList);

                // Update
                m_lastLookIn = lookIn;
            }

            public void GatherAndReconcileDirs(string[] lookInFolders, bool bIncludeSubFolders, IEnumerable<string> fileExts, ThreadSafeProgressDialog progressBar)
            {
                // Completely update doc list for a directory search
                m_docList.Clear();

                int iCount = lookInFolders.Length, i = 0;

                // Go through gathering files
                foreach (var folder in lookInFolders)
                {
                    // Gather files...
                    var files = Directory.GetFiles(folder, AllExtension, bIncludeSubFolders ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);

                    // ... and add to doc list if not a duplicate and extension matches one in fileExts
                    foreach (var file in files)
                    {
                        var assoc = new DocumentStateAssociation(file);
                        if (!GatherDocsHelperIsDuplicate(assoc, m_docList) && GatherDocsHelperHasFileExt(assoc, fileExts))
                            m_docList.Add(assoc);
                    }
                    
                    if (progressBar == null)
                        continue;

                    // Update progress bar
                    var flPercent = ((((float)(++i)) / ((float)iCount)) * 100.0f);
                    progressBar.Percent = SledUtil.Clamp((int)flPercent, 0, 100);
                }

                // Start back @ beginning of doc list
                m_iSearchDoc = 0;

                // Make sure any ISledDocument references in lstDocs are valid
                CheckStaleReferences(ref m_docList);

                // Update
                m_lastLookIn = SledFindAndReplaceLookIn.Custom;
            }

            public DocumentStateAssociation Get()
            {
                if (m_docList.Count <= 0)
                    return null;

                if ((m_iSearchDoc >= 0) && (m_iSearchDoc >= m_docList.Count))
                    return null;

                return m_docList[m_iSearchDoc];
            }

            public void Update()
            {
                m_iSearchDoc += 1;
                if (m_iSearchDoc >= m_docList.Count)
                    m_iSearchDoc = 0;
            }

            public int Count
            {
                get { return m_docList.Count; }
            }

            public static ISledDocumentService DocumentService { private get; set; }

            public static ISledProjectService ProjectService { private get; set; }

            private static List<DocumentStateAssociation> GatherDocs(SledFindAndReplaceLookIn lookIn, IEnumerable<string> fileExts)
            {
                var lstDocs = new List<DocumentStateAssociation>();

                // Gather current document
                ISledDocument sdoc;
                if (TryGetActiveDocument(DocumentService, out sdoc))
                {
                    if ((lookIn == SledFindAndReplaceLookIn.CurrentDocument) ||
                        (lookIn == SledFindAndReplaceLookIn.AllOpenDocuments) ||
                        ((lookIn == SledFindAndReplaceLookIn.CurrentProject) && (sdoc.SledProjectFile != null)) ||
                        (lookIn == SledFindAndReplaceLookIn.EntireSolution))
                    {
                        // Add if not a duplicate and file extension is correct
                        var assoc = new DocumentStateAssociation(sdoc);

                        if (!GatherDocsHelperIsDuplicate(assoc, lstDocs) && GatherDocsHelperHasFileExt(assoc, fileExts))
                            lstDocs.Add(assoc);
                    }
                }

                // Can bail now if only getting current document
                if (lookIn == SledFindAndReplaceLookIn.CurrentDocument)
                    return lstDocs;

                ISledDocument[] sdocs;
                if (TryGetOpenDocuments(DocumentService, out sdocs))
                {
                    foreach (var sd in sdocs)
                    {
                        if ((lookIn == SledFindAndReplaceLookIn.CurrentProject) && (sd.SledProjectFile == null))
                            continue;

                        // Add if not duplicate and file extension is correct
                        var assoc = new DocumentStateAssociation(sd);

                        if (!GatherDocsHelperIsDuplicate(assoc, lstDocs) && GatherDocsHelperHasFileExt(assoc, fileExts))
                            lstDocs.Add(assoc);
                    }
                }

                // Can bail now if only getting all open documents
                if (lookIn == SledFindAndReplaceLookIn.AllOpenDocuments)
                    return lstDocs;

                foreach (var file in ProjectService.AllFiles)
                {
                    // Add if not duplicate and file extension is correct
                    var assoc = new DocumentStateAssociation(file.AbsolutePath);

                    if (!GatherDocsHelperIsDuplicate(assoc, lstDocs) && GatherDocsHelperHasFileExt(assoc, fileExts))
                        lstDocs.Add(assoc);
                }

                return lstDocs;
            }

            private static bool GatherDocsHelperIsDuplicate(DocumentStateAssociation assoc, IEnumerable<DocumentStateAssociation> lstDocs)
            {
                return lstDocs.Contains(assoc);
            }

            private static bool GatherDocsHelperHasFileExt(DocumentStateAssociation assoc, IEnumerable<string> fileExts)
            {
                // A fileExts array with *.* gets converted to null and if we don't care if the extension
                // matches anything then null gets passed into this function as well.
                if (fileExts == null)
                    return true;

                // Grab extension from file
                var assocExt = Path.GetExtension(assoc.UnopenedDocument);
                if (string.IsNullOrEmpty(assocExt))
                    return false;

                // Compare extensions (need to contain the period, too! ie. ".lua" or ".txt" etc.)
                return fileExts.Any(ext => string.Compare(ext, assocExt, true) == 0);
            }

            private static bool AreDocsListDifferent(
                ref List<DocumentStateAssociation> lstDocsLast,
                ref List<DocumentStateAssociation> lstDocsCur)
            {
                // Quick check
                if (lstDocsLast.Count != lstDocsCur.Count)
                    return true;

                // Check each entry
                foreach (var assocLast in lstDocsLast)
                {
                    var last = assocLast;
                    var bFound = lstDocsCur.Any(last.Equals);

                    if (!bFound)
                        return true;
                }

                return false;
            }

            private static void CheckStaleReferences(ref List<DocumentStateAssociation> lstDocs)
            {
                // Go through making sure any ISledDocument references are valid. The list might be used
                // for many iterations of searching and during that time the user could have opened
                // and closed a document keeping the list intact PathName wise but not ISledDocument wise.
                foreach (var assoc in lstDocs)
                {
                    assoc.UpdateReferences();
                }
            }

            private int m_iSearchDoc;
            private SledFindAndReplaceLookIn m_lastLookIn = SledFindAndReplaceLookIn.Invalid;
            private List<DocumentStateAssociation> m_docList = new List<DocumentStateAssociation>();
        }

        #endregion

        #region DocumentState Enum

        private enum DocumentState
        {
            Opened,
            Unopened,
        }

        #endregion

        #region ResultSet Class

        private class ResultSet
        {
            public ResultSet(ISyntaxEditorFindReplaceResultSet results, string pathName)
            {
                Results = results;
                PathName = pathName;
            }

            public ResultSet(ISyntaxEditorFindReplaceResultSet results, string pathName, List<int> lstLines, List<string> lstTexts)
                : this(results, pathName)
            {
                Lines = lstLines;
                Texts = lstTexts;
            }

            public ISyntaxEditorFindReplaceResultSet Results { get; private set; }

            public string PathName { get; private set; }

            public List<int> Lines { get; private set; }

            public List<string> Texts { get; private set; }
        }

        #endregion
    }
}