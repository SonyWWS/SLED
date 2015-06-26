/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

namespace Sce.Sled
{
    partial class SledProjectModifiedForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SledProjectModifiedForm));
            this.label1 = new System.Windows.Forms.Label();
            this.m_lstChanges = new System.Windows.Forms.ListView();
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.m_btnAcceptSelected = new System.Windows.Forms.Button();
            this.m_btnIgnoreSelected = new System.Windows.Forms.Button();
            this.m_btnSubmit = new System.Windows.Forms.Button();
            this.m_btnAcceptAll = new System.Windows.Forms.Button();
            this.m_btnIgnoreAll = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label1.Location = new System.Drawing.Point(12, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(660, 30);
            this.label1.TabIndex = 0;
            this.label1.Text = resources.GetString("label1.Text");
            // 
            // m_lstChanges
            // 
            this.m_lstChanges.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.m_lstChanges.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1});
            this.m_lstChanges.Location = new System.Drawing.Point(12, 42);
            this.m_lstChanges.Name = "m_lstChanges";
            this.m_lstChanges.OwnerDraw = true;
            this.m_lstChanges.Size = new System.Drawing.Size(660, 272);
            this.m_lstChanges.TabIndex = 1;
            this.m_lstChanges.UseCompatibleStateImageBehavior = false;
            this.m_lstChanges.View = System.Windows.Forms.View.Details;
            this.m_lstChanges.DrawColumnHeader += new System.Windows.Forms.DrawListViewColumnHeaderEventHandler(this.LstChangesDrawColumnHeader);
            this.m_lstChanges.DrawItem += new System.Windows.Forms.DrawListViewItemEventHandler(this.LstChangesDrawItem);
            this.m_lstChanges.DrawSubItem += new System.Windows.Forms.DrawListViewSubItemEventHandler(this.LstChangesDrawSubItem);
            this.m_lstChanges.SelectedIndexChanged += new System.EventHandler(this.LstChangesSelectedIndexChanged);
            this.m_lstChanges.KeyUp += new System.Windows.Forms.KeyEventHandler(this.LstChangesKeyUp);
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "Changes";
            // 
            // m_btnAcceptSelected
            // 
            this.m_btnAcceptSelected.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.m_btnAcceptSelected.Location = new System.Drawing.Point(128, 327);
            this.m_btnAcceptSelected.Name = "m_btnAcceptSelected";
            this.m_btnAcceptSelected.Size = new System.Drawing.Size(110, 23);
            this.m_btnAcceptSelected.TabIndex = 3;
            this.m_btnAcceptSelected.Text = "Accept Selected";
            this.m_btnAcceptSelected.UseVisualStyleBackColor = true;
            this.m_btnAcceptSelected.Click += new System.EventHandler(this.BtnAcceptSelectedClick);
            // 
            // m_btnIgnoreSelected
            // 
            this.m_btnIgnoreSelected.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.m_btnIgnoreSelected.Location = new System.Drawing.Point(244, 327);
            this.m_btnIgnoreSelected.Name = "m_btnIgnoreSelected";
            this.m_btnIgnoreSelected.Size = new System.Drawing.Size(110, 23);
            this.m_btnIgnoreSelected.TabIndex = 4;
            this.m_btnIgnoreSelected.Text = "Ignore Selected";
            this.m_btnIgnoreSelected.UseVisualStyleBackColor = true;
            this.m_btnIgnoreSelected.Click += new System.EventHandler(this.BtnIgnoreSelectedClick);
            // 
            // m_btnSubmit
            // 
            this.m_btnSubmit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.m_btnSubmit.Location = new System.Drawing.Point(562, 327);
            this.m_btnSubmit.Name = "m_btnSubmit";
            this.m_btnSubmit.Size = new System.Drawing.Size(110, 23);
            this.m_btnSubmit.TabIndex = 6;
            this.m_btnSubmit.Text = "Submit Changes";
            this.m_btnSubmit.UseVisualStyleBackColor = true;
            this.m_btnSubmit.Click += new System.EventHandler(this.BtnSubmitClick);
            // 
            // m_btnAcceptAll
            // 
            this.m_btnAcceptAll.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.m_btnAcceptAll.Location = new System.Drawing.Point(12, 327);
            this.m_btnAcceptAll.Name = "m_btnAcceptAll";
            this.m_btnAcceptAll.Size = new System.Drawing.Size(110, 23);
            this.m_btnAcceptAll.TabIndex = 2;
            this.m_btnAcceptAll.Text = "Accept All";
            this.m_btnAcceptAll.UseVisualStyleBackColor = true;
            this.m_btnAcceptAll.Click += new System.EventHandler(this.BtnAcceptAllClick);
            // 
            // m_btnIgnoreAll
            // 
            this.m_btnIgnoreAll.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.m_btnIgnoreAll.Location = new System.Drawing.Point(360, 327);
            this.m_btnIgnoreAll.Name = "m_btnIgnoreAll";
            this.m_btnIgnoreAll.Size = new System.Drawing.Size(110, 23);
            this.m_btnIgnoreAll.TabIndex = 5;
            this.m_btnIgnoreAll.Text = "Ignore All";
            this.m_btnIgnoreAll.UseVisualStyleBackColor = true;
            this.m_btnIgnoreAll.Click += new System.EventHandler(this.BtnIgnoreAllClick);
            // 
            // SledProjectModifiedForm
            // 
            this.AcceptButton = this.m_btnSubmit;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(684, 362);
            this.Controls.Add(this.m_btnIgnoreAll);
            this.Controls.Add(this.m_btnAcceptAll);
            this.Controls.Add(this.m_btnSubmit);
            this.Controls.Add(this.m_btnIgnoreSelected);
            this.Controls.Add(this.m_btnAcceptSelected);
            this.Controls.Add(this.m_lstChanges);
            this.Controls.Add(this.label1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimumSize = new System.Drawing.Size(640, 278);
            this.Name = "SledProjectModifiedForm";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "SLED - Project Modified";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.SledProjectModifiedFormFormClosing);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ListView m_lstChanges;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.Button m_btnAcceptSelected;
        private System.Windows.Forms.Button m_btnIgnoreSelected;
        private System.Windows.Forms.Button m_btnSubmit;
        private System.Windows.Forms.Button m_btnAcceptAll;
        private System.Windows.Forms.Button m_btnIgnoreAll;
    }
}
