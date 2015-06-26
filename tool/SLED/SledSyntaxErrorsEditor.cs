/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Windows.Forms;

using Sce.Atf.Adaptation;
using Sce.Atf.Applications;
using Sce.Atf.Controls;
using Sce.Atf.Dom;

using Sce.Sled.Resources;
using Sce.Sled.Shared.Controls;
using Sce.Sled.Shared.Dom;
using Sce.Sled.Shared.Plugin;
using Sce.Sled.Shared.Services;

namespace Sce.Sled
{
    [Export(typeof(SledSyntaxErrorsEditor))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    class SledSyntaxErrorsEditor : SledTreeListViewEditor
    {
        [ImportingConstructor]
        public SledSyntaxErrorsEditor()
            : base(
                Localization.SledSyntaxErrorsTitle,
                null,
                SledSyntaxErrorType.TheColumnNames,
                TreeListView.Style.List,
                StandardControlGroup.Right)
        {
        }

        #region IInitializable Interface

        public override void Initialize()
        {
            base.Initialize();

            m_projectService = SledServiceInstance.Get<ISledProjectService>();
            m_projectService.Created += ProjectServiceCreated;
            m_projectService.Opened += ProjectServiceOpened;
            m_projectService.Closed += ProjectServiceClosed;

            m_syntaxCheckerService = SledServiceInstance.Get<ISledSyntaxCheckerService>();
            m_syntaxCheckerService.FilesCheckFinished += SyntaxCheckerServiceFilesCheckFinished;

            MouseDoubleClick += ControlMouseDoubleClick;
        }

        #endregion

        #region ISledSyntaxCheckerService interface

        private void SyntaxCheckerServiceFilesCheckFinished(object sender, SledSyntaxCheckFilesEventArgs e)
        {
            try
            {
                View = null;

                if (m_syntaxErrorsCollection == null)
                    return;

                var fixedErrors = new List<SledSyntaxErrorType>();

                var newErrors = new List<SledSyntaxCheckerEntry>();
                var existingErrors = new List<SledSyntaxCheckerEntry>();

                foreach (var domError in m_syntaxErrorsCollection.Errors)
                {
                    List<SledSyntaxCheckerEntry> items;

                    // check if the file was syntax checked this time
                    if (!e.FilesAndErrors.TryGetValue(domError.File, out items))
                        continue;

                    // check if the file previously had errors but now doesn't
                    if (items.Count == 0)
                    {
                        fixedErrors.Add(domError);
                        continue;
                    }

                    // check if this error is already present on the GUI
                    var scee = FindCorrespondingSyntaxCheckerEntryError(domError, items);
                    if (scee == null)
                        fixedErrors.Add(domError);
                    else
                        existingErrors.Add(scee);
                }

                // remove errors that have been fixed
                foreach (var error in fixedErrors)
                    m_syntaxErrorsCollection.Errors.Remove(error);

                // add remaining new errors
                foreach (var kv in e.FilesAndErrors)
                {
                    foreach (var syntaxCheckerEntry in kv.Value)
                    {
                        if (existingErrors.Contains(syntaxCheckerEntry))
                            continue;

                        newErrors.Add(syntaxCheckerEntry);
                    }
                }

                // Convert items
                foreach (var syntaxCheckerEntry in newErrors)
                {
                    var domError =
                        new DomNode(SledSchema.SledSyntaxErrorType.Type)
                        .As<SledSyntaxErrorType>();

                    domError.File = syntaxCheckerEntry.File;
                    domError.Language = syntaxCheckerEntry.Language;
                    domError.Line = syntaxCheckerEntry.Line;
                    domError.Error = syntaxCheckerEntry.Error;

                    m_syntaxErrorsCollection.Errors.Add(domError);
                }
            }
            finally
            {
                View = m_syntaxErrorsCollection;
            }
        }

        #endregion

        #region ISledProjectService Events

        private void ProjectServiceCreated(object sender, SledProjectServiceProjectEventArgs e)
        {
            CreateSyntaxErrorsCollection();
        }

        private void ProjectServiceOpened(object sender, SledProjectServiceProjectEventArgs e)
        {
            CreateSyntaxErrorsCollection();
        }

        private void ProjectServiceClosed(object sender, SledProjectServiceProjectEventArgs e)
        {
            DestroySyntaxErrorsCollection();
        }

        #endregion

        #region Member Methods

        private void CreateSyntaxErrorsCollection()
        {
            var root =
                new DomNode(
                    SledSchema.SledSyntaxErrorListType.Type,
                    SledSchema.SledSyntaxErrorsRootElement);

            m_syntaxErrorsCollection =
                root.As<SledSyntaxErrorListType>();

            m_syntaxErrorsCollection.Name =
                string.Format(
                    "{0}{1}{2}",
                    m_projectService.ProjectName,
                    Resource.Space,
                    Resource.SyntaxErrorsTitle);
        }

        private void DestroySyntaxErrorsCollection()
        {
            // Clear GUI
            View = null;

            if (m_syntaxErrorsCollection == null)
                return;

            m_syntaxErrorsCollection.Errors.Clear();
            m_syntaxErrorsCollection = null;
        }

        private void ControlMouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left)
                return;

            var lastHit = LastHit;
            if (lastHit == null)
                return;

            var error = lastHit.As<SledSyntaxErrorType>();
            if (error == null)
                return;

            if (error.File == null)
                return;

            if (!File.Exists(error.File.AbsolutePath))
                return;

            m_gotoService.Get.GotoLine(error.File.AbsolutePath, error.Line, false);
        }

        private static SledSyntaxCheckerEntry FindCorrespondingSyntaxCheckerEntryError(SledSyntaxErrorType domError, IEnumerable<SledSyntaxCheckerEntry> syntaxErrors)
        {
            foreach (var syntaxError in syntaxErrors)
            {
                if (domError.Language != syntaxError.Language)
                    continue;

                if (domError.File != syntaxError.File)
                    continue;

                if (domError.Line != syntaxError.Line)
                    continue;

                if (string.Compare(domError.Error, syntaxError.Error) != 0)
                    continue;

                return syntaxError;
            }

            return null;
        }

        #endregion

        private SledSyntaxErrorListType m_syntaxErrorsCollection;

        private ISledProjectService m_projectService;
        private ISledSyntaxCheckerService m_syntaxCheckerService;

        private readonly SledServiceReference<ISledGotoService> m_gotoService =
            new SledServiceReference<ISledGotoService>();
    }
}
