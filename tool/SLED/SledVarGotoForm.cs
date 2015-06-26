/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

using System.Windows.Forms;

using Sce.Sled.Resources;
using Sce.Sled.Shared.Dom;
using Sce.Sled.Shared.Services;
using Sce.Sled.Shared.Utilities;

namespace Sce.Sled
{
    public partial class SledVarGotoForm : Form
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public SledVarGotoForm()
        {
            InitializeComponent();

            m_lstView.Columns.Add(Localization.SledFile);
            m_lstView.Columns.Add(Localization.SledLine);
            m_lstView.Columns.Add(Localization.SledOccurrence);
            m_lstView.Columns.Add(Localization.SledText);
        }

        /// <summary>
        /// Add location to the list
        /// </summary>
        /// <param name="loc"></param>
        /// <param name="lineText"></param>
        public void AddLocation(SledVarLocationType loc, string lineText)
        {
            var lstItem =
                new ListViewItem(
                    new[]
                    {
                        SledUtil.GetRelativePath(loc.File, m_projectService.Get.AssetDirectory),
                        loc.Line.ToString(),
                        loc.Occurence.ToString(),
                        lineText
                    }) {Tag = loc};

            m_lstView.Items.Add(lstItem);

            m_lstView.Columns[0].Width = -1;
            m_lstView.Columns[1].Width = -1;
            m_lstView.Columns[2].Width = -1;
            m_lstView.Columns[3].Width = -2;
        }

        /// <summary>
        /// Return the selected location
        /// </summary>
        public SledVarLocationType SelectedLocation { get; private set; }

        /// <summary>
        /// Event for double clicking an item
        /// </summary>
        /// <param name="sender">object that fired the event</param>
        /// <param name="e">event arguments</param>
        private void LstView_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            var lstView = (ListView)sender;
            if (lstView == null)
                return;

            var lstItem = lstView.GetItemAt(e.X, e.Y);
            if (lstItem == null)
                return;

            var loc = (SledVarLocationType)lstItem.Tag;
            if (loc == null)
                return;

            // Store selected variable
            SelectedLocation = loc;

            // Close form
            DialogResult = DialogResult.OK;
        }

        private readonly SledServiceReference<ISledProjectService> m_projectService =
            new SledServiceReference<ISledProjectService>();
    }
}
