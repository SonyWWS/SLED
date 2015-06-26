/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

namespace Sce.Sled
{
    partial class SledFindAndReplaceForm_FindInFiles
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SledFindAndReplaceForm_FindInFiles));
            this.m_grpTitle = new System.Windows.Forms.GroupBox();
            this.m_btnFindAll = new System.Windows.Forms.Button();
            this.m_lblFindWhat = new System.Windows.Forms.Label();
            this.button1 = new System.Windows.Forms.Button();
            this.m_chkIncludeSubFolders = new System.Windows.Forms.CheckBox();
            this.m_btnLookIn = new System.Windows.Forms.Button();
            this.m_cmbLookIn = new System.Windows.Forms.ComboBox();
            this.m_cmbFindWhat = new System.Windows.Forms.ComboBox();
            this.m_lblLookIn = new System.Windows.Forms.Label();
            this.m_grpResultOptions = new Sce.Sled.Shared.Controls.SledCollapsibleGroupBox();
            this.m_lblListResultsIn = new System.Windows.Forms.Label();
            this.m_chkDisplayFileNamesOnly = new System.Windows.Forms.CheckBox();
            this.m_rdoFindResults2Window = new System.Windows.Forms.RadioButton();
            this.m_rdoFindResults1Window = new System.Windows.Forms.RadioButton();
            this.m_grpFindOptions = new Sce.Sled.Shared.Controls.SledCollapsibleGroupBox();
            this.m_cmbLookAtTheseFileTypes = new System.Windows.Forms.ComboBox();
            this.m_lblLookAtTheseFileTypes = new System.Windows.Forms.Label();
            this.m_cmbUse = new System.Windows.Forms.ComboBox();
            this.m_chkUse = new System.Windows.Forms.CheckBox();
            this.m_chkMatchWholeWord = new System.Windows.Forms.CheckBox();
            this.m_chkMatchCase = new System.Windows.Forms.CheckBox();
            this.m_grpTitle.SuspendLayout();
            this.m_grpResultOptions.SuspendLayout();
            this.m_grpFindOptions.SuspendLayout();
            this.SuspendLayout();
            // 
            // m_grpTitle
            // 
            resources.ApplyResources(this.m_grpTitle, "m_grpTitle");
            this.m_grpTitle.Controls.Add(this.m_btnFindAll);
            this.m_grpTitle.Controls.Add(this.m_lblFindWhat);
            this.m_grpTitle.Controls.Add(this.m_grpResultOptions);
            this.m_grpTitle.Controls.Add(this.button1);
            this.m_grpTitle.Controls.Add(this.m_grpFindOptions);
            this.m_grpTitle.Controls.Add(this.m_chkIncludeSubFolders);
            this.m_grpTitle.Controls.Add(this.m_btnLookIn);
            this.m_grpTitle.Controls.Add(this.m_cmbLookIn);
            this.m_grpTitle.Controls.Add(this.m_cmbFindWhat);
            this.m_grpTitle.Controls.Add(this.m_lblLookIn);
            this.m_grpTitle.Name = "m_grpTitle";
            this.m_grpTitle.TabStop = false;
            // 
            // m_btnFindAll
            // 
            resources.ApplyResources(this.m_btnFindAll, "m_btnFindAll");
            this.m_btnFindAll.Name = "m_btnFindAll";
            this.m_btnFindAll.UseVisualStyleBackColor = true;
            this.m_btnFindAll.Click += new System.EventHandler(this.BtnFindAllClick);
            // 
            // m_lblFindWhat
            // 
            resources.ApplyResources(this.m_lblFindWhat, "m_lblFindWhat");
            this.m_lblFindWhat.Name = "m_lblFindWhat";
            // 
            // button1
            // 
            resources.ApplyResources(this.button1, "button1");
            this.button1.Name = "button1";
            this.button1.UseVisualStyleBackColor = true;
            // 
            // m_chkIncludeSubFolders
            // 
            resources.ApplyResources(this.m_chkIncludeSubFolders, "m_chkIncludeSubFolders");
            this.m_chkIncludeSubFolders.Name = "m_chkIncludeSubFolders";
            this.m_chkIncludeSubFolders.UseVisualStyleBackColor = true;
            this.m_chkIncludeSubFolders.CheckedChanged += new System.EventHandler(this.ChkIncludeSubFoldersCheckedChanged);
            // 
            // m_btnLookIn
            // 
            resources.ApplyResources(this.m_btnLookIn, "m_btnLookIn");
            this.m_btnLookIn.Name = "m_btnLookIn";
            this.m_btnLookIn.UseVisualStyleBackColor = true;
            this.m_btnLookIn.Click += new System.EventHandler(this.BtnLookInClick);
            // 
            // m_cmbLookIn
            // 
            this.m_cmbLookIn.FormattingEnabled = true;
            resources.ApplyResources(this.m_cmbLookIn, "m_cmbLookIn");
            this.m_cmbLookIn.Name = "m_cmbLookIn";
            this.m_cmbLookIn.SelectedIndexChanged += new System.EventHandler(this.CmbLookInSelectedIndexChanged);
            // 
            // m_cmbFindWhat
            // 
            this.m_cmbFindWhat.FormattingEnabled = true;
            resources.ApplyResources(this.m_cmbFindWhat, "m_cmbFindWhat");
            this.m_cmbFindWhat.Name = "m_cmbFindWhat";
            this.m_cmbFindWhat.TextChanged += new System.EventHandler(this.CmbFindWhatTextChanged);
            this.m_cmbFindWhat.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.CmbFindWhatKeyPress);
            // 
            // m_lblLookIn
            // 
            resources.ApplyResources(this.m_lblLookIn, "m_lblLookIn");
            this.m_lblLookIn.Name = "m_lblLookIn";
            // 
            // m_grpResultOptions
            // 
            this.m_grpResultOptions.Controls.Add(this.m_lblListResultsIn);
            this.m_grpResultOptions.Controls.Add(this.m_chkDisplayFileNamesOnly);
            this.m_grpResultOptions.Controls.Add(this.m_rdoFindResults2Window);
            this.m_grpResultOptions.Controls.Add(this.m_rdoFindResults1Window);
            resources.ApplyResources(this.m_grpResultOptions, "m_grpResultOptions");
            this.m_grpResultOptions.Name = "m_grpResultOptions";
            this.m_grpResultOptions.TabStop = false;
            // 
            // m_lblListResultsIn
            // 
            resources.ApplyResources(this.m_lblListResultsIn, "m_lblListResultsIn");
            this.m_lblListResultsIn.Name = "m_lblListResultsIn";
            // 
            // m_chkDisplayFileNamesOnly
            // 
            resources.ApplyResources(this.m_chkDisplayFileNamesOnly, "m_chkDisplayFileNamesOnly");
            this.m_chkDisplayFileNamesOnly.Name = "m_chkDisplayFileNamesOnly";
            this.m_chkDisplayFileNamesOnly.UseVisualStyleBackColor = true;
            this.m_chkDisplayFileNamesOnly.CheckedChanged += new System.EventHandler(this.ChkDisplayFileNamesOnlyCheckedChanged);
            // 
            // m_rdoFindResults2Window
            // 
            resources.ApplyResources(this.m_rdoFindResults2Window, "m_rdoFindResults2Window");
            this.m_rdoFindResults2Window.Name = "m_rdoFindResults2Window";
            this.m_rdoFindResults2Window.TabStop = true;
            this.m_rdoFindResults2Window.UseVisualStyleBackColor = true;
            // 
            // m_rdoFindResults1Window
            // 
            resources.ApplyResources(this.m_rdoFindResults1Window, "m_rdoFindResults1Window");
            this.m_rdoFindResults1Window.Name = "m_rdoFindResults1Window";
            this.m_rdoFindResults1Window.TabStop = true;
            this.m_rdoFindResults1Window.UseVisualStyleBackColor = true;
            this.m_rdoFindResults1Window.CheckedChanged += new System.EventHandler(this.RdoFindResults1WindowCheckedChanged);
            // 
            // m_grpFindOptions
            // 
            this.m_grpFindOptions.Controls.Add(this.m_cmbLookAtTheseFileTypes);
            this.m_grpFindOptions.Controls.Add(this.m_lblLookAtTheseFileTypes);
            this.m_grpFindOptions.Controls.Add(this.m_cmbUse);
            this.m_grpFindOptions.Controls.Add(this.m_chkUse);
            this.m_grpFindOptions.Controls.Add(this.m_chkMatchWholeWord);
            this.m_grpFindOptions.Controls.Add(this.m_chkMatchCase);
            resources.ApplyResources(this.m_grpFindOptions, "m_grpFindOptions");
            this.m_grpFindOptions.Name = "m_grpFindOptions";
            this.m_grpFindOptions.TabStop = false;
            // 
            // m_cmbLookAtTheseFileTypes
            // 
            resources.ApplyResources(this.m_cmbLookAtTheseFileTypes, "m_cmbLookAtTheseFileTypes");
            this.m_cmbLookAtTheseFileTypes.FormattingEnabled = true;
            this.m_cmbLookAtTheseFileTypes.Name = "m_cmbLookAtTheseFileTypes";
            // 
            // m_lblLookAtTheseFileTypes
            // 
            resources.ApplyResources(this.m_lblLookAtTheseFileTypes, "m_lblLookAtTheseFileTypes");
            this.m_lblLookAtTheseFileTypes.Name = "m_lblLookAtTheseFileTypes";
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
            // SledFindAndReplaceForm_FindInFiles
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.m_grpTitle);
            this.Name = "SledFindAndReplaceForm_FindInFiles";
            this.Load += new System.EventHandler(this.SledFindAndReplaceFormFindInFilesLoad);
            this.m_grpTitle.ResumeLayout(false);
            this.m_grpTitle.PerformLayout();
            this.m_grpResultOptions.ResumeLayout(false);
            this.m_grpResultOptions.PerformLayout();
            this.m_grpFindOptions.ResumeLayout(false);
            this.m_grpFindOptions.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox m_grpTitle;
        private System.Windows.Forms.Label m_lblFindWhat;
        private Sce.Sled.Shared.Controls.SledCollapsibleGroupBox m_grpResultOptions;
        private System.Windows.Forms.Label m_lblListResultsIn;
        private System.Windows.Forms.RadioButton m_rdoFindResults2Window;
        private System.Windows.Forms.RadioButton m_rdoFindResults1Window;
        private System.Windows.Forms.Button button1;
        private Sce.Sled.Shared.Controls.SledCollapsibleGroupBox m_grpFindOptions;
        private System.Windows.Forms.ComboBox m_cmbLookAtTheseFileTypes;
        private System.Windows.Forms.Label m_lblLookAtTheseFileTypes;
        private System.Windows.Forms.ComboBox m_cmbUse;
        private System.Windows.Forms.CheckBox m_chkUse;
        private System.Windows.Forms.CheckBox m_chkMatchWholeWord;
        private System.Windows.Forms.CheckBox m_chkMatchCase;
        private System.Windows.Forms.CheckBox m_chkIncludeSubFolders;
        private System.Windows.Forms.Button m_btnLookIn;
        private System.Windows.Forms.ComboBox m_cmbLookIn;
        private System.Windows.Forms.ComboBox m_cmbFindWhat;
        private System.Windows.Forms.Label m_lblLookIn;
        private System.Windows.Forms.Button m_btnFindAll;
        private System.Windows.Forms.CheckBox m_chkDisplayFileNamesOnly;

    }
}
