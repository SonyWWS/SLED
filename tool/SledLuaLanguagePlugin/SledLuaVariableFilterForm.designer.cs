/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

namespace Sce.Sled.Lua
{
    partial class SledLuaVariableFilterForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SledLuaVariableFilterForm));
            this.m_grpBoxLocalType = new System.Windows.Forms.GroupBox();
            this.m_chkLocalLuaTThread = new System.Windows.Forms.CheckBox();
            this.m_chkLocalLuaTUserData = new System.Windows.Forms.CheckBox();
            this.m_chkLocalLuaTFunction = new System.Windows.Forms.CheckBox();
            this.m_chkLocalLuaTTable = new System.Windows.Forms.CheckBox();
            this.m_chkLocalLuaTString = new System.Windows.Forms.CheckBox();
            this.m_chkLocalLuaTNumber = new System.Windows.Forms.CheckBox();
            this.m_chkLocalLuaTLightUserData = new System.Windows.Forms.CheckBox();
            this.m_chkLocalLuaTBoolean = new System.Windows.Forms.CheckBox();
            this.m_chkLocalLuaTNil = new System.Windows.Forms.CheckBox();
            this.m_grpLocalNames = new System.Windows.Forms.GroupBox();
            this.m_btnLocalListNamesDelete = new System.Windows.Forms.Button();
            this.m_btnLocalListNamesEdit = new System.Windows.Forms.Button();
            this.m_btnLocalListNamesAdd = new System.Windows.Forms.Button();
            this.m_lstLocalNames = new System.Windows.Forms.ListBox();
            this.m_tabControl = new System.Windows.Forms.TabControl();
            this.m_tabLocal = new System.Windows.Forms.TabPage();
            this.m_tabTarget = new System.Windows.Forms.TabPage();
            this.m_grpBoxTargetType = new System.Windows.Forms.GroupBox();
            this.m_chkTargetLuaTThread = new System.Windows.Forms.CheckBox();
            this.m_chkTargetLuaTUserData = new System.Windows.Forms.CheckBox();
            this.m_chkTargetLuaTFunction = new System.Windows.Forms.CheckBox();
            this.m_chkTargetLuaTTable = new System.Windows.Forms.CheckBox();
            this.m_chkTargetLuaTString = new System.Windows.Forms.CheckBox();
            this.m_chkTargetLuaTNumber = new System.Windows.Forms.CheckBox();
            this.m_chkTargetLuaTLightUserData = new System.Windows.Forms.CheckBox();
            this.m_chkTargetLuaTBoolean = new System.Windows.Forms.CheckBox();
            this.m_chkTargetLuaTNil = new System.Windows.Forms.CheckBox();
            this.m_grpTargetNames = new System.Windows.Forms.GroupBox();
            this.m_btnTargetListNamesDelete = new System.Windows.Forms.Button();
            this.m_btnTargetListNamesEdit = new System.Windows.Forms.Button();
            this.m_btnTargetListNamesAdd = new System.Windows.Forms.Button();
            this.m_lstTargetNames = new System.Windows.Forms.ListBox();
            this.m_grpBoxLocalType.SuspendLayout();
            this.m_grpLocalNames.SuspendLayout();
            this.m_tabControl.SuspendLayout();
            this.m_tabLocal.SuspendLayout();
            this.m_tabTarget.SuspendLayout();
            this.m_grpBoxTargetType.SuspendLayout();
            this.m_grpTargetNames.SuspendLayout();
            this.SuspendLayout();
            // 
            // m_grpBoxLocalType
            // 
            resources.ApplyResources(this.m_grpBoxLocalType, "m_grpBoxLocalType");
            this.m_grpBoxLocalType.Controls.Add(this.m_chkLocalLuaTThread);
            this.m_grpBoxLocalType.Controls.Add(this.m_chkLocalLuaTUserData);
            this.m_grpBoxLocalType.Controls.Add(this.m_chkLocalLuaTFunction);
            this.m_grpBoxLocalType.Controls.Add(this.m_chkLocalLuaTTable);
            this.m_grpBoxLocalType.Controls.Add(this.m_chkLocalLuaTString);
            this.m_grpBoxLocalType.Controls.Add(this.m_chkLocalLuaTNumber);
            this.m_grpBoxLocalType.Controls.Add(this.m_chkLocalLuaTLightUserData);
            this.m_grpBoxLocalType.Controls.Add(this.m_chkLocalLuaTBoolean);
            this.m_grpBoxLocalType.Controls.Add(this.m_chkLocalLuaTNil);
            this.m_grpBoxLocalType.Name = "m_grpBoxLocalType";
            this.m_grpBoxLocalType.TabStop = false;
            // 
            // m_chkLocalLuaTThread
            // 
            resources.ApplyResources(this.m_chkLocalLuaTThread, "m_chkLocalLuaTThread");
            this.m_chkLocalLuaTThread.Name = "m_chkLocalLuaTThread";
            this.m_chkLocalLuaTThread.UseVisualStyleBackColor = true;
            this.m_chkLocalLuaTThread.CheckedChanged += new System.EventHandler(this.ChkLocalLuaTThreadCheckedChanged);
            // 
            // m_chkLocalLuaTUserData
            // 
            resources.ApplyResources(this.m_chkLocalLuaTUserData, "m_chkLocalLuaTUserData");
            this.m_chkLocalLuaTUserData.Name = "m_chkLocalLuaTUserData";
            this.m_chkLocalLuaTUserData.UseVisualStyleBackColor = true;
            this.m_chkLocalLuaTUserData.CheckedChanged += new System.EventHandler(this.ChkLocalLuaTUserDataCheckedChanged);
            // 
            // m_chkLocalLuaTFunction
            // 
            resources.ApplyResources(this.m_chkLocalLuaTFunction, "m_chkLocalLuaTFunction");
            this.m_chkLocalLuaTFunction.Name = "m_chkLocalLuaTFunction";
            this.m_chkLocalLuaTFunction.UseVisualStyleBackColor = true;
            this.m_chkLocalLuaTFunction.CheckedChanged += new System.EventHandler(this.ChkLocalLuaTFunctionCheckedChanged);
            // 
            // m_chkLocalLuaTTable
            // 
            resources.ApplyResources(this.m_chkLocalLuaTTable, "m_chkLocalLuaTTable");
            this.m_chkLocalLuaTTable.Name = "m_chkLocalLuaTTable";
            this.m_chkLocalLuaTTable.UseVisualStyleBackColor = true;
            this.m_chkLocalLuaTTable.CheckedChanged += new System.EventHandler(this.ChkLocalLuaTTableCheckedChanged);
            // 
            // m_chkLocalLuaTString
            // 
            resources.ApplyResources(this.m_chkLocalLuaTString, "m_chkLocalLuaTString");
            this.m_chkLocalLuaTString.Name = "m_chkLocalLuaTString";
            this.m_chkLocalLuaTString.UseVisualStyleBackColor = true;
            this.m_chkLocalLuaTString.CheckedChanged += new System.EventHandler(this.ChkLocalLuaTStringCheckedChanged);
            // 
            // m_chkLocalLuaTNumber
            // 
            resources.ApplyResources(this.m_chkLocalLuaTNumber, "m_chkLocalLuaTNumber");
            this.m_chkLocalLuaTNumber.Name = "m_chkLocalLuaTNumber";
            this.m_chkLocalLuaTNumber.UseVisualStyleBackColor = true;
            this.m_chkLocalLuaTNumber.CheckedChanged += new System.EventHandler(this.ChkLocalLuaTNumberCheckedChanged);
            // 
            // m_chkLocalLuaTLightUserData
            // 
            resources.ApplyResources(this.m_chkLocalLuaTLightUserData, "m_chkLocalLuaTLightUserData");
            this.m_chkLocalLuaTLightUserData.Name = "m_chkLocalLuaTLightUserData";
            this.m_chkLocalLuaTLightUserData.UseVisualStyleBackColor = true;
            this.m_chkLocalLuaTLightUserData.CheckedChanged += new System.EventHandler(this.ChkLocalLuaTLightUserDataCheckedChanged);
            // 
            // m_chkLocalLuaTBoolean
            // 
            resources.ApplyResources(this.m_chkLocalLuaTBoolean, "m_chkLocalLuaTBoolean");
            this.m_chkLocalLuaTBoolean.Name = "m_chkLocalLuaTBoolean";
            this.m_chkLocalLuaTBoolean.UseVisualStyleBackColor = true;
            this.m_chkLocalLuaTBoolean.CheckedChanged += new System.EventHandler(this.ChkLocalLuaTBooleanCheckedChanged);
            // 
            // m_chkLocalLuaTNil
            // 
            resources.ApplyResources(this.m_chkLocalLuaTNil, "m_chkLocalLuaTNil");
            this.m_chkLocalLuaTNil.Name = "m_chkLocalLuaTNil";
            this.m_chkLocalLuaTNil.UseVisualStyleBackColor = true;
            this.m_chkLocalLuaTNil.CheckedChanged += new System.EventHandler(this.ChkLocalLuaTNilCheckedChanged);
            // 
            // m_grpLocalNames
            // 
            resources.ApplyResources(this.m_grpLocalNames, "m_grpLocalNames");
            this.m_grpLocalNames.Controls.Add(this.m_btnLocalListNamesDelete);
            this.m_grpLocalNames.Controls.Add(this.m_btnLocalListNamesEdit);
            this.m_grpLocalNames.Controls.Add(this.m_btnLocalListNamesAdd);
            this.m_grpLocalNames.Controls.Add(this.m_lstLocalNames);
            this.m_grpLocalNames.Name = "m_grpLocalNames";
            this.m_grpLocalNames.TabStop = false;
            // 
            // m_btnLocalListNamesDelete
            // 
            resources.ApplyResources(this.m_btnLocalListNamesDelete, "m_btnLocalListNamesDelete");
            this.m_btnLocalListNamesDelete.Name = "m_btnLocalListNamesDelete";
            this.m_btnLocalListNamesDelete.UseVisualStyleBackColor = true;
            this.m_btnLocalListNamesDelete.Click += new System.EventHandler(this.BtnLocalListNamesDeleteClick);
            // 
            // m_btnLocalListNamesEdit
            // 
            resources.ApplyResources(this.m_btnLocalListNamesEdit, "m_btnLocalListNamesEdit");
            this.m_btnLocalListNamesEdit.Name = "m_btnLocalListNamesEdit";
            this.m_btnLocalListNamesEdit.UseVisualStyleBackColor = true;
            this.m_btnLocalListNamesEdit.Click += new System.EventHandler(this.BtnLocalListNamesEditClick);
            // 
            // m_btnLocalListNamesAdd
            // 
            resources.ApplyResources(this.m_btnLocalListNamesAdd, "m_btnLocalListNamesAdd");
            this.m_btnLocalListNamesAdd.Name = "m_btnLocalListNamesAdd";
            this.m_btnLocalListNamesAdd.UseVisualStyleBackColor = true;
            this.m_btnLocalListNamesAdd.Click += new System.EventHandler(this.BtnLocalListNamesAddClick);
            // 
            // m_lstLocalNames
            // 
            resources.ApplyResources(this.m_lstLocalNames, "m_lstLocalNames");
            this.m_lstLocalNames.FormattingEnabled = true;
            this.m_lstLocalNames.MultiColumn = true;
            this.m_lstLocalNames.Name = "m_lstLocalNames";
            this.m_lstLocalNames.Sorted = true;
            this.m_lstLocalNames.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.LstLocalNamesMouseDoubleClick);
            this.m_lstLocalNames.SelectedIndexChanged += new System.EventHandler(this.LstLocalNamesSelectedIndexChanged);
            // 
            // m_tabControl
            // 
            resources.ApplyResources(this.m_tabControl, "m_tabControl");
            this.m_tabControl.Controls.Add(this.m_tabLocal);
            this.m_tabControl.Controls.Add(this.m_tabTarget);
            this.m_tabControl.Name = "m_tabControl";
            this.m_tabControl.SelectedIndex = 0;
            // 
            // m_tabLocal
            // 
            this.m_tabLocal.Controls.Add(this.m_grpBoxLocalType);
            this.m_tabLocal.Controls.Add(this.m_grpLocalNames);
            resources.ApplyResources(this.m_tabLocal, "m_tabLocal");
            this.m_tabLocal.Name = "m_tabLocal";
            this.m_tabLocal.UseVisualStyleBackColor = true;
            // 
            // m_tabTarget
            // 
            this.m_tabTarget.Controls.Add(this.m_grpBoxTargetType);
            this.m_tabTarget.Controls.Add(this.m_grpTargetNames);
            resources.ApplyResources(this.m_tabTarget, "m_tabTarget");
            this.m_tabTarget.Name = "m_tabTarget";
            this.m_tabTarget.UseVisualStyleBackColor = true;
            // 
            // m_grpBoxTargetType
            // 
            resources.ApplyResources(this.m_grpBoxTargetType, "m_grpBoxTargetType");
            this.m_grpBoxTargetType.Controls.Add(this.m_chkTargetLuaTThread);
            this.m_grpBoxTargetType.Controls.Add(this.m_chkTargetLuaTUserData);
            this.m_grpBoxTargetType.Controls.Add(this.m_chkTargetLuaTFunction);
            this.m_grpBoxTargetType.Controls.Add(this.m_chkTargetLuaTTable);
            this.m_grpBoxTargetType.Controls.Add(this.m_chkTargetLuaTString);
            this.m_grpBoxTargetType.Controls.Add(this.m_chkTargetLuaTNumber);
            this.m_grpBoxTargetType.Controls.Add(this.m_chkTargetLuaTLightUserData);
            this.m_grpBoxTargetType.Controls.Add(this.m_chkTargetLuaTBoolean);
            this.m_grpBoxTargetType.Controls.Add(this.m_chkTargetLuaTNil);
            this.m_grpBoxTargetType.Name = "m_grpBoxTargetType";
            this.m_grpBoxTargetType.TabStop = false;
            // 
            // m_chkTargetLuaTThread
            // 
            resources.ApplyResources(this.m_chkTargetLuaTThread, "m_chkTargetLuaTThread");
            this.m_chkTargetLuaTThread.Name = "m_chkTargetLuaTThread";
            this.m_chkTargetLuaTThread.UseVisualStyleBackColor = true;
            this.m_chkTargetLuaTThread.CheckedChanged += new System.EventHandler(this.ChkTargetLuaTThreadCheckedChanged);
            // 
            // m_chkTargetLuaTUserData
            // 
            resources.ApplyResources(this.m_chkTargetLuaTUserData, "m_chkTargetLuaTUserData");
            this.m_chkTargetLuaTUserData.Name = "m_chkTargetLuaTUserData";
            this.m_chkTargetLuaTUserData.UseVisualStyleBackColor = true;
            this.m_chkTargetLuaTUserData.CheckedChanged += new System.EventHandler(this.ChkTargetLuaTUserDataCheckedChanged);
            // 
            // m_chkTargetLuaTFunction
            // 
            resources.ApplyResources(this.m_chkTargetLuaTFunction, "m_chkTargetLuaTFunction");
            this.m_chkTargetLuaTFunction.Name = "m_chkTargetLuaTFunction";
            this.m_chkTargetLuaTFunction.UseVisualStyleBackColor = true;
            this.m_chkTargetLuaTFunction.CheckedChanged += new System.EventHandler(this.ChkTargetLuaTFunctionCheckedChanged);
            // 
            // m_chkTargetLuaTTable
            // 
            resources.ApplyResources(this.m_chkTargetLuaTTable, "m_chkTargetLuaTTable");
            this.m_chkTargetLuaTTable.Name = "m_chkTargetLuaTTable";
            this.m_chkTargetLuaTTable.UseVisualStyleBackColor = true;
            this.m_chkTargetLuaTTable.CheckedChanged += new System.EventHandler(this.ChkTargetLuaTTableCheckedChanged);
            // 
            // m_chkTargetLuaTString
            // 
            resources.ApplyResources(this.m_chkTargetLuaTString, "m_chkTargetLuaTString");
            this.m_chkTargetLuaTString.Name = "m_chkTargetLuaTString";
            this.m_chkTargetLuaTString.UseVisualStyleBackColor = true;
            this.m_chkTargetLuaTString.CheckedChanged += new System.EventHandler(this.ChkTargetLuaTStringCheckedChanged);
            // 
            // m_chkTargetLuaTNumber
            // 
            resources.ApplyResources(this.m_chkTargetLuaTNumber, "m_chkTargetLuaTNumber");
            this.m_chkTargetLuaTNumber.Name = "m_chkTargetLuaTNumber";
            this.m_chkTargetLuaTNumber.UseVisualStyleBackColor = true;
            this.m_chkTargetLuaTNumber.CheckedChanged += new System.EventHandler(this.ChkTargetLuaTNumberCheckedChanged);
            // 
            // m_chkTargetLuaTLightUserData
            // 
            resources.ApplyResources(this.m_chkTargetLuaTLightUserData, "m_chkTargetLuaTLightUserData");
            this.m_chkTargetLuaTLightUserData.Name = "m_chkTargetLuaTLightUserData";
            this.m_chkTargetLuaTLightUserData.UseVisualStyleBackColor = true;
            this.m_chkTargetLuaTLightUserData.CheckedChanged += new System.EventHandler(this.ChkTargetLuaTLightUserDataCheckedChanged);
            // 
            // m_chkTargetLuaTBoolean
            // 
            resources.ApplyResources(this.m_chkTargetLuaTBoolean, "m_chkTargetLuaTBoolean");
            this.m_chkTargetLuaTBoolean.Name = "m_chkTargetLuaTBoolean";
            this.m_chkTargetLuaTBoolean.UseVisualStyleBackColor = true;
            this.m_chkTargetLuaTBoolean.CheckedChanged += new System.EventHandler(this.ChkTargetLuaTBooleanCheckedChanged);
            // 
            // m_chkTargetLuaTNil
            // 
            resources.ApplyResources(this.m_chkTargetLuaTNil, "m_chkTargetLuaTNil");
            this.m_chkTargetLuaTNil.Name = "m_chkTargetLuaTNil";
            this.m_chkTargetLuaTNil.UseVisualStyleBackColor = true;
            this.m_chkTargetLuaTNil.CheckedChanged += new System.EventHandler(this.ChkTargetLuaTNilCheckedChanged);
            // 
            // m_grpTargetNames
            // 
            resources.ApplyResources(this.m_grpTargetNames, "m_grpTargetNames");
            this.m_grpTargetNames.Controls.Add(this.m_btnTargetListNamesDelete);
            this.m_grpTargetNames.Controls.Add(this.m_btnTargetListNamesEdit);
            this.m_grpTargetNames.Controls.Add(this.m_btnTargetListNamesAdd);
            this.m_grpTargetNames.Controls.Add(this.m_lstTargetNames);
            this.m_grpTargetNames.Name = "m_grpTargetNames";
            this.m_grpTargetNames.TabStop = false;
            // 
            // m_btnTargetListNamesDelete
            // 
            resources.ApplyResources(this.m_btnTargetListNamesDelete, "m_btnTargetListNamesDelete");
            this.m_btnTargetListNamesDelete.Name = "m_btnTargetListNamesDelete";
            this.m_btnTargetListNamesDelete.UseVisualStyleBackColor = true;
            this.m_btnTargetListNamesDelete.Click += new System.EventHandler(this.BtnTargetListNamesDeleteClick);
            // 
            // m_btnTargetListNamesEdit
            // 
            resources.ApplyResources(this.m_btnTargetListNamesEdit, "m_btnTargetListNamesEdit");
            this.m_btnTargetListNamesEdit.Name = "m_btnTargetListNamesEdit";
            this.m_btnTargetListNamesEdit.UseVisualStyleBackColor = true;
            this.m_btnTargetListNamesEdit.Click += new System.EventHandler(this.BtnTargetListNamesEditClick);
            // 
            // m_btnTargetListNamesAdd
            // 
            resources.ApplyResources(this.m_btnTargetListNamesAdd, "m_btnTargetListNamesAdd");
            this.m_btnTargetListNamesAdd.Name = "m_btnTargetListNamesAdd";
            this.m_btnTargetListNamesAdd.UseVisualStyleBackColor = true;
            this.m_btnTargetListNamesAdd.Click += new System.EventHandler(this.BtnTargetListNamesAddClick);
            // 
            // m_lstTargetNames
            // 
            resources.ApplyResources(this.m_lstTargetNames, "m_lstTargetNames");
            this.m_lstTargetNames.FormattingEnabled = true;
            this.m_lstTargetNames.MultiColumn = true;
            this.m_lstTargetNames.Name = "m_lstTargetNames";
            this.m_lstTargetNames.Sorted = true;
            this.m_lstTargetNames.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.LstTargetNamesMouseDoubleClick);
            this.m_lstTargetNames.SelectedIndexChanged += new System.EventHandler(this.LstTargetNamesSelectedIndexChanged);
            // 
            // SledLuaVariableFilterForm
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.m_tabControl);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "SledLuaVariableFilterForm";
            this.ShowInTaskbar = false;
            this.m_grpBoxLocalType.ResumeLayout(false);
            this.m_grpBoxLocalType.PerformLayout();
            this.m_grpLocalNames.ResumeLayout(false);
            this.m_tabControl.ResumeLayout(false);
            this.m_tabLocal.ResumeLayout(false);
            this.m_tabTarget.ResumeLayout(false);
            this.m_grpBoxTargetType.ResumeLayout(false);
            this.m_grpBoxTargetType.PerformLayout();
            this.m_grpTargetNames.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox m_grpBoxLocalType;
        private System.Windows.Forms.CheckBox m_chkLocalLuaTNil;
        private System.Windows.Forms.CheckBox m_chkLocalLuaTThread;
        private System.Windows.Forms.CheckBox m_chkLocalLuaTUserData;
        private System.Windows.Forms.CheckBox m_chkLocalLuaTFunction;
        private System.Windows.Forms.CheckBox m_chkLocalLuaTTable;
        private System.Windows.Forms.CheckBox m_chkLocalLuaTString;
        private System.Windows.Forms.CheckBox m_chkLocalLuaTNumber;
        private System.Windows.Forms.CheckBox m_chkLocalLuaTLightUserData;
        private System.Windows.Forms.CheckBox m_chkLocalLuaTBoolean;
        private System.Windows.Forms.GroupBox m_grpLocalNames;
        private System.Windows.Forms.ListBox m_lstLocalNames;
        private System.Windows.Forms.Button m_btnLocalListNamesDelete;
        private System.Windows.Forms.Button m_btnLocalListNamesEdit;
        private System.Windows.Forms.Button m_btnLocalListNamesAdd;
        private System.Windows.Forms.TabControl m_tabControl;
        private System.Windows.Forms.TabPage m_tabLocal;
        private System.Windows.Forms.TabPage m_tabTarget;
        private System.Windows.Forms.Button m_btnTargetListNamesDelete;
        private System.Windows.Forms.Button m_btnTargetListNamesEdit;
        private System.Windows.Forms.Button m_btnTargetListNamesAdd;
        private System.Windows.Forms.GroupBox m_grpTargetNames;
        private System.Windows.Forms.ListBox m_lstTargetNames;
        private System.Windows.Forms.GroupBox m_grpBoxTargetType;
        private System.Windows.Forms.CheckBox m_chkTargetLuaTThread;
        private System.Windows.Forms.CheckBox m_chkTargetLuaTUserData;
        private System.Windows.Forms.CheckBox m_chkTargetLuaTFunction;
        private System.Windows.Forms.CheckBox m_chkTargetLuaTTable;
        private System.Windows.Forms.CheckBox m_chkTargetLuaTString;
        private System.Windows.Forms.CheckBox m_chkTargetLuaTNumber;
        private System.Windows.Forms.CheckBox m_chkTargetLuaTLightUserData;
        private System.Windows.Forms.CheckBox m_chkTargetLuaTBoolean;
        private System.Windows.Forms.CheckBox m_chkTargetLuaTNil;
    }
}
