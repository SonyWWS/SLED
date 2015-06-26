/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

using System;
using System.Collections.Generic;
using System.Windows.Forms;

using Sce.Sled.Lua.Resources;

namespace Sce.Sled.Lua
{
    public partial class SledLuaVariableFilterForm : Form
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public SledLuaVariableFilterForm()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Indicates whether the local filter state changed during
        /// the course of the dialog being shown
        /// </summary>
        public bool LocalFilterStateChanged
        {
            get
            {
                if (m_bLocalChanged)
                    return true;

                for (var i = 0; i < 9; i++)
                {
                    if ((m_iLocalFilterTypes[i] % 2) != 0)
                        return true;
                }

                return false;
            }
        }

        /// <summary>
        /// Indicates whether the target filter state changed during
        /// the course of the dialog being shown
        /// </summary>
        public bool TargetFilterStateChanged
        {
            get
            {
                if (m_bTargetChanged)
                    return true;

                for (var i = 0; i < 9; i++)
                {
                    if ((m_iTargetFilterTypes[i] % 2) != 0)
                        return true;
                }

                return false;
            }
        }

        /// <summary>
        /// Gets/sets the local filter types
        /// </summary>
        public bool[] LocalFilterTypes
        {
            get { return m_bLocalFilterTypes; }
            set
            {
                m_bLocalFilterTypes = value;

                m_bLocalModifyingCheckstate = true;

                if (m_bLocalFilterTypes[(int)LuaType.LUA_TNIL])
                    m_chkLocalLuaTNil.CheckState = CheckState.Checked;
                if (m_bLocalFilterTypes[(int)LuaType.LUA_TBOOLEAN])
                    m_chkLocalLuaTBoolean.CheckState = CheckState.Checked;
                if (m_bLocalFilterTypes[(int)LuaType.LUA_TLIGHTUSERDATA])
                    m_chkLocalLuaTLightUserData.CheckState = CheckState.Checked;
                if (m_bLocalFilterTypes[(int)LuaType.LUA_TNUMBER])
                    m_chkLocalLuaTNumber.CheckState = CheckState.Checked;
                if (m_bLocalFilterTypes[(int)LuaType.LUA_TSTRING])
                    m_chkLocalLuaTString.CheckState = CheckState.Checked;
                if (m_bLocalFilterTypes[(int)LuaType.LUA_TTABLE])
                    m_chkLocalLuaTTable.CheckState = CheckState.Checked;
                if (m_bLocalFilterTypes[(int)LuaType.LUA_TFUNCTION])
                    m_chkLocalLuaTFunction.CheckState = CheckState.Checked;
                if (m_bLocalFilterTypes[(int)LuaType.LUA_TUSERDATA])
                    m_chkLocalLuaTUserData.CheckState = CheckState.Checked;
                if (m_bLocalFilterTypes[(int)LuaType.LUA_TTHREAD])
                    m_chkLocalLuaTThread.CheckState = CheckState.Checked;

                m_bLocalModifyingCheckstate = false;
            }
        }

        /// <summary>
        /// Gets/sets the target filter types
        /// </summary>
        public bool[] TargetFilterTypes
        {
            get { return m_bTargetFilterTypes; }
            set
            {
                m_bTargetFilterTypes = value;

                m_bTargetModifyingCheckstate = true;

                if (m_bTargetFilterTypes[(int)LuaType.LUA_TNIL])
                    m_chkTargetLuaTNil.CheckState = CheckState.Checked;
                if (m_bTargetFilterTypes[(int)LuaType.LUA_TBOOLEAN])
                    m_chkTargetLuaTBoolean.CheckState = CheckState.Checked;
                if (m_bTargetFilterTypes[(int)LuaType.LUA_TLIGHTUSERDATA])
                    m_chkTargetLuaTLightUserData.CheckState = CheckState.Checked;
                if (m_bTargetFilterTypes[(int)LuaType.LUA_TNUMBER])
                    m_chkTargetLuaTNumber.CheckState = CheckState.Checked;
                if (m_bTargetFilterTypes[(int)LuaType.LUA_TSTRING])
                    m_chkTargetLuaTString.CheckState = CheckState.Checked;
                if (m_bTargetFilterTypes[(int)LuaType.LUA_TTABLE])
                    m_chkTargetLuaTTable.CheckState = CheckState.Checked;
                if (m_bTargetFilterTypes[(int)LuaType.LUA_TFUNCTION])
                    m_chkTargetLuaTFunction.CheckState = CheckState.Checked;
                if (m_bTargetFilterTypes[(int)LuaType.LUA_TUSERDATA])
                    m_chkTargetLuaTUserData.CheckState = CheckState.Checked;
                if (m_bTargetFilterTypes[(int)LuaType.LUA_TTHREAD])
                    m_chkTargetLuaTThread.CheckState = CheckState.Checked;

                m_bTargetModifyingCheckstate = false;
            }
        }

        /// <summary>
        /// Gets/sets local filtered names
        /// </summary>
        public List<string> LocalFilterNames
        {
            get { return m_lstLocalFilterNames; }
            set
            {
                m_lstLocalFilterNames = value;

                foreach (var s in m_lstLocalFilterNames)
                    m_lstLocalNames.Items.Add(s);
            }
        }

        /// <summary>
        /// Gets/sets target filtered names
        /// </summary>
        public List<string> TargetFilterNames
        {
            get { return m_lstTargetFilterNames; }
            set
            {
                m_lstTargetFilterNames = value;

                foreach (var s in m_lstTargetFilterNames)
                    m_lstTargetNames.Items.Add(s);
            }
        }

        private bool[] m_bLocalFilterTypes;
        private bool[] m_bTargetFilterTypes;
        private readonly int[] m_iLocalFilterTypes = { 0, 0, 0, 0, 0, 0, 0, 0, 0 };
        private readonly int[] m_iTargetFilterTypes = { 0, 0, 0, 0, 0, 0, 0, 0, 0 };
        private List<string> m_lstLocalFilterNames;
        private List<string> m_lstTargetFilterNames;

        private void ChkLocalLuaTNilCheckedChanged(object sender, EventArgs e)
        {
            if (m_bLocalModifyingCheckstate)
                return;

            m_bLocalFilterTypes[(int)LuaType.LUA_TNIL] = m_chkLocalLuaTNil.CheckState == CheckState.Checked;
            m_iLocalFilterTypes[(int)LuaType.LUA_TNIL]++;
        }

        private void ChkLocalLuaTBooleanCheckedChanged(object sender, EventArgs e)
        {
            if (m_bLocalModifyingCheckstate)
                return;

            m_bLocalFilterTypes[(int)LuaType.LUA_TBOOLEAN] = m_chkLocalLuaTBoolean.CheckState == CheckState.Checked;
            m_iLocalFilterTypes[(int)LuaType.LUA_TBOOLEAN]++;
        }

        private void ChkLocalLuaTLightUserDataCheckedChanged(object sender, EventArgs e)
        {
            if (m_bLocalModifyingCheckstate)
                return;

            m_bLocalFilterTypes[(int)LuaType.LUA_TLIGHTUSERDATA] = m_chkLocalLuaTLightUserData.CheckState == CheckState.Checked;
            m_iLocalFilterTypes[(int)LuaType.LUA_TLIGHTUSERDATA]++;
        }

        private void ChkLocalLuaTNumberCheckedChanged(object sender, EventArgs e)
        {
            if (m_bLocalModifyingCheckstate)
                return;

            m_bLocalFilterTypes[(int)LuaType.LUA_TNUMBER] = m_chkLocalLuaTNumber.CheckState == CheckState.Checked;
            m_iLocalFilterTypes[(int)LuaType.LUA_TNUMBER]++;
        }

        private void ChkLocalLuaTStringCheckedChanged(object sender, EventArgs e)
        {
            if (m_bLocalModifyingCheckstate)
                return;

            m_bLocalFilterTypes[(int)LuaType.LUA_TSTRING] = m_chkLocalLuaTString.CheckState == CheckState.Checked;
            m_iLocalFilterTypes[(int)LuaType.LUA_TSTRING]++;
        }

        private void ChkLocalLuaTTableCheckedChanged(object sender, EventArgs e)
        {
            if (m_bLocalModifyingCheckstate)
                return;

            m_bLocalFilterTypes[(int)LuaType.LUA_TTABLE] = m_chkLocalLuaTTable.CheckState == CheckState.Checked;
            m_iLocalFilterTypes[(int)LuaType.LUA_TTABLE]++;
        }

        private void ChkLocalLuaTFunctionCheckedChanged(object sender, EventArgs e)
        {
            if (m_bLocalModifyingCheckstate)
                return;

            m_bLocalFilterTypes[(int)LuaType.LUA_TFUNCTION] = m_chkLocalLuaTFunction.CheckState == CheckState.Checked;
            m_iLocalFilterTypes[(int)LuaType.LUA_TFUNCTION]++;
        }

        private void ChkLocalLuaTUserDataCheckedChanged(object sender, EventArgs e)
        {
            if (m_bLocalModifyingCheckstate)
                return;

            m_bLocalFilterTypes[(int)LuaType.LUA_TUSERDATA] = m_chkLocalLuaTUserData.CheckState == CheckState.Checked;
            m_iLocalFilterTypes[(int)LuaType.LUA_TUSERDATA]++;
        }

        private void ChkLocalLuaTThreadCheckedChanged(object sender, EventArgs e)
        {
            if (m_bLocalModifyingCheckstate)
                return;

            m_bLocalFilterTypes[(int)LuaType.LUA_TTHREAD] = m_chkLocalLuaTThread.CheckState == CheckState.Checked;
            m_iLocalFilterTypes[(int)LuaType.LUA_TTHREAD]++;
        }

        private void ChkTargetLuaTNilCheckedChanged(object sender, EventArgs e)
        {
            if (m_bTargetModifyingCheckstate)
                return;

            m_bTargetFilterTypes[(int)LuaType.LUA_TNIL] = m_chkTargetLuaTNil.CheckState == CheckState.Checked;
            m_iTargetFilterTypes[(int)LuaType.LUA_TNIL]++;
        }

        private void ChkTargetLuaTBooleanCheckedChanged(object sender, EventArgs e)
        {
            if (m_bTargetModifyingCheckstate)
                return;

            m_bTargetFilterTypes[(int)LuaType.LUA_TBOOLEAN] = m_chkTargetLuaTBoolean.CheckState == CheckState.Checked;
            m_iTargetFilterTypes[(int)LuaType.LUA_TBOOLEAN]++;
        }

        private void ChkTargetLuaTLightUserDataCheckedChanged(object sender, EventArgs e)
        {
            if (m_bTargetModifyingCheckstate)
                return;

            m_bTargetFilterTypes[(int)LuaType.LUA_TLIGHTUSERDATA] = m_chkTargetLuaTLightUserData.CheckState == CheckState.Checked;
            m_iTargetFilterTypes[(int)LuaType.LUA_TLIGHTUSERDATA]++;
        }

        private void ChkTargetLuaTNumberCheckedChanged(object sender, EventArgs e)
        {
            if (m_bTargetModifyingCheckstate)
                return;

            m_bTargetFilterTypes[(int)LuaType.LUA_TNUMBER] = m_chkTargetLuaTNumber.CheckState == CheckState.Checked;
            m_iTargetFilterTypes[(int)LuaType.LUA_TNUMBER]++;
        }

        private void ChkTargetLuaTStringCheckedChanged(object sender, EventArgs e)
        {
            if (m_bTargetModifyingCheckstate)
                return;

            m_bTargetFilterTypes[(int)LuaType.LUA_TSTRING] = m_chkTargetLuaTString.CheckState == CheckState.Checked;
            m_iTargetFilterTypes[(int)LuaType.LUA_TSTRING]++;
        }

        private void ChkTargetLuaTTableCheckedChanged(object sender, EventArgs e)
        {
            if (m_bTargetModifyingCheckstate)
                return;

            m_bTargetFilterTypes[(int)LuaType.LUA_TTABLE] = m_chkTargetLuaTTable.CheckState == CheckState.Checked;
            m_iTargetFilterTypes[(int)LuaType.LUA_TTABLE]++;
        }

        private void ChkTargetLuaTFunctionCheckedChanged(object sender, EventArgs e)
        {
            if (m_bTargetModifyingCheckstate)
                return;

            m_bTargetFilterTypes[(int)LuaType.LUA_TFUNCTION] = m_chkTargetLuaTFunction.CheckState == CheckState.Checked;
            m_iTargetFilterTypes[(int)LuaType.LUA_TFUNCTION]++;
        }

        private void ChkTargetLuaTUserDataCheckedChanged(object sender, EventArgs e)
        {
            if (m_bTargetModifyingCheckstate)
                return;

            m_bTargetFilterTypes[(int)LuaType.LUA_TUSERDATA] = m_chkTargetLuaTUserData.CheckState == CheckState.Checked;
            m_iTargetFilterTypes[(int)LuaType.LUA_TUSERDATA]++;
        }

