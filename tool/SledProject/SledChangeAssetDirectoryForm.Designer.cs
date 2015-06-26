/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

namespace Sce.Sled.Project
{
    partial class SledChangeAssetDirectoryForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SledChangeAssetDirectoryForm));
            this.label1 = new System.Windows.Forms.Label();
            this.m_grpQuestion = new System.Windows.Forms.GroupBox();
            this.m_rdoOption2 = new System.Windows.Forms.RadioButton();
            this.m_rdoOption1 = new System.Windows.Forms.RadioButton();
            this.label2 = new System.Windows.Forms.Label();
            this.m_grpExample = new System.Windows.Forms.GroupBox();
            this.m_lstFiles = new System.Windows.Forms.ListView();
            this.columnHeader1 = new System.Windows.Forms.ColumnHeader();
            this.columnHeader2 = new System.Windows.Forms.ColumnHeader();
            this.columnHeader3 = new System.Windows.Forms.ColumnHeader();
            this.m_btnOk = new System.Windows.Forms.Button();
            this.m_btnCancel = new System.Windows.Forms.Button();
            this.m_grpQuestion.SuspendLayout();
            this.m_grpExample.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            resources.ApplyResources(this.label1, "label1");
            this.label1.Name = "label1";
            // 
            // m_grpQuestion
            // 
            resources.ApplyResources(this.m_grpQuestion, "m_grpQuestion");
            this.m_grpQuestion.Controls.Add(this.m_rdoOption2);
            this.m_grpQuestion.Controls.Add(this.m_rdoOption1);
            this.m_grpQuestion.Controls.Add(this.label2);
            this.m_grpQuestion.Name = "m_grpQuestion";
            this.m_grpQuestion.TabStop = false;
            // 
            // m_rdoOption2
            // 
            resources.ApplyResources(this.m_rdoOption2, "m_rdoOption2");
            this.m_rdoOption2.Name = "m_rdoOption2";
            this.m_rdoOption2.TabStop = true;
            this.m_rdoOption2.UseVisualStyleBackColor = true;
            // 
            // m_rdoOption1
            // 
            resources.ApplyResources(this.m_rdoOption1, "m_rdoOption1");
            this.m_rdoOption1.Name = "m_rdoOption1";
            this.m_rdoOption1.TabStop = true;
            this.m_rdoOption1.UseVisualStyleBackColor = true;
            this.m_rdoOption1.CheckedChanged += new System.EventHandler(this.RdoOption1_CheckedChanged);
            // 
            // label2
            // 
            resources.ApplyResources(this.label2, "label2");
            this.label2.Name = "label2";
            // 
            // m_grpExample
            // 
            resources.ApplyResources(this.m_grpExample, "m_grpExample");
            this.m_grpExample.Controls.Add(this.m_lstFiles);
            this.m_grpExample.Name = "m_grpExample";
            this.m_grpExample.TabStop = false;
            // 
            // m_lstFiles
            // 
            resources.ApplyResources(this.m_lstFiles, "m_lstFiles");
            this.m_lstFiles.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader2,
            this.columnHeader3});
            this.m_lstFiles.Name = "m_lstFiles";
            this.m_lstFiles.UseCompatibleStateImageBehavior = false;
            this.m_lstFiles.View = System.Windows.Forms.View.Details;
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
            // m_btnOk
            // 
            resources.ApplyResources(this.m_btnOk, "m_btnOk");
            this.m_btnOk.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.m_btnOk.Name = "m_btnOk";
            this.m_btnOk.UseVisualStyleBackColor = true;
            // 
            // m_btnCancel
            // 
            resources.ApplyResources(this.m_btnCancel, "m_btnCancel");
            this.m_btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.m_btnCancel.Name = "m_btnCancel";
            this.m_btnCancel.UseVisualStyleBackColor = true;
            // 
            // SledChangeAssetDirectoryForm
            // 
            this.AcceptButton = this.m_btnOk;
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.m_btnCancel;
            this.Controls.Add(this.m_btnCancel);
            this.Controls.Add(this.m_btnOk);
            this.Controls.Add(this.m_grpExample);
            this.Controls.Add(this.m_grpQuestion);
            this.Controls.Add(this.label1);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "SledChangeAssetDirectoryForm";
            this.Load += new System.EventHandler(this.SledChangeAssetDirectoryForm_Load);
            this.m_grpQuestion.ResumeLayout(false);
            this.m_grpQuestion.PerformLayout();
            this.m_grpExample.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.GroupBox m_grpQuestion;
        private System.Windows.Forms.RadioButton m_rdoOption2;
        private System.Windows.Forms.RadioButton m_rdoOption1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.GroupBox m_grpExample;
        private System.Windows.Forms.ListView m_lstFiles;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ColumnHeader columnHeader2;
        private System.Windows.Forms.ColumnHeader columnHeader3;
        private System.Windows.Forms.Button m_btnOk;
        private System.Windows.Forms.Button m_btnCancel;
    }
}
