/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

namespace Sce.Sled
{
    partial class SledSourceControlResolutionForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SledSourceControlResolutionForm));
            this.m_txtFile = new System.Windows.Forms.TextBox();
            this.m_btnGetLatest = new System.Windows.Forms.Button();
            this.m_btnEditCurrent = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // m_txtFile
            // 
            resources.ApplyResources(this.m_txtFile, "m_txtFile");
            this.m_txtFile.Name = "m_txtFile";
            this.m_txtFile.ReadOnly = true;
            // 
            // m_btnGetLatest
            // 
            resources.ApplyResources(this.m_btnGetLatest, "m_btnGetLatest");
            this.m_btnGetLatest.Name = "m_btnGetLatest";
            this.m_btnGetLatest.UseVisualStyleBackColor = true;
            this.m_btnGetLatest.Click += new System.EventHandler(this.BtnGetLatestClick);
            // 
            // m_btnEditCurrent
            // 
            resources.ApplyResources(this.m_btnEditCurrent, "m_btnEditCurrent");
            this.m_btnEditCurrent.Name = "m_btnEditCurrent";
            this.m_btnEditCurrent.UseVisualStyleBackColor = true;
            this.m_btnEditCurrent.Click += new System.EventHandler(this.BtnEditCurrentClick);
            // 
            // groupBox1
            // 
            resources.ApplyResources(this.groupBox1, "groupBox1");
            this.groupBox1.Controls.Add(this.m_txtFile);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.TabStop = false;
            // 
            // groupBox2
            // 
            resources.ApplyResources(this.groupBox2, "groupBox2");
            this.groupBox2.Controls.Add(this.m_btnEditCurrent);
            this.groupBox2.Controls.Add(this.m_btnGetLatest);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.TabStop = false;
            // 
            // SledSourceControlResolutionForm
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "SledSourceControlResolutionForm";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TextBox m_txtFile;
        private System.Windows.Forms.Button m_btnGetLatest;
        private System.Windows.Forms.Button m_btnEditCurrent;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox groupBox2;
    }
}