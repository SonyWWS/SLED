/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

using System;
using System.IO;
using System.Windows.Forms;

using Sce.Sled.Project.Resources;

namespace Sce.Sled.Project
{
    public partial class SledProjectNewForm : Form
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public SledProjectNewForm()
        {
            InitializeComponent();
            m_iHeight = Height;
        }
 
        /// <summary>
        /// Get the project name
        /// </summary>
        public string ProjectName
        {
            get { return m_txtProjectName.Text.Trim(); }
        }

        /// <summary>
        /// Get the project directory
        /// </summary>
        public string ProjectDirectory
        {
            get { return FixupDirectoryPath(m_txtProjectDir.Text.Trim()); }
        }

        /// <summary>
        /// Get the project asset directory
        /// </summary>
        public string ProjectAssetDirectory
        {
            get { return FixupDirectoryPath(m_txtAssetDir.Text.Trim()); }
        }

        /// <summary>
        /// Get the full path to the .spf file
        /// </summary>
        public string ProjectFullSpfPath
        {
            get { return m_txtProjectOutput.Text; }
        }

        /// <summary>
        /// Gets whether or not to pop up the auto-file-add box
        /// </summary>
        public bool RecursiveAdd
        {
            get { return m_chkRecursiveAdd.Checked; }
        }

        private static string FixupDirectoryPath(string path)
        {
            if (string.IsNullOrEmpty(path))
                return string.Empty;

            return
                path.EndsWith(Path.DirectorySeparatorChar.ToString(), StringComparison.Ordinal)
                    ? path
                    : path + Path.DirectorySeparatorChar;
        }

        /// <summary>
        /// When user hits the [project] copy button
        /// </summary>
        /// <param name="sender">object that fired the event</param>
        /// <param name="e">event arguments</param>
        private void BtnProjCopy_Click(object sender, EventArgs e)
        {
            var text = m_txtProjectDir.Text;

            if (string.IsNullOrEmpty(text))
                return;

            try
            {
                Clipboard.Clear();
                Clipboard.SetText(text);
            }
            catch (System.Runtime.InteropServices.ExternalException ex)     { ShowCopyErrorMessageBox(ex); }
            catch (System.Threading.ThreadStateException ex)                { ShowCopyErrorMessageBox(ex); }
            catch (System.ComponentModel.InvalidEnumArgumentException ex)   { ShowCopyErrorMessageBox(ex); }
            catch (ArgumentNullException ex)                                { ShowCopyErrorMessageBox(ex); }
        }

        /// <summary>
        /// When user hits the [project] paste button
        /// </summary>
        /// <param name="sender">object that fired the event</param>
        /// <param name="e">event arguments</param>
        private void BtnProjPaste_Click(object sender, EventArgs e)
        {
            try
            {
                if (Clipboard.ContainsText())
                {
                    var text = Clipboard.GetText();
                    var path = Path.GetFullPath(text + Path.DirectorySeparatorChar);

                    if (Directory.Exists(path))
                    {
                        // Update project dir path
                        m_txtProjectDir.Text = path;

                        // Mirror asset dir if option is checked
                        if (m_chkAssetDir.Checked)
                            m_txtAssetDir.Text = path;

                        // Update full path at bottom of GUI
                        m_txtProjectOutput.Text = ProjectDirectory + ProjectName + Resource.ProjectFileExtensionSpf;
                    }
                }
            }
            catch (System.Security.SecurityException ex)                    { ShowPasteErrorMessageBox(ex); }
            catch (System.Runtime.InteropServices.ExternalException ex)     { ShowPasteErrorMessageBox(ex); }
            catch (System.Threading.ThreadStateException ex)                { ShowPasteErrorMessageBox(ex); }
            catch (System.ComponentModel.InvalidEnumArgumentException ex)   { ShowPasteErrorMessageBox(ex); }
            catch (PathTooLongException ex)                                 { ShowPasteErrorMessageBox(ex); }
            catch (NotSupportedException ex)                                { ShowPasteErrorMessageBox(ex); }
            catch (ArgumentNullException ex)                                { ShowPasteErrorMessageBox(ex); }
            catch (ArgumentException ex)                                    { ShowPasteErrorMessageBox(ex); }
        }

