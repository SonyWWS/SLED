/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

namespace Sce.Sled
{
    partial class SledTargetsForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SledTargetsForm));
            this.m_lstView = new Sce.Sled.SledTargetsForm.TargetListView();
            this.columnHeader1 = new System.Windows.Forms.ColumnHeader();
            this.columnHeader2 = new System.Windows.Forms.ColumnHeader();
            this.columnHeader3 = new System.Windows.Forms.ColumnHeader();
            this.m_btnAdd = new System.Windows.Forms.Button();
            this.m_btnEdit = new System.Windows.Forms.Button();
            this.m_btnDelete = new System.Windows.Forms.Button();
            this.m_btnClose = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // m_lstView
            // 
            resources.ApplyResources(this.m_lstView, "m_lstView");
            this.m_lstView.CheckBoxes = true;
            this.m_lstView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader2,
            this.columnHeader3});
            this.m_lstView.FullRowSelect = true;
            this.m_lstView.HideSelection = false;
            this.m_lstView.Name = "m_lstView";
            this.m_lstView.OwnerDraw = true;
            this.m_lstView.Sorting = System.Windows.Forms.SortOrder.Ascending;
            this.m_lstView.UseCompatibleStateImageBehavior = false;
            this.m_lstView.View = System.Windows.Forms.View.Details;
            this.m_lstView.DrawColumnHeader += new System.Windows.Forms.DrawListViewColumnHeaderEventHandler(this.LstViewDrawColumnHeader);
            this.m_lstView.ItemChecked += new System.Windows.Forms.ItemCheckedEventHandler(this.LstViewItemChecked);
            this.m_lstView.DrawItem += new System.Windows.Forms.DrawListViewItemEventHandler(this.LstViewDrawItem);
            this.m_lstView.SelectedIndexChanged += new System.EventHandler(this.LstViewSelectedIndexChanged);
            this.m_lstView.DrawSubItem += new System.Windows.Forms.DrawListViewSubItemEventHandler(this.LstViewDrawSubItem);
            // 
            // columnHeader1
            // 
            resources.ApplyResources(this.columnHeader1, "columnHeader1");
            // 
            // columnHeader2
            // 
            resources.ApplyResources(this.columnHeader2, "columnHeader2");
            // 
            // columnHeader3
            // 
            resources.ApplyResources(this.columnHeader3, "columnHeader3");
            // 
            // m_btnAdd
            // 
            resources.ApplyResources(this.m_btnAdd, "m_btnAdd");
            this.m_btnAdd.Name = "m_btnAdd";
            this.m_btnAdd.UseVisualStyleBackColor = true;
            this.m_btnAdd.Click += new System.EventHandler(this.BtnAddClick);
            // 
            // m_btnEdit
            // 
            resources.ApplyResources(this.m_btnEdit, "m_btnEdit");
            this.m_btnEdit.Name = "m_btnEdit";
            this.m_btnEdit.UseVisualStyleBackColor = true;
            this.m_btnEdit.Click += new System.EventHandler(this.BtnEditClick);
            // 
            // m_btnDelete
            // 
            resources.ApplyResources(this.m_btnDelete, "m_btnDelete");
            this.m_btnDelete.Name = "m_btnDelete";
            this.m_btnDelete.UseVisualStyleBackColor = true;
            this.m_btnDelete.Click += new System.EventHandler(this.BtnDeleteClick);
            // 
            // m_btnClose
            // 
            resources.ApplyResources(this.m_btnClose, "m_btnClose");
            this.m_btnClose.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.m_btnClose.Name = "m_btnClose";
            this.m_btnClose.UseVisualStyleBackColor = true;
            this.m_btnClose.Click += new System.EventHandler(this.BtnCloseClick);
            // 
            // label1
            // 
            resources.ApplyResources(this.label1, "label1");
            this.label1.Name = "label1";
            // 
            // SledTargetsForm
            // 
            this.AcceptButton = this.m_btnClose;
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.label1);
            this.Controls.Add(this.m_btnClose);
            this.Controls.Add(this.m_btnDelete);
            this.Controls.Add(this.m_btnEdit);
            this.Controls.Add(this.m_btnAdd);
            this.Controls.Add(this.m_lstView);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "SledTargetsForm";
            this.Load += new System.EventHandler(this.SledTargetsFormLoad);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.SledTargetsFormFormClosing);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private Sce.Sled.SledTargetsForm.TargetListView m_lstView;
        private System.Windows.Forms.Button m_btnAdd;
        private System.Windows.Forms.Button m_btnEdit;
        private System.Windows.Forms.Button m_btnDelete;
        private System.Windows.Forms.Button m_btnClose;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ColumnHeader columnHeader2;
        private System.Windows.Forms.ColumnHeader columnHeader3;
        private System.Windows.Forms.Label label1;
    }
}
