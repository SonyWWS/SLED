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
using Sce.Atf.Applications;

using Sce.Sled.Project;
using Sce.Sled.Shared.Services;

namespace Sce.Sled
{
    [Export(typeof(IInitializable))]
    [Export(typeof(ISledModifiedProjectFormService))]
    [Export(typeof(SledModifiedProjectFormService))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    class SledModifiedProjectFormService : IInitializable, ISledModifiedProjectFormService
    {
        [ImportingConstructor]
        public SledModifiedProjectFormService(MainForm mainForm)
        {
            m_mainForm = mainForm;
            m_mainForm.FormClosing += MainFormFormClosing;
        }

        #region IInitializable Interface

        void IInitializable.Initialize()
        {
            m_projectService =
                SledServiceInstance.Get<ISledProjectService>();

            m_projectWatcherService =
                SledServiceInstance.Get<ISledProjectWatcherService>();

            m_projectWatcherService.AttributeChangedEvent += ProjectWatcherServiceAttributeChangedEvent;
            m_projectWatcherService.FileChangedEvent += ProjectWatcherServiceFileChangedEvent;
        }

        #endregion

        #region ISledModifiedProjectFormService Interface

        /// <summary>
        /// Event fired when another program modifies the currently opened SLED project
        /// and SLED is determining whether or not the changes warrant user interaction
        /// </summary>
        public event EventHandler<SledModifiedProjectChangesDetectedEventArgs> ChangesDetected;

        /// <summary>
        /// Event fired when another program modifies the currently opened SLED project
        /// and the user has selected which changes to take and which changes to ignore
        /// </summary>
        public event EventHandler<SledModifiedProjectChangesEventArgs> GuiChangesSubmitted;

        #endregion

        #region MainForm Events

        private void MainFormFormClosing(object sender, FormClosingEventArgs e)
        {
            if (m_modifiedProjectForm == null)
                return;

            m_modifiedProjectForm.ChangesSubmitted -= ModifiedProjectFormChangesSubmitted;
            m_modifiedProjectForm.Close();
            m_modifiedProjectForm.Dispose();
            m_modifiedProjectForm = null;
        }

        #endregion

        #region ISledProjectWatcherService Events

        private void ProjectWatcherServiceAttributeChangedEvent(object sender, SledProjectWatcherServiceEventArgs e)
        {
            // Eventually implement read-only projects (?)

            // Temp code so ReSharper stops telling me to make the method static
            if (m_projectService == null)
                return;
        }

        private void ProjectWatcherServiceFileChangedEvent(object sender, SledProjectWatcherServiceEventArgs e)
        {
            var curProject = m_projectService.ActiveProject;

            if (curProject == null)
                return;

            if (m_modifiedProjectForm == null)
            {
                m_modifiedProjectForm = new SledProjectModifiedForm();
                m_modifiedProjectForm.ChangesSubmitted += ModifiedProjectFormChangesSubmitted;
            }

            //
            // Figure out what changed between the in-memory
            // project (m_projectService.ActiveProject) and project
            // on disk at e.AbsolutePath
            //

            string name;
            string projectDir;
            string assetDir;
            Guid guid;
            List<string> lstFiles;

            // Read details of changed-on-disk project
            var bGotProjectDetails =
                SledProjectUtilities.TryGetProjectDetails(
                    e.AbsolutePath,
                    out name,
                    out projectDir,
                    out assetDir,
                    out guid,
                    out lstFiles);

            if (!bGotProjectDetails)
                return;

            // Gather up all changes to
            // then report them to the GUI
            var lstChanges =
                new List<SledModifiedProjectChange>();

            // Check if name changed
            if (string.Compare(curProject.Name, name) != 0)
            {
                lstChanges.Add(
                    new SledModifiedProjectNameChange(
                        curProject.Name, name));
            }

            // Check if asset directory changed
            if (string.Compare(curProject.AssetDirectory, assetDir, true) != 0)
            {
                lstChanges.Add(
                    new SledModifiedProjectAssetDirChange(
                        curProject.AssetDirectory,
                        assetDir));
            }

            // Check if guid changed
            if (curProject.Guid != guid)
            {
                lstChanges.Add(
                    new SledModifiedProjectGuidChange(
                        curProject.Guid,
                        guid));
            }

            // Check if files added or removed
            {
                var curFilesAbsPaths =
                    curProject.AllFiles.Select(
                        file => file.AbsolutePath).ToList();

                // Check for removals
                lstChanges.AddRange(
                    (from curAbsFile in curFilesAbsPaths
                     let bExists = lstFiles.Any(newAbsFile => string.Compare(curAbsFile, newAbsFile, true) == 0)
                     where !bExists
                     select new SledModifiedProjectFileRemovedChange(curAbsFile)).Cast<SledModifiedProjectChange>());

                // Check for additions
                lstChanges.AddRange(
                    (from newAbsFile in lstFiles
                     let bExists = curFilesAbsPaths.Any(curAbsFile => string.Compare(newAbsFile, curAbsFile, true) == 0)
                     where !bExists
                     select new SledModifiedProjectFileAddedChange(newAbsFile)).Cast<SledModifiedProjectChange>());
            }

            // Report changes to GUI
            m_modifiedProjectForm.ReportChanges(lstChanges);

            // Are there any changes?
            var formChanges =
                new List<SledModifiedProjectChange>(
                    m_modifiedProjectForm.Changes);

            // Do we need to show the GUI?
            var bShowingGui =
                formChanges.Count > 0;

            // Fire event
            ChangesDetected.Raise(this, new SledModifiedProjectChangesDetectedEventArgs(bShowingGui));
            
            if (bShowingGui)
            {
                // Show the form finally
                m_modifiedProjectForm.Show(m_mainForm);
            }
            else
            {
                // Do we need to hide the GUI?

                // If the form is already visible but there are
                // no changes then we want to hide the form as
                // all changes have been dealt with or reverted
                if (m_modifiedProjectForm.Visible)
                    m_modifiedProjectForm.Hide();
            }
        }

        #endregion

        #region SledModifiedProjectForm Events

        private void ModifiedProjectFormChangesSubmitted(object sender, SledModifiedProjectChangesEventArgs e)
        {
            // Fire event
            GuiChangesSubmitted.Raise(this, e);
        }

        #endregion
        
        private ISledProjectService m_projectService;
        private ISledProjectWatcherService m_projectWatcherService;

        private SledProjectModifiedForm m_modifiedProjectForm;

        private readonly MainForm m_mainForm;
    }
}