        /// <summary>
        /// When user hits select directory button
        /// </summary>
        /// <param name="sender">object that fired the event</param>
        /// <param name="e">event arguments</param>
        private void BtnSelectDir_Click(object sender, EventArgs e)
        {
            // Create a folder browser dialog and set it up
            using (var hDialog = new FolderBrowserDialog())
            {
                hDialog.Description = Localization.SledProjectNewFormProjectDirSelectDescription;
                hDialog.ShowNewFolderButton = true;

                var szProjectDir = ProjectDirectory;

                if (!string.IsNullOrEmpty(szProjectDir) && Directory.Exists(szProjectDir))
                    hDialog.SelectedPath = szProjectDir;
                else
                    hDialog.RootFolder = Environment.SpecialFolder.Desktop;

                // Show the dialog
                if (hDialog.ShowDialog(this) != DialogResult.OK)
                    return;

                // Make sure path ends with directory separator
                var szSelectedPath = 
                    FixupDirectoryPath(hDialog.SelectedPath);

                // Update new project dialog
                m_txtProjectDir.Text = szSelectedPath;

                // Modify asset directory if asset dir is project dir
                if (m_chkAssetDir.Checked)
                    m_txtAssetDir.Text = szSelectedPath;

                UpdateFinalPath();
            }
        }

        /// <summary>
        /// Update project directory when project name changes
        /// </summary>
        /// <param name="sender">object that fired the event</param>
        /// <param name="e">event arguments</param>
        private void TxtProjectName_TextChanged(object sender, EventArgs e)
        {
            UpdateFinalPath();
        }

        private void TxtProjectDir_TextChanged(object sender, EventArgs e)
        {
            UpdateFinalPath();
        }

        private void TxtAssetDir_TextChanged(object sender, EventArgs e)
        {
            UpdateFinalPath();
        }

        /// <summary>
        /// Event for checking/unchecking
        /// </summary>
        /// <param name="sender">object that fired the event</param>
        /// <param name="e">event arguments</param>
        private void ChkAssetDir_CheckedChanged(object sender, EventArgs e)
        {
            if (m_chkAssetDir.CheckState == CheckState.Checked)
            {
                m_txtAssetDir.Enabled = false;
                m_btnSelectAssetDir.Enabled = false;
                m_txtAssetDir.Text = ProjectDirectory;
                m_btnAssetCopy.Enabled = false;
                m_btnAssetPaste.Enabled = false;
            }
            else
            {
                m_txtAssetDir.Enabled = true;
                m_btnSelectAssetDir.Enabled = true;
                m_txtAssetDir.Text = String.Empty;
                m_btnAssetCopy.Enabled = true;
                m_btnAssetPaste.Enabled = true;
            }
        }

        /// <summary>
        /// Event for clicking the select asset directory button
        /// </summary>
        /// <param name="sender">object that fired the event</param>
        /// <param name="e">event arguments</param>
        private void BtnSelectAssetDir_Click(object sender, EventArgs e)
        {
            // Create a folder browser dialog and set it up
            using (var hDialog = new FolderBrowserDialog())
            {
                hDialog.Description = Localization.SledProjectNewFormAssetDirSelectDescription;
                hDialog.ShowNewFolderButton = true;

                var szProjectDir = ProjectDirectory;
                var szAssetDir = ProjectAssetDirectory;

                if (!string.IsNullOrEmpty(szAssetDir) && Directory.Exists(szAssetDir))
                    hDialog.SelectedPath = szAssetDir;
                else if (!string.IsNullOrEmpty(szProjectDir) && Directory.Exists(szProjectDir))
                    hDialog.SelectedPath = szProjectDir;
                else
                    hDialog.RootFolder = Environment.SpecialFolder.Desktop;                    

                // Show the dialog
                if (hDialog.ShowDialog(this) != DialogResult.OK)
                    return;

                // Make sure the path ends with a directory separator
                var szSelectedPath = 
                    FixupDirectoryPath(hDialog.SelectedPath);

                // Update new project dialog
                m_txtAssetDir.Text = szSelectedPath;                
            }
        }

        /// <summary>
        /// When user hits the [asset] copy button
        /// </summary>
        /// <param name="sender">object that fired the event</param>
        /// <param name="e">event arguments</param>
        private void BtnAssetCopy_Click(object sender, EventArgs e)
        {
            var text = m_txtAssetDir.Text;

            if (string.IsNullOrEmpty(text))
                return;

            try
            {
                Clipboard.Clear();
                Clipboard.SetText(text);
            }
            catch (System.Runtime.InteropServices.ExternalException ex)     { ShowCopyErrorMessageBox(ex); }
            catch (System.Threading.ThreadStateException ex)                { ShowCopyErrorMessageBox(ex); }
            catch (System.ComponentModel.InvalidEnumArgumentException ex)   { ShowCopyErrorMessageBox(ex); }
            catch (ArgumentNullException ex)                                { ShowCopyErrorMessageBox(ex); }
        }

