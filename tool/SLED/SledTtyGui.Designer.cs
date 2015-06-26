/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

namespace Sce.Sled
{
    partial class SledTtyGui
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.m_split = new System.Windows.Forms.SplitContainer();
            this.m_lstOutput = new Sce.Sled.SledTtyListView();
            this.m_cmbLanguages = new System.Windows.Forms.ComboBox();
            this.m_btnSend = new System.Windows.Forms.Button();
            this.m_txtInput = new System.Windows.Forms.TextBox();
            this.m_split.Panel1.SuspendLayout();
            this.m_split.Panel2.SuspendLayout();
            this.m_split.SuspendLayout();
            this.SuspendLayout();
            // 
            // m_split
            // 
            this.m_split.Dock = System.Windows.Forms.DockStyle.Fill;
            this.m_split.Location = new System.Drawing.Point(0, 0);
            this.m_split.Name = "m_split";
            this.m_split.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // m_split.Panel1
            // 
            this.m_split.Panel1.Controls.Add(this.m_lstOutput);
            // 
            // m_split.Panel2
            // 
            this.m_split.Panel2.Controls.Add(this.m_cmbLanguages);
            this.m_split.Panel2.Controls.Add(this.m_btnSend);
            this.m_split.Panel2.Controls.Add(this.m_txtInput);
            this.m_split.Size = new System.Drawing.Size(393, 266);
            this.m_split.SplitterDistance = 205;
            this.m_split.TabIndex = 0;
            // 
            // m_lstOutput
            // 
            this.m_lstOutput.Dock = System.Windows.Forms.DockStyle.Fill;
            this.m_lstOutput.FullRowSelect = true;
            this.m_lstOutput.GridLines = true;
            this.m_lstOutput.Location = new System.Drawing.Point(0, 0);
            this.m_lstOutput.Name = "m_lstOutput";
            this.m_lstOutput.OwnerDraw = true;
            this.m_lstOutput.Size = new System.Drawing.Size(393, 205);
            this.m_lstOutput.TabIndex = 0;
            this.m_lstOutput.UseCompatibleStateImageBehavior = false;
            this.m_lstOutput.View = System.Windows.Forms.View.Details;
            this.m_lstOutput.VirtualMode = true;
            this.m_lstOutput.ColumnWidthChanged += new System.Windows.Forms.ColumnWidthChangedEventHandler(this.LstOutputColumnWidthChanged);
            this.m_lstOutput.RetrieveVirtualItem += new System.Windows.Forms.RetrieveVirtualItemEventHandler(this.LstOutputRetrieveVirtualItem);
            // 
            // m_cmbLanguages
            // 
            this.m_cmbLanguages.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.m_cmbLanguages.FormattingEnabled = true;
            this.m_cmbLanguages.Location = new System.Drawing.Point(295, 33);
            this.m_cmbLanguages.Name = "m_cmbLanguages";
            this.m_cmbLanguages.Size = new System.Drawing.Size(95, 21);
            this.m_cmbLanguages.TabIndex = 2;
            // 
            // m_btnSend
            // 
            this.m_btnSend.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.m_btnSend.Location = new System.Drawing.Point(295, 3);
            this.m_btnSend.Name = "m_btnSend";
            this.m_btnSend.Size = new System.Drawing.Size(95, 24);
            this.m_btnSend.TabIndex = 1;
            this.m_btnSend.Text = "Send";
            this.m_btnSend.UseVisualStyleBackColor = true;
            this.m_btnSend.Click += new System.EventHandler(this.BtnSendClick);
            // 
            // m_txtInput
            // 
            this.m_txtInput.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.m_txtInput.Location = new System.Drawing.Point(3, 2);
            this.m_txtInput.Multiline = true;
            this.m_txtInput.Name = "m_txtInput";
            this.m_txtInput.Size = new System.Drawing.Size(286, 52);
            this.m_txtInput.TabIndex = 0;
            this.m_txtInput.KeyUp += new System.Windows.Forms.KeyEventHandler(this.TxtInputKeyUp);
            // 
            // SledTtyGui
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.m_split);
            this.Name = "SledTtyGui";
            this.Size = new System.Drawing.Size(393, 266);
            this.m_split.Panel1.ResumeLayout(false);
            this.m_split.Panel2.ResumeLayout(false);
            this.m_split.Panel2.PerformLayout();
            this.m_split.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer m_split;
        private Sce.Sled.SledTtyListView m_lstOutput;
        private System.Windows.Forms.TextBox m_txtInput;
        private System.Windows.Forms.ComboBox m_cmbLanguages;
        private System.Windows.Forms.Button m_btnSend;
    }
}
