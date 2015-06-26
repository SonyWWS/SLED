/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

namespace Sce.Sled
{
    /// <summary>
    /// Goto Variable Form
    /// </summary>
    partial class SledVarGotoForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SledVarGotoForm));
            this.m_lstView = new System.Windows.Forms.ListView();
            this.SuspendLayout();
            // 
            // m_lstView
            // 
            resources.ApplyResources(this.m_lstView, "m_lstView");
            this.m_lstView.FullRowSelect = true;
            this.m_lstView.Name = "m_lstView";
            this.m_lstView.UseCompatibleStateImageBehavior = false;
            this.m_lstView.View = System.Windows.Forms.View.Details;
            this.m_lstView.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.LstView_MouseDoubleClick);
            // 
            // SledVarGotoForm
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.m_lstView);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "SledVarGotoForm";
            this.ShowInTaskbar = false;
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ListView m_lstView;
    }
}
