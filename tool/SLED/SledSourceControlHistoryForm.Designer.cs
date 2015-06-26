/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

namespace Sce.Sled
{
    partial class SledSourceControlHistoryForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SledSourceControlHistoryForm));
            this.m_grpHistory = new System.Windows.Forms.GroupBox();
            this.m_lstHistory = new System.Windows.Forms.ListView();
            this.columnHeader1 = new System.Windows.Forms.ColumnHeader();
            this.columnHeader2 = new System.Windows.Forms.ColumnHeader();
            this.columnHeader3 = new System.Windows.Forms.ColumnHeader();
            this.columnHeader4 = new System.Windows.Forms.ColumnHeader();
            this.m_txtHistory = new System.Windows.Forms.TextBox();
            this.m_btnClose = new System.Windows.Forms.Button();
            this.m_grpHistory.SuspendLayout();
            this.SuspendLayout();
            // 
            // m_grpHistory
            // 
            resources.ApplyResources(this.m_grpHistory, "m_grpHistory");
            this.m_grpHistory.Controls.Add(this.m_lstHistory);
            this.m_grpHistory.Controls.Add(this.m_txtHistory);
            this.m_grpHistory.Name = "m_grpHistory";
            this.m_grpHistory.TabStop = false;
            // 
            // m_lstHistory
            // 
            resources.ApplyResources(this.m_lstHistory, "m_lstHistory");
            this.m_lstHistory.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader2,
            this.columnHeader3,
            this.columnHeader4});
            this.m_lstHistory.Name = "m_lstHistory";
            this.m_lstHistory.UseCompatibleStateImageBehavior = false;
            this.m_lstHistory.View = System.Windows.Forms.View.Details;
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
            // columnHeader4
            // 
            resources.ApplyResources(this.columnHeader4, "columnHeader4");
            // 
            // m_txtHistory
            // 
            resources.ApplyResources(this.m_txtHistory, "m_txtHistory");
            this.m_txtHistory.Name = "m_txtHistory";
            this.m_txtHistory.ReadOnly = true;
            // 
            // m_btnClose
            // 
            resources.ApplyResources(this.m_btnClose, "m_btnClose");
            this.m_btnClose.Name = "m_btnClose";
            this.m_btnClose.UseVisualStyleBackColor = true;
            this.m_btnClose.Click += new System.EventHandler(this.BtnCloseClick);
            // 
            // SledSourceControlHistoryForm
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.m_btnClose);
            this.Controls.Add(this.m_grpHistory);
            this.Name = "SledSourceControlHistoryForm";
            this.Load += new System.EventHandler(this.SledSourceControlHistoryFormLoad);
            this.m_grpHistory.ResumeLayout(false);
            this.m_grpHistory.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox m_grpHistory;
        private System.Windows.Forms.ListView m_lstHistory;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ColumnHeader columnHeader2;
        private System.Windows.Forms.ColumnHeader columnHeader3;
        private System.Windows.Forms.ColumnHeader columnHeader4;
        private System.Windows.Forms.TextBox m_txtHistory;
        private System.Windows.Forms.Button m_btnClose;
    }
}