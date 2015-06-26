/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

using System;
using System.Linq;
using System.Windows.Forms;

using Sce.Sled.Resources;
using Sce.Sled.Shared.Document;
using Sce.Sled.Shared.Plugin;
using Sce.Sled.Shared.Services;
using Sce.Sled.Shared.Utilities;
using Sce.Sled.SyntaxEditor;

namespace Sce.Sled
{
    public partial class SledBreakpointConditionForm : Form
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public SledBreakpointConditionForm()
        {
            InitializeComponent();

            // Add items to combo box
            m_cmbEnvironment.Items.Add(new ComboBoxItem("Global", false));
            m_cmbEnvironment.Items.Add(new ComboBoxItem("Environment", true));
            m_cmbEnvironment.SelectedIndex = 0;

            // Create SyntaxEditorControl
            m_txtCondition = TextEditorFactory.CreateSyntaxHighlightingEditor();

            // These values were what the previous TextBox used
            m_txtCondition.Control.Location = new System.Drawing.Point(12, 35);
            m_txtCondition.Control.Size = new System.Drawing.Size(383, 20);
            m_txtCondition.Control.TabIndex = 2;
            m_txtCondition.Control.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            
            // Set some special stuff to make it single line
            m_txtCondition.Multiline = false;
            m_txtCondition.LineNumberMarginVisible = false;
            m_txtCondition.IndicatorMarginVisible = false;

            Controls.Add(m_txtCondition.Control);

            m_txtCondition.Control.Focus();
        }

        /// <summary>
        /// Get/set the condition string
        /// </summary>
        public string Condition
        {
            get { return m_txtCondition.Text; }
            set { m_txtCondition.Text = value; }
        }

        /// <summary>
        /// Get/set the condition enabled flag
        /// </summary>
        public bool ConditionEnabled
        {
            get { return (m_chkCondition.CheckState == CheckState.Checked); }
            set
            {
                m_bModifyingCheckState = true;

                if (value)
                {   
                    m_chkCondition.CheckState = CheckState.Checked;                    

                    m_rdoIsTrue.Enabled = true;
                    m_rdoIsFalse.Enabled = true;
                    m_txtCondition.Control.Enabled = true;
                }
                else
                {
                    m_chkCondition.CheckState = CheckState.Unchecked;

                    m_rdoIsTrue.Enabled = false;
                    m_rdoIsFalse.Enabled = false;
                    m_txtCondition.Control.Enabled = false;
                }

                m_bModifyingCheckState = false;
            }
        }

        private bool m_bModifyingCheckState;

        /// <summary>
        /// Gets/sets the condition result
        /// </summary>
        public bool ConditionResult
        {
            get { return (m_rdoIsTrue.Checked); }
            set
            {
                if (value)
                    m_rdoIsTrue.Checked = true;
                else
                    m_rdoIsFalse.Checked = true;
            }
        }

        /// <summary>
        /// Whether to use the function's environment or _G
        /// </summary>
        public bool UseFunctionEnvironment
        {
            get
            {
                var item =
                    m_cmbEnvironment.SelectedItem as ComboBoxItem;

                return item != null && item.UseFunctionEnvironment;
            }

            set
            {
                // If already set to the correct value then bail
                if (UseFunctionEnvironment == value)
                    return;

                foreach (ComboBoxItem item in m_cmbEnvironment.Items)
                {
                    // Not the right item
                    if (item.UseFunctionEnvironment != value)
                        continue;

                    // Correct item already selected
                    if (m_cmbEnvironment.SelectedItem == item)
                        return;

                    // Select correct item
                    m_cmbEnvironment.SelectedItem = item;
                    return;
                }
            }
        }

        /// <summary>
        /// Toggle the state of some controls
        /// </summary>
        /// <param name="sender">object that fired the event</param>
        /// <param name="e">event arguments</param>
        private void ChkConditionCheckStateChanged(object sender, EventArgs e)
        {
            if (m_bModifyingCheckState)
                return;

            if (m_chkCondition.CheckState == CheckState.Checked)
            {
                m_rdoIsTrue.Enabled = true;
                m_rdoIsFalse.Enabled = true;
                m_txtCondition.Control.Enabled = true;
            }
            else
            {
                m_rdoIsTrue.Enabled = false;
                m_rdoIsFalse.Enabled = false;
                m_txtCondition.Control.Enabled = false;
            }
        }

        /// <summary>
        /// Validate the condition string has some text in it
        /// </summary>
        /// <param name="sender">object that fired the event</param>
        /// <param name="e">event arguments</param>
        private void BtnOkClick(object sender, EventArgs e)
        {
            // Need to make sure if a condition is being saved then it must be valid
            var bSyntaxCheckCondition = (ConditionEnabled || !string.IsNullOrEmpty(Condition));

            if (!bSyntaxCheckCondition)
                return;

            if (string.IsNullOrEmpty(Condition))
            {
                MessageBox.Show(
                    this,
                    Localization.SledBreakpointConditionErrorNoCondition,
                    Localization.SledBreakpointConditionError,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                    
                // Make user enter valid stuff
                DialogResult = DialogResult.None;
            }
            else
            {
                // Check syntax
                if (m_plugin != null)
                {
                    // Wrap the condition in a function like how it would be when running in libluaplugin
                    var szCondFunc = string.Format("function libluaplugin_condfunc(){0}    return ({1}){0}end", Environment.NewLine, Condition);
                        
                    // Format actual syntax checker to use
                    var szSyntaxCheckFunc =
                        string.Format("function libluaplugin_condfunc()\nreturn ({0})\nend", Condition);

                    var syntaxCheckerService = SledServiceInstance.Get<ISledSyntaxCheckerService>();

                    // Force a syntax check of the string
                    var errors = syntaxCheckerService.CheckString(m_plugin, szSyntaxCheckFunc);
                    if (errors.Any())
                    {
                        // Show error
                        MessageBox.Show(
                            this,
                            SledUtil.TransSub(
                                Localization.SledBreakpointConditionErrorVarArg,
                                Environment.NewLine, szCondFunc, errors.ElementAt(0).Error),
                            Localization.SledBreakpointConditionSyntaxError);

                        // Make user fix error
                        DialogResult = DialogResult.None;
                    }
                }
            }
        }

        /// <summary>
        /// Gets/sets the language plugin
        /// </summary>
        public ISledLanguagePlugin Plugin
        {
            get { return m_plugin; }
            set { m_plugin = value; }
        }

        /// <summary>
        /// Gets/sets the syntax highlighter to use
        /// </summary>
        public SledDocumentSyntaxHighlighter SyntaxHighlighter
        {
            get { return m_highlighter; }
            set
            {
                m_highlighter = value;

                // Set proper syntax highlighting
                SledDocumentSyntaxHighlighter.FeedHighlighterToSyntaxEditor(
                    m_highlighter,
                    m_txtCondition);
            }
        }

        #region Private Classes

        private class ComboBoxItem
        {
            public ComboBoxItem(string text, bool bUseFunctionEnvironment)
            {
                m_text = text;
                UseFunctionEnvironment = bUseFunctionEnvironment;
            }

            public override string ToString()
            {
                return m_text;
            }

            private readonly string m_text;

            public readonly bool UseFunctionEnvironment;
        }

        #endregion
        
        private bool m_bTxtConditionDisposed;

        private ISledLanguagePlugin m_plugin;
        private SledDocumentSyntaxHighlighter m_highlighter;

        private readonly ISyntaxEditorControl m_txtCondition;
    }
}
