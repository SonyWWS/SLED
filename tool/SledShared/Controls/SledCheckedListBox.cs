/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

using System.Collections.Generic;
using System.Windows.Forms;

namespace Sce.Sled.Shared.Controls
{
    /// <summary>
    /// Modified CheckListBox to be able to hide items that aren't checked
    /// </summary>
    public class SledCheckedListBox : CheckedListBox
    {
        /// <summary>
        /// Get or set the hide unchecked items property
        /// <remarks>True if items get hidden, false if hidden items get re-added</remarks>
        /// </summary>
        public bool HideUncheckedItems
        {
            get { return m_bHideUncheckedItems; }
            set
            {
                m_bHideUncheckedItems = value;

                if (value)
                {
                    var lstRemove = new List<object>();
                    foreach (var obj in Items)
                    {
                        if (CheckedItems.Contains(obj))
                            continue;

                        lstRemove.Add(obj);
                        m_items.Add(obj);
                    }

                    foreach (var obj in lstRemove)
                    {
                        Items.Remove(obj);
                    }
                }
                else
                {
                    foreach (var obj in m_items)
                    {
                        Items.Add(obj);
                    }

                    m_items.Clear();
                }
            }
        }

        private bool m_bHideUncheckedItems;
        private readonly List<object> m_items = new List<object>();
    }
}
