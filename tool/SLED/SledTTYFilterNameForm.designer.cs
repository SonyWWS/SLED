/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

namespace Sce.Sled
{
    /// <summary>
    /// TTY Filter Name Form
    /// </summary>
    partial class SledTtyFilterNameForm
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

            if (disposing)
            {
                if (m_txtBrush != null)
                {
                    m_txtBrush.Dispose();
                    m_txtBrush = null;
                }

                if (m_bgBrush != null)
                {
                    m_bgBrush.Dispose();
                    m_bgBrush = null;
                }
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SledTtyFilterNameForm));
            this.m_btnCancel = new System.Windows.Forms.Button();
            this.m_btnOk = new System.Windows.Forms.Button();
            this.m_lblName = new System.Windows.Forms.Label();
            this.m_txtName = new System.Windows.Forms.TextBox();
            this.m_btnTxtColor = new System.Windows.Forms.Button();
            this.m_btnBgColor = new System.Windows.Forms.Button();
            this.m_rdoColorText = new System.Windows.Forms.RadioButton();
            this.m_rdoIgnoreText = new System.Windows.Forms.RadioButton();
            this.SuspendLayout();
            // 
            // m_btnCancel
            // 
            resources.ApplyResources(this.m_btnCancel, "m_btnCancel");
            this.m_btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.m_btnCancel.Name = "m_btnCancel";
            this.m_btnCancel.UseVisualStyleBackColor = true;
            // 
            // m_btnOk
            // 
            resources.ApplyResources(this.m_btnOk, "m_btnOk");
            this.m_btnOk.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.m_btnOk.Name = "m_btnOk";
            this.m_btnOk.UseVisualStyleBackColor = true;
            this.m_btnOk.Click += new System.EventHandler(this.BtnOk_Click);
            // 
            // m_lblName
            // 
            resources.ApplyResources(this.m_lblName, "m_lblName");
            this.m_lblName.Name = "m_lblName";
            // 
            // m_txtName
            // 
            resources.ApplyResources(this.m_txtName, "m_txtName");
            this.m_txtName.Name = "m_txtName";
            // 
            // m_btnTxtColor
            // 
            resources.ApplyResources(this.m_btnTxtColor, "m_btnTxtColor");
            this.m_btnTxtColor.Name = "m_btnTxtColor";
            this.m_btnTxtColor.UseVisualStyleBackColor = true;
            this.m_btnTxtColor.Click += new System.EventHandler(this.BtnTxtColor_Click);
            // 
            // m_btnBgColor
            // 
            resources.ApplyResources(this.m_btnBgColor, "m_btnBgColor");
            this.m_btnBgColor.Name = "m_btnBgColor";
            this.m_btnBgColor.UseVisualStyleBackColor = true;
            this.m_btnBgColor.Click += new System.EventHandler(this.BtnBgColor_Click);
            // 
            // m_rdoColorText
            // 
            resources.ApplyResources(this.m_rdoColorText, "m_rdoColorText");
            this.m_rdoColorText.Checked = true;
            this.m_rdoColorText.Name = "m_rdoColorText";
            this.m_rdoColorText.TabStop = true;
            this.m_rdoColorText.UseVisualStyleBackColor = true;
            this.m_rdoColorText.CheckedChanged += new System.EventHandler(this.RdoColorText_CheckedChanged);
            // 
            // m_rdoIgnoreText
            // 
            resources.ApplyResources(this.m_rdoIgnoreText, "m_rdoIgnoreText");
            this.m_rdoIgnoreText.Name = "m_rdoIgnoreText";
            this.m_rdoIgnoreText.UseVisualStyleBackColor = true;
            // 
            // SledTtyFilterNameForm
            // 
            this.AcceptButton = this.m_btnOk;
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.m_btnCancel;
            this.Controls.Add(this.m_rdoIgnoreText);
            this.Controls.Add(this.m_rdoColorText);
            this.Controls.Add(this.m_btnBgColor);
            this.Controls.Add(this.m_btnTxtColor);
            this.Controls.Add(this.m_btnCancel);
            this.Controls.Add(this.m_btnOk);
            this.Controls.Add(this.m_lblName);
            this.Controls.Add(this.m_txtName);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "SledTtyFilterNameForm";
            this.ShowInTaskbar = false;
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button m_btnCancel;
        private System.Windows.Forms.Button m_btnOk;
        private System.Windows.Forms.Label m_lblName;
        private System.Windows.Forms.TextBox m_txtName;
        private System.Windows.Forms.Button m_btnTxtColor;
        private System.Windows.Forms.Button m_btnBgColor;
        private System.Windows.Forms.RadioButton m_rdoColorText;
        private System.Windows.Forms.RadioButton m_rdoIgnoreText;
    }
}
