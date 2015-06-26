/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

using System;
using System.Windows.Forms;

using Sce.Atf.Applications;
using Sce.Atf.Dom;

namespace Sce.Sled.Shared.Dom
{
    /// <summary>
    /// Complex Type for a call stack entry
    /// </summary>
    public class SledFindResultsType : DomNodeAdapter, IItemView
    {
        /// <summary>
        /// Get or set the name
        /// </summary>
        public string Name
        {
            get { return GetAttribute<string>(SledSchema.SledFindResultsType.nameAttribute); }
            set { SetAttribute(SledSchema.SledFindResultsType.nameAttribute, value); }
        }

        /// <summary>
        /// Get or set the file
        /// </summary>
        public string File
        {
            get { return GetAttribute<string>(SledSchema.SledFindResultsType.fileAttribute); }
            set { SetAttribute(SledSchema.SledFindResultsType.fileAttribute, value); }
        }

        /// <summary>
        /// Get or set the line
        /// </summary>
        public int Line
        {
            get { return GetAttribute<int>(SledSchema.SledFindResultsType.lineAttribute); }
            set { SetAttribute(SledSchema.SledFindResultsType.lineAttribute, value); }
        }

        /// <summary>
        /// Get or set the start_offset attribute
        /// </summary>
        public int StartOffset
        {
            get { return GetAttribute<int>(SledSchema.SledFindResultsType.start_offsetAttribute); }
            set { SetAttribute(SledSchema.SledFindResultsType.start_offsetAttribute, value); }
        }

        /// <summary>
        /// Get or set the end_offset attribute
        /// </summary>
        public int EndOffset
        {
            get { return GetAttribute<int>(SledSchema.SledFindResultsType.end_offsetAttribute); }
            set { SetAttribute(SledSchema.SledFindResultsType.end_offsetAttribute, value); }
        }

        /// <summary>
        /// Get or set the line_text attribute
        /// </summary>
        public string LineText
        {
            get { return GetAttribute<string>(SledSchema.SledFindResultsType.line_textAttribute); }
            set { SetAttribute(SledSchema.SledFindResultsType.line_textAttribute, value); }
        }

        #region IItemView Interface

        /// <summary>
        /// Fills in or modifies the given display info for the item</summary>
        /// <param name="item">Item</param>
        /// <param name="info">Display info to update</param>
        public void GetInfo(object item, ItemInfo info)
        {
            var file = s_bShowFileNamesOnly ? Name : File;
            info.Label = file;
            info.Properties = new[] { Line.ToString(), LineText };
            info.IsLeaf = true;
            info.ImageIndex = info.GetImageIndex(Atf.Resources.DocumentImage);
        }

        #endregion

        /// <summary>
        /// Column names
        /// </summary>
        public static readonly string[] TheColumnNames =
        {
            "File",
            "Line",
            "Text"
        };

        /// <summary>
        /// Get or set whether to just show the file name or the full path to the file
        /// </summary>
        public static bool ShowFileNamesOnly
        {
            get { return s_bShowFileNamesOnly; }
            set { s_bShowFileNamesOnly = value; }
        }

        /// <summary>
        /// Compare two find results
        /// </summary>
        /// <param name="x">First result</param>
        /// <param name="y">Second result</param>
        /// <param name="column">Column of result</param>
        /// <param name="order">SortOrder</param>
        /// <returns>Less than zero: x is less than y. Zero: x equals y. Greater than zero: x is greater than y.</returns>
        public static int Compare(SledFindResultsType x, SledFindResultsType y, int column, SortOrder order)
        {
            int result;

            switch (column)
            {
                default:
                {
                    result = CompareFiles(x, y);

                    if (result == 0)
                        result = CompareLines(x, y);

                    if (result == 0)
                        result = CompareLineTexts(x, y);
                }
                break;

                case 1:
                {
                    result = CompareLines(x, y);

                    if (result == 0)
                        result = CompareFiles(x, y);

                    if (result == 0)
                        result = CompareLineTexts(x, y);
                }
                break;

                case 2:
                {
                    result = CompareLineTexts(x, y);

                    if (result == 0)
                        result = CompareFiles(x, y);

                    if (result == 0)
                        result = CompareLines(x, y);
                }
                break;
            }

            if (order == SortOrder.Descending)
                result *= -1;

            return result;
        }

        private static int CompareFiles(SledFindResultsType findResult1, SledFindResultsType findResult2)
        {
            return
                string.Compare(
                    s_bShowFileNamesOnly
                        ? findResult1.Name
                        : findResult1.File,
                    s_bShowFileNamesOnly
                        ? findResult2.Name
                        : findResult2.File,
                    StringComparison.OrdinalIgnoreCase);
        }

        private static int CompareLines(SledFindResultsType findResult1, SledFindResultsType findResult2)
        {
            return
                findResult1.Line == findResult2.Line
                    ? 0
                    : findResult1.Line < findResult2.Line
                        ? -1
                        : 1;
        }

        private static int CompareLineTexts(SledFindResultsType findResult1, SledFindResultsType findResult2)
        {
            return
                string.Compare(
                    findResult1.LineText,
                    findResult2.LineText,
                    StringComparison.OrdinalIgnoreCase);
        }

        private static bool s_bShowFileNamesOnly;
    }
}
