/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml;

using Sce.Sled.Resources;
using Sce.Sled.Shared.Utilities;

namespace Sce.Sled
{
    public partial class SledTtyFilterForm : Form
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public SledTtyFilterForm()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Gets/sets the TTY filter list
        /// </summary>
        public List<SledTtyFilter> TtyFilterList
        {
            get { return m_lstTtyFilters; }
            set
            {
                m_lstTtyFilters = value;

                foreach (var filter in m_lstTtyFilters)
                    m_lstBoxFilters.Items.Add(filter);
            }
        }

        /// <summary>
        /// Event for clicking the add button
        /// </summary>
        /// <param name="sender">object that fired the event</param>
        /// <param name="e">event arguments</param>
        private void BtnAdd_Click(object sender, EventArgs e)
        {
            using (var form = new SledTtyFilterNameForm())
            {
                if (form.ShowDialog(this) == DialogResult.OK)
                {
                    TryAddTtyFilter(
                        new SledTtyFilter(
                            form.FilterName,
                            form.FilterResult,
                            form.TextColor,
                            form.BackgroundColor));
                }
            }
        }

        /// <summary>
        /// Event for clicking the edit button
        /// </summary>
        /// <param name="sender">object that fired the event</param>
        /// <param name="e">event arguments</param>
        private void BtnEdit_Click(object sender, EventArgs e)
        {
            if (m_lstBoxFilters.SelectedItem == null)
                return;

            var selection =
                m_lstBoxFilters.SelectedItem as SledTtyFilter;

            if (selection == null)
                return;

            using (var form = new SledTtyFilterNameForm())
            {
                form.FilterName = selection.Filter;
                form.TextColor = selection.TextColor;
                form.BackgroundColor = selection.BackgroundColor;
                form.FilterResult = selection.Result;
                
                if (form.ShowDialog(this) != DialogResult.OK)
                    return;

                var bCanEdit = true;

                // Grab name
                var szFilterName = form.FilterName;

                // Check if the name was changed (no duplicate names!)
                if (szFilterName != selection.Filter)
                {
                    // Name changed, verify its not a duplicate
                    var bFound =
                        m_lstTtyFilters.Any(
                            filter => filter.Filter == szFilterName);

                    if (bFound)
                        bCanEdit = false;
                }

                if (!bCanEdit)
                    return;

                // Make the edit
                var editedFilter =
                    new SledTtyFilter(
                        form.FilterName,
                        form.FilterResult,
                        form.TextColor,
                        form.BackgroundColor);

                // Remove selected item
                m_lstBoxFilters.Items.Remove(selection);
                m_lstTtyFilters.Remove(selection);

                // Add new item
                m_lstBoxFilters.Items.Add(editedFilter);
                m_lstTtyFilters.Add(editedFilter);

                // Select new item
                m_lstBoxFilters.SelectedItem = editedFilter;
            }
        }

        /// <summary>
        /// Event for clicking the delete button
        /// </summary>
        /// <param name="sender">object that fired the event</param>
        /// <param name="e">event arguments</param>
        private void BtnDelete_Click(object sender, EventArgs e)
        {
            if (m_lstBoxFilters.SelectedItem == null)
                return;

            // Save current selection index
            var iIndex = m_lstBoxFilters.SelectedIndex;

            // Grab selected item
            var selection = (SledTtyFilter)m_lstBoxFilters.SelectedItem;

            // Remove selected item from the listbox
            m_lstBoxFilters.Items.Remove(selection);

            // Remove selected item from the TTY filter list
            m_lstTtyFilters.Remove(selection);

            // Update selection if any items are remaining
            if (m_lstBoxFilters.Items.Count <= 0)
                return;
            
            if (iIndex >= m_lstBoxFilters.Items.Count)
                iIndex--;

            m_lstBoxFilters.SelectedIndex = iIndex;
        }

        /// <summary>
        /// Event for double clicking the listbox of filters
        /// </summary>
        /// <param name="sender">object that fired the event</param>
        /// <param name="e">event arguments</param>
        private void LstFilters_DoubleClick(object sender, EventArgs e)
        {
            BtnEdit_Click(sender, e);
        }

