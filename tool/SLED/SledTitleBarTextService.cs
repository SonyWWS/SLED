/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

using System;
using System.ComponentModel.Composition;

using Sce.Atf;
using Sce.Atf.Applications;

using Sce.Sled.Shared.Services;

namespace Sce.Sled
{
    /// <summary>
    /// SledTitleBarTextService Class
    /// </summary>
    [Export(typeof(IInitializable))]
    [Export(typeof(SledTitleBarTextService))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    class SledTitleBarTextService : MainWindowTitleService, IInitializable
    {
        [ImportingConstructor]
        public SledTitleBarTextService(MainForm mainForm, IDocumentRegistry documentRegistry)
            : base(mainForm, documentRegistry)
        {
            m_mainForm = mainForm;
            m_appName = mainForm.Text;
            m_projectText = null;
            m_userText = null;
        }

        #region IInitializable Interface

        void IInitializable.Initialize()
        {
            m_documentService = SledServiceInstance.Get<ISledDocumentService>();
            m_documentService.ActiveDocumentChanged += DocumentServiceActiveDocumentChanged;

            m_projectService = SledServiceInstance.Get<ISledProjectService>();
            m_projectService.Created += ProjectServiceCreated;
            m_projectService.Opened += ProjectServiceOpened;
            m_projectService.Closed += ProjectServiceClosed;
            m_projectService.NameChanged += ProjectServiceNameChanged;
        }

        #endregion

        #region ISledDocumentService Events

        private void DocumentServiceActiveDocumentChanged(object sender, EventArgs e)
        {
            m_userText =
                !m_documentService.Active
                    ? null
                    : m_documentService.ActiveDocument.Uri.LocalPath;

            UpdateMainWindow(null, null);
        }

        #endregion

        #region ISledProjectService Events

        private void ProjectServiceCreated(object sender, SledProjectServiceProjectEventArgs e)
        {
            m_projectText = e.Project.Name;
            UpdateMainWindow(null, null);
        }

        private void ProjectServiceOpened(object sender, SledProjectServiceProjectEventArgs e)
        {
            m_projectText = e.Project.Name;
            UpdateMainWindow(null, null);
        }

        private void ProjectServiceClosed(object sender, SledProjectServiceProjectEventArgs e)
        {
            m_projectText = null;
            UpdateMainWindow(null, null);
        }

        private void ProjectServiceNameChanged(object sender, SledProjectServiceProjectNameEventArgs e)
        {
            m_projectText = e.NewName;
            UpdateMainWindow(null, null);
        }

        #endregion

        protected override void UpdateMainWindow(IMainWindow form, IDocument activeDocument)
        {
            string titleText;

            var bUserEmpty = string.IsNullOrEmpty(m_userText);
            var bProjEmpty = string.IsNullOrEmpty(m_projectText);

            if (bUserEmpty && bProjEmpty)
            {
                titleText = m_appName;
            }
            else if (!bProjEmpty && bUserEmpty)
            {
                titleText = string.Format("{0} - {1}", m_appName, m_projectText);
            }
            else if (bProjEmpty && !bUserEmpty)
            {
                titleText = string.Format("{0} - {1}", m_appName, m_userText);
            }
            else
            {
                titleText = string.Format("{0} - {1} - {2}", m_appName, m_projectText, m_userText);
            }

            m_mainForm.Text = titleText;
        }

        private ISledDocumentService m_documentService;
        private ISledProjectService m_projectService;

        private string m_projectText;
        private string m_userText;

        private readonly MainForm m_mainForm;
        private readonly string m_appName = string.Empty;
    }
}
