/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

namespace Sce.Sled.Project
{
    partial class SledViewProjectPathsForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SledViewProjectPathsForm));
            this.m_lstView = new System.Windows.Forms.ListView();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.m_txtProjectName = new System.Windows.Forms.TextBox();
            this.m_txtProjectDirectory = new System.Windows.Forms.TextBox();
            this.m_txtAssetDirectory = new System.Windows.Forms.TextBox();
            this.m_txtProjectGuid = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // m_lstView
            // 
            resources.ApplyResources(this.m_lstView, "m_lstView");
            this.m_lstView.FullRowSelect = true;
            this.m_lstView.Name = "m_lstView";
            this.m_lstView.UseCompatibleStateImageBehavior = false;
            this.m_lstView.View = System.Windows.Forms.View.Details;
            // 
            // label1
            // 
            resources.ApplyResources(this.label1, "label1");
            this.label1.Name = "label1";
            // 
            // label2
            // 
            resources.ApplyResources(this.label2, "label2");
            this.label2.Name = "label2";
            // 
            // label3
            // 
            resources.ApplyResources(this.label3, "label3");
            this.label3.Name = "label3";
            // 
            // m_txtProjectName
            // 
            resources.ApplyResources(this.m_txtProjectName, "m_txtProjectName");
            this.m_txtProjectName.Name = "m_txtProjectName";
            this.m_txtProjectName.ReadOnly = true;
            // 
            // m_txtProjectDirectory
            // 
            resources.ApplyResources(this.m_txtProjectDirectory, "m_txtProjectDirectory");
            this.m_txtProjectDirectory.Name = "m_txtProjectDirectory";
            this.m_txtProjectDirectory.ReadOnly = true;
            // 
            // m_txtAssetDirectory
            // 
            resources.ApplyResources(this.m_txtAssetDirectory, "m_txtAssetDirectory");
            this.m_txtAssetDirectory.Name = "m_txtAssetDirectory";
            this.m_txtAssetDirectory.ReadOnly = true;
            // 
            // m_txtProjectGuid
            // 
            resources.ApplyResources(this.m_txtProjectGuid, "m_txtProjectGuid");
            this.m_txtProjectGuid.Name = "m_txtProjectGuid";
            this.m_txtProjectGuid.ReadOnly = true;
            // 
            // label4
            // 
            resources.ApplyResources(this.label4, "label4");
            this.label4.Name = "label4";
            // 
            // SledViewProjectPathsForm
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.label4);
            this.Controls.Add(this.m_txtProjectGuid);
            this.Controls.Add(this.m_txtAssetDirectory);
            this.Controls.Add(this.m_txtProjectDirectory);
            this.Controls.Add(this.m_txtProjectName);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.m_lstView);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "SledViewProjectPathsForm";
            this.ShowInTaskbar = false;
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ListView m_lstView;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox m_txtProjectName;
        private System.Windows.Forms.TextBox m_txtProjectDirectory;
        private System.Windows.Forms.TextBox m_txtAssetDirectory;
        private System.Windows.Forms.TextBox m_txtProjectGuid;
        private System.Windows.Forms.Label label4;
    }
}
