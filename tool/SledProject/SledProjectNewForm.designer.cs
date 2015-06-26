/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

namespace Sce.Sled.Project
{
    partial class SledProjectNewForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SledProjectNewForm));
            this.m_btnCreate = new System.Windows.Forms.Button();
            this.m_btnCancel = new System.Windows.Forms.Button();
            this.m_txtProjectName = new System.Windows.Forms.TextBox();
            this.m_txtProjectDir = new System.Windows.Forms.TextBox();
            this.m_btnSelectDir = new System.Windows.Forms.Button();
            this.m_grpProjectDir = new System.Windows.Forms.GroupBox();
            this.m_btnProjPaste = new System.Windows.Forms.Button();
            this.m_btnProjCopy = new System.Windows.Forms.Button();
            this.m_grpAssetDir = new System.Windows.Forms.GroupBox();
            this.m_btnAssetPaste = new System.Windows.Forms.Button();
            this.m_btnAssetCopy = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.m_chkAssetDir = new System.Windows.Forms.CheckBox();
            this.m_txtAssetDir = new System.Windows.Forms.TextBox();
            this.m_btnSelectAssetDir = new System.Windows.Forms.Button();
            this.m_chkRecursiveAdd = new System.Windows.Forms.CheckBox();
            this.m_grpRecursiveAdd = new System.Windows.Forms.GroupBox();
            this.m_grpOutput = new System.Windows.Forms.GroupBox();
            this.m_txtProjectOutput = new System.Windows.Forms.TextBox();
            this.m_grpProjectName = new System.Windows.Forms.GroupBox();
            this.m_grpProjectDir.SuspendLayout();
            this.m_grpAssetDir.SuspendLayout();
            this.m_grpRecursiveAdd.SuspendLayout();
            this.m_grpOutput.SuspendLayout();
            this.m_grpProjectName.SuspendLayout();
            this.SuspendLayout();
            // 
            // m_btnCreate
            // 
            resources.ApplyResources(this.m_btnCreate, "m_btnCreate");
            this.m_btnCreate.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.m_btnCreate.Name = "m_btnCreate";
            this.m_btnCreate.UseVisualStyleBackColor = true;
            this.m_btnCreate.Click += new System.EventHandler(this.BtnCreate_Click);
            // 
            // m_btnCancel
            // 
            resources.ApplyResources(this.m_btnCancel, "m_btnCancel");
            this.m_btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.m_btnCancel.Name = "m_btnCancel";
            this.m_btnCancel.UseVisualStyleBackColor = true;
            // 
            // m_txtProjectName
            // 
            resources.ApplyResources(this.m_txtProjectName, "m_txtProjectName");
            this.m_txtProjectName.Name = "m_txtProjectName";
            this.m_txtProjectName.TextChanged += new System.EventHandler(this.TxtProjectName_TextChanged);
            // 
            // m_txtProjectDir
            // 
            resources.ApplyResources(this.m_txtProjectDir, "m_txtProjectDir");
            this.m_txtProjectDir.Name = "m_txtProjectDir";
            this.m_txtProjectDir.TextChanged += new System.EventHandler(this.TxtProjectDir_TextChanged);
            // 
            // m_btnSelectDir
            // 
            resources.ApplyResources(this.m_btnSelectDir, "m_btnSelectDir");
            this.m_btnSelectDir.Name = "m_btnSelectDir";
            this.m_btnSelectDir.UseVisualStyleBackColor = true;
            this.m_btnSelectDir.Click += new System.EventHandler(this.BtnSelectDir_Click);
            // 
            // m_grpProjectDir
            // 
            resources.ApplyResources(this.m_grpProjectDir, "m_grpProjectDir");
            this.m_grpProjectDir.Controls.Add(this.m_btnProjPaste);
            this.m_grpProjectDir.Controls.Add(this.m_btnProjCopy);
            this.m_grpProjectDir.Controls.Add(this.m_txtProjectDir);
            this.m_grpProjectDir.Controls.Add(this.m_btnSelectDir);
            this.m_grpProjectDir.Name = "m_grpProjectDir";
            this.m_grpProjectDir.TabStop = false;
            // 
            // m_btnProjPaste
            // 
            resources.ApplyResources(this.m_btnProjPaste, "m_btnProjPaste");
            this.m_btnProjPaste.Name = "m_btnProjPaste";
            this.m_btnProjPaste.UseVisualStyleBackColor = true;
            this.m_btnProjPaste.Click += new System.EventHandler(this.BtnProjPaste_Click);
            // 
            // m_btnProjCopy
            // 
            resources.ApplyResources(this.m_btnProjCopy, "m_btnProjCopy");
            this.m_btnProjCopy.Name = "m_btnProjCopy";
            this.m_btnProjCopy.UseVisualStyleBackColor = true;
            this.m_btnProjCopy.Click += new System.EventHandler(this.BtnProjCopy_Click);
            // 
            // m_grpAssetDir
            // 
            resources.ApplyResources(this.m_grpAssetDir, "m_grpAssetDir");
            this.m_grpAssetDir.Controls.Add(this.m_btnAssetPaste);
            this.m_grpAssetDir.Controls.Add(this.m_btnAssetCopy);
            this.m_grpAssetDir.Controls.Add(this.label1);
            this.m_grpAssetDir.Controls.Add(this.m_chkAssetDir);
            this.m_grpAssetDir.Controls.Add(this.m_txtAssetDir);
            this.m_grpAssetDir.Controls.Add(this.m_btnSelectAssetDir);
            this.m_grpAssetDir.Name = "m_grpAssetDir";
            this.m_grpAssetDir.TabStop = false;
            // 
            // m_btnAssetPaste
            // 
            resources.ApplyResources(this.m_btnAssetPaste, "m_btnAssetPaste");
            this.m_btnAssetPaste.Name = "m_btnAssetPaste";
            this.m_btnAssetPaste.UseVisualStyleBackColor = true;
            this.m_btnAssetPaste.Click += new System.EventHandler(this.BtnAssetPaste_Click);
            // 
            // m_btnAssetCopy
            // 
            resources.ApplyResources(this.m_btnAssetCopy, "m_btnAssetCopy");
            this.m_btnAssetCopy.Name = "m_btnAssetCopy";
            this.m_btnAssetCopy.UseVisualStyleBackColor = true;
            this.m_btnAssetCopy.Click += new System.EventHandler(this.BtnAssetCopy_Click);
            // 
            // label1
            // 
            resources.ApplyResources(this.label1, "label1");
            this.label1.Name = "label1";
            // 
            // m_chkAssetDir
            // 
            resources.ApplyResources(this.m_chkAssetDir, "m_chkAssetDir");
            this.m_chkAssetDir.Name = "m_chkAssetDir";
            this.m_chkAssetDir.UseVisualStyleBackColor = true;
            this.m_chkAssetDir.CheckedChanged += new System.EventHandler(this.ChkAssetDir_CheckedChanged);
            // 
            // m_txtAssetDir
            // 
            resources.ApplyResources(this.m_txtAssetDir, "m_txtAssetDir");
            this.m_txtAssetDir.Name = "m_txtAssetDir";
            this.m_txtAssetDir.TextChanged += new System.EventHandler(this.TxtAssetDir_TextChanged);
            // 
            // m_btnSelectAssetDir
            // 
            resources.ApplyResources(this.m_btnSelectAssetDir, "m_btnSelectAssetDir");
            this.m_btnSelectAssetDir.Name = "m_btnSelectAssetDir";
            this.m_btnSelectAssetDir.UseVisualStyleBackColor = true;
            this.m_btnSelectAssetDir.Click += new System.EventHandler(this.BtnSelectAssetDir_Click);
            // 
            // m_chkRecursiveAdd
            // 
            resources.ApplyResources(this.m_chkRecursiveAdd, "m_chkRecursiveAdd");
            this.m_chkRecursiveAdd.Name = "m_chkRecursiveAdd";
            this.m_chkRecursiveAdd.UseVisualStyleBackColor = true;
            // 
            // m_grpRecursiveAdd
            // 
            resources.ApplyResources(this.m_grpRecursiveAdd, "m_grpRecursiveAdd");
            this.m_grpRecursiveAdd.Controls.Add(this.m_chkRecursiveAdd);
            this.m_grpRecursiveAdd.Name = "m_grpRecursiveAdd";
            this.m_grpRecursiveAdd.TabStop = false;
            // 
            // m_grpOutput
            // 
            resources.ApplyResources(this.m_grpOutput, "m_grpOutput");
            this.m_grpOutput.Controls.Add(this.m_txtProjectOutput);
            this.m_grpOutput.Name = "m_grpOutput";
            this.m_grpOutput.TabStop = false;
            // 
            // m_txtProjectOutput
            // 
            resources.ApplyResources(this.m_txtProjectOutput, "m_txtProjectOutput");
            this.m_txtProjectOutput.Name = "m_txtProjectOutput";
            this.m_txtProjectOutput.ReadOnly = true;
            // 
            // m_grpProjectName
            // 
            resources.ApplyResources(this.m_grpProjectName, "m_grpProjectName");
            this.m_grpProjectName.Controls.Add(this.m_txtProjectName);
            this.m_grpProjectName.Controls.Add(this.m_btnCancel);
            this.m_grpProjectName.Controls.Add(this.m_btnCreate);
            this.m_grpProjectName.Name = "m_grpProjectName";
            this.m_grpProjectName.TabStop = false;
            // 
            // SledProjectNewForm
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.m_grpProjectName);
            this.Controls.Add(this.m_grpOutput);
            this.Controls.Add(this.m_grpRecursiveAdd);
            this.Controls.Add(this.m_grpAssetDir);
            this.Controls.Add(this.m_grpProjectDir);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "SledProjectNewForm";
            this.ShowInTaskbar = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.m_grpProjectDir.ResumeLayout(false);
            this.m_grpProjectDir.PerformLayout();
            this.m_grpAssetDir.ResumeLayout(false);
            this.m_grpAssetDir.PerformLayout();
            this.m_grpRecursiveAdd.ResumeLayout(false);
            this.m_grpRecursiveAdd.PerformLayout();
            this.m_grpOutput.ResumeLayout(false);
            this.m_grpOutput.PerformLayout();
            this.m_grpProjectName.ResumeLayout(false);
            this.m_grpProjectName.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button m_btnCreate;
        private System.Windows.Forms.Button m_btnCancel;
        private System.Windows.Forms.TextBox m_txtProjectName;
        private System.Windows.Forms.TextBox m_txtProjectDir;
        private System.Windows.Forms.Button m_btnSelectDir;
        private System.Windows.Forms.GroupBox m_grpProjectDir;
        private System.Windows.Forms.GroupBox m_grpAssetDir;
        private System.Windows.Forms.CheckBox m_chkAssetDir;
        private System.Windows.Forms.TextBox m_txtAssetDir;
        private System.Windows.Forms.Button m_btnSelectAssetDir;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.CheckBox m_chkRecursiveAdd;
        private System.Windows.Forms.GroupBox m_grpRecursiveAdd;
        private System.Windows.Forms.GroupBox m_grpOutput;
        private System.Windows.Forms.TextBox m_txtProjectOutput;
        private System.Windows.Forms.GroupBox m_grpProjectName;
        private System.Windows.Forms.Button m_btnProjPaste;
        private System.Windows.Forms.Button m_btnProjCopy;
        private System.Windows.Forms.Button m_btnAssetPaste;
        private System.Windows.Forms.Button m_btnAssetCopy;
    }
}
