/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

namespace Sce.Sled
{
    partial class SledFindAndReplaceForm_QuickReplace
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SledFindAndReplaceForm_QuickReplace));
            this.m_grpTitle = new System.Windows.Forms.GroupBox();
            this.m_btnReplaceAll = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.m_btnFindNext = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            this.m_btnReplace = new System.Windows.Forms.Button();
            this.m_grpFindOptions = new Sce.Sled.Shared.Controls.SledCollapsibleGroupBox();
            this.m_chkSearchUp = new System.Windows.Forms.CheckBox();
            this.m_cmbUse = new System.Windows.Forms.ComboBox();
            this.m_chkUse = new System.Windows.Forms.CheckBox();
            this.m_chkMatchWholeWord = new System.Windows.Forms.CheckBox();
            this.m_chkMatchCase = new System.Windows.Forms.CheckBox();
            this.m_cmbLookIn = new System.Windows.Forms.ComboBox();
            this.m_lblLookIn = new System.Windows.Forms.Label();
            this.m_cmbReplaceWith = new System.Windows.Forms.ComboBox();
            this.m_lblReplaceWith = new System.Windows.Forms.Label();
            this.m_cmbFindWhat = new System.Windows.Forms.ComboBox();
            this.m_lblFindWhat = new System.Windows.Forms.Label();
            this.m_grpTitle.SuspendLayout();
            this.m_grpFindOptions.SuspendLayout();
            this.SuspendLayout();
            // 
            // m_grpTitle
            // 
            resources.ApplyResources(this.m_grpTitle, "m_grpTitle");
            this.m_grpTitle.Controls.Add(this.m_btnReplaceAll);
            this.m_grpTitle.Controls.Add(this.button2);
            this.m_grpTitle.Controls.Add(this.m_btnFindNext);
            this.m_grpTitle.Controls.Add(this.button1);
            this.m_grpTitle.Controls.Add(this.m_btnReplace);
            this.m_grpTitle.Controls.Add(this.m_grpFindOptions);
            this.m_grpTitle.Controls.Add(this.m_cmbLookIn);
            this.m_grpTitle.Controls.Add(this.m_lblLookIn);
            this.m_grpTitle.Controls.Add(this.m_cmbReplaceWith);
            this.m_grpTitle.Controls.Add(this.m_lblReplaceWith);
            this.m_grpTitle.Controls.Add(this.m_cmbFindWhat);
            this.m_grpTitle.Controls.Add(this.m_lblFindWhat);
            this.m_grpTitle.Name = "m_grpTitle";
            this.m_grpTitle.TabStop = false;
            // 
            // m_btnReplaceAll
            // 
            resources.ApplyResources(this.m_btnReplaceAll, "m_btnReplaceAll");
            this.m_btnReplaceAll.Name = "m_btnReplaceAll";
            this.m_btnReplaceAll.UseVisualStyleBackColor = true;
            this.m_btnReplaceAll.Click += new System.EventHandler(this.BtnReplaceAllClick);
            // 
            // button2
            // 
            resources.ApplyResources(this.button2, "button2");
            this.button2.Name = "button2";
            this.button2.UseVisualStyleBackColor = true;
            // 
            // m_btnFindNext
            // 
            resources.ApplyResources(this.m_btnFindNext, "m_btnFindNext");
            this.m_btnFindNext.Name = "m_btnFindNext";
            this.m_btnFindNext.UseVisualStyleBackColor = true;
            this.m_btnFindNext.Click += new System.EventHandler(this.BtnFindNextClick);
            // 
            // button1
            // 
            resources.ApplyResources(this.button1, "button1");
            this.button1.Name = "button1";
            this.button1.UseVisualStyleBackColor = true;
            // 
            // m_btnReplace
            // 
            resources.ApplyResources(this.m_btnReplace, "m_btnReplace");
            this.m_btnReplace.Name = "m_btnReplace";
            this.m_btnReplace.UseVisualStyleBackColor = true;
            this.m_btnReplace.Click += new System.EventHandler(this.BtnReplaceClick);
            // 
            // m_grpFindOptions
            // 
            this.m_grpFindOptions.Controls.Add(this.m_chkSearchUp);
            this.m_grpFindOptions.Controls.Add(this.m_cmbUse);
            this.m_grpFindOptions.Controls.Add(this.m_chkUse);
            this.m_grpFindOptions.Controls.Add(this.m_chkMatchWholeWord);
            this.m_grpFindOptions.Controls.Add(this.m_chkMatchCase);
            resources.ApplyResources(this.m_grpFindOptions, "m_grpFindOptions");
            this.m_grpFindOptions.Name = "m_grpFindOptions";
            this.m_grpFindOptions.TabStop = false;
            // 
            // m_chkSearchUp
            // 
            resources.ApplyResources(this.m_chkSearchUp, "m_chkSearchUp");
            this.m_chkSearchUp.Name = "m_chkSearchUp";
            this.m_chkSearchUp.UseVisualStyleBackColor = true;
            this.m_chkSearchUp.CheckedChanged += new System.EventHandler(this.ChkSearchUpCheckedChanged);
            // 
            // m_cmbUse
            // 
            this.m_cmbUse.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            resources.ApplyResources(this.m_cmbUse, "m_cmbUse");
            this.m_cmbUse.FormattingEnabled = true;
            this.m_cmbUse.Name = "m_cmbUse";
            this.m_cmbUse.SelectedIndexChanged += new System.EventHandler(this.CmbUseSelectedIndexChanged);
            // 
            // m_chkUse
            // 
            resources.ApplyResources(this.m_chkUse, "m_chkUse");
            this.m_chkUse.Name = "m_chkUse";
            this.m_chkUse.UseVisualStyleBackColor = true;
            this.m_chkUse.CheckedChanged += new System.EventHandler(this.ChkUseCheckedChanged);
            // 
            // m_chkMatchWholeWord
            // 
            resources.ApplyResources(this.m_chkMatchWholeWord, "m_chkMatchWholeWord");
            this.m_chkMatchWholeWord.Name = "m_chkMatchWholeWord";
            this.m_chkMatchWholeWord.UseVisualStyleBackColor = true;
            this.m_chkMatchWholeWord.CheckedChanged += new System.EventHandler(this.ChkMatchWholeWordCheckedChanged);
            // 
            // m_chkMatchCase
            // 
            resources.ApplyResources(this.m_chkMatchCase, "m_chkMatchCase");
            this.m_chkMatchCase.Name = "m_chkMatchCase";
            this.m_chkMatchCase.UseVisualStyleBackColor = true;
            this.m_chkMatchCase.CheckedChanged += new System.EventHandler(this.ChkMatchCaseCheckedChanged);
            // 
            // m_cmbLookIn
            // 
            this.m_cmbLookIn.FormattingEnabled = true;
            resources.ApplyResources(this.m_cmbLookIn, "m_cmbLookIn");
            this.m_cmbLookIn.Name = "m_cmbLookIn";
            this.m_cmbLookIn.SelectedIndexChanged += new System.EventHandler(this.CmbLookInSelectedIndexChanged);
            // 
            // m_lblLookIn
            // 
            resources.ApplyResources(this.m_lblLookIn, "m_lblLookIn");
            this.m_lblLookIn.Name = "m_lblLookIn";
            // 
            // m_cmbReplaceWith
            // 
            this.m_cmbReplaceWith.FormattingEnabled = true;
            resources.ApplyResources(this.m_cmbReplaceWith, "m_cmbReplaceWith");
            this.m_cmbReplaceWith.Name = "m_cmbReplaceWith";
            this.m_cmbReplaceWith.TextChanged += new System.EventHandler(this.CmbReplaceWithTextChanged);
            // 
            // m_lblReplaceWith
            // 
            resources.ApplyResources(this.m_lblReplaceWith, "m_lblReplaceWith");
            this.m_lblReplaceWith.Name = "m_lblReplaceWith";
            // 
            // m_cmbFindWhat
            // 
            this.m_cmbFindWhat.FormattingEnabled = true;
            resources.ApplyResources(this.m_cmbFindWhat, "m_cmbFindWhat");
            this.m_cmbFindWhat.Name = "m_cmbFindWhat";
            this.m_cmbFindWhat.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.CmbFindWhatKeyPress);
            this.m_cmbFindWhat.TextChanged += new System.EventHandler(this.CmbFindWhatTextChanged);
            // 
            // m_lblFindWhat
            // 
            resources.ApplyResources(this.m_lblFindWhat, "m_lblFindWhat");
            this.m_lblFindWhat.Name = "m_lblFindWhat";
            // 
            // SledFindAndReplaceForm_QuickReplace
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.m_grpTitle);
            this.Name = "SledFindAndReplaceForm_QuickReplace";
            this.Load += new System.EventHandler(this.SledFindAndReplaceFormQuickReplaceLoad);
            this.m_grpTitle.ResumeLayout(false);
            this.m_grpTitle.PerformLayout();
            this.m_grpFindOptions.ResumeLayout(false);
            this.m_grpFindOptions.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox m_grpTitle;
        private System.Windows.Forms.Button m_btnReplaceAll;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Button m_btnFindNext;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button m_btnReplace;
        private Sce.Sled.Shared.Controls.SledCollapsibleGroupBox m_grpFindOptions;
        private System.Windows.Forms.ComboBox m_cmbUse;
        private System.Windows.Forms.CheckBox m_chkUse;
        private System.Windows.Forms.CheckBox m_chkMatchWholeWord;
        private System.Windows.Forms.CheckBox m_chkMatchCase;
        private System.Windows.Forms.ComboBox m_cmbLookIn;
        private System.Windows.Forms.Label m_lblLookIn;
        private System.Windows.Forms.ComboBox m_cmbReplaceWith;
        private System.Windows.Forms.Label m_lblReplaceWith;
        private System.Windows.Forms.ComboBox m_cmbFindWhat;
        private System.Windows.Forms.Label m_lblFindWhat;
        private System.Windows.Forms.CheckBox m_chkSearchUp;

    }
}
