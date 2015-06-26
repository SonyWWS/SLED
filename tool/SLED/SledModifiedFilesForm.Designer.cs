/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

namespace Sce.Sled
{
    partial class SledModifiedFilesForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SledModifiedFilesForm));
            this.m_lstBoxFiles = new System.Windows.Forms.ListBox();
            this.m_btnReload = new System.Windows.Forms.Button();
            this.m_btnIgnore = new System.Windows.Forms.Button();
            this.m_btnReloadAll = new System.Windows.Forms.Button();
            this.m_btnIgnoreAll = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // m_lstBoxFiles
            // 
            resources.ApplyResources(this.m_lstBoxFiles, "m_lstBoxFiles");
            this.m_lstBoxFiles.FormattingEnabled = true;
            this.m_lstBoxFiles.Name = "m_lstBoxFiles";
            // 
            // m_btnReload
            // 
            resources.ApplyResources(this.m_btnReload, "m_btnReload");
            this.m_btnReload.Name = "m_btnReload";
            this.m_btnReload.UseVisualStyleBackColor = true;
            this.m_btnReload.Click += new System.EventHandler(this.BtnReloadClick);
            // 
            // m_btnIgnore
            // 
            resources.ApplyResources(this.m_btnIgnore, "m_btnIgnore");
            this.m_btnIgnore.Name = "m_btnIgnore";
            this.m_btnIgnore.UseVisualStyleBackColor = true;
            this.m_btnIgnore.Click += new System.EventHandler(this.BtnIgnoreClick);
            // 
            // m_btnReloadAll
            // 
            resources.ApplyResources(this.m_btnReloadAll, "m_btnReloadAll");
            this.m_btnReloadAll.Name = "m_btnReloadAll";
            this.m_btnReloadAll.UseVisualStyleBackColor = true;
            this.m_btnReloadAll.Click += new System.EventHandler(this.BtnReloadAllClick);
            // 
            // m_btnIgnoreAll
            // 
            resources.ApplyResources(this.m_btnIgnoreAll, "m_btnIgnoreAll");
            this.m_btnIgnoreAll.Name = "m_btnIgnoreAll";
            this.m_btnIgnoreAll.UseVisualStyleBackColor = true;
            this.m_btnIgnoreAll.Click += new System.EventHandler(this.BtnIgnoreAllClick);
            // 
            // label1
            // 
            resources.ApplyResources(this.label1, "label1");
            this.label1.Name = "label1";
            // 
            // SledModifiedFilesForm
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.label1);
            this.Controls.Add(this.m_btnIgnoreAll);
            this.Controls.Add(this.m_btnReloadAll);
            this.Controls.Add(this.m_btnIgnore);
            this.Controls.Add(this.m_btnReload);
            this.Controls.Add(this.m_lstBoxFiles);
            this.MaximizeBox = false;
            this.Name = "SledModifiedFilesForm";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.SledModifiedFilesFormFormClosing);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ListBox m_lstBoxFiles;
        private System.Windows.Forms.Button m_btnReload;
        private System.Windows.Forms.Button m_btnIgnore;
        private System.Windows.Forms.Button m_btnReloadAll;
        private System.Windows.Forms.Button m_btnIgnoreAll;
        private System.Windows.Forms.Label label1;
    }
}
