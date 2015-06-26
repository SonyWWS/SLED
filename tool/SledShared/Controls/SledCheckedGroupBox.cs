/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Sce.Sled.Shared.Controls
{
    /// <summary>
    /// Checked GroupBox control
    /// </summary>
    public class SledCheckedGroupBox : GroupBox
    {
        private const string PaddingString = "   ";

        private const int ChkWidth = 16;
        private const int ChkHeight = 16;

        /// <summary>
        /// Constructor
        /// </summary>
        public SledCheckedGroupBox()
        {
            m_chk =
                new CheckBox
                {
                    Location = new Point(0, 0),
                    Size = new Size(ChkWidth, ChkHeight),
                    Checked = true
                };
            m_chk.CheckedChanged += Chk_CheckedChanged;

            Controls.Add(m_chk);
        }

        /// <summary>
        /// Get or set the Text property
        /// </summary>
        public override string Text
        {
            get { return base.Text; }
            set
            {
                // In DesignMode don't add the extra padding. It messes up things
                // when the app is run live (ie. it will have extra padding).
                base.Text =
                    DesignMode
                        ? value
                        : value.Insert(0, PaddingString);
            }
        }

        /// <summary>
        /// Get or set whether the control is checked
        /// </summary>
        [BrowsableAttribute(false)]
        public bool Checked
        {
            get { return m_chk.Checked; }
            set { m_chk.Checked = value; }
        }

        /// <summary>
        /// Event when the GroupBox's Checked property changes
        /// </summary>
        public event EventHandler CheckedChanged;

        /// <summary>
        /// Disposes resources</summary>
        /// <param name="disposing">If true, then Dispose() called this method and managed resources should
        /// be released in addition to unmanaged resources. If false, then the finalizer called this method
        /// and no managed objects should be called and only unmanaged resources should be released.</param>
        protected override void Dispose(bool disposing)
        {
            m_dictEnabledState.Clear();

            if (disposing)
            {
                if (m_chk != null)
                {
                    m_chk.Dispose();
                    m_chk = null;
                }
            }

            base.Dispose(disposing);
        }

        /// <summary>
        /// Event fired when check state changes
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">Event arguments</param>
        private void Chk_CheckedChanged(object sender, EventArgs e)
        {
            if (m_bCheckChanging)
                return;

            try
            {
                m_bCheckChanging = true;

                var bChecked = m_chk.Checked;

                if (!bChecked)
                    SaveEnabledState();

                SetEnabledState(bChecked);

                // Fire CheckedChanged event
                var handler = CheckedChanged;
                if (handler != null)
                    handler(this, EventArgs.Empty);
            }
            finally
            {
                m_bCheckChanging = false;
            }
        }

        /// <summary>
        /// Save the Enabled state of each control in the GroupBox
        /// </summary>
        private void SaveEnabledState()
        {
            m_dictEnabledState.Clear();

            foreach (Control ctrl in Controls)
            {
                if (ctrl == m_chk)
                    continue;

                if (!m_dictEnabledState.ContainsKey(ctrl))
                    m_dictEnabledState.Add(ctrl, ctrl.Enabled);
            }
        }

        /// <summary>
        /// Set the Enabled state of each control in the GroupBox (but
        /// also take into account its previous Enabled state)
        /// </summary>
        /// <param name="bState">New state</param>
        private void SetEnabledState(bool bState)
        {
            foreach (Control ctrl in Controls)
            {
                if (ctrl == m_chk)
                    continue;

                if (!bState)
                {
                    // m_chk is not Checked so everything goes disabled
                    ctrl.Enabled = bState;
                }
                else
                {
                    // m_chk is Checked so restore Enabled states
                    bool bPrevEnabledState;
                    if (m_dictEnabledState.TryGetValue(ctrl, out bPrevEnabledState))
                    {
                        // Only enable the item is it was previously enabled
                        if (bPrevEnabledState)
                            ctrl.Enabled = bState;
                    }
                    else
                    {
                        // Not in the dictionary; what?
                        ctrl.Enabled = bState;
                    }
                }
            }
        }

        private CheckBox m_chk;
        private bool m_bCheckChanging;

        private readonly Dictionary<Control, bool> m_dictEnabledState =
            new Dictionary<Control, bool>();
    }
}