        /// <summary>
        /// When user hits the [asset] paste button
        /// </summary>
        /// <param name="sender">object that fired the event</param>
        /// <param name="e">event arguments</param>
        private void BtnAssetPaste_Click(object sender, EventArgs e)
        {
            try
            {
                if (Clipboard.ContainsText())
                {
                    var text = Clipboard.GetText();
                    var path = Path.GetFullPath(text + Path.DirectorySeparatorChar);

                    if (Directory.Exists(path))
                    {
                        // Update asset dir path
                        m_txtAssetDir.Text = path;
                    }
                }
            }
            catch (System.Security.SecurityException ex)                    { ShowPasteErrorMessageBox(ex); }
            catch (System.Runtime.InteropServices.ExternalException ex)     { ShowPasteErrorMessageBox(ex); }
            catch (System.Threading.ThreadStateException ex)                { ShowPasteErrorMessageBox(ex); }
            catch (System.ComponentModel.InvalidEnumArgumentException ex)   { ShowPasteErrorMessageBox(ex); }
            catch (PathTooLongException ex)                                 { ShowPasteErrorMessageBox(ex); }
            catch (NotSupportedException ex)                                { ShowPasteErrorMessageBox(ex); }
            catch (ArgumentNullException ex)                                { ShowPasteErrorMessageBox(ex); }
            catch (ArgumentException ex)                                    { ShowPasteErrorMessageBox(ex); }
        }
        
        /// <summary>
        /// Event for clicking create project
        /// </summary>
        /// <param name="sender">object that fired the event</param>
        /// <param name="e">event arguments</param>
        private void BtnCreate_Click(object sender, EventArgs e)
        {
            var name = ProjectName;
            var dir = ProjectDirectory;
            var asset = ProjectAssetDirectory;

            // Verify project has name
            if (string.IsNullOrEmpty(name))
            {
                MessageBox.Show(
                    this,
                    Localization.SledProjectNewFormErrorProjectName,
                    Localization.SledProjectNewFormErrorTitle,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);

                DialogResult = DialogResult.None;
            }
            // Verify directory path exists
            else if (!Directory.Exists(dir))
            {
                MessageBox.Show(
                    this,
                    Localization.SledProjectNewFormErrorProjectDir,
                    Localization.SledProjectNewFormErrorTitle,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);

                DialogResult = DialogResult.None;
            }
            else if (!Directory.Exists(asset))
            {
                MessageBox.Show(
                    this,
                    Localization.SledProjectNewFormErrorAssetDir,
                    Localization.SledProjectNewFormErrorTitle,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);

                DialogResult = DialogResult.None;
            }
            else
            {
                DialogResult = DialogResult.OK;
            }
        }

        protected override void OnResize(EventArgs e)
        {
            // Clamp height
            if (Height > m_iHeight)
                Height = m_iHeight;

            base.OnResize(e);
        }

        private void ShowCopyErrorMessageBox(Exception ex)
        {
            ShowErrorMessageBox(
                string.Format("Exception copying!{0}{0}Exception details:{0}{1}{0}{0}{2}",
                    Environment.NewLine,
                    ex.Message,
                    ex.StackTrace));
        }

        private void ShowPasteErrorMessageBox(Exception ex)
        {
            ShowErrorMessageBox(
                string.Format("Exception pasting!{0}{0}Exception details:{0}{1}{0}{0}{2}",
                    Environment.NewLine,
                    ex.Message,
                    ex.StackTrace));
        }

        private void ShowErrorMessageBox(string message)
        {
            MessageBox.Show(
                this,
                message,
                "Exception Encountered",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }

        private void UpdateFinalPath()
        {
            if (m_bUpdatingFinalPath)
                return;

            try
            {
                m_bUpdatingFinalPath = true;

                var name = ProjectName;
                var dir = ProjectDirectory;

                var finalPath =
                    dir + name + Resource.ProjectFileExtensionSpf;

                var bValid = true;

                try
                {
                    Path.GetFullPath(finalPath);
                }
                catch (Exception)
                {
                    bValid = false;
                }

                m_txtProjectOutput.Text =
                    bValid
                        ? finalPath
                        : "<Invalid path>";
            }
            finally
            {
                m_bUpdatingFinalPath = false;
            }
        }
        
        private readonly int m_iHeight;
        private bool m_bUpdatingFinalPath;
    }
}
