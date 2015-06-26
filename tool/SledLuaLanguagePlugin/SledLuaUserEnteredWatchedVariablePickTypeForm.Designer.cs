/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

namespace Sce.Sled.Lua
{
    partial class SledLuaUserEnteredWatchedVariablePickTypeForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SledLuaUserEnteredWatchedVariablePickTypeForm));
            this.m_grpType = new System.Windows.Forms.GroupBox();
            this.m_rdoEnvironment = new System.Windows.Forms.RadioButton();
            this.m_rdoUpvalue = new System.Windows.Forms.RadioButton();
            this.m_rdoLocal = new System.Windows.Forms.RadioButton();
            this.m_rdoGlobal = new System.Windows.Forms.RadioButton();
            this.m_btnOK = new System.Windows.Forms.Button();
            this.m_btnCancel = new System.Windows.Forms.Button();
            this.m_grpType.SuspendLayout();
            this.SuspendLayout();
            // 
            // m_grpType
            // 
            resources.ApplyResources(this.m_grpType, "m_grpType");
            this.m_grpType.Controls.Add(this.m_rdoEnvironment);
            this.m_grpType.Controls.Add(this.m_rdoUpvalue);
            this.m_grpType.Controls.Add(this.m_rdoLocal);
            this.m_grpType.Controls.Add(this.m_rdoGlobal);
            this.m_grpType.Name = "m_grpType";
            this.m_grpType.TabStop = false;
            // 
            // m_rdoEnvironment
            // 
            resources.ApplyResources(this.m_rdoEnvironment, "m_rdoEnvironment");
            this.m_rdoEnvironment.Name = "m_rdoEnvironment";
            this.m_rdoEnvironment.TabStop = true;
            this.m_rdoEnvironment.UseVisualStyleBackColor = true;
            // 
            // m_rdoUpvalue
            // 
            resources.ApplyResources(this.m_rdoUpvalue, "m_rdoUpvalue");
            this.m_rdoUpvalue.Name = "m_rdoUpvalue";
            this.m_rdoUpvalue.TabStop = true;
            this.m_rdoUpvalue.UseVisualStyleBackColor = true;
            // 
            // m_rdoLocal
            // 
            resources.ApplyResources(this.m_rdoLocal, "m_rdoLocal");
            this.m_rdoLocal.Name = "m_rdoLocal";
            this.m_rdoLocal.TabStop = true;
            this.m_rdoLocal.UseVisualStyleBackColor = true;
            // 
            // m_rdoGlobal
            // 
            resources.ApplyResources(this.m_rdoGlobal, "m_rdoGlobal");
            this.m_rdoGlobal.Name = "m_rdoGlobal";
            this.m_rdoGlobal.TabStop = true;
            this.m_rdoGlobal.UseVisualStyleBackColor = true;
            // 
            // m_btnOK
            // 
            resources.ApplyResources(this.m_btnOK, "m_btnOK");
            this.m_btnOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.m_btnOK.Name = "m_btnOK";
            this.m_btnOK.UseVisualStyleBackColor = true;
            // 
            // m_btnCancel
            // 
            resources.ApplyResources(this.m_btnCancel, "m_btnCancel");
            this.m_btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.m_btnCancel.Name = "m_btnCancel";
            this.m_btnCancel.UseVisualStyleBackColor = true;
            // 
            // SledLuaUserEnteredWatchedVariablePickTypeForm
            // 
            this.AcceptButton = this.m_btnOK;
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.m_btnCancel;
            this.Controls.Add(this.m_btnCancel);
            this.Controls.Add(this.m_btnOK);
            this.Controls.Add(this.m_grpType);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "SledLuaUserEnteredWatchedVariablePickTypeForm";
            this.ShowInTaskbar = false;
            this.m_grpType.ResumeLayout(false);
            this.m_grpType.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox m_grpType;
        private System.Windows.Forms.RadioButton m_rdoEnvironment;
        private System.Windows.Forms.RadioButton m_rdoUpvalue;
        private System.Windows.Forms.RadioButton m_rdoLocal;
        private System.Windows.Forms.RadioButton m_rdoGlobal;
        private System.Windows.Forms.Button m_btnOK;
        private System.Windows.Forms.Button m_btnCancel;
    }
}
