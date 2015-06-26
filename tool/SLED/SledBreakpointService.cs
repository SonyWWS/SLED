/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Windows.Forms;

using Sce.Atf;
using Sce.Atf.Adaptation;
using Sce.Atf.Applications;
using Sce.Atf.Dom;

using Sce.Sled.Resources;
using Sce.Sled.Shared;
using Sce.Sled.Shared.Document;
using Sce.Sled.Shared.Dom;
using Sce.Sled.Shared.Plugin;
using Sce.Sled.Shared.Services;
using Sce.Sled.SyntaxEditor;

namespace Sce.Sled
{
    /// <summary>
    /// SledBreakpointService Class
    /// </summary>
    [Export(typeof(IInitializable))]
    [Export(typeof(ISledBreakpointService))]
    [Export(typeof(ISledDocumentPlugin))]
    [Export(typeof(IContextMenuCommandProvider))]
    [Export(typeof(SledBreakpointService))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    class SledBreakpointService : IInitializable, ISledBreakpointService, ISledDocumentPlugin, IContextMenuCommandProvider, ICommandClient
    {
        [ImportingConstructor]
        public SledBreakpointService(MainForm mainForm, ICommandService commandService)
        {
            m_mainForm = mainForm;

            var menuInfoContext =
                commandService.RegisterMenu(
                    Menu.Context,
                    "Breakpoint Context",
                    "Breakpoint Context Menu");

            commandService.RegisterCommand(
                Command.BreakpointAdd,
                Menu.Context,
                CommandGroup.Context,
                Localization.SledBreakpointMenuBreakpointAdd,
                Localization.SledBreakpointMenuBreakpointAddComment,
                this);

            commandService.RegisterCommand(
                Command.BreakpointRemove,
                Menu.Context,
                CommandGroup.Context,
                Localization.SledBreakpointMenuBreakpointRemove,
                Localization.SledBreakpointMenuBreakpointRemoveComment,
                this);

            commandService.RegisterCommand(
                Command.BreakpointEnable,
                Menu.Context,
                CommandGroup.Context,
                Localization.SledBreakpointMenuBreakpointEnable,
                Localization.SledBreakpointMenuBreakpointEnableComment,
                this);

            commandService.RegisterCommand(
                Command.BreakpointDisable,
                Menu.Context,
                CommandGroup.Context,
                Localization.SledBreakpointMenuBreakpointDisable,
                Localization.SledBreakpointMenuBreakpointDisableComment,
                this);

            commandService.RegisterCommand(
                Command.BreakpointCondition,
                Menu.Context,
                CommandGroup.Context,
                Localization.SledBreakpointMenuBreakpointCondition,
                Localization.SledBreakpointMenuBreakpointConditionComment,
                this);

            commandService.RegisterCommand(
                Command.BreakpointRemoveAll,
                Menu.Context,
                CommandGroup.Context,
                Localization.SledBreakpointMenuBreakpointRemoveAll,
                Localization.SledBreakpointMenuBreakpointRemoveAllComment,
                this);

            menuInfoContext.GetMenuItem().Visible = false;

            var menuInfoWindow =
                commandService.RegisterMenu(
                    Menu.Window,
                    "Breakpoint Window",
                    "Breakpoint Window Menu");

            commandService.RegisterCommand(
                Command.BreakpointWindowRemove,
                Menu.Window,
                CommandGroup.Window,
                Localization.BreakpointWindowRemove,
                Localization.BreakpointWindowRemoveComment,
                this);

            commandService.RegisterCommand(
                Command.BreakpointWindowEnable,
                Menu.Window,
                CommandGroup.Window,
                Localization.BreakpointWindowEnable,
                Localization.BreakpointWindowEnableComment,
                this);

            commandService.RegisterCommand(
                Command.BreakpointWindowDisable,
                Menu.Window,
                CommandGroup.Window,
                Localization.BreakpointWindowDisable,
                Localization.BreakpointWindowDisableComment,
                this);

            commandService.RegisterCommand(
                Command.BreakpointWindowCondition,
                Menu.Window,
                CommandGroup.Window,
                Localization.BreakpointWindowCondition,
                Localization.BreakpointWindowConditionComment,
                this);

            commandService.RegisterCommand(
                Command.BreakpointWindowConditionEnable,
                Menu.Window,
                CommandGroup.Window,
                Localization.BreakpointWindowConditionEnable,
                Localization.BreakpointWindowConditionEnableComment,
                this);

            commandService.RegisterCommand(
                Command.BreakpointWindowConditionDisable,
                Menu.Window,
                CommandGroup.Window,
                Localization.BreakpointWindowConditionDisable,
                Localization.BreakpointWindowConditionDisableComment,
                this);

            commandService.RegisterCommand(
                Command.BreakpointWindowRemoveAll,
                Menu.Window,
                CommandGroup.Window,
                Localization.BreakpointWindowRemoveAll,
                Localization.BreakpointWindowRemoveAllComment,
                this);

            menuInfoWindow.GetMenuItem().Visible = false;
        }

        #region IInitializable Interface

        void IInitializable.Initialize()
        {
            m_documentService = SledServiceInstance.Get<ISledDocumentService>();
            m_documentService.Opened += DocumentServiceOpened;
            m_documentService.Closed += DocumentServiceClosed;
            
            m_projectService = SledServiceInstance.Get<ISledProjectService>();
            m_projectService.Created += ProjectServiceCreated;
            m_projectService.Opened += ProjectServiceOpened;
            m_projectService.Closing += ProjectServiceClosing;
            m_projectService.FileAdded += ProjectServiceFileAdded;
            m_projectService.FileOpened += ProjectServiceFileOpened;
            m_projectService.FileRemoving += ProjectServiceFileRemoving;

            m_breakpointEditor = SledServiceInstance.Get<SledBreakpointEditor>();

            m_modifiedFilesFormService = SledServiceInstance.Get<ISledModifiedFilesFormService>();
            m_modifiedFilesFormService.FileReloading += ModifiedFilesFormServiceFileReloading;
            m_modifiedFilesFormService.FileReloaded += ModifiedFilesFormServiceFileReloaded;
        }

        #endregion

        #region Commands

        enum Command
        {
            // Breakpoint context menu commands (commands shown in SledDocument context menu)
            BreakpointAdd,                      // Command to add a breakpoint to a line
            BreakpointRemove,                   // Command to remove a breakpoint from a line
            BreakpointEnable,                   // Command to enable a breakpoint on a line
            BreakpointDisable,                  // Command to disable a breakpoint on a line
            BreakpointCondition,                // Command to bring up the breakpoint condition window
            BreakpointRemoveAll,                // Command to remote all breakpoints from a file

            // Breakpoint window commands (commands from the breakpoint window)
            BreakpointWindowRemove,             // Command to remove a breakpoint from a line
            BreakpointWindowEnable,             // Command to enable a breakpoint on a line
            BreakpointWindowDisable,            // Command to disable a breakpoint on a line
            BreakpointWindowCondition,          // Command to bring up the breakpoint condition window
            BreakpointWindowConditionEnable,    // Command to enable the condition
            BreakpointWindowConditionDisable,   // Command to disable the condition
            BreakpointWindowRemoveAll,          // Command to remove all selected breakpoints
        }

        enum Menu
        {
            Context,
            Window,
        }

        enum CommandGroup
        {
            Context,
            Window,
        }

        #endregion

        #region ICommandClient Interface

        public bool CanDoCommand(object commandTag)
        {
            var bEnabled = false;

            if (commandTag is Command)
            {
                var command = (Command)commandTag;
                bEnabled = CanDoContextCommand(command);
                if (!bEnabled)
                    bEnabled = CanDoWindowCommand(command);
            }

            return bEnabled;
        }

        private bool CanDoContextCommand(Command command)
        {
            // Rest of the commands rely on there being an active document
            // so we can bail now if no active document exists
            if (!m_documentService.Active)
                return false;

            var bEnabled = false;

            // Grab active document
            var sd = m_documentService.ActiveDocument;

            // Several of the following commands check these
            var bIsValidLine = sd.IsValidLine(m_iContextMenuClickedLine);
            var bBreakpointSet = sd.IsBreakpointSet(m_iContextMenuClickedLine);

            switch (command)
            {
                case Command.BreakpointAdd:
                    bEnabled = bIsValidLine && !bBreakpointSet;
                    break;

                case Command.BreakpointRemove:
                    bEnabled = bIsValidLine && bBreakpointSet;
                    break;

                case Command.BreakpointEnable:
                    bEnabled =
                        bIsValidLine &&
                        bBreakpointSet &&
                        !sd.IsBreakpointEnabled(m_iContextMenuClickedLine);
                    break;

                case Command.BreakpointDisable:
                    bEnabled =
                        bIsValidLine &&
                        bBreakpointSet &&
                        sd.IsBreakpointSet(m_iContextMenuClickedLine);
                    break;

                case Command.BreakpointCondition:
                    bEnabled = bIsValidLine && bBreakpointSet && (sd.LanguagePlugin != null) && (sd.SledProjectFile != null);
                    break;

                case Command.BreakpointRemoveAll:
                    bEnabled = (sd.Editor.GetBreakpoints().Length > 0);
                    break;
            }

            return bEnabled;
        }

        private bool CanDoWindowCommand(Command command)
        {
            var bEnabled = false;

            switch (command)
            {
                case Command.BreakpointWindowRemove:
                case Command.BreakpointWindowEnable:
                case Command.BreakpointWindowDisable:
                case Command.BreakpointWindowCondition:
                    bEnabled = (m_windowSelection.Count == 1);
                    break;

                case Command.BreakpointWindowConditionEnable:
                {
                    if (m_windowSelection.Count == 1)
                    {
                        var bp =
                            m_windowSelection[0].As<SledProjectFilesBreakpointType>();

                        if (!string.IsNullOrEmpty(bp.Condition))
                            bEnabled = !bp.ConditionEnabled;
                    }
                }
                break;

                case Command.BreakpointWindowConditionDisable:
                {
                    if (m_windowSelection.Count == 1)
                    {
                        var bp =
                            m_windowSelection[0].As<SledProjectFilesBreakpointType>();

                        if (!string.IsNullOrEmpty(bp.Condition))
                            bEnabled = bp.ConditionEnabled;
                    }
                }
                break;
                    
                case Command.BreakpointWindowRemoveAll:
                    bEnabled = (m_windowSelection.Count > 1);
                    break;
            }

            return bEnabled;
        }

        public void DoCommand(object commandTag)
        {
            if (commandTag is Command)
            {
                switch ((Command)commandTag)
                {
                    case Command.BreakpointAdd:
                        m_documentService.ActiveDocument.Editor.Breakpoint(m_iContextMenuClickedLine, true);
                        break;

                    case Command.BreakpointRemove:
                        m_documentService.ActiveDocument.Editor.Breakpoint(m_iContextMenuClickedLine, false);
                        break;

                    case Command.BreakpointEnable:
                        EnableOrDisable(m_documentService.ActiveDocument, m_iContextMenuClickedLine, true);
                        break;

                    case Command.BreakpointDisable:
                        EnableOrDisable(m_documentService.ActiveDocument, m_iContextMenuClickedLine, false);
                        break;

                    case Command.BreakpointCondition:
                        ShowBreakpointConditionForm(m_documentService.ActiveDocument, m_iContextMenuClickedLine);
                        break;

                    case Command.BreakpointRemoveAll:
                        RemoveBreakpoints(m_documentService.ActiveDocument, m_documentService.ActiveDocument.Editor.GetBreakpoints());
                        break;

                    case Command.BreakpointWindowRemove:
                        RemoveBreakpoint(m_windowSelection[0]);
                        break;

                    case Command.BreakpointWindowEnable:
                        EnableOrDisable(m_windowSelection[0], true);
                        break;

                    case Command.BreakpointWindowDisable:
                        EnableOrDisable(m_windowSelection[0], false);
                        break;

                    case Command.BreakpointWindowCondition:
                        ShowBreakpointConditionForm(m_windowSelection[0]);
                        break;

                    case Command.BreakpointWindowConditionEnable:
                        ConditionEnableOrDisable(m_windowSelection[0], true);
                        break;

                    case Command.BreakpointWindowConditionDisable:
                        ConditionEnableOrDisable(m_windowSelection[0], false);
                        break;

                    case Command.BreakpointWindowRemoveAll:
                    {
                        foreach (var domNode in m_windowSelection)
                        {
                            RemoveBreakpoint(domNode);
                        }
                    }
                    break;
                }

                // Save changes
                m_projectService.SaveSettings();
            }
        }

        public void UpdateCommand(object commandTag, CommandState state)
        {
        }

        #endregion

        #region ISledBreakpointService Interface

        /// <summary>
        /// Event fired when a breakpoint has been added
        /// </summary>
        public event EventHandler<SledBreakpointServiceBreakpointEventArgs> Added;

        /// <summary>
        /// Event fired when a breakpoint has been silently added
        /// </summary>
        public event EventHandler<SledBreakpointServiceBreakpointEventArgs> SilentAdded;

        /// <summary>
        /// Event fired when a breakpoint is being removed
        /// </summary>
        public event EventHandler<SledBreakpointServiceBreakpointEventArgs> Removing;

        /// <summary>
        /// Event fired when a breakpoint is about to be changed
        /// </summary>
        public event EventHandler<SledBreakpointServiceBreakpointChangingEventArgs> Changing;

        /// <summary>
        /// Event fired after a breakpoint has changed
        /// </summary>
        public event EventHandler<SledBreakpointServiceBreakpointChangingEventArgs> Changed;

        /// <summary>
        /// Add a breakpoint to a file
        /// </summary>
        /// <param name="file">File to add breakpoint to</param>
        /// <param name="lineNumber">Line number</param>
        public void AddBreakpoint(SledProjectFilesFileType file, int lineNumber)
        {
            AddBreakpoint(file, lineNumber, null, true, false);
        }

        /// <summary>
        /// Add a breakpoint to a file supplying a condition
        /// </summary>
        /// <param name="file">File to add breakpoint to</param>
        /// <param name="lineNumber">Line number</param>
        /// <param name="condition">Condition</param>
        /// <param name="bConditionResult">Whether condition evaluates to true or false</param>
        public void AddBreakpoint(SledProjectFilesFileType file, int lineNumber, string condition, bool bConditionResult)
        {
            AddBreakpoint(file, lineNumber, condition, bConditionResult, false);
        }

        /// <summary>
        /// Add a breakpoint to a file supplying a condition
        /// </summary>
        /// <param name="file">File to add breakpoint to</param>
        /// <param name="lineNumber">Line number</param>
        /// <param name="condition">Condition</param>
        /// <param name="bConditionResult">Whether condition evaluates to true or false</param>
        /// <param name="bUseFunctionEnvironment">Whether to use the current function's environment or _G when checking the breakpoint condition (if any)</param>
        public void AddBreakpoint(SledProjectFilesFileType file, int lineNumber, string condition, bool bConditionResult, bool bUseFunctionEnvironment)
        {
            SledProjectFilesBreakpointType breakpoint;
            AddBreakpoint(file, lineNumber, condition, bConditionResult, true, bUseFunctionEnvironment, out breakpoint);
        }

        /// <summary>
        /// Add a breakpoint to a file supplying a condition
        /// </summary>
        /// <param name="file">File to add breakpoint to</param>
        /// <param name="lineNumber">Line number</param>
        /// <param name="condition">Condition</param>
        /// <param name="conditionResult">Whether condition evaluates to true or false</param>
        /// <param name="conditionEnabled">Whether the condition is enabled or not</param>
        /// <param name="useFunctionEnvironment">Whether to use the current function's environment or _G when checking the breakpoint condition (if any)</param>
        /// <param name="breakpoint">The breakpoint if it was added otherwise null</param>
        /// <returns>Whether the breakpoint was added or not</returns>
        public bool AddBreakpoint(SledProjectFilesFileType file, int lineNumber, string condition, bool conditionResult, bool conditionEnabled, bool useFunctionEnvironment, out SledProjectFilesBreakpointType breakpoint)
        {
            breakpoint = null;

            if (file == null)
                return false;

            // Can't add more than one breakpoint per line
            if (IsDuplicate(file, lineNumber))
                return false;

            // Breakpoint in the document (or none if document not open)
            IBreakpoint ibp = null;

            var sd = file.SledDocument;
            if (sd != null)
            {
                m_bAddingOrRemoving = true;

                // Add breakpoint to open document
                sd.Editor.Breakpoint(lineNumber, true);
                ibp = sd.Editor.GetBreakpoint(lineNumber);

                m_bAddingOrRemoving = false;
            }

            // Create project style breakpoint
            breakpoint = SledProjectFilesBreakpointType.Create(ibp);

            // set some properties in case the document isn't open and we don't have a real IBreakpoint yet
            if (ibp == null)
            {
                breakpoint.Line = lineNumber;
                breakpoint.Enabled = true;
            }

            // Setup condition if any
            if (!string.IsNullOrEmpty(condition))
            {
                breakpoint.Condition = condition;
                breakpoint.ConditionResult = conditionResult;
                breakpoint.ConditionEnabled = conditionEnabled;
                breakpoint.UseFunctionEnvironment = useFunctionEnvironment;

                // Draw breakpoint indicator in open document breakpoint
                if (ibp != null)
                    ibp.Marker = true;
            }

            // Add breakpoint to file finally
            file.Breakpoints.Add(breakpoint);
            
            return breakpoint != null;
        }

        /// <summary>
        /// Remove a breakpoint from a file
        /// </summary>
        /// <param name="file">File to remove breakpoint from</param>
        /// <param name="lineNumber">Line number</param>
        public void RemoveBreakpoint(SledProjectFilesFileType file, int lineNumber)
        {
            if (file == null)
                return;

            if (lineNumber < 0)
                return;

            var breakpoint = file.Breakpoints.FirstOrDefault(bp => bp.Line == lineNumber);
            if (breakpoint == null)
                return;

            file.Breakpoints.Remove(breakpoint);
        }

        #endregion

        #region ISledDocument Breakpoint Events

        private void EditorBreakpointChanging(object sender, BreakpointEventArgs e)
        {
            // Set to cancel unless we meet criteria later
            e.Cancel = true;

            CheckLineNumber(e.LineNumber);

            var sd = GetSledDocumentFromSender(sender);
            if (sd == null)
                return;

            // Don't allow breakpoints in files that don't have a plugin associated with them
            if (sd.LanguagePlugin == null)
                return;

            // By default allow the breakpoint change to continue
            e.Cancel = false;

            // When adding a breakpoint we need to check with the language plugin to see if it
            // supports the addition of a new breakpoint
            if (e.IsSet)
            {
                // Grab all breakpoint plugins
                var plugins =
                    SledServiceInstance.GetAll<ISledBreakpointPlugin>();
                
                foreach (var listable in plugins)
                {
                    // Find plugin that implments the breakpoint plugin
                    if (listable != sd.LanguagePlugin)
                        continue;

                    // Check with plugin to see if breakpoint can be added
                    e.Cancel =
                        !listable.CanAdd(
                            sd,
                            e.LineNumber,
                            e.LineText,
                            GetNumberOfBreakpointsForLanguagePlugin(sd.LanguagePlugin));
                }
            }
        }

        private void EditorBreakpointAdded(object sender, IBreakpointEventArgs e)
        {
            if (m_bAddingOrRemoving)
                return;

            var sd = GetSledDocumentFromSender(sender);
            if (sd == null)
                return;

            if (sd.SledProjectFile == null)
                return;

            AddBreakpoint(sd, e.Breakpoint);
        }

        private void EditorBreakpointRemoved(object sender, IBreakpointEventArgs e)
        {
            if (m_bAddingOrRemoving)
                return;

            var sd = GetSledDocumentFromSender(sender);
            if (sd == null)
                return;

            if (sd.SledProjectFile == null)
                return;

            RemoveBreakpoint(sd, e.Breakpoint);
        }

        private void SledDocumentDocumentLineCountChanged(object sender, SledDocumentLineCountChangedArgs e)
        {
            var sd = GetSledDocumentFromSender(sender);
            if (sd == null)
                return;

            // Don't care about files not in the project
            if (sd.SledProjectFile == null)
                return;

            var bChanged = false;

            foreach (var bp in sd.SledProjectFile.Breakpoints)
            {
                if (bp.Breakpoint == null)
                    continue;

                // Only want to process breakpoints that actually moved lines
                if (bp.RawLine == bp.Breakpoint.LineNumber)
                    continue;

                bChanged = true;

                // Create event
                var ea =
                    new SledBreakpointServiceBreakpointChangingEventArgs(
                        SledBreakpointChangeType.LineNumber,
                        bp,
                        bp.RawLine,
                        bp.Breakpoint.LineNumber);

                // Fire event
                OnBreakpointChanging(ea);

                // Sync up line numbers from IBreakpoint counterpart
                bp.Refresh();

                // Fire event
                OnBreakpointChanged(ea);
            }

            // Save changes
            if (bChanged)
                m_projectService.SaveSettings();
        }

        private static ISledDocument GetSledDocumentFromSender(object sender)
        {
            if (sender == null)
                return null;

            if (sender is ISledDocument)
                return sender as ISledDocument;

            var sec = sender as ISyntaxEditorControl;
            if (sec == null)
                return null;

            if (sec.Control == null)
                return null;

            if (sec.Control.Tag == null)
                return null;

            return sec.Control.Tag as ISledDocument;
        }

        private int GetNumberOfBreakpointsForLanguagePlugin(ISledLanguagePlugin languagePlugin)
        {
            // Grab all files this language plugin 'owns'
            var where =
                m_projectService.AllFiles.Where(
                    file => file.LanguagePlugin == languagePlugin);

            // Count files' breakpoints
            return where.Sum(file => file.Breakpoints.Count);
        }

        #endregion

        #region ISledDocumentService Events

        private void DocumentServiceOpened(object sender, SledDocumentServiceEventArgs e)
        {
            var sd = e.Document;

            if (sd == null)
                return;

            if (sd.Editor == null)
                return;

            sd.Editor.BreakpointChanging += EditorBreakpointChanging;
            sd.Editor.BreakpointAdded += EditorBreakpointAdded;
            sd.Editor.BreakpointRemoved += EditorBreakpointRemoved;
            sd.DocumentLineCountChanged += SledDocumentDocumentLineCountChanged;
        }

        private void DocumentServiceClosed(object sender, SledDocumentServiceEventArgs e)
        {
            var sd = e.Document;

            if (sd == null)
                return;

            if (sd.Editor == null)
                return;

            sd.Editor.BreakpointChanging -= EditorBreakpointChanging;
            sd.Editor.BreakpointAdded -= EditorBreakpointAdded;
            sd.Editor.BreakpointRemoved -= EditorBreakpointRemoved;
            sd.DocumentLineCountChanged -= SledDocumentDocumentLineCountChanged;
        }

        #endregion

        #region DomNode Events

        private void SubscribeToEvents(SledProjectFilesType project)
        {
            project.DomNode.AttributeChanging += CollectionAttributeChanging;
            project.DomNode.AttributeChanged += CollectionAttributeChanged;
            project.DomNode.ChildInserted += CollectionChildInserted;
            project.DomNode.ChildRemoving += CollectionChildRemoving;
        }

        private void UnsubscribeFromEvents(SledProjectFilesType project)
        {
            project.DomNode.ChildInserted -= CollectionChildInserted;
            project.DomNode.ChildRemoving -= CollectionChildRemoving;
        }

        private void CollectionAttributeChanging(object sender, AttributeEventArgs e)
        {
            if (e.DomNode.Type != SledSchema.SledProjectFilesBreakpointType.Type)
                return;

            var bp = e.DomNode.As<SledProjectFilesBreakpointType>();

            if (e.AttributeInfo == SledSchema.SledProjectFilesBreakpointType.enabledAttribute)
            {
                var bOldValue = e.OldValue == null ? (bool)e.AttributeInfo.DefaultValue : (bool)e.OldValue;
                var bNewValue = (bool)e.NewValue;

                if (bOldValue != bNewValue)
                {
                    var changeType = 
                        bNewValue
                            ? SledBreakpointChangeType.Enabled
                            : SledBreakpointChangeType.Disabled;

                    var ea = new SledBreakpointServiceBreakpointChangingEventArgs(changeType, bp);

                    // Fire event
                    OnBreakpointChanging(ea);
                }
            }
            else if (e.AttributeInfo == SledSchema.SledProjectFilesBreakpointType.conditionenabledAttribute)
            {
                var bOldValue = e.OldValue == null ? (bool)e.AttributeInfo.DefaultValue : (bool)e.OldValue;
                var bNewValue = (bool)e.NewValue;

                if (bOldValue != bNewValue)
                {
                    var changeType = 
                        bNewValue
                            ? SledBreakpointChangeType.ConditionEnabled
                            : SledBreakpointChangeType.ConditionDisabled;

                    var ea = new SledBreakpointServiceBreakpointChangingEventArgs(changeType, bp);

                    // Fire event
                    OnBreakpointChanging(ea);
                }
            }
            else if (e.AttributeInfo == SledSchema.SledProjectFilesBreakpointType.conditionresultAttribute)
            {
                var bOldValue = e.OldValue == null ? (bool)e.AttributeInfo.DefaultValue : (bool)e.OldValue;
                var bNewValue = (bool)e.NewValue;

                if (bOldValue != bNewValue)
                {
                    var changeType = 
                        bNewValue
                            ? SledBreakpointChangeType.ConditionResultTrue
                            : SledBreakpointChangeType.ConditionResultFalse;

                    var ea = new SledBreakpointServiceBreakpointChangingEventArgs(changeType, bp);

                    // Fire event
                    OnBreakpointChanging(ea);
                }
            }
            else if (e.AttributeInfo == SledSchema.SledProjectFilesBreakpointType.conditionAttribute)
            {
                var oldValue = e.OldValue as string;
                var newValue = e.NewValue as string;

                if (string.Compare(oldValue, newValue) != 0)
                {
                    const SledBreakpointChangeType changeType =
                        SledBreakpointChangeType.Condition;

                    var ea = new SledBreakpointServiceBreakpointChangingEventArgs(changeType, bp, oldValue, newValue);

                    // Fire event
                    OnBreakpointChanging(ea);
                }
            }
            else if (e.AttributeInfo == SledSchema.SledProjectFilesBreakpointType.usefunctionenvironmentAttribute)
            {
                var bOldValue = e.OldValue == null ? (bool)e.AttributeInfo.DefaultValue : (bool)e.OldValue;
                var bNewValue = (bool)e.NewValue;

                if (bOldValue != bNewValue)
                {
                    var changeType =
                        bNewValue
                            ? SledBreakpointChangeType.UseFunctionEnvironmentTrue
                            : SledBreakpointChangeType.UseFunctionEnvironmentFalse;

                    var ea = new SledBreakpointServiceBreakpointChangingEventArgs(changeType, bp);

                    // Fire event
                    OnBreakpointChanging(ea);
                }
            }
        }

        private void CollectionAttributeChanged(object sender, AttributeEventArgs e)
        {
            if (e.DomNode.Type != SledSchema.SledProjectFilesBreakpointType.Type)
                return;

            var bp = e.DomNode.As<SledProjectFilesBreakpointType>();

            if (e.AttributeInfo == SledSchema.SledProjectFilesBreakpointType.enabledAttribute)
            {
                var bOldValue = e.OldValue == null ? (bool)e.AttributeInfo.DefaultValue : (bool)e.OldValue;
                var bNewValue = (bool)e.NewValue;

                if (bOldValue != bNewValue)
                {
                    var changeType = 
                        bNewValue
                            ? SledBreakpointChangeType.Enabled
                            : SledBreakpointChangeType.Disabled;

                    var ea = new SledBreakpointServiceBreakpointChangingEventArgs(changeType, bp);

                    // Fire event
                    OnBreakpointChanged(ea);

                    // Assure open document's breakpoints are drawn correctly
                    var sd = bp.File.SledDocument;
                    if (sd != null)
                        sd.Control.Refresh();
                }
            }
            else if (e.AttributeInfo == SledSchema.SledProjectFilesBreakpointType.conditionenabledAttribute)
            {
                var bOldValue = e.OldValue == null ? (bool)e.AttributeInfo.DefaultValue : (bool)e.OldValue;
                var bNewValue = (bool)e.NewValue;

                if (bOldValue != bNewValue)
                {
                    var changeType = 
                        bNewValue
                            ? SledBreakpointChangeType.ConditionEnabled
                            : SledBreakpointChangeType.ConditionDisabled;

                    var ea = new SledBreakpointServiceBreakpointChangingEventArgs(changeType, bp);

                    // Fire event
                    OnBreakpointChanged(ea);

                    // Assure open document's breakpoints are drawn correctly
                    var sd = bp.File.SledDocument;
                    if (sd != null)
                        sd.Control.Refresh();
                }
            }
            else if (e.AttributeInfo == SledSchema.SledProjectFilesBreakpointType.conditionresultAttribute)
            {
                var bOldValue = e.OldValue == null ? (bool)e.AttributeInfo.DefaultValue : (bool)e.OldValue;
                var bNewValue = (bool)e.NewValue;

                if (bOldValue != bNewValue)
                {
                    var changeType = 
                        bNewValue
                            ? SledBreakpointChangeType.ConditionResultTrue
                            : SledBreakpointChangeType.ConditionResultFalse;

                    var ea = new SledBreakpointServiceBreakpointChangingEventArgs(changeType, bp);

                    // Fire event
                    OnBreakpointChanged(ea);

                    // Assure open document's breakpoints are drawn correctly
                    var sd = bp.File.SledDocument;
                    if (sd != null)
                        sd.Control.Refresh();
                }
            }
            else if (e.AttributeInfo == SledSchema.SledProjectFilesBreakpointType.conditionAttribute)
            {
                var oldValue = e.OldValue as string;
                var newValue = e.NewValue as string;

                if (string.Compare(oldValue, newValue) != 0)
                {
                    const SledBreakpointChangeType changeType =
                        SledBreakpointChangeType.Condition;

                    var ea = new SledBreakpointServiceBreakpointChangingEventArgs(changeType, bp, oldValue, newValue);

                    // Fire event
                    OnBreakpointChanged(ea);

                    // Assure open document's breakpoints are drawn correctly
                    var sd = bp.File.SledDocument;
                    if (sd != null)
                        sd.Control.Refresh();
                }
            }
            else if (e.AttributeInfo == SledSchema.SledProjectFilesBreakpointType.usefunctionenvironmentAttribute)
            {
                var bOldValue = e.OldValue == null ? (bool)e.AttributeInfo.DefaultValue : (bool)e.OldValue;
                var bNewValue = (bool)e.NewValue;

                if (bOldValue != bNewValue)
                {
                    var changeType =
                        bNewValue
                            ? SledBreakpointChangeType.UseFunctionEnvironmentTrue
                            : SledBreakpointChangeType.UseFunctionEnvironmentFalse;

                    var ea = new SledBreakpointServiceBreakpointChangingEventArgs(changeType, bp);

                    // Fire event
                    OnBreakpointChanged(ea);
                }
            }
        }

        private void CollectionChildInserted(object sender, ChildEventArgs e)
        {
            if (e.Child.Type != SledSchema.SledProjectFilesBreakpointType.Type)
                return;

            var bp = e.Child.As<SledProjectFilesBreakpointType>();

            // Fire event
            OnBreakpointAdded(new SledBreakpointServiceBreakpointEventArgs(bp));
        }

        private void CollectionChildRemoving(object sender, ChildEventArgs e)
        {
            if (e.Child.Type != SledSchema.SledProjectFilesBreakpointType.Type)
                return;

            var bp = e.Child.As<SledProjectFilesBreakpointType>();

            // Fire event
            OnBreakpointRemoving(new SledBreakpointServiceBreakpointEventArgs(bp));

            if (m_bPreserveOpenDocumentBreakpoints)
                return;

            // Try and remove breakpoints from the
            // corresponding open document (if any)
            var sd = bp.File.SledDocument;
            if (sd == null)
                return;

            // Preserve previous value
            var bValue = m_bAddingOrRemoving;

            // Make sure SledDocument breakpoint events won't fire
            m_bAddingOrRemoving = true;

            // Verify breakpoint on line in open document & remove if so
            if (sd.IsBreakpointSet(bp.Line))
                sd.Editor.Breakpoint(bp.Line, false);

            // Reset to previous value
            m_bAddingOrRemoving = bValue;
        }

        #endregion

        #region ISledProjectService Events

        public void ProjectServiceCreated(object sender, SledProjectServiceProjectEventArgs e)
        {
            SubscribeToEvents(e.Project);
            AddBreakpoints(e.Project);
        }

        public void ProjectServiceOpened(object sender, SledProjectServiceProjectEventArgs e)
        {
            SubscribeToEvents(e.Project);
            AddBreakpoints(e.Project);
            //SendLiveConnectProjectBreakpoints(e.Project);
        }

        public void ProjectServiceClosing(object sender, SledProjectServiceProjectEventArgs e)
        {
            RemoveBreakpoints(e.Project);
            UnsubscribeFromEvents(e.Project);
            m_bAddingOrRemoving = false;
        }

        private void ProjectServiceFileAdded(object sender, SledProjectServiceFileEventArgs e)
        {
            var sd = e.File.SledDocument;

            // If no open document then do nothing
            if (sd == null)
                return;

            // Add breakpoints from the open document to the GUI
            AddBreakpoints(sd, sd.Editor.GetBreakpoints());
        }

        private void ProjectServiceFileOpened(object sender, SledProjectServiceFileEventArgs e)
        {
            // Silently add breakpoints from the project to
            // the newly opened document
            AddBreakpoints(e.File);
        }

        private void ProjectServiceFileRemoving(object sender, SledProjectServiceFileEventArgs e)
        {
            // Keep breakpoints in the open document (if its open)
            m_bPreserveOpenDocumentBreakpoints = true;

            // Remove breakpoints in the file from the project
            // but leave the open document (if any) alone
            RemoveBreakpoints(e.File);

            m_bPreserveOpenDocumentBreakpoints = false;
        }

        #endregion

        #region ISledModifiedFilesFormService Events

        private void ModifiedFilesFormServiceFileReloading(object sender, SledFileWatcherServiceEventArgs e)
        {
            m_breakpointsMoving.Clear();

            // Try and track breakpoints

            if ((e.Document == null) ||
                (e.Document.SledProjectFile == null) ||
                (e.Document.SledProjectFile.Breakpoints.Count <= 0))
                return;

            using (var sd = SledDocument.CreateHidden(e.Document.Uri, null))
            {
                if (sd.Editor == null)
                    return;

                try
                {
                    sd.Read();
                }
                catch (Exception ex)
                {
                    SledOutDevice.OutLine(
                        SledMessageType.Error,
                        "{0}: Exception reading file for tracking breakpoints after external editor modification: {1}",
                        this, ex.Message);

                    return;
                }

                var breakpoints = e.Document.SledProjectFile.Breakpoints;
                var bpsToRemove = new List<SledProjectFilesBreakpointType>();

                foreach (var bp in breakpoints)
                {
                    var originalFileLineText = bp.LineText;
                    if (string.IsNullOrEmpty(originalFileLineText))
                        continue;

                    const int invalidLine = -1;
                    var foundLine = invalidLine;
                    var pos = bp.Line;

                    const int iterationThreshold = 128;
                    var iterations = 0;

                    // check down
                    for (; (pos < sd.Editor.DocumentLineCount) && (foundLine == invalidLine) && (iterations < iterationThreshold); ++pos)
                    {
                        var modifiedFileLineText = GetLineText(sd.Editor, pos);
                        if (string.Compare(originalFileLineText, modifiedFileLineText, true) == 0)
                            foundLine = pos;

                        iterations++;
                    }

                    // check up if not found down
                    if (foundLine == invalidLine)
                    {
                        pos = bp.Line - 1;
                        iterations = 0;

                        for (; (pos >= 1) && (foundLine == invalidLine) && (iterations < iterationThreshold); --pos)
                        {
                            var modifiedFileLineText = GetLineText(sd.Editor, pos);
                            if (string.Compare(originalFileLineText, modifiedFileLineText, true) == 0)
                                foundLine = pos;

                            iterations++;
                        }
                    }

                    if (foundLine != invalidLine)
                    {
                        if (bp.Line != foundLine)
                        {
                            var tempBp =
                                new TempBpDetails(
                                    bp.File,
                                    foundLine,
                                    bp.Condition,
                                    bp.ConditionResult,
                                    bp.ConditionEnabled,
                                    bp.UseFunctionEnvironment);

                            m_breakpointsMoving.Add(tempBp);
                            bpsToRemove.Add(bp);
                        }
                    }
                    else
                    {
                        bpsToRemove.Add(bp);
                    }
                }

                // Remove any breakpoints that moved or that we were unable to track
                foreach (var bp in bpsToRemove)
                    RemoveBreakpoint(bp.File, bp.Line);
            }
        }

        private void ModifiedFilesFormServiceFileReloaded(object sender, SledFileWatcherServiceEventArgs e)
        {
            if (m_breakpointsMoving.Count <= 0)
                return;

            try
            {
                foreach (var tempBpDetails in m_breakpointsMoving)
                {
                    SledProjectFilesBreakpointType breakpoint;
                    AddBreakpoint(
                        tempBpDetails.File,
                        tempBpDetails.Line,
                        tempBpDetails.Condition,
                        tempBpDetails.ConditionResult,
                        tempBpDetails.ConditionEnabled,
                        tempBpDetails.UseFunctionEnvironment,
                        out breakpoint);
                }
            }
            finally
            {
                m_breakpointsMoving.Clear();
            }

            // Write out any breakpoint changes
            m_projectService.SaveSettings();
        }

        private static string GetLineText(ISyntaxEditorControl control, int line)
        {
            try
            {
                return control.GetLineText(line);
            }
            catch (ArgumentOutOfRangeException)
            {
                return string.Empty;
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
            var commands = new List<object>();

            // Only care about breakpoint region
            if (args.Region != SledDocumentRegions.Breakpoint)
                return commands;

            // Get out if no active document
            if (!m_documentService.Active)
                return commands;

            // Grab active document
            var sd = m_documentService.ActiveDocument;

            // Store line number
            m_iContextMenuClickedLine = args.LineNumber;

            // Add breakpoint commands
            if (sd.IsValidLine(m_iContextMenuClickedLine))
            {
                if (sd.IsBreakpointSet(m_iContextMenuClickedLine))
                {
                    commands.Add(Command.BreakpointRemove);

                    if (sd.SledProjectFile != null)
                    {
                        commands.Add(
                            sd.IsBreakpointEnabled(m_iContextMenuClickedLine)
                                ? Command.BreakpointDisable
                                : Command.BreakpointEnable);

                        commands.Add(Command.BreakpointCondition);
                    }
                }
                else
                    commands.Add(Command.BreakpointAdd);
            }

            if (sd.Editor.GetBreakpoints().Length > 0)
                commands.Add(Command.BreakpointRemoveAll);

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

        #region IContextMenuCommandProvider Interface

        public IEnumerable<object> GetCommands(object context, object target)
        {
            if (target == null)
                return s_emptyObjectList;

            if (!target.Is<SledProjectFilesBreakpointType>())
                return s_emptyObjectList;

            var commands = new List<object>();

            // Copy over selection
            m_windowSelection.Clear();

            foreach (object item in m_breakpointEditor.Selection)
            {
                if (!item.Is<DomNode>())
                    continue;

                m_windowSelection.Add(item.As<DomNode>());
            }

            if (m_windowSelection.Count == 1)
            {
                var bp = m_windowSelection[0].As<SledProjectFilesBreakpointType>();

                commands.Add(Command.BreakpointWindowRemove);

                commands.Add(
                    bp.Enabled
                        ? Command.BreakpointWindowDisable
                        : Command.BreakpointWindowEnable);

                commands.Add(Command.BreakpointWindowCondition);

                if (!string.IsNullOrEmpty(bp.Condition))
                {
                    commands.Add(
                        bp.ConditionEnabled
                            ? Command.BreakpointWindowConditionDisable
                            : Command.BreakpointWindowConditionEnable);
                }
            }
            else if (m_windowSelection.Count > 1)
            {
                commands.Add(Command.BreakpointWindowRemoveAll);
            }

            return commands;
        }

        #endregion

        #region Member Methods

        private void AddBreakpoints(SledProjectFilesType project)
        {
            try
            {
                m_bAddingOrRemoving = true;

                var bpInvalids = new List<SledProjectFilesBreakpointType>();

                // Go through files in project adding breakpoints to the GUI
                // Also add breakpoint indicators to open documents that
                // correspond to files in the project
                foreach (var file in project.AllFiles)
                {
                    var sd = file.SledDocument;

                    foreach (var bp in file.Breakpoints)
                    {
                        var bValid = true;

                        if (sd != null)
                        {
                            // Verify breakpoint not already set on line
                            // and that the line is valid
                            if (!sd.IsBreakpointSet(bp.Line) &&
                                !sd.IsValidLine(bp.Line))
                            {
                                bValid = false;
                                bpInvalids.Add(bp);
                            }
                        }

                        // Breakpoint isn't valid so skip to next
                        if (!bValid)
                            continue;

                        // Breakpoint needs to get added but first lets see
                        // if there's an IBreakpoint we can associate it with

                        // Get breakpoint from the open document
                        IBreakpoint ibp = null;
                        if (sd != null)
                        {
                            if (sd.IsBreakpointSet(bp.Line))
                            {
                                // Grab existing
                                ibp = sd.Editor.GetBreakpoint(bp.Line);
                            }
                            else
                            {
                                // Create new indicator in the document
                                sd.Editor.Breakpoint(bp.Line, true);
                                if (sd.IsBreakpointSet(bp.Line))
                                    ibp = sd.Editor.GetBreakpoint(bp.Line);
                            }

                            // Couldn't set breakpoint somehow?
                            if (ibp == null)
                            {
                                bpInvalids.Add(bp);
                                continue;
                            }
                        }

                        if (ibp != null)
                        {
                            // Copy over values first
                            ibp.Enabled = bp.Enabled;
                            ibp.Marker = bp.ConditionEnabled;
                        }

                        // Set reference
                        bp.Breakpoint = ibp;

                        // Fire event
                        OnBreakpointAdded(new SledBreakpointServiceBreakpointEventArgs(bp));
                    }

                    if (sd != null)
                    {
                        AddBreakpoints(sd, sd.Editor.GetBreakpoints());

                        sd.Control.Refresh();
                    }
                }

                // Remove invalid breakpoints
                foreach (var bp in bpInvalids)
                {
                    bp.File.Breakpoints.Remove(bp);
                }
            }
            finally
            {
                m_bAddingOrRemoving = false;
            }
        }

        private void RemoveBreakpoints(SledProjectFilesType project)
        {
            try
            {
                m_bAddingOrRemoving = true;

                foreach (var file in project.AllFiles)
                {
                    var sd = file.SledDocument;

                    foreach (var bp in file.Breakpoints)
                    {
                        // Remove from open document (if any)
                        if ((sd != null) && sd.IsBreakpointSet(bp.Line))
                        {
                            sd.Editor.Breakpoint(bp.Line, false);
                        }

                        // Fire event
                        OnBreakpointRemoving(new SledBreakpointServiceBreakpointEventArgs(bp));
                    }
                }
            }
            finally
            {
                m_bAddingOrRemoving = false;
            }
        }

        private void AddBreakpoints(SledProjectFilesFileType file)
        {
            // Silently add breakpoints from the project to
            // the newly opened document

            if (m_bAddingOrRemoving)
                return;

            try
            {
                m_bAddingOrRemoving = true;

                var sd = file.SledDocument;
                if (sd == null)
                    return;

                var bpInvalids = new List<SledProjectFilesBreakpointType>();

                foreach (var bp in file.Breakpoints)
                {
                    if (sd.IsBreakpointSet(bp.Line))
                        continue;

                    if (!sd.IsValidLine(bp.Line))
                    {
                        bpInvalids.Add(bp);
                        continue;
                    }

                    sd.Editor.Breakpoint(bp.Line, true);
                    if (!sd.IsBreakpointSet(bp.Line))
                    {
                        bpInvalids.Add(bp);
                        continue;
                    }

                    var ibp = sd.Editor.GetBreakpoint(bp.Line);
                    ibp.Enabled = bp.Enabled;
                    ibp.Marker = bp.ConditionEnabled;
                    bp.Breakpoint = ibp;
                    bp.Refresh();

                    // Fire event
                    OnBreakpointSilentAdded(new SledBreakpointServiceBreakpointEventArgs(bp));
                }

                foreach (var bp in bpInvalids)
                {
                    bp.File.Breakpoints.Remove(bp);
                }

                sd.Control.Refresh();
            }
            finally
            {
                m_bAddingOrRemoving = false;
            }
        }

        private void RemoveBreakpoints(SledProjectFilesFileType file)
        {
            // Remove breakpoints in the file from the project
            // but leave the open document (if any) alone

            var needsSaving = file.Breakpoints.Count > 0;

            while (file.Breakpoints.Count > 0)
            {
                // Remove breakpoint from GUI (will fire event through Collection_Removing)
                file.Breakpoints.RemoveAt(0);
            }

            if (needsSaving)
                m_projectService.SaveSettings();
        }

        private void AddBreakpoints(ISledDocument sd, IEnumerable<IBreakpoint> ibps)
        {
            // Add breakpoints from "ibps" to the open document "sd"

            foreach (var ibp in ibps)
            {
                // Check for duplicates
                var bp = FindBreakpointInFile(sd.SledProjectFile, ibp);
                if (bp != null)
                    continue;

                // Create new
                bp = SledProjectFilesBreakpointType.Create(ibp);

                // Add breakpoint to GUI (will fire event through Collection_Inserted)
                sd.SledProjectFile.Breakpoints.Add(bp);
            }

            m_projectService.SaveSettings();
        }

        private void AddBreakpoint(ISledDocument sd, IBreakpoint ibp)
        {
            // Called when click-adding a breakpoint in an open document

            AddBreakpoints(sd, new[] { ibp });
        }

        private void RemoveBreakpoints(ISledDocument sd, IEnumerable<IBreakpoint> ibps)
        {
            // Called when remove-all is selected

            try
            {
                m_bAddingOrRemoving = true;

                var file = sd.SledProjectFile;

                foreach (var ibp in ibps)
                {
                    sd.Editor.Breakpoint(ibp.LineNumber, false);

                    if (file == null)
                        continue;

                    var bp = FindBreakpointInFile(file, ibp);

                    // Remove breakpoint from file (will fire event through Collection_Removing)
                    if (file.Breakpoints.Contains(bp))
                        file.Breakpoints.Remove(bp);
                }

                // Save changes
                m_projectService.SaveSettings();
            }
            finally
            {
                m_bAddingOrRemoving = false;
            }
        }

        private void RemoveBreakpoint(ISledDocument sd, IBreakpoint ibp)
        {
            // Called when click-removing a breakpoint in an open document

            var file = sd.SledProjectFile;
            if (file == null)
                return;
            
            var bp = FindBreakpointInFile(file, ibp);
            if (bp == null)
                return;

            // Remove breakpoint from file (will fire event through Collection_Removing)
            if (file.Breakpoints.Contains(bp))
                file.Breakpoints.Remove(bp);

            // Save changes
            m_projectService.SaveSettings();
        }

        private static void RemoveBreakpoint(DomNode domNode)
        {
            var bp = domNode.As<SledProjectFilesBreakpointType>();

            // Remove from project and breakpoint window (event gets fired through DomCollection_ChildRemoving)
            bp.File.Breakpoints.Remove(bp);
        }

        [System.Diagnostics.Conditional("DEBUG")]
        private static void CheckLineNumber(int lineNumber)
        {
            if (lineNumber <= 0)
                System.Diagnostics.Debugger.Break();
        }

        private static SledProjectFilesBreakpointType FindBreakpointInFile(SledProjectFilesFileType file, IBreakpoint ibp)
        {
            return file.Breakpoints.FirstOrDefault(bp => bp.Breakpoint == ibp);
        }

        private static bool IsDuplicate(SledProjectFilesFileType file, int lineNumber)
        {
            return file.Breakpoints.Any(bp => bp.Line == lineNumber);
        }

        private static void EnableOrDisable(ISledDocument sd, int iLine, bool bEnable)
        {
            if (sd == null)
                return;

            if (!sd.IsValidLine(iLine))
                return;

            var ibp = sd.Editor.GetBreakpoint(iLine);
            if (ibp == null)
                return;

            var file = sd.SledProjectFile;
            if (file == null)
                return;

            var bp = FindBreakpointInFile(file, ibp);
            if (bp == null)
                return;

            // Make change (will cause events to fire in DomCollection_AttributeChanging/DomCollection_AttributeChanged)
            bp.Enabled = bEnable;
        }

        private static void EnableOrDisable(DomNode domNode, bool bEnable)
        {
            var bp = 
                domNode.As<SledProjectFilesBreakpointType>();

            // Make change (will cause events to fire in DomCollection_AttributeChanging/DomCollection_AttributeChanged)
            bp.Enabled = bEnable;
        }

        private static void ConditionEnableOrDisable(DomNode domNode, bool bEnable)
        {
            var bp =
                domNode.As<SledProjectFilesBreakpointType>();

            // Make change (will cause events to fire in DomCollection_AttributeChanging/DomCollection_AttributeChanged)
            bp.ConditionEnabled = bEnable;
        }

        private void ShowBreakpointConditionForm(ISledDocument sd, int lineNumber)
        {
            // Show form coming from context menu in an open document
            if (sd == null)
                return;

            if (!sd.IsValidLine(lineNumber))
                return;

            if (sd.LanguagePlugin == null)
                return;

            if (sd.SledProjectFile == null)
                return;

            var ibp = sd.Editor.GetBreakpoint(lineNumber);
            if (ibp == null)
                return;

            var bp =
                FindBreakpointInFile(sd.SledProjectFile, ibp);

            if (bp == null)
                return;

            ShowBreakpointConditionFormInternal(bp);
        }

        private void ShowBreakpointConditionForm(DomNode domNode)
        {
            if (domNode == null)
                return;

            if (!domNode.Is<SledProjectFilesBreakpointType>())
                return;

            var bp =
                domNode.As<SledProjectFilesBreakpointType>();

            if (bp == null)
                return;

            ShowBreakpointConditionFormInternal(bp);
        }

        private void ShowBreakpointConditionFormInternal(SledProjectFilesBreakpointType bp)
        {
            using (var form = new SledBreakpointConditionForm())
            {
                // Store values
                var condition = bp.Condition;
                var bConditionResult = bp.ConditionResult;
                var bConditionEnabled = string.IsNullOrEmpty(condition) ? false : bp.ConditionEnabled;
                var bUseFunctionEnvironment = bp.UseFunctionEnvironment;

                // Setup form
                form.Plugin = bp.File.LanguagePlugin;
                form.SyntaxHighlighter = GetHighlighter(bp);
                form.Condition = condition;
                form.ConditionResult = bConditionResult;
                form.ConditionEnabled = bConditionEnabled;
                form.UseFunctionEnvironment = bUseFunctionEnvironment;

                // Show form
                if (form.ShowDialog(m_mainForm) != DialogResult.OK)
                    return;

                // Update if changed
                if (string.Compare(condition, form.Condition) != 0)
                    bp.Condition = form.Condition;

                // Update if changed
                if (bConditionResult != form.ConditionResult)
                    bp.ConditionResult = form.ConditionResult;

                // Update if changed
                if (bConditionEnabled != form.ConditionEnabled)
                    bp.ConditionEnabled = form.ConditionEnabled;

                // Update if changed
                if (bUseFunctionEnvironment != form.UseFunctionEnvironment)
                    bp.UseFunctionEnvironment = form.UseFunctionEnvironment;
            }
        }

        private SledDocumentSyntaxHighlighter GetHighlighter(SledProjectFilesBreakpointType bp)
        {
            if (bp == null)
                return null;

            if (bp.File == null)
                return null;

            if (bp.File.Uri == null)
                return null;

            if (m_documentService == null)
                return null;

            var documentClient = m_documentService.GetDocumentClient(bp.File.Uri);

            return documentClient.SyntaxHighlighter;
        }

        private void OnBreakpointAdded(SledBreakpointServiceBreakpointEventArgs e)
        {
            Added.Raise(this, e);
        }

        private void OnBreakpointSilentAdded(SledBreakpointServiceBreakpointEventArgs e)
        {
            SilentAdded.Raise(this, e);
        }

        private void OnBreakpointChanging(SledBreakpointServiceBreakpointChangingEventArgs e)
        {
            Changing.Raise(this, e);
        }

        private void OnBreakpointChanged(SledBreakpointServiceBreakpointChangingEventArgs e)
        {
            Changed.Raise(this, e);
        }

        private void OnBreakpointRemoving(SledBreakpointServiceBreakpointEventArgs e)
        {
            Removing.Raise(this, e);
        }

        #endregion

        #region Private Classes

        private class TempBpDetails
        {
            public TempBpDetails(SledProjectFilesFileType file, int line, string condition, bool conditionResult, bool conditionEnabled, bool useFunctionEnvironment)
            {
                File = file;
                Line = line;
                Condition = condition;
                ConditionResult = conditionResult;
                ConditionEnabled = conditionEnabled;
                UseFunctionEnvironment = useFunctionEnvironment;
            }

            public SledProjectFilesFileType File { get; private set; }
            public int Line { get; private set; }
            public string Condition { get; private set; }
            public bool ConditionResult { get; private set; }
            public bool ConditionEnabled { get; private set; }
            public bool UseFunctionEnvironment { get; private set; }
        }

        #endregion

        private ISledProjectService m_projectService;
        private ISledDocumentService m_documentService;
        private SledBreakpointEditor m_breakpointEditor;
        private ISledModifiedFilesFormService m_modifiedFilesFormService;
        
        private bool m_bAddingOrRemoving;
        private int m_iContextMenuClickedLine = -1;

        private bool m_bPreserveOpenDocumentBreakpoints;

        private readonly MainForm m_mainForm;

        private readonly List<DomNode> m_windowSelection =
            new List<DomNode>();

        private readonly List<TempBpDetails> m_breakpointsMoving =
            new List<TempBpDetails>();

        private static readonly IEnumerable<object> s_emptyObjectList =
            EmptyEnumerable<object>.Instance;
    }
}
