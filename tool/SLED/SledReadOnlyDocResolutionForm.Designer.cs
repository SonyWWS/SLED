/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

namespace Sce.Sled
{
    partial class SledReadOnlyDocResolutionForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SledReadOnlyDocResolutionForm));
            this.m_lblDescription = new System.Windows.Forms.Label();
            this.m_grpOptions = new System.Windows.Forms.GroupBox();
            this.m_btnSaveAs = new System.Windows.Forms.Button();
            this.m_btnUndoChanges = new System.Windows.Forms.Button();
            this.m_btnRemoveReadOnlyAndSave = new System.Windows.Forms.Button();
            this.m_btnRemoveReadOnly = new System.Windows.Forms.Button();
            this.m_grpFile = new System.Windows.Forms.GroupBox();
            this.m_txtFile = new System.Windows.Forms.TextBox();
            this.m_grpOptions.SuspendLayout();
            this.m_grpFile.SuspendLayout();
            this.SuspendLayout();
            // 
            // m_lblDescription
            // 
            resources.ApplyResources(this.m_lblDescription, "m_lblDescription");
            this.m_lblDescription.Name = "m_lblDescription";
            // 
            // m_grpOptions
            // 
            resources.ApplyResources(this.m_grpOptions, "m_grpOptions");
            this.m_grpOptions.Controls.Add(this.m_btnSaveAs);
            this.m_grpOptions.Controls.Add(this.m_btnUndoChanges);
            this.m_grpOptions.Controls.Add(this.m_btnRemoveReadOnlyAndSave);
            this.m_grpOptions.Controls.Add(this.m_btnRemoveReadOnly);
            this.m_grpOptions.Name = "m_grpOptions";
            this.m_grpOptions.TabStop = false;
            // 
            // m_btnSaveAs
            // 
            resources.ApplyResources(this.m_btnSaveAs, "m_btnSaveAs");
            this.m_btnSaveAs.Name = "m_btnSaveAs";
            this.m_btnSaveAs.UseVisualStyleBackColor = true;
            this.m_btnSaveAs.Click += new System.EventHandler(this.BtnSaveAs_Click);
            // 
            // m_btnUndoChanges
            // 
            resources.ApplyResources(this.m_btnUndoChanges, "m_btnUndoChanges");
            this.m_btnUndoChanges.Name = "m_btnUndoChanges";
            this.m_btnUndoChanges.UseVisualStyleBackColor = true;
            this.m_btnUndoChanges.Click += new System.EventHandler(this.BtnUndoChanges_Click);
            // 
            // m_btnRemoveReadOnlyAndSave
            // 
            resources.ApplyResources(this.m_btnRemoveReadOnlyAndSave, "m_btnRemoveReadOnlyAndSave");
            this.m_btnRemoveReadOnlyAndSave.Name = "m_btnRemoveReadOnlyAndSave";
            this.m_btnRemoveReadOnlyAndSave.UseVisualStyleBackColor = true;
            this.m_btnRemoveReadOnlyAndSave.Click += new System.EventHandler(this.BtnRemoveReadOnlyAndSave_Click);
            // 
            // m_btnRemoveReadOnly
            // 
            resources.ApplyResources(this.m_btnRemoveReadOnly, "m_btnRemoveReadOnly");
            this.m_btnRemoveReadOnly.Name = "m_btnRemoveReadOnly";
            this.m_btnRemoveReadOnly.UseVisualStyleBackColor = true;
            this.m_btnRemoveReadOnly.Click += new System.EventHandler(this.BtnRemoveReadOnly_Click);
            // 
            // m_grpFile
            // 
            resources.ApplyResources(this.m_grpFile, "m_grpFile");
            this.m_grpFile.Controls.Add(this.m_txtFile);
            this.m_grpFile.Name = "m_grpFile";
            this.m_grpFile.TabStop = false;
            // 
            // m_txtFile
            // 
            resources.ApplyResources(this.m_txtFile, "m_txtFile");
            this.m_txtFile.Name = "m_txtFile";
            this.m_txtFile.ReadOnly = true;
            // 
            // SledReadOnlyDocResolutionForm
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.m_grpFile);
            this.Controls.Add(this.m_grpOptions);
            this.Controls.Add(this.m_lblDescription);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "SledReadOnlyDocResolutionForm";
            this.m_grpOptions.ResumeLayout(false);
            this.m_grpFile.ResumeLayout(false);
            this.m_grpFile.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label m_lblDescription;
        private System.Windows.Forms.GroupBox m_grpOptions;
        private System.Windows.Forms.GroupBox m_grpFile;
        private System.Windows.Forms.TextBox m_txtFile;
        private System.Windows.Forms.Button m_btnRemoveReadOnly;
        private System.Windows.Forms.Button m_btnRemoveReadOnlyAndSave;
        private System.Windows.Forms.Button m_btnUndoChanges;
        private System.Windows.Forms.Button m_btnSaveAs;
    }
}
