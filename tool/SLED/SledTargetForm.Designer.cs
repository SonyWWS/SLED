/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

namespace Sce.Sled
{
    partial class SledTargetForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SledTargetForm));
            this.m_grpCommonSettings = new System.Windows.Forms.GroupBox();
            this.m_cmbProtocol = new System.Windows.Forms.ComboBox();
            this.m_lblProtocol = new System.Windows.Forms.Label();
            this.m_txtPort = new System.Windows.Forms.TextBox();
            this.m_lblPort = new System.Windows.Forms.Label();
            this.m_txtHost = new System.Windows.Forms.TextBox();
            this.m_lblHost = new System.Windows.Forms.Label();
            this.m_txtName = new System.Windows.Forms.TextBox();
            this.m_lblName = new System.Windows.Forms.Label();
            this.m_grpProtocolSettings = new System.Windows.Forms.GroupBox();
            this.m_btnOK = new System.Windows.Forms.Button();
            this.m_btnCancel = new System.Windows.Forms.Button();
            this.m_grpCommonSettings.SuspendLayout();
            this.SuspendLayout();
            // 
            // m_grpCommonSettings
            // 
            resources.ApplyResources(this.m_grpCommonSettings, "m_grpCommonSettings");
            this.m_grpCommonSettings.Controls.Add(this.m_cmbProtocol);
            this.m_grpCommonSettings.Controls.Add(this.m_lblProtocol);
            this.m_grpCommonSettings.Controls.Add(this.m_txtPort);
            this.m_grpCommonSettings.Controls.Add(this.m_lblPort);
            this.m_grpCommonSettings.Controls.Add(this.m_txtHost);
            this.m_grpCommonSettings.Controls.Add(this.m_lblHost);
            this.m_grpCommonSettings.Controls.Add(this.m_txtName);
            this.m_grpCommonSettings.Controls.Add(this.m_lblName);
            this.m_grpCommonSettings.Name = "m_grpCommonSettings";
            this.m_grpCommonSettings.TabStop = false;
            // 
            // m_cmbProtocol
            // 
            resources.ApplyResources(this.m_cmbProtocol, "m_cmbProtocol");
            this.m_cmbProtocol.FormattingEnabled = true;
            this.m_cmbProtocol.Name = "m_cmbProtocol";
            this.m_cmbProtocol.SelectedIndexChanged += new System.EventHandler(this.CmbProtocolSelectedIndexChanged);
            // 
            // m_lblProtocol
            // 
            resources.ApplyResources(this.m_lblProtocol, "m_lblProtocol");
            this.m_lblProtocol.Name = "m_lblProtocol";
            // 
            // m_txtPort
            // 
            resources.ApplyResources(this.m_txtPort, "m_txtPort");
            this.m_txtPort.Name = "m_txtPort";
            // 
            // m_lblPort
            // 
            resources.ApplyResources(this.m_lblPort, "m_lblPort");
            this.m_lblPort.Name = "m_lblPort";
            // 
            // m_txtHost
            // 
            resources.ApplyResources(this.m_txtHost, "m_txtHost");
            this.m_txtHost.Name = "m_txtHost";
            // 
            // m_lblHost
            // 
            resources.ApplyResources(this.m_lblHost, "m_lblHost");
            this.m_lblHost.Name = "m_lblHost";
            // 
            // m_txtName
            // 
            resources.ApplyResources(this.m_txtName, "m_txtName");
            this.m_txtName.Name = "m_txtName";
            // 
            // m_lblName
            // 
            resources.ApplyResources(this.m_lblName, "m_lblName");
            this.m_lblName.Name = "m_lblName";
            // 
            // m_grpProtocolSettings
            // 
            resources.ApplyResources(this.m_grpProtocolSettings, "m_grpProtocolSettings");
            this.m_grpProtocolSettings.Name = "m_grpProtocolSettings";
            this.m_grpProtocolSettings.TabStop = false;
            // 
            // m_btnOK
            // 
            resources.ApplyResources(this.m_btnOK, "m_btnOK");
            this.m_btnOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.m_btnOK.Name = "m_btnOK";
            this.m_btnOK.UseVisualStyleBackColor = true;
            this.m_btnOK.Click += new System.EventHandler(this.BtnOkClick);
            // 
            // m_btnCancel
            // 
            resources.ApplyResources(this.m_btnCancel, "m_btnCancel");
            this.m_btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.m_btnCancel.Name = "m_btnCancel";
            this.m_btnCancel.UseVisualStyleBackColor = true;
            // 
            // SledTargetForm
            // 
            this.AcceptButton = this.m_btnOK;
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.m_btnCancel;
            this.Controls.Add(this.m_btnCancel);
            this.Controls.Add(this.m_btnOK);
            this.Controls.Add(this.m_grpProtocolSettings);
            this.Controls.Add(this.m_grpCommonSettings);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "SledTargetForm";
            this.m_grpCommonSettings.ResumeLayout(false);
            this.m_grpCommonSettings.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox m_grpCommonSettings;
        private System.Windows.Forms.Label m_lblName;
        private System.Windows.Forms.TextBox m_txtPort;
        private System.Windows.Forms.Label m_lblPort;
        private System.Windows.Forms.TextBox m_txtHost;
        private System.Windows.Forms.Label m_lblHost;
        private System.Windows.Forms.TextBox m_txtName;
        private System.Windows.Forms.Label m_lblProtocol;
        private System.Windows.Forms.ComboBox m_cmbProtocol;
        private System.Windows.Forms.GroupBox m_grpProtocolSettings;
        private System.Windows.Forms.Button m_btnOK;
        private System.Windows.Forms.Button m_btnCancel;
    }
}
