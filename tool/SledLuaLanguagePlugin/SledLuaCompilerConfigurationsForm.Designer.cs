/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

namespace Sce.Sled.Lua
{
    partial class SledLuaCompilerConfigurationsForm
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
                if (m_txtBox != null)
                {
                    m_txtBox.KeyPress -= TxtBoxKeyPress;
                    m_txtBox.Leave -= TxtBoxLeave;
                    m_txtBox.Dispose();
                    m_txtBox = null;
                }

                if (m_lstBigLittleBox != null)
                {
                    m_lstBigLittleBox.SelectedIndexChanged -= LstYesNoBoxSelectedIndexChanged;
                    m_lstBigLittleBox.Leave -= LstBigLittleBoxLeave;
                    m_lstBigLittleBox.Dispose();
                    m_lstBigLittleBox = null;
                }

                if (m_lstYesNoBox != null)
                {
                    m_lstYesNoBox.SelectedIndexChanged -= LstYesNoBoxSelectedIndexChanged;
                    m_lstYesNoBox.Leave -= LstYesNoBoxLeave;
                    m_lstYesNoBox.Dispose();
                    m_lstYesNoBox = null;
                }

                if (m_folderBrowserDlg != null)
                {
                    m_folderBrowserDlg.Dispose();
                    m_folderBrowserDlg = null;
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SledLuaCompilerConfigurationsForm));
            this.m_grpConfigurations = new System.Windows.Forms.GroupBox();
            this.label1 = new System.Windows.Forms.Label();
            this.m_btnClose = new System.Windows.Forms.Button();
            this.m_btnDelete = new System.Windows.Forms.Button();
            this.m_btnAdd = new System.Windows.Forms.Button();
            this.m_lstConfigurations = new System.Windows.Forms.ListView();
            this.columnHeader1 = new System.Windows.Forms.ColumnHeader();
            this.columnHeader2 = new System.Windows.Forms.ColumnHeader();
            this.columnHeader3 = new System.Windows.Forms.ColumnHeader();
            this.columnHeader4 = new System.Windows.Forms.ColumnHeader();
            this.columnHeader5 = new System.Windows.Forms.ColumnHeader();
            this.columnHeader6 = new System.Windows.Forms.ColumnHeader();
            this.columnHeader7 = new System.Windows.Forms.ColumnHeader();
            this.columnHeader8 = new System.Windows.Forms.ColumnHeader();
            this.columnHeader9 = new System.Windows.Forms.ColumnHeader();
            this.m_grpConfigurations.SuspendLayout();
            this.SuspendLayout();
            // 
            // m_grpConfigurations
            // 
            resources.ApplyResources(this.m_grpConfigurations, "m_grpConfigurations");
            this.m_grpConfigurations.Controls.Add(this.label1);
            this.m_grpConfigurations.Controls.Add(this.m_btnClose);
            this.m_grpConfigurations.Controls.Add(this.m_btnDelete);
            this.m_grpConfigurations.Controls.Add(this.m_btnAdd);
            this.m_grpConfigurations.Controls.Add(this.m_lstConfigurations);
            this.m_grpConfigurations.Name = "m_grpConfigurations";
            this.m_grpConfigurations.TabStop = false;
            // 
            // label1
            // 
            resources.ApplyResources(this.label1, "label1");
            this.label1.Name = "label1";
            // 
            // m_btnClose
            // 
            resources.ApplyResources(this.m_btnClose, "m_btnClose");
            this.m_btnClose.Name = "m_btnClose";
            this.m_btnClose.UseVisualStyleBackColor = true;
            this.m_btnClose.Click += new System.EventHandler(this.BtnCloseClick);
            // 
            // m_btnDelete
            // 
            resources.ApplyResources(this.m_btnDelete, "m_btnDelete");
            this.m_btnDelete.Name = "m_btnDelete";
            this.m_btnDelete.UseVisualStyleBackColor = true;
            this.m_btnDelete.Click += new System.EventHandler(this.BtnDeleteClick);
            // 
            // m_btnAdd
            // 
            resources.ApplyResources(this.m_btnAdd, "m_btnAdd");
            this.m_btnAdd.Name = "m_btnAdd";
            this.m_btnAdd.UseVisualStyleBackColor = true;
            this.m_btnAdd.Click += new System.EventHandler(this.BtnAddClick);
            // 
            // m_lstConfigurations
            // 
            resources.ApplyResources(this.m_lstConfigurations, "m_lstConfigurations");
            this.m_lstConfigurations.CheckBoxes = true;
            this.m_lstConfigurations.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader2,
            this.columnHeader3,
            this.columnHeader4,
            this.columnHeader5,
            this.columnHeader6,
            this.columnHeader7,
            this.columnHeader8,
            this.columnHeader9});
            this.m_lstConfigurations.FullRowSelect = true;
            this.m_lstConfigurations.GridLines = true;
            this.m_lstConfigurations.HideSelection = false;
            this.m_lstConfigurations.LabelEdit = true;
            this.m_lstConfigurations.MultiSelect = false;
            this.m_lstConfigurations.Name = "m_lstConfigurations";
            this.m_lstConfigurations.UseCompatibleStateImageBehavior = false;
            this.m_lstConfigurations.View = System.Windows.Forms.View.Details;
            this.m_lstConfigurations.ItemChecked += new System.Windows.Forms.ItemCheckedEventHandler(this.LstConfigurationsItemChecked);
            this.m_lstConfigurations.SelectedIndexChanged += new System.EventHandler(this.LstConfigurationsSelectedIndexChanged);
            this.m_lstConfigurations.BeforeLabelEdit += new System.Windows.Forms.LabelEditEventHandler(this.LstConfigurationsBeforeLabelEdit);
            // 
            // columnHeader1
            // 
            resources.ApplyResources(this.columnHeader1, "columnHeader1");
            // 
            // columnHeader2
            // 
            resources.ApplyResources(this.columnHeader2, "columnHeader2");
            // 
            // columnHeader3
            // 
            resources.ApplyResources(this.columnHeader3, "columnHeader3");
            // 
            // columnHeader4
            // 
            resources.ApplyResources(this.columnHeader4, "columnHeader4");
            // 
            // columnHeader5
            // 
            resources.ApplyResources(this.columnHeader5, "columnHeader5");
            // 
            // columnHeader6
            // 
            resources.ApplyResources(this.columnHeader6, "columnHeader6");
            // 
            // columnHeader7
            // 
            resources.ApplyResources(this.columnHeader7, "columnHeader7");
            // 
            // columnHeader8
            // 
            resources.ApplyResources(this.columnHeader8, "columnHeader8");
            // 
            // columnHeader9
            // 
            resources.ApplyResources(this.columnHeader9, "columnHeader9");
            // 
            // SledLuaCompilerConfigurationsForm
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.m_grpConfigurations);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "SledLuaCompilerConfigurationsForm";
            this.Load += new System.EventHandler(this.SledLuaCompilerConfigurationsFormLoad);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.SledLuaCompilerConfigurationsFormFormClosing);
            this.m_grpConfigurations.ResumeLayout(false);
            this.m_grpConfigurations.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox m_grpConfigurations;
        private System.Windows.Forms.ListView m_lstConfigurations;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.Button m_btnDelete;
        private System.Windows.Forms.Button m_btnAdd;
        private System.Windows.Forms.ColumnHeader columnHeader2;
        private System.Windows.Forms.ColumnHeader columnHeader3;
        private System.Windows.Forms.ColumnHeader columnHeader4;
        private System.Windows.Forms.ColumnHeader columnHeader5;
        private System.Windows.Forms.ColumnHeader columnHeader6;
        private System.Windows.Forms.Button m_btnClose;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ColumnHeader columnHeader7;
        private System.Windows.Forms.ColumnHeader columnHeader8;
        private System.Windows.Forms.ColumnHeader columnHeader9;


    }
}