        private void ChkTargetLuaTThreadCheckedChanged(object sender, EventArgs e)
        {
            if (m_bTargetModifyingCheckstate)
                return;

            m_bTargetFilterTypes[(int)LuaType.LUA_TTHREAD] = m_chkTargetLuaTThread.CheckState == CheckState.Checked;
            m_iTargetFilterTypes[(int)LuaType.LUA_TTHREAD]++;
        }

        private bool m_bLocalModifyingCheckstate;
        private bool m_bTargetModifyingCheckstate;
        private bool m_bLocalChanged; 
        private bool m_bTargetChanged;

        /// <summary>
        /// Event for clicking the local add button
        /// </summary>
        /// <param name="sender">object that fired the event</param>
        /// <param name="e">event arguments</param>
        private void BtnLocalListNamesAddClick(object sender, EventArgs e)
        {
            var form = new SledLuaVariableFilterVarNameForm();
            form.Text += Localization.SledLuaTTYFilterAdd;

            // Show form
            if (form.ShowDialog(this) == DialogResult.OK)
            {
                var bDuplicate = false;

                foreach (var o in m_lstLocalNames.Items)
                {
                    if (o.ToString() == form.VarName)
                    {
                        bDuplicate = true;
                        break;
                    }
                }

                if (!bDuplicate)
                {
                    // Add item
                    m_lstLocalNames.Items.Add(form.VarName);
                    m_lstLocalFilterNames.Add(form.VarName);

                    m_bLocalChanged = true;
                }
            }
        }

        /// <summary>
        /// Event for clicking the local edit button
        /// </summary>
        /// <param name="sender">object that fired the event</param>
        /// <param name="e">event arguments</param>
        private void BtnLocalListNamesEditClick(object sender, EventArgs e)
        {
            if (m_lstLocalNames.SelectedItem == null)
                return;

            var selection = m_lstLocalNames.SelectedItem.ToString();

            var form = new SledLuaVariableFilterVarNameForm();
            form.Text += Localization.SledLuaTTYFilterEdit;
            form.VarName = selection;

            // Show form
            if (form.ShowDialog(this) == DialogResult.OK)
            {
                if (selection != form.VarName)
                {
                    var bDuplicate = false;

                    foreach (var o in m_lstLocalNames.Items)
                    {
                        if (o.ToString() == form.VarName)
                        {
                            bDuplicate = true;
                            break;
                        }
                    }

                    if (!bDuplicate)
                    {
                        // Remove old item
                        m_lstLocalNames.Items.Remove(selection);
                        m_lstLocalFilterNames.Remove(selection);

                        // Add new item
                        m_lstLocalNames.Items.Add(form.VarName);
                        m_lstLocalFilterNames.Add(form.VarName);

                        // Adjust selection to point to new item
                        m_lstLocalNames.SelectedItem = form.VarName;

                        m_bLocalChanged = true;
                    }
                }
            }
        }

        /// <summary>
        /// Event for clicking the local delete button
        /// </summary>
        /// <param name="sender">object that fired the event</param>
        /// <param name="e">event arguments</param>
        private void BtnLocalListNamesDeleteClick(object sender, EventArgs e)
        {
            if (m_lstLocalNames.SelectedItem == null)
                return;

            var iIndex = m_lstLocalNames.SelectedIndex;

            var selection = m_lstLocalNames.SelectedItem.ToString();

            // Remove the selected item
            m_lstLocalFilterNames.Remove(selection);
            m_lstLocalNames.Items.Remove(selection);
            m_bLocalChanged = true;

            // Update selection
            if (m_lstLocalNames.Items.Count > 0)
            {
                if (iIndex >= m_lstLocalNames.Items.Count)
                    iIndex--;

                m_lstLocalNames.SelectedIndex = iIndex;
            }
        }

        /// <summary>
        /// Event for double clicking the local list box
        /// </summary>
        /// <param name="sender">object that fired the event</param>
        /// <param name="e">event arguments</param>
        private void LstLocalNamesMouseDoubleClick(object sender, MouseEventArgs e)
        {
            // Double clicking edits so piggy back existing method
            BtnLocalListNamesEditClick(sender, e);
        }

