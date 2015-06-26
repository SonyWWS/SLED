/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

namespace Sce.Sled
{
    /// <summary>
    /// TTY Filter Form
    /// </summary>
    partial class SledTtyFilterForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SledTtyFilterForm));
            this.m_lstBoxFilters = new System.Windows.Forms.ListBox();
            this.m_btnAdd = new System.Windows.Forms.Button();
            this.m_btnEdit = new System.Windows.Forms.Button();
            this.m_btnDelete = new System.Windows.Forms.Button();
            this.m_btnClose = new System.Windows.Forms.Button();
            this.m_btnLoadFile = new System.Windows.Forms.Button();
            this.m_btnSaveFile = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // m_lstBoxFilters
            // 
            resources.ApplyResources(this.m_lstBoxFilters, "m_lstBoxFilters");
            this.m_lstBoxFilters.FormattingEnabled = true;
            this.m_lstBoxFilters.Name = "m_lstBoxFilters";
            this.m_lstBoxFilters.Sorted = true;
            this.m_lstBoxFilters.DoubleClick += new System.EventHandler(this.LstFilters_DoubleClick);
            // 
            // m_btnAdd
            // 
            resources.ApplyResources(this.m_btnAdd, "m_btnAdd");
            this.m_btnAdd.Name = "m_btnAdd";
            this.m_btnAdd.UseVisualStyleBackColor = true;
            this.m_btnAdd.Click += new System.EventHandler(this.BtnAdd_Click);
            // 
            // m_btnEdit
            // 
            resources.ApplyResources(this.m_btnEdit, "m_btnEdit");
            this.m_btnEdit.Name = "m_btnEdit";
            this.m_btnEdit.UseVisualStyleBackColor = true;
            this.m_btnEdit.Click += new System.EventHandler(this.BtnEdit_Click);
            // 
            // m_btnDelete
            // 
            resources.ApplyResources(this.m_btnDelete, "m_btnDelete");
            this.m_btnDelete.Name = "m_btnDelete";
            this.m_btnDelete.UseVisualStyleBackColor = true;
            this.m_btnDelete.Click += new System.EventHandler(this.BtnDelete_Click);
            // 
            // m_btnClose
            // 
            resources.ApplyResources(this.m_btnClose, "m_btnClose");
            this.m_btnClose.Name = "m_btnClose";
            this.m_btnClose.UseVisualStyleBackColor = true;
            this.m_btnClose.Click += new System.EventHandler(this.BtnClose_Click);
            // 
            // m_btnLoadFile
            // 
            resources.ApplyResources(this.m_btnLoadFile, "m_btnLoadFile");
            this.m_btnLoadFile.Name = "m_btnLoadFile";
            this.m_btnLoadFile.UseVisualStyleBackColor = true;
            this.m_btnLoadFile.Click += new System.EventHandler(this.BtnLoadFile_Click);
            // 
            // m_btnSaveFile
            // 
            resources.ApplyResources(this.m_btnSaveFile, "m_btnSaveFile");
            this.m_btnSaveFile.Name = "m_btnSaveFile";
            this.m_btnSaveFile.UseVisualStyleBackColor = true;
            this.m_btnSaveFile.Click += new System.EventHandler(this.BtnSaveFile_Click);
            // 
            // SledTtyFilterForm
            // 
            this.AcceptButton = this.m_btnClose;
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.m_btnSaveFile);
            this.Controls.Add(this.m_btnLoadFile);
            this.Controls.Add(this.m_btnClose);
            this.Controls.Add(this.m_btnDelete);
            this.Controls.Add(this.m_btnEdit);
            this.Controls.Add(this.m_btnAdd);
            this.Controls.Add(this.m_lstBoxFilters);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "SledTtyFilterForm";
            this.ShowInTaskbar = false;
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ListBox m_lstBoxFilters;
        private System.Windows.Forms.Button m_btnAdd;
        private System.Windows.Forms.Button m_btnEdit;
        private System.Windows.Forms.Button m_btnDelete;
        private System.Windows.Forms.Button m_btnClose;
        private System.Windows.Forms.Button m_btnLoadFile;
        private System.Windows.Forms.Button m_btnSaveFile;
    }
}