        /// <summary>
        /// Event when pressing the close button
        /// </summary>
        /// <param name="sender">object that fired the event</param>
        /// <param name="e">event arguments</param>
        private void BtnClose_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
        }

        /// <summary>
        /// Event for clicking load file button
        /// </summary>
        /// <param name="sender">object that fired the event</param>
        /// <param name="e">event arguments</param>
        private void BtnLoadFile_Click(object sender, EventArgs e)
        {
            using (var dlg = new OpenFileDialog())
            {
                dlg.Multiselect = false;
                if (dlg.ShowDialog(this) == DialogResult.OK)
                    TryLoadTtyFilterFile(dlg.FileName);
            }
        }

        /// <summary>
        /// Event for clicking save to file button
        /// </summary>
        /// <param name="sender">object that fired the event</param>
        /// <param name="e">event arguments</param>
        private void BtnSaveFile_Click(object sender, EventArgs e)
        {
            if (m_lstTtyFilters.Count <= 0)
            {
                MessageBox.Show(
                    this,
                    Localization.SledTTYFilterEntriesSave,
                    Localization.SledTTYFilter,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);

                return;
            }

            using (var dlg = new SaveFileDialog())
            {
                dlg.AddExtension = true;
                dlg.CreatePrompt = true;
                dlg.DefaultExt = ".xml";
                dlg.Filter = "Xml Files (*.xml)|*.xml";

                if (dlg.ShowDialog(this) == DialogResult.OK)
                    TrySaveTtyFilterFile(dlg.FileName);
            }
        }

        /// <summary>
        /// Try and add a SledTtyFilter to the list
        /// </summary>
        /// <param name="filter"></param>
        private void TryAddTtyFilter(SledTtyFilter filter)
        {
            // Search for existing filter with same filter patern
            var bFound =
                m_lstTtyFilters.Any(
                    ttyFilter => ttyFilter.Filter == filter.Filter);

            if (bFound)
                return;

            // Add to TTY list
            m_lstTtyFilters.Add(filter);

            // Add to listbox
            m_lstBoxFilters.Items.Add(filter);

            // Select item
            m_lstBoxFilters.SelectedItem = filter;
        }

        /// <summary>
        /// Load a TTY filter file
        /// </summary>
        /// <param name="szAbsPath"></param>
        private void TryLoadTtyFilterFile(string szAbsPath)
        {
            try
            {
                var xmlDoc = new XmlDocument();
                xmlDoc.Load(szAbsPath);

                if (xmlDoc.DocumentElement == null)
                    return;

                var nodes = xmlDoc.DocumentElement.SelectNodes("TTYFilter");
                if ((nodes == null) || (nodes.Count == 0))
                {
                    MessageBox.Show(
                        this,
                        SledUtil.TransSub(Localization.SledTTYFilterNoNodesFoundInFile, szAbsPath),
                        Localization.SledTTYFilterFileError,
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);

                    return;
                }

                foreach (XmlElement elem in nodes)
                {
                    var filter = new SledTtyFilter(
                        elem.GetAttribute("filter"),
                        ((int.Parse(elem.GetAttribute("result")) == 1) ? SledTtyFilterResult.Show : SledTtyFilterResult.Ignore),
                        Color.FromArgb(
                            int.Parse(elem.GetAttribute("txtColorR")),
                            int.Parse(elem.GetAttribute("txtColorG")),
                            int.Parse(elem.GetAttribute("txtColorB"))
                            ),
                        Color.FromArgb(
                            int.Parse(elem.GetAttribute("bgColorR")),
                            int.Parse(elem.GetAttribute("bgColorG")),
                            int.Parse(elem.GetAttribute("bgColorB"))
                            ));

                    TryAddTtyFilter(filter);
                }
            }
            catch
            {
                MessageBox.Show(
                    this,
                    SledUtil.TransSub(Localization.SledTTYFilterErrorLoadingFile, szAbsPath),
                    Localization.SledTTYFilterFileError,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Try and save the TTY filter list to a file
        /// </summary>
        /// <param name="szAbsPath"></param>
        private void TrySaveTtyFilterFile(string szAbsPath)
        {
            // Generate Xml document to contain TTY filter list
            var xmlDoc = new XmlDocument();
            xmlDoc.AppendChild(xmlDoc.CreateXmlDeclaration("1.0", "utf-8", "yes"));
            var root = xmlDoc.CreateElement("TTYFilters");
            xmlDoc.AppendChild(root);

            foreach (var filter in m_lstTtyFilters)
            {
                var elem = xmlDoc.CreateElement("TTYFilter");
                elem.SetAttribute("filter", filter.Filter);
                elem.SetAttribute("txtColorR", filter.TextColor.R.ToString());
                elem.SetAttribute("txtColorG", filter.TextColor.G.ToString());
                elem.SetAttribute("txtColorB", filter.TextColor.B.ToString());
                elem.SetAttribute("bgColorR", filter.BackgroundColor.R.ToString());
                elem.SetAttribute("bgColorG", filter.BackgroundColor.G.ToString());
                elem.SetAttribute("bgColorB", filter.BackgroundColor.B.ToString());
                elem.SetAttribute("result", (filter.Result == SledTtyFilterResult.Show) ? "1" : "0");
                root.AppendChild(elem);
            }

            try
            {
                // Try to write to disk
                var xmlWriter = new XmlTextWriter(szAbsPath, Encoding.UTF8);
                xmlDoc.WriteTo(xmlWriter);
                xmlWriter.Close();
            }
            catch
            {
                MessageBox.Show(
                    this,
                    SledUtil.TransSub(Localization.SledTTYFilterErrorWritingFile, szAbsPath),
                    Localization.SledTTYFilterFileError,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private List<SledTtyFilter> m_lstTtyFilters;        
    }
}
