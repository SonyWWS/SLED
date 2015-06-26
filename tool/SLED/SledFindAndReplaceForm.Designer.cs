/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

namespace Sce.Sled
{
    partial class SledFindAndReplaceForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SledFindAndReplaceForm));
            this.m_strpMain = new System.Windows.Forms.ToolStrip();
            this.m_strpFindDropDown = new System.Windows.Forms.ToolStripDropDownButton();
            this.m_strpFindDropDown_QuickFindItem = new System.Windows.Forms.ToolStripMenuItem();
            this.m_strpFindDropDown_FindInFilesItem = new System.Windows.Forms.ToolStripMenuItem();
            this.m_strpReplaceDropDown = new System.Windows.Forms.ToolStripDropDownButton();
            this.m_strpReplaceDropDown_QuickReplaceItem = new System.Windows.Forms.ToolStripMenuItem();
            this.m_strpReplaceDropDown_ReplaceInFilesItem = new System.Windows.Forms.ToolStripMenuItem();
            this.m_strpMain.SuspendLayout();
            this.SuspendLayout();
            // 
            // m_strpMain
            // 
            this.m_strpMain.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.m_strpFindDropDown,
            this.m_strpReplaceDropDown});
            resources.ApplyResources(this.m_strpMain, "m_strpMain");
            this.m_strpMain.Name = "m_strpMain";
            // 
            // m_strpFindDropDown
            // 
            this.m_strpFindDropDown.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.m_strpFindDropDown_QuickFindItem,
            this.m_strpFindDropDown_FindInFilesItem});
            resources.ApplyResources(this.m_strpFindDropDown, "m_strpFindDropDown");
            this.m_strpFindDropDown.Name = "m_strpFindDropDown";
            this.m_strpFindDropDown.DropDownItemClicked += new System.Windows.Forms.ToolStripItemClickedEventHandler(this.StrpDropDownDropDownItemClicked);
            // 
            // m_strpFindDropDown_QuickFindItem
            // 
            this.m_strpFindDropDown_QuickFindItem.Name = "m_strpFindDropDown_QuickFindItem";
            resources.ApplyResources(this.m_strpFindDropDown_QuickFindItem, "m_strpFindDropDown_QuickFindItem");
            // 
            // m_strpFindDropDown_FindInFilesItem
            // 
            this.m_strpFindDropDown_FindInFilesItem.Name = "m_strpFindDropDown_FindInFilesItem";
            resources.ApplyResources(this.m_strpFindDropDown_FindInFilesItem, "m_strpFindDropDown_FindInFilesItem");
            // 
            // m_strpReplaceDropDown
            // 
            this.m_strpReplaceDropDown.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.m_strpReplaceDropDown_QuickReplaceItem,
            this.m_strpReplaceDropDown_ReplaceInFilesItem});
            resources.ApplyResources(this.m_strpReplaceDropDown, "m_strpReplaceDropDown");
            this.m_strpReplaceDropDown.Name = "m_strpReplaceDropDown";
            this.m_strpReplaceDropDown.DropDownItemClicked += new System.Windows.Forms.ToolStripItemClickedEventHandler(this.StrpDropDownDropDownItemClicked);
            // 
            // m_strpReplaceDropDown_QuickReplaceItem
            // 
            this.m_strpReplaceDropDown_QuickReplaceItem.Name = "m_strpReplaceDropDown_QuickReplaceItem";
            resources.ApplyResources(this.m_strpReplaceDropDown_QuickReplaceItem, "m_strpReplaceDropDown_QuickReplaceItem");
            // 
            // m_strpReplaceDropDown_ReplaceInFilesItem
            // 
            this.m_strpReplaceDropDown_ReplaceInFilesItem.Name = "m_strpReplaceDropDown_ReplaceInFilesItem";
            resources.ApplyResources(this.m_strpReplaceDropDown_ReplaceInFilesItem, "m_strpReplaceDropDown_ReplaceInFilesItem");
            // 
            // SledFindAndReplaceForm
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.m_strpMain);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "SledFindAndReplaceForm";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.SledFindAndReplaceFormFormClosed);
            this.Load += new System.EventHandler(this.SledFindAndReplaceFormLoad);
            this.Shown += new System.EventHandler(this.SledFindAndReplaceFormShown);
            this.LocationChanged += new System.EventHandler(this.SledFindAndReplaceFormLocationChanged);
            this.m_strpMain.ResumeLayout(false);
            this.m_strpMain.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ToolStrip m_strpMain;
        private System.Windows.Forms.ToolStripDropDownButton m_strpFindDropDown;
        private System.Windows.Forms.ToolStripMenuItem m_strpFindDropDown_QuickFindItem;
        private System.Windows.Forms.ToolStripMenuItem m_strpFindDropDown_FindInFilesItem;
        private System.Windows.Forms.ToolStripDropDownButton m_strpReplaceDropDown;
        private System.Windows.Forms.ToolStripMenuItem m_strpReplaceDropDown_QuickReplaceItem;
        private System.Windows.Forms.ToolStripMenuItem m_strpReplaceDropDown_ReplaceInFilesItem;



    }
}
