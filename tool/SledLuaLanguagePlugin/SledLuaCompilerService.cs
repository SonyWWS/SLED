/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Xml;

using Sce.Atf;
using Sce.Atf.Adaptation;
using Sce.Atf.Applications;
using Sce.Atf.Controls.PropertyEditing;
using Sce.Atf.Dom;

using Sce.Lua.Utilities;

using Sce.Sled.Lua.Dom;
using Sce.Sled.Lua.Resources;
using Sce.Sled.Shared;
using Sce.Sled.Shared.Dom;
using Sce.Sled.Shared.Services;
using Sce.Sled.Shared.Utilities;

namespace Sce.Sled.Lua
{
    [Export(typeof(IInitializable))]
    [Export(typeof(SledLuaCompilerService))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    sealed class SledLuaCompilerService : IInitializable, ICommandClient
    {
        [ImportingConstructor]
        public SledLuaCompilerService(
            MainForm mainForm,
            ICommandService commandService,
            ISettingsService settingsService)
        {
            m_mainForm = mainForm;

            commandService.RegisterCommand(
                Command.Compile,
                SledLuaMenuShared.MenuTag,
                SledLuaMenuShared.CommandGroupTag,
                Localization.SledLuaCompilerCompile,
                Localization.SledLuaCompilerCompileComment,
                Keys.None,
                SledLuaIcon.Compile,
                CommandVisibility.All,
                this);

            commandService.RegisterCommand(
                Command.Settings,
                SledLuaMenuShared.MenuTag,
                SledLuaMenuShared.CommandGroupTag,
                Localization.SledLuaCompilerSettings,
                Localization.SledLuaCompilerSettingsComment,
                Keys.None,
                null,
                CommandVisibility.Menu,
                this);


            // Save Lua compiler settings
            settingsService.RegisterSettings(
                this,
                new BoundPropertyDescriptor(
                    this,
                    () => LuaCompilerSettings,
                    Resources.Resource.LuaCompilerSettingsTitle,
                    Resources.Resource.LuaCompilerSettings,
                    Resources.Resource.LuaCompilerSettingsTitle));

            // Add some user settings to edit > preferences
            settingsService.RegisterUserSettings(
                SledLuaSettings.Category,
                new BoundPropertyDescriptor(
                    this,
                    () => Verbose,
                    Resources.Resource.Verbose,
                    Resources.Resource.LuaCompilerSettingsTitle,
                    Resources.Resource.Verbose));
        }

        #region IInitializable Interface

        void IInitializable.Initialize()
        {
            m_debugService =
                SledServiceInstance.Get<ISledDebugService>();

            m_luaLanguagePlugin =
                SledServiceInstance.Get<SledLuaLanguagePlugin>();

            m_projectService =
                SledServiceInstance.Get<ISledProjectService>();

            m_projectService.Created += ProjectServiceCreated;
            m_projectService.Opened += ProjectServiceOpened;
            m_projectService.FileAdded += ProjectServiceFileAdded;
            m_projectService.Closing += ProjectServiceClosing;
        }

        #endregion

        #region Commands

        enum Command
        {
            Compile,
            Settings,
        }

        #endregion

        #region ICommandClient Interface

        public bool CanDoCommand(object commandTag)
        {
            var bEnabled = false;

            if (commandTag is Command)
            {
                switch ((Command)commandTag)
                {
                    case Command.Compile:
                        bEnabled = m_projectService.Active && m_debugService.IsDisconnected;
                        break;

                    case Command.Settings:
                        bEnabled = m_projectService.Active;
                        break;
                }
            }

            return bEnabled;
        }

        public void DoCommand(object commandTag)
        {
            if (!(commandTag is Command))
                return;

            switch ((Command)commandTag)
            {
                case Command.Compile:
                    Compile();
                    break;

                case Command.Settings:
                    ShowSettings();
                    break;
            }
        }

        public void UpdateCommand(object commandTag, CommandState state)
        {
        }

        #endregion

        #region Persisted Settings

        public string LuaCompilerSettings
        {
            get
            {
                // Generate Xml string to contain the verbose settings
                var xmlDoc = new XmlDocument();
                xmlDoc.AppendChild(xmlDoc.CreateXmlDeclaration(Resources.Resource.OnePointZero, Resources.Resource.UtfDashEight, Resources.Resource.YesLower));
                var root = xmlDoc.CreateElement(Resources.Resource.LuaCompilerSettings);
                xmlDoc.AppendChild(root);

                try
                {
                    var elem = xmlDoc.CreateElement(Resources.Resource.Settings);
                    elem.SetAttribute(Resources.Resource.Verbose, m_bVerbose ? Resources.Resource.One : Resources.Resource.Zero);
                    root.AppendChild(elem);

                    if (xmlDoc.DocumentElement == null)
                        xmlDoc.RemoveAll();
                    else if (xmlDoc.DocumentElement.ChildNodes.Count == 0)
                        xmlDoc.RemoveAll();
                }
                catch (Exception ex)
                {
                    xmlDoc.RemoveAll();

                    var szSetting = Resources.Resource.LuaCompilerSettings;
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

                    var nodes = xmlDoc.DocumentElement.SelectNodes(Resources.Resource.Settings);
                    if ((nodes == null) || (nodes.Count == 0))
                        return;

                    foreach (XmlElement elem in nodes)
                    {
                        m_bVerbose = (int.Parse(elem.GetAttribute(Resources.Resource.Verbose)) != 0);
                        break;
                    }
                }
                catch (Exception ex)
                {
                    var szSetting = Resources.Resource.LuaCompilerSettings;
                    SledOutDevice.OutLine(
                        SledMessageType.Info,
                        SledUtil.TransSub(Localization.SledSettingsErrorExceptionLoadingSetting, szSetting, ex.Message));
                }
            }
        }

        public bool Verbose
        {
            get { return m_bVerbose; }
            set { m_bVerbose = value; }
        }

        #endregion

        #region ISledProjectService Events

        private void ProjectServiceCreated(object sender, SledProjectServiceProjectEventArgs e)
        {
            SetupCompileSettings(e.Project);
            SetupCompileAttribute(e.Project);
        }

        private void ProjectServiceOpened(object sender, SledProjectServiceProjectEventArgs e)
        {
            SetupCompileSettings(e.Project);
            SetupCompileAttribute(e.Project);
        }

        private void ProjectServiceFileAdded(object sender, SledProjectServiceFileEventArgs e)
        {
            SetupCompileAttribute(e.File);
        }

        private void ProjectServiceClosing(object sender, SledProjectServiceProjectEventArgs e)
        {
            m_luaCompileSettings = null;
        }

        #endregion

        #region Member Methods

        private void SetupCompileAttribute(SledProjectFilesFileType file)
        {
            if (file == null)
                return;

            // Look for a SledLuaCompileAttributeType attribute in file.Attributes
            var iface = SledDomUtil.GetFirstAs<SledLuaCompileAttributeType, SledAttributeBaseType>(file.Attributes);

            // Files' attribute is already set up
            if (iface != null)
                return;

            var domNode = new DomNode(SledLuaSchema.SledLuaCompileAttributeType.Type);

            // Create new attribute
            var attr = domNode.As<SledLuaCompileAttributeType>();

            // Default values
            attr.Name = m_luaLanguagePlugin.LanguageName;
            attr.Compile = true;

            // Add new attribute to file
            file.Attributes.Add(attr);
        }

        private void SetupCompileAttribute(SledProjectFilesType project)
        {
            if (project == null)
                return;

            foreach (var file in project.AllFiles)
            {
                if (file.LanguagePlugin != m_luaLanguagePlugin)
                    continue;

                SetupCompileAttribute(file);
            }
        }

        private void Compile()
        {
            // No project so nothing to compile
            if (!m_projectService.Active)
            {
                SledOutDevice.OutLine(
                    SledMessageType.Info,
                    Localization.SledLuaCompilerErrorNoActiveProject);

                return;
            }

            // Find a Lua compiler configuration
            var configType = FindSelectedCompileConfiguration(m_luaCompileSettings.Configurations);
            if (configType == null)
            {
                // Show message indicating the user needs to select a compiler configuration
                MessageBox.Show(
                    m_mainForm,
                    Localization.SledLuaCompilerNotifySelectConfiguration,
                    Localization.SledLuaCompiler,
                    MessageBoxButtons.OK);

                // Show select/add-new configuration dialog
                using (var form = new SledLuaCompilerConfigurationsForm())
                {
                    form.AddConfigurations(m_luaCompileSettings.Configurations);
                    form.ShowDialog();

                    // Save any configuration additions/modifications/deletions
                    m_projectService.SaveSettings();
                }

                // Find the newly selected item (if any)
                configType = FindSelectedCompileConfiguration(m_luaCompileSettings.Configurations);

                if (configType == null)
                {
                    // Show message box saying can't compile due to no compile configuration being set
                    MessageBox.Show(
                        m_mainForm,
                        Localization.SledLuaCompilerNotifySelectConfigurationError,
                        Localization.SledLuaCompiler,
                        MessageBoxButtons.OK);

                    return;
                }
            }

            // Compile w/ selected configuration
            Compile(configType);
        }

        private void Compile(SledLuaCompileConfigurationType configType)
        {
            if (configType == null)
                return;

            // Gather files owned by the Lua plugin
            IList<SledProjectFilesFileType> lstFiles =
                new List<SledProjectFilesFileType>(
                    m_projectFileGathererService.Get.GetFilesOwnedByPlugin(
                        m_luaLanguagePlugin));

            // No files so nothing to compile
            if (lstFiles.Count <= 0)
            {
                SledOutDevice.OutLine(
                    SledMessageType.Info,
                    Localization.SledLuaCompilerErrorNoPluginOwnedFiles);

                return;
            }

            IList<SledProjectFilesFileType> lstNoCompileFiles =
                new List<SledProjectFilesFileType>();

            // Gather any files aren't supposed to be compiled
            foreach (var file in lstFiles)
            {
                var bCompileFile = false;

                foreach (SledLuaCompileAttributeType attr in file.Attributes)
                {
                    if (!attr.Is<SledLuaCompileAttributeType>())
                        continue;
                    
                    var luaAttr = 
                        attr.As<SledLuaCompileAttributeType>();

                    bCompileFile = luaAttr.Compile;
                }

                // Don't compile the file
                if (!bCompileFile)
                    lstNoCompileFiles.Add(file);
            }

            // Remove any files that aren't supposed to be compiled
            foreach (var file in lstNoCompileFiles)
            {
                lstFiles.Remove(file);
            }

            // No files so nothing to compile
            if (lstFiles.Count <= 0)
            {
                SledOutDevice.OutLine(
                    SledMessageType.Info,
                    Localization.SledLuaCompilerErrorNoFiles);

                return;
            }

            ILuaCompiler compiler = null;

            try
            {
                // Create compiler based on platform
                compiler = SledLuaCompilerServiceFactory.Create();

                // Set up configuration
                var config =
                    new LuaCompilerConfig(
                        configType.LittleEndian
                            ? LuaCompilerConfig.Endian.Little
                            : LuaCompilerConfig.Endian.Big,
                        configType.SizeOfInt,
                        configType.SizeOfSizeT,
                        configType.SizeOfLuaNumber,
                        configType.StripDebugInfo);

                // Compile each file individually
                foreach (var file in lstFiles)
                {
                    try
                    {
                        //
                        // Fix up path to respect users' configType settings
                        //

                        // Fix up extension
                        var dumpPath = Path.ChangeExtension(file.AbsolutePath, configType.OutputExtension);

                        // Grab new file name w/ updated extension
                        var newName = Path.GetFileName(dumpPath);
                        if (string.IsNullOrEmpty(newName))
                            throw new InvalidOperationException("new filename null or empty");

                        // Fix up output directory
                        if (configType.PreserveRelativePathInfo)
                        {
                            // Get relative path hierarchy
                            var dirHierarchy = Path.GetDirectoryName(file.Path);
                            dumpPath = string.Format("{0}{1}{2}{1}{3}", configType.OutputPath, Path.DirectorySeparatorChar, dirHierarchy, newName);
                        }
                        else
                        {
                            // Just take output path + new name (which includes new extension)
                            dumpPath = string.Format("{0}{1}{2}", configType.OutputPath, Path.DirectorySeparatorChar, newName);
                        }

                        dumpPath = Path.GetFullPath(dumpPath);

                        if (m_bVerbose)
                        {
                            SledOutDevice.OutLine(
                                SledMessageType.Info,
                                "[Lua compiler] Compiling {0} to {1}",
                                file.AbsolutePath, dumpPath);
                        }

                        // Make sure directory exists before trying to place compiled script there
                        var newDir = Path.GetDirectoryName(dumpPath);
                        if (string.IsNullOrEmpty(newDir))
                            throw new InvalidOperationException("new directory null or empty");

                        if (!Directory.Exists(newDir))
                        {
                            var bDirExists = false;
                            var message = string.Empty;

                            try
                            {
                                Directory.CreateDirectory(newDir);
                                bDirExists = true;
                            }
                            catch (UnauthorizedAccessException ex)  { message = ex.Message; }
                            catch (ArgumentNullException ex)        { message = ex.Message; }
                            catch (ArgumentException ex)            { message = ex.Message; }
                            catch (PathTooLongException ex)         { message = ex.Message; }
                            catch (DirectoryNotFoundException ex)   { message = ex.Message; }
                            catch (NotSupportedException ex)        { message = ex.Message; }
                            catch (IOException ex)                  { message = ex.Message; }

                            // Show message if directory couldn't be created
                            if (!bDirExists)
                            {
                                // Can't compile script to user supplied destination directory
                                // because the directory doesn't exist and can't be created!
                                SledOutDevice.OutLine(
                                    SledMessageType.Error,
                                    SledUtil.TransSub(Localization.SledLuaCompilerErrorCantCreateDir, message));

                                // Skip this file...
                                continue;
                            }
                        }

                        // Try and compile file
                        var succeeded = compiler.Compile(new Uri(file.AbsolutePath), new Uri(dumpPath), config);
                        SledOutDevice.OutLine(
                            SledMessageType.Info,
                            succeeded
                                ? SledUtil.TransSub(Localization.SledLuaCompilerErrorSuccess, file.Name)
                                : SledUtil.TransSub(Localization.SledLuaCompilerErrorFailed, file.Name, compiler.Error));
                    }
                    catch (Exception ex2)
                    {
                        SledOutDevice.OutLine(
                            SledMessageType.Error,
                            SledUtil.TransSub(Localization.SledLuaCompilerErrorExceptionCompilingFile, file.Name, ex2.Message));
                    }
                }
            }
            catch (Exception ex1)
            {
                SledOutDevice.OutLine(
                    SledMessageType.Error,
                    SledUtil.TransSub(Localization.SledLuaCompilerErrorExceptionInLuaCompiler, ex1.Message));
            }
            finally
            {
                if (compiler != null)
                    compiler.Dispose();
            }
        }

        private void ShowSettings()
        {
            using (var form = new SledLuaCompilerConfigurationsForm())
            {
                if (m_luaCompileSettings != null)
                    form.AddConfigurations(m_luaCompileSettings.Configurations);

                form.ShowDialog(m_mainForm);

                // Save any configuration additions/modifications/deletions
                m_projectService.SaveSettings();
            }
        }

        private void SetupCompileSettings(SledProjectFilesType project)
        {
            // Find compile settings
            m_luaCompileSettings = FindLuaCompileSettingsElement(project);
            if (m_luaCompileSettings != null)
                return;

            var domNode = new DomNode(SledLuaSchema.SledLuaCompileSettingsType.Type);

            // Create compile settings if they don't exist
            m_luaCompileSettings = domNode.As<SledLuaCompileSettingsType>();
            m_luaCompileSettings.Name = "Lua Compile Settings";

            // Add compile settings to project
            project.UserSettings.Add(m_luaCompileSettings);

            // Write compile settings to disk
            m_projectService.SaveSettings();
        }

        private static SledLuaCompileSettingsType FindLuaCompileSettingsElement(SledProjectFilesType project)
        {
            var settingsElement =
                project.UserSettings.FirstOrDefault(
                    setting => setting.Is<SledLuaCompileSettingsType>());

            return
                settingsElement == null
                    ? null
                    : settingsElement.As<SledLuaCompileSettingsType>();
        }

        private static SledLuaCompileConfigurationType FindSelectedCompileConfiguration(IEnumerable<SledLuaCompileConfigurationType> configurations)
        {
            return
                configurations == null
                    ? null
                    : configurations.FirstOrDefault(
                        configType => configType.Selected);
        }

        #endregion

        private ISledDebugService m_debugService;
        private ISledProjectService m_projectService;
        private SledLuaLanguagePlugin m_luaLanguagePlugin;
        private SledLuaCompileSettingsType m_luaCompileSettings;

        private bool m_bVerbose = true;

        private readonly MainForm m_mainForm;

        private readonly SledServiceReference<ISledProjectFileGathererService> m_projectFileGathererService =
            new SledServiceReference<ISledProjectFileGathererService>();

        #region Private Classes

        private static class SledLuaCompilerServiceFactory
        {
            public static ILuaCompiler Create()
            {
                try
                {
                    if (s_luaVersionService == null)
                        s_luaVersionService = SledServiceInstance.TryGet<ISledLuaLuaVersionService>();

                    if (s_luaVersionService == null)
                        return new Sce.Lua.Utilities.Lua51.x86.LuaCompiler();

                    switch (s_luaVersionService.CurrentLuaVersion)
                    {
                        case LuaVersion.Lua51: return new Sce.Lua.Utilities.Lua51.x86.LuaCompiler();
                        case LuaVersion.Lua52: return new Sce.Lua.Utilities.Lua52.x86.LuaCompiler();
                        default: throw new NullReferenceException("Unknown Lua version!");
                    }
                }
                catch (Exception ex)
                {
                    SledOutDevice.OutLine(
                        SledMessageType.Error,
                        "{0}: Exception creating Lua compiler: {1}",
                        typeof(SledLuaCompilerServiceFactory), ex.Message);

                    return null;
                }
            }

            private static ISledLuaLuaVersionService s_luaVersionService;
        }

        #endregion
    }
}
