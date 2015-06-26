/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

namespace Sce.Sled.Shared.Controls
{
    partial class SledMultiFolderSelectionForm
    {
        /// <summary>
        /// Required designer variable
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used
        /// </summary>
        /// <param name="disposing">True iff managed resources should be disposed</param>
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SledMultiFolderSelectionForm));
            this.m_btnAdd = new System.Windows.Forms.Button();
            this.m_btnRemove = new System.Windows.Forms.Button();
            this.m_btnOk = new System.Windows.Forms.Button();
            this.m_btnCancel = new System.Windows.Forms.Button();
            this.m_lstFolders = new System.Windows.Forms.ListView();
            this.m_lstFolders_Header_Folder = new System.Windows.Forms.ColumnHeader();
            this.SuspendLayout();
            // 
            // m_btnAdd
            // 
            resources.ApplyResources(this.m_btnAdd, "m_btnAdd");
            this.m_btnAdd.Name = "m_btnAdd";
            this.m_btnAdd.UseVisualStyleBackColor = true;
            this.m_btnAdd.Click += new System.EventHandler(this.BtnAdd_Click);
            // 
            // m_btnRemove
            // 
            resources.ApplyResources(this.m_btnRemove, "m_btnRemove");
            this.m_btnRemove.Name = "m_btnRemove";
            this.m_btnRemove.UseVisualStyleBackColor = true;
            this.m_btnRemove.Click += new System.EventHandler(this.BtnRemove_Click);
            // 
            // m_btnOk
            // 
            resources.ApplyResources(this.m_btnOk, "m_btnOk");
            this.m_btnOk.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.m_btnOk.Name = "m_btnOk";
            this.m_btnOk.UseVisualStyleBackColor = true;
            this.m_btnOk.Click += new System.EventHandler(this.BtnOk_Click);
            // 
            // m_btnCancel
            // 
            resources.ApplyResources(this.m_btnCancel, "m_btnCancel");
            this.m_btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.m_btnCancel.Name = "m_btnCancel";
            this.m_btnCancel.UseVisualStyleBackColor = true;
            // 
            // m_lstFolders
            // 
            resources.ApplyResources(this.m_lstFolders, "m_lstFolders");
            this.m_lstFolders.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.m_lstFolders_Header_Folder});
            this.m_lstFolders.FullRowSelect = true;
            this.m_lstFolders.Name = "m_lstFolders";
            this.m_lstFolders.Sorting = System.Windows.Forms.SortOrder.Ascending;
            this.m_lstFolders.UseCompatibleStateImageBehavior = false;
            this.m_lstFolders.View = System.Windows.Forms.View.Details;
            // 
            // m_lstFolders_Header_Folder
            // 
            resources.ApplyResources(this.m_lstFolders_Header_Folder, "m_lstFolders_Header_Folder");
            // 
            // SledMultiFolderSelectionForm
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.m_lstFolders);
            this.Controls.Add(this.m_btnCancel);
            this.Controls.Add(this.m_btnOk);
            this.Controls.Add(this.m_btnRemove);
            this.Controls.Add(this.m_btnAdd);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "SledMultiFolderSelectionForm";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button m_btnAdd;
        private System.Windows.Forms.Button m_btnRemove;
        private System.Windows.Forms.Button m_btnOk;
        private System.Windows.Forms.Button m_btnCancel;
        private System.Windows.Forms.ListView m_lstFolders;
        private System.Windows.Forms.ColumnHeader m_lstFolders_Header_Folder;
    }
}
