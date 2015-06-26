/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

using System;
using System.ComponentModel.Composition;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

using Sce.Atf;
using Sce.Atf.Adaptation;
using Sce.Atf.Applications;
using Sce.Atf.Controls;

using Sce.Sled.Resources;
using Sce.Sled.Shared;
using Sce.Sled.Shared.Services;
using Sce.Sled.Shared.Utilities;

namespace Sce.Sled
{
    /// <summary>
    /// SledAboutService Class
    /// </summary>
    [Export(typeof(IInitializable))]
    [Export(typeof(HelpAboutCommand))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    class SledAboutService : HelpAboutCommand, IInitializable
    {
        [ImportingConstructor]
        public SledAboutService(MainForm mainForm)
        {
            m_mainForm = mainForm;
        }

        #region IInitializable Interface

        void IInitializable.Initialize()
        {
            // Hijack the image property of the
            // built in Help/About command info
            var info = CommandInfo.HelpAbout;
            CommandService.RegisterCommand(
                info.CommandTag,
                info.MenuTag,
                info.GroupTag,
                info.MenuText,
                info.Description,
                Keys.None,
                SledIcon.Sled,
                info.Visibility,
                this);
        }

        #endregion

        protected override void ShowHelpAbout()
        {
            try
            {
                var assem = Assembly.GetAssembly(typeof(SledAboutService));
                var version = assem.GetName().Version.ToString();

                var title =
                    SledUtil.TransSub(Resources.Resource.HelpAboutTitleWithVersion, version);

                using (Image image = m_mainForm.Icon.ToBitmap())
                {
                    {
                        //
                        // WWS version
                        //

                        using (var richTextBox = new RichTextBox())
                        {
                            richTextBox.BorderStyle = BorderStyle.None;
                            richTextBox.ReadOnly = true;

                            using (var strm = assem.GetManifestResourceStream(Resources.Resource.HelpAboutAssemblyPath))
                            {
                                if (strm != null)
                                    richTextBox.LoadFile(strm, RichTextBoxStreamType.RichText);

                                using (var dialog =
                                    new AboutDialog(
                                        title,
                                        ApplicationUrl,
                                        richTextBox,
                                        image,
                                        null,
                                        true))
                                {
                                    dialog.ShowDialog(m_mainForm);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                SledOutDevice.OutLine(
                    SledMessageType.Error,
                    SledUtil.TransSub(Localization.SledHelpAboutErrorException, ex.Message));
            }
        }

        private readonly MainForm m_mainForm;

        public const string ApplicationUrl = "http://sf.ship.scea.com/sf/go/proj1060";
    }

    [Export(typeof(IInitializable))]
    [Export(typeof(SledAboutDocumentService))]
    class SledAboutDocumentService : IInitializable, ICommandClient
    {
        [ImportingConstructor]
        public SledAboutDocumentService()
        {
        }

        #region IInitializable Interface

        void IInitializable.Initialize()
        {
            if (m_commandService == null)
                return;

            if (m_directoryInfoService == null)
                return;

            string[] files;

            try
            {
                var docPath =
                    Path.Combine(
                        m_directoryInfoService.ExeDirectory + Path.DirectorySeparatorChar,
                        DocDirectoryName);

                files = Directory.GetFiles(docPath, "*.*", SearchOption.TopDirectoryOnly);
            }
            catch (Exception ex)
            {
                SledOutDevice.OutLine(
                    SledMessageType.Error,
                    "SledAboutDocumentService: Exception " + 
                    "enumerating doc directory: {0}",
                    ex.Message);

                files = null;
            }

            if (files == null)
                return;

            if (files.Length <= 0)
                return;

            foreach (var file in files)
            {
                var name = Path.GetFileName(file);
                if (string.IsNullOrEmpty(name))
                    continue;

                var docTag = new HelpDocumentTag(file);
                m_commandService.RegisterCommand(
                    docTag,
                    StandardMenu.Help,
                    Group.SledAboutDocument,
                    name,
                    string.Format("Open the {0} documet", name),
                    Keys.None,
                    null,
                    CommandVisibility.Menu,
                    this);
            }
        }

        #endregion

        #region ICommandClient Interface

        public bool CanDoCommand(object commandTag)
        {
            return commandTag.Is<HelpDocumentTag>();
        }

        public void DoCommand(object commandTag)
        {
            var docTag = commandTag.As<HelpDocumentTag>();
            if (docTag == null)
                return;

            try
            {
                SledUtil.ShellOpen(docTag.File);
            }
            catch (Exception ex)
            {
                SledOutDevice.OutLine(
                       SledMessageType.Error,
                        "SledAboutDocumentService: Exception " + 
                        "opening doc file \"{0}\": {1}",
                        docTag.File, ex.Message);
            }
        }

        public void UpdateCommand(object commandTag, CommandState state)
        {
        }

        #endregion

        #region Commands

        private enum Group
        {
            SledAboutDocument,
        }

        #endregion

        #region Private Classes

        private class HelpDocumentTag
        {
            public HelpDocumentTag(string absFilePath)
            {
                File = absFilePath;
            }

            public string File { get; private set; }
        }

        #endregion

#pragma warning disable 649 // Field is never assigned to and will always have its default value null

        [Import]
        private ICommandService m_commandService;

        [Import]
        private ISledDirectoryInfoService m_directoryInfoService;

#pragma warning restore 649

        private const string DocDirectoryName = "Doc";
    }
}
