/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

using System;
using System.ComponentModel;
using System.Windows.Forms;

using Sce.Sled.Shared.Utilities;

namespace Sce.Sled.Shared.Controls
{
    /// <summary>
    /// SledAsyncTaskForm Class
    /// </summary>
    public partial class SledAsyncTaskForm : Form
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public SledAsyncTaskForm()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Gets/sets the task to run
        /// </summary>
        public Func<SledUtil.BoolWrapper, bool> Task { get; set; }

        /// <summary>
        /// Gets/sets the label
        /// </summary>
        public string Label
        {
            get { return m_label.Text; }
            set { SetLabel(value); }
        }

        private delegate void SetLabelDelegate(string value);
        private void SetLabel(string value)
        {
            if (Disposing)
                return;

            if (InvokeRequired)
            {
                BeginInvoke(new SetLabelDelegate(SetLabel), value);
                return;
            }

            m_label.Text = value;
        }

        private void SledAsyncTaskFormShown(object sender, EventArgs e)
        {
            m_worker = new BackgroundWorker {WorkerSupportsCancellation = true};
            m_worker.DoWork += WorkerDoWork;
            m_worker.RunWorkerCompleted += WorkerRunWorkerCompleted;
            m_worker.RunWorkerAsync(Task);
        }

        private void SledAsyncTaskFormFormClosing(object sender, FormClosingEventArgs e)
        {
            if (m_worker == null)
                return;

            if (!m_worker.IsBusy)
                return;

            if (m_worker.CancellationPending)
            {
                e.Cancel = true;
                return;
            }

            m_worker.CancelAsync();
            m_cancel.Value = true;
            e.Cancel = true;
        }

        private void WorkerDoWork(object sender, DoWorkEventArgs e)
        {
            var task = (Func<SledUtil.BoolWrapper, bool>)e.Argument;
            
            e.Result = task(m_cancel);

            if (m_cancel.Value)
                e.Cancel = true;
        }

        private void WorkerRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (Disposing)
                return;

            if (e.Error != null)
                DialogResult = DialogResult.Abort;
            else if (e.Cancelled)
                DialogResult = DialogResult.Cancel;
            else
            {
                var result = (bool) e.Result;
                DialogResult = result ? DialogResult.Yes : DialogResult.No;
            }
        }

        private BackgroundWorker m_worker;

        private readonly SledUtil.BoolWrapper m_cancel =
            new SledUtil.BoolWrapper();
    }
}
