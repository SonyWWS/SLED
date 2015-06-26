/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

using System;
using System.Collections.Generic;
using System.Drawing;

using Sce.Sled.Shared.Utilities;

namespace Sce.Sled
{
    /// <summary>
    /// TTY Filter Result
    /// </summary>
    public enum SledTtyFilterResult
    {
        /// <summary>
        /// Show items that pass this filter
        /// </summary>
        Show,

        /// <summary>
        /// Ignore items that pass this filter
        /// </summary>
        Ignore
    }

    /// <summary>
    /// Class that represents a TTY Filter
    /// </summary>
    public class SledTtyFilter
    {
        /// <summary>
        /// Default constructor to use default colors
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="result"></param>
        public SledTtyFilter(string filter, SledTtyFilterResult result)
        {
            Filter = filter;
            Result = result;
        }

        /// <summary>
        /// Constructor to use custom colors
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="result"></param>
        /// <param name="txtColor"></param>
        /// <param name="bgColor"></param>
        public SledTtyFilter(string filter, SledTtyFilterResult result, Color txtColor, Color bgColor)
            : this(filter, result)
        {
            TextColor = txtColor;
            BackgroundColor = bgColor;
        }

        /// <summary>
        /// Gets/sets the filter string
        /// </summary>
        public string Filter
        {
            get { return m_filter; }
            set
            {
                m_filter = value;

                // Empty
                m_lstFilters.Clear();

                // Positions of all asterisks
                m_asterisks = SledUtil.IndicesOf(m_filter, '*');
                if (m_asterisks.Length > 0)
                {
                    // 1 or more asterisk, parse out strings

                    // Is first character of filter an asterisk?
                    m_bFirst = (m_asterisks[0] == 0);

                    // Is last character of filter an asterisk?
                    m_bLast = (m_asterisks[m_asterisks.Length - 1] == (m_filter.Length - 1));

                    // Grab all patterns
                    var filters = m_filter.Split(s_chPatternSep, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var filter in filters)
                        m_lstFilters.Add(filter);
                }
                else
                {
                    // Just one item and no asterisks
                    m_lstFilters.Add(m_filter);
                    m_bFirst = false;
                    m_bLast = false;
                }
            }
        }

        /// <summary>
        /// A list of all the filters
        /// </summary>
        public List<string> FilterList
        {
            get { return m_lstFilters; }
        }

        /// <summary>
        /// A list of all asterisk positions in Filter
        /// </summary>
        public int[] Asterisks
        {
            get { return m_asterisks; }
        }

        /// <summary>
        /// Gets whether there is an asterisk at the beginning of the filter
        /// </summary>
        public bool FirstAsterisk
        {
            get { return m_bFirst; }
        }

        /// <summary>
        /// Gets whether there is an asterisk at the end of the filter string
        /// </summary>
        public bool LastAsterisk
        {
            get { return m_bLast; }
        }

        /// <summary>
        /// Gets/sets the result of the TTY filter (to show or not to show items that pass this filter)
        /// </summary>
        public SledTtyFilterResult Result { get; set; }

        /// <summary>
        /// Gets the text color assigned to this filter
        /// </summary>
        public Color TextColor
        {
            get { return m_txtColor; }
            set { m_txtColor = value; }
        }

        /// <summary>
        /// Gets the background color assigned to this filter
        /// </summary>
        public Color BackgroundColor
        {
            get { return m_bgColor; }
            set { m_bgColor = value; }
        }

        /// <summary>
        /// For displaying in listbox
        /// </summary>
        /// <returns>string representation of a TTY Filter</returns>
        public override string ToString()
        {
            return
                Result == SledTtyFilterResult.Show
                    ? string.Format("{0} (Text {1}) (Bg {2})", Filter, TextColor, BackgroundColor)
                    : string.Format("{0} - Ignore", Filter);
        }

        private string m_filter;
        private readonly List<string> m_lstFilters = new List<string>();
        private int[] m_asterisks;
        private bool m_bFirst;
        private bool m_bLast;
        private Color m_txtColor = Color.Black;
        private Color m_bgColor = Color.White;
        private readonly static char[] s_chPatternSep = new[] { '*' };
    }
}
