/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

namespace Sce.Sled.Shared.Controls
{
    partial class SledAsyncTaskForm
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
            if (disposing)
            {
                if (m_worker != null)
                {
                    m_worker.Dispose();
                    m_worker = null;
                }
            }

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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SledAsyncTaskForm));
            this.m_label = new System.Windows.Forms.Label();
            this.m_progress = new System.Windows.Forms.ProgressBar();
            this.SuspendLayout();
            // 
            // m_label
            // 
            resources.ApplyResources(this.m_label, "m_label");
            this.m_label.Name = "m_label";
            // 
            // m_progress
            // 
            resources.ApplyResources(this.m_progress, "m_progress");
            this.m_progress.Name = "m_progress";
            this.m_progress.Style = System.Windows.Forms.ProgressBarStyle.Marquee;
            // 
            // SledAsyncTaskForm
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.m_progress);
            this.Controls.Add(this.m_label);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "SledAsyncTaskForm";
            this.Shown += new System.EventHandler(this.SledAsyncTaskFormShown);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.SledAsyncTaskFormFormClosing);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label m_label;
        private System.Windows.Forms.ProgressBar m_progress;
    }
}