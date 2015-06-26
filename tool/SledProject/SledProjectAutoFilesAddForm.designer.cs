/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

namespace Sce.Sled.Project
{
    partial class SledProjectAutoFilesAddForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SledProjectAutoFilesAddForm));
            this.m_grpFilesToBeAdded = new System.Windows.Forms.GroupBox();
            this.m_treeCheckedFiles = new Sce.Sled.Shared.Controls.SledRecursiveCheckBoxesTreeView();
            this.m_txtFileExtsAutoCheck = new System.Windows.Forms.TextBox();
            this.m_grpFilesToAutoCheck = new System.Windows.Forms.GroupBox();
            this.m_chkAutoFiles = new System.Windows.Forms.CheckBox();
            this.m_btnDirAdd = new System.Windows.Forms.Button();
            this.m_grpDirScanned = new System.Windows.Forms.GroupBox();
            this.m_btnRemoveDir = new System.Windows.Forms.Button();
            this.m_lstDirUsed = new System.Windows.Forms.ListBox();
            this.m_btnOK = new System.Windows.Forms.Button();
            this.m_btnCancel = new System.Windows.Forms.Button();
            this.m_grpFilesToBeAdded.SuspendLayout();
            this.m_grpFilesToAutoCheck.SuspendLayout();
            this.m_grpDirScanned.SuspendLayout();
            this.SuspendLayout();
            // 
            // m_grpFilesToBeAdded
            // 
            resources.ApplyResources(this.m_grpFilesToBeAdded, "m_grpFilesToBeAdded");
            this.m_grpFilesToBeAdded.Controls.Add(this.m_treeCheckedFiles);
            this.m_grpFilesToBeAdded.Name = "m_grpFilesToBeAdded";
            this.m_grpFilesToBeAdded.TabStop = false;
            // 
            // m_treeCheckedFiles
            // 
            resources.ApplyResources(this.m_treeCheckedFiles, "m_treeCheckedFiles");
            this.m_treeCheckedFiles.CheckBoxes = true;
            this.m_treeCheckedFiles.Name = "m_treeCheckedFiles";
            this.m_treeCheckedFiles.RecursiveCheckBoxes = false;
            // 
            // m_txtFileExtsAutoCheck
            // 
            resources.ApplyResources(this.m_txtFileExtsAutoCheck, "m_txtFileExtsAutoCheck");
            this.m_txtFileExtsAutoCheck.Name = "m_txtFileExtsAutoCheck";
            // 
            // m_grpFilesToAutoCheck
            // 
            resources.ApplyResources(this.m_grpFilesToAutoCheck, "m_grpFilesToAutoCheck");
            this.m_grpFilesToAutoCheck.Controls.Add(this.m_chkAutoFiles);
            this.m_grpFilesToAutoCheck.Controls.Add(this.m_txtFileExtsAutoCheck);
            this.m_grpFilesToAutoCheck.Name = "m_grpFilesToAutoCheck";
            this.m_grpFilesToAutoCheck.TabStop = false;
            // 
            // m_chkAutoFiles
            // 
            resources.ApplyResources(this.m_chkAutoFiles, "m_chkAutoFiles");
            this.m_chkAutoFiles.Name = "m_chkAutoFiles";
            this.m_chkAutoFiles.UseVisualStyleBackColor = true;
            this.m_chkAutoFiles.CheckedChanged += new System.EventHandler(this.ChkAutoFilesCheckedChanged);
            // 
            // m_btnDirAdd
            // 
            resources.ApplyResources(this.m_btnDirAdd, "m_btnDirAdd");
            this.m_btnDirAdd.Name = "m_btnDirAdd";
            this.m_btnDirAdd.UseVisualStyleBackColor = true;
            this.m_btnDirAdd.Click += new System.EventHandler(this.BtnDirAddClick);
            // 
            // m_grpDirScanned
            // 
            resources.ApplyResources(this.m_grpDirScanned, "m_grpDirScanned");
            this.m_grpDirScanned.Controls.Add(this.m_btnRemoveDir);
            this.m_grpDirScanned.Controls.Add(this.m_lstDirUsed);
            this.m_grpDirScanned.Controls.Add(this.m_btnDirAdd);
            this.m_grpDirScanned.Name = "m_grpDirScanned";
            this.m_grpDirScanned.TabStop = false;
            // 
            // m_btnRemoveDir
            // 
            resources.ApplyResources(this.m_btnRemoveDir, "m_btnRemoveDir");
            this.m_btnRemoveDir.Name = "m_btnRemoveDir";
            this.m_btnRemoveDir.UseVisualStyleBackColor = true;
            this.m_btnRemoveDir.Click += new System.EventHandler(this.BtnRemoveDirClick);
            // 
            // m_lstDirUsed
            // 
            resources.ApplyResources(this.m_lstDirUsed, "m_lstDirUsed");
            this.m_lstDirUsed.FormattingEnabled = true;
            this.m_lstDirUsed.Name = "m_lstDirUsed";
            this.m_lstDirUsed.Sorted = true;
            this.m_lstDirUsed.SelectedIndexChanged += new System.EventHandler(this.LstDirUsedSelectedIndexChanged);
            // 
            // m_btnOK
            // 
            resources.ApplyResources(this.m_btnOK, "m_btnOK");
            this.m_btnOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.m_btnOK.Name = "m_btnOK";
            this.m_btnOK.UseVisualStyleBackColor = true;
            // 
            // m_btnCancel
            // 
            resources.ApplyResources(this.m_btnCancel, "m_btnCancel");
            this.m_btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.m_btnCancel.Name = "m_btnCancel";
            this.m_btnCancel.UseVisualStyleBackColor = true;
            // 
            // SledProjectAutoFilesAddForm
            // 
            this.AcceptButton = this.m_btnOK;
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.m_btnCancel;
            this.Controls.Add(this.m_btnCancel);
            this.Controls.Add(this.m_btnOK);
            this.Controls.Add(this.m_grpDirScanned);
            this.Controls.Add(this.m_grpFilesToAutoCheck);
            this.Controls.Add(this.m_grpFilesToBeAdded);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "SledProjectAutoFilesAddForm";
            this.ShowInTaskbar = false;
            this.m_grpFilesToBeAdded.ResumeLayout(false);
            this.m_grpFilesToAutoCheck.ResumeLayout(false);
            this.m_grpFilesToAutoCheck.PerformLayout();
            this.m_grpDirScanned.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox m_grpFilesToBeAdded;
        private System.Windows.Forms.TextBox m_txtFileExtsAutoCheck;
        private System.Windows.Forms.GroupBox m_grpFilesToAutoCheck;
        private System.Windows.Forms.Button m_btnDirAdd;
        private System.Windows.Forms.GroupBox m_grpDirScanned;
        private System.Windows.Forms.ListBox m_lstDirUsed;
        private System.Windows.Forms.Button m_btnRemoveDir;
        private System.Windows.Forms.Button m_btnOK;
        private System.Windows.Forms.Button m_btnCancel;
        private Sce.Sled.Shared.Controls.SledRecursiveCheckBoxesTreeView m_treeCheckedFiles;
        private System.Windows.Forms.CheckBox m_chkAutoFiles;
    }
}