        /// <summary>
        /// Event for clicking the target add button
        /// </summary>
        /// <param name="sender">object that fired the event</param>
        /// <param name="e">event arguments</param>
        private void BtnTargetListNamesAddClick(object sender, EventArgs e)
        {
            var form = new SledLuaVariableFilterVarNameForm();
            form.Text += Localization.SledLuaTTYFilterAdd;

            // Show form
            if (form.ShowDialog(this) == DialogResult.OK)
            {
                var bDuplicate = false;

                foreach (var o in m_lstTargetNames.Items)
                {
                    if (o.ToString() == form.VarName)
                    {
                        bDuplicate = true;
                        break;
                    }
                }

                if (!bDuplicate)
                {
                    // Add item
                    m_lstTargetNames.Items.Add(form.VarName);
                    m_lstTargetFilterNames.Add(form.VarName);

                    m_bTargetChanged = true;
                }
            }
        }

        /// <summary>
        /// Event for clicking the target edit button
        /// </summary>
        /// <param name="sender">object that fired the event</param>
        /// <param name="e">event arguments</param>
        private void BtnTargetListNamesEditClick(object sender, EventArgs e)
        {
            if (m_lstTargetNames.SelectedItem == null)
                return;

            var selection = m_lstTargetNames.SelectedItem.ToString();

            var form = new SledLuaVariableFilterVarNameForm();
            form.Text += Localization.SledLuaTTYFilterEdit;
            form.VarName = selection;

            // Show form
            if (form.ShowDialog(this) == DialogResult.OK)
            {
                if (selection != form.VarName)
                {
                    var bDuplicate = false;

                    foreach (var o in m_lstTargetNames.Items)
                    {
                        if (o.ToString() == form.VarName)
                        {
                            bDuplicate = true;
                            break;
                        }
                    }

                    if (!bDuplicate)
                    {
                        // Remove old item
                        m_lstTargetNames.Items.Remove(selection);
                        m_lstTargetFilterNames.Remove(selection);

                        // Add new item
                        m_lstTargetNames.Items.Add(form.VarName);
                        m_lstTargetFilterNames.Add(form.VarName);

                        // Adjust selection to point to new item
                        m_lstTargetNames.SelectedItem = form.VarName;

                        m_bTargetChanged = true;
                    }
                }
            }
        }

        /// <summary>
        /// Event for clicking the target delete button
        /// </summary>
        /// <param name="sender">object that fired the event</param>
        /// <param name="e">event arguments</param>
        private void BtnTargetListNamesDeleteClick(object sender, EventArgs e)
        {
            if (m_lstTargetNames.SelectedItem == null)
                return;

            var iIndex = m_lstTargetNames.SelectedIndex;

            var selection = m_lstTargetNames.SelectedItem.ToString();

            // Remove the selected item
            m_lstTargetFilterNames.Remove(selection);
            m_lstTargetNames.Items.Remove(selection);
            m_bTargetChanged = true;

            // Update selection
            if (m_lstTargetNames.Items.Count > 0)
            {
                if (iIndex >= m_lstTargetNames.Items.Count)
                    iIndex--;

                m_lstTargetNames.SelectedIndex = iIndex;
            }
        }

        /// <summary>
        /// Event for double clicking the target list box
        /// </summary>
        /// <param name="sender">object that fired the event</param>
        /// <param name="e">event arguments</param>
        private void LstTargetNamesMouseDoubleClick(object sender, MouseEventArgs e)
        {
            // Double clicking edits so piggy back existing method
            BtnTargetListNamesEditClick(sender, e);
        }

        /// <summary>
        /// Enable/disable edit & delete buttons based on if item is selected or not
        /// </summary>
        /// <param name="sender">object that fired the event</param>
        /// <param name="e">event arguments</param>
        private void LstLocalNamesSelectedIndexChanged(object sender, EventArgs e)
        {
            var lstBox = (ListBox)sender;

            if (lstBox.SelectedItem == null)
            {
                m_btnLocalListNamesEdit.Enabled = false;
                m_btnLocalListNamesDelete.Enabled = false;
            }
            else
            {
                m_btnLocalListNamesEdit.Enabled = true;
                m_btnLocalListNamesDelete.Enabled = true;
            }
        }

        /// <summary>
        /// Enable/disable edit & delete buttons based on if item is selected or not
        /// </summary>
        /// <param name="sender">object that fired the event</param>
        /// <param name="e">event arguments</param>
        private void LstTargetNamesSelectedIndexChanged(object sender, EventArgs e)
        {
            var lstBox = (ListBox)sender;

            if (lstBox.SelectedItem == null)
            {
                m_btnTargetListNamesEdit.Enabled = false;
                m_btnTargetListNamesDelete.Enabled = false;
            }
            else
            {
                m_btnTargetListNamesEdit.Enabled = true;
                m_btnTargetListNamesDelete.Enabled = true;
            }
        }
    }
}
