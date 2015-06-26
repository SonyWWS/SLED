/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

namespace Sce.Sled
{
    partial class SledBreakpointConditionForm
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
            if (disposing)
            {
                if ((m_txtCondition != null) && !m_bTxtConditionDisposed)
                {
                    m_txtCondition.Dispose();
                    m_bTxtConditionDisposed = true;
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SledBreakpointConditionForm));
            this.m_rdoIsTrue = new System.Windows.Forms.RadioButton();
            this.m_btnOk = new System.Windows.Forms.Button();
            this.m_btnCancel = new System.Windows.Forms.Button();
            this.m_chkCondition = new System.Windows.Forms.CheckBox();
            this.m_rdoIsFalse = new System.Windows.Forms.RadioButton();
            this.m_cmbEnvironment = new System.Windows.Forms.ComboBox();
            this.m_lblEnvironment = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // m_rdoIsTrue
            // 
            resources.ApplyResources(this.m_rdoIsTrue, "m_rdoIsTrue");
            this.m_rdoIsTrue.Checked = true;
            this.m_rdoIsTrue.Name = "m_rdoIsTrue";
            this.m_rdoIsTrue.TabStop = true;
            this.m_rdoIsTrue.UseVisualStyleBackColor = true;
            // 
            // m_btnOk
            // 
            resources.ApplyResources(this.m_btnOk, "m_btnOk");
            this.m_btnOk.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.m_btnOk.Name = "m_btnOk";
            this.m_btnOk.UseVisualStyleBackColor = true;
            this.m_btnOk.Click += new System.EventHandler(this.BtnOkClick);
            // 
            // m_btnCancel
            // 
            resources.ApplyResources(this.m_btnCancel, "m_btnCancel");
            this.m_btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.m_btnCancel.Name = "m_btnCancel";
            this.m_btnCancel.UseVisualStyleBackColor = true;
            // 
            // m_chkCondition
            // 
            resources.ApplyResources(this.m_chkCondition, "m_chkCondition");
            this.m_chkCondition.Checked = true;
            this.m_chkCondition.CheckState = System.Windows.Forms.CheckState.Checked;
            this.m_chkCondition.Name = "m_chkCondition";
            this.m_chkCondition.UseVisualStyleBackColor = true;
            this.m_chkCondition.CheckStateChanged += new System.EventHandler(this.ChkConditionCheckStateChanged);
            // 
            // m_rdoIsFalse
            // 
            resources.ApplyResources(this.m_rdoIsFalse, "m_rdoIsFalse");
            this.m_rdoIsFalse.Name = "m_rdoIsFalse";
            this.m_rdoIsFalse.TabStop = true;
            this.m_rdoIsFalse.UseVisualStyleBackColor = true;
            // 
            // m_cmbEnvironment
            // 
            resources.ApplyResources(this.m_cmbEnvironment, "m_cmbEnvironment");
            this.m_cmbEnvironment.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.m_cmbEnvironment.FormattingEnabled = true;
            this.m_cmbEnvironment.Name = "m_cmbEnvironment";
            // 
            // m_lblEnvironment
            // 
            resources.ApplyResources(this.m_lblEnvironment, "m_lblEnvironment");
            this.m_lblEnvironment.Name = "m_lblEnvironment";
            // 
            // SledBreakpointConditionForm
            // 
            this.AcceptButton = this.m_btnOk;
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.m_btnCancel;
            this.Controls.Add(this.m_lblEnvironment);
            this.Controls.Add(this.m_cmbEnvironment);
            this.Controls.Add(this.m_rdoIsFalse);
            this.Controls.Add(this.m_chkCondition);
            this.Controls.Add(this.m_btnCancel);
            this.Controls.Add(this.m_btnOk);
            this.Controls.Add(this.m_rdoIsTrue);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "SledBreakpointConditionForm";
            this.ShowInTaskbar = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.RadioButton m_rdoIsTrue;
        private System.Windows.Forms.Button m_btnOk;
        private System.Windows.Forms.Button m_btnCancel;
        private System.Windows.Forms.CheckBox m_chkCondition;
        private System.Windows.Forms.RadioButton m_rdoIsFalse;
        private System.Windows.Forms.ComboBox m_cmbEnvironment;
        private System.Windows.Forms.Label m_lblEnvironment;
    }
}
