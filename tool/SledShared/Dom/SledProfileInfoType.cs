/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

using System;
using System.Collections.Generic;
using System.Windows.Forms;

using Sce.Atf.Adaptation;
using Sce.Atf.Applications;
using Sce.Atf.Dom;

using Sce.Sled.Shared.Resources;
using Sce.Sled.Shared.Utilities;

namespace Sce.Sled.Shared.Dom
{
    /// <summary>
    /// Complex Type for a profile info entry
    /// </summary>
    public class SledProfileInfoType : DomNodeAdapter, IItemView, ICloneable
    {
        /// <summary>
        /// Gets or sets name
        /// </summary>
        public string Name
        {
            get { return Function; }
            set { Function = value; }
        }

        /// <summary>
        /// Gets or sets the function attribute
        /// </summary>
        public string Function
        {
            get { return GetAttribute<string>(SledSchema.SledProfileInfoType.functionAttribute); }
            set { SetAttribute(SledSchema.SledProfileInfoType.functionAttribute, value); }
        }

        /// <summary>
        /// Gets or sets the line attribute
        /// </summary>
        public int Line
        {
            get { return GetAttribute<int>(SledSchema.SledProfileInfoType.lineAttribute); }
            set { SetAttribute(SledSchema.SledProfileInfoType.lineAttribute, value); }
        }

        /// <summary>
        /// Gets the ProfileInfo sequence
        /// </summary>
        public IList<SledProfileInfoType> ProfileInfo
        {
            get { return GetChildList<SledProfileInfoType>(SledSchema.SledProfileInfoType.ProfileInfoChild); }
        }

        /// <summary>
        /// Gets or sets the file attribute
        /// </summary>
        public string File
        {
            get { return GetAttribute<string>(SledSchema.SledProfileInfoType.fileAttribute); }
            set { SetAttribute(SledSchema.SledProfileInfoType.fileAttribute, value); }
        }

        /// <summary>
        /// Gets or sets the time_total attribute
        /// </summary>
        public float TimeTotal
        {
            get { return GetAttribute<float>(SledSchema.SledProfileInfoType.time_totalAttribute); }
            set { SetAttribute(SledSchema.SledProfileInfoType.time_totalAttribute, value); }
        }

        /// <summary>
        /// Gets or sets the time_avg attribute
        /// </summary>
        public float TimeAverage
        {
            get { return GetAttribute<float>(SledSchema.SledProfileInfoType.time_avgAttribute); }
            set { SetAttribute(SledSchema.SledProfileInfoType.time_avgAttribute, value); }
        }

        /// <summary>
        /// Gets or sets the time_min attribute
        /// </summary>
        public float TimeMin
        {
            get { return GetAttribute<float>(SledSchema.SledProfileInfoType.time_minAttribute); }
            set { SetAttribute(SledSchema.SledProfileInfoType.time_minAttribute, value); }
        }

        /// <summary>
        /// Gets or sets the time_max attribute
        /// </summary>
        public float TimeMax
        {
            get { return GetAttribute<float>(SledSchema.SledProfileInfoType.time_maxAttribute); }
            set { SetAttribute(SledSchema.SledProfileInfoType.time_maxAttribute, value); }
        }

        /// <summary>
        /// Gets or sets the time_total_inner attribute
        /// </summary>
        public float TimeTotalInner
        {
            get { return GetAttribute<float>(SledSchema.SledProfileInfoType.time_total_innerAttribute); }
            set { SetAttribute(SledSchema.SledProfileInfoType.time_total_innerAttribute, value); }
        }

        /// <summary>
        /// Gets or sets the time_avg_inner attribute
        /// </summary>
        public float TimeAverageInner
        {
            get { return GetAttribute<float>(SledSchema.SledProfileInfoType.time_avg_innerAttribute); }
            set { SetAttribute(SledSchema.SledProfileInfoType.time_avg_innerAttribute, value); }
        }

        /// <summary>
        /// Gets or sets the time_min_inner attribute
        /// </summary>
        public float TimeMinInner
        {
            get { return GetAttribute<float>(SledSchema.SledProfileInfoType.time_min_innerAttribute); }
            set { SetAttribute(SledSchema.SledProfileInfoType.time_min_innerAttribute, value); }
        }

        /// <summary>
        /// Gets or sets the time_max_inner attribute
        /// </summary>
        public float TimeMaxInner
        {
            get { return GetAttribute<float>(SledSchema.SledProfileInfoType.time_max_innerAttribute); }
            set { SetAttribute(SledSchema.SledProfileInfoType.time_max_innerAttribute, value); }
        }

        /// <summary>
        /// Gets or sets the num_calls attribute
        /// </summary>
        public int NumCalls
        {
            get { return GetAttribute<int>(SledSchema.SledProfileInfoType.num_callsAttribute); }
            set { SetAttribute(SledSchema.SledProfileInfoType.num_callsAttribute, value); }
        }

        /// <summary>
        /// Gets or sets the num_funcs_called attribute
        /// </summary>
        public int NumFuncsCalled
        {
            get { return GetAttribute<int>(SledSchema.SledProfileInfoType.num_funcs_calledAttribute); }
            set { SetAttribute(SledSchema.SledProfileInfoType.num_funcs_calledAttribute, value); }
        }

        #region IItemView Interface

        /// <summary>
        /// Fills in or modifies the given display info for the item</summary>
        /// <param name="item">Item</param>
        /// <param name="info">Display info to update</param>
        public void GetInfo(object item, ItemInfo info)
        {
            info.Label = Function;
            info.Properties =
                new[]
                {
                    NumCalls.ToString(),
                    TimeTotal.ToString(),
                    TimeAverage.ToString(),
                    TimeMin.ToString(),
                    TimeMax.ToString(),
                    TimeTotalInner.ToString(),
                    TimeAverageInner.ToString(),
                    TimeMinInner.ToString(),
                    TimeMaxInner.ToString()
                };
            info.IsLeaf = !CanBeLookedUp;
            info.ImageIndex = info.GetImageIndex(Atf.Resources.DataImage);
        }

        #endregion

        /// <summary>
        /// Gets or sets node unique name
        /// </summary>
        public string NodeUniqueName { get; set; }

        /// <summary>
        /// Gets or sets node sort name
        /// </summary>
        public string NodeSortName { get; set; }

        /// <summary>
        /// Generate a unique name for this item
        /// </summary>
        public void GenerateUniqueName()
        {
            NodeUniqueName = "PI:" + Function + ":" + File + ":" + Line;
            NodeSortName = NodeUniqueName;

            if (m_md5Hash == 0)
                m_md5Hash = SledUtil.GetMd5HashForText(NodeUniqueName);
        }

        private Int64 m_md5Hash;

        /// <summary>
        /// Normal column names
        /// </summary>
        public static readonly string[] NormalColumnNames =
        {
            Localization.SledSharedFunction,
            Localization.SledSharedCalls,
            Localization.SledSharedTotal,
            Localization.SledSharedAverage,
            Localization.SledSharedShortest,
            Localization.SledSharedLongest,
            Localization.SledSharedTotalInner,
            Localization.SledSharedAverageInner,
            Localization.SledSharedShortestInner,
            Localization.SledSharedLongestInner
        };

        /// <summary>
        /// Call graph column names
        /// </summary>
        public static readonly string[] CallGraphColumnNames = { Localization.SledSharedFunction };

        /// <summary>
        /// Compare this SledProfileInfoType to another object
        /// </summary>
        /// <param name="x">First object</param>
        /// <param name="y">Second object</param>
        /// <param name="column">Column of objects</param>
        /// <param name="order">SortOrder</param>
        /// <returns>Less than zero: x is less than y. Zero: x equals y. Greater than zero: x is greater than y.</returns>
        public static int Compare(SledProfileInfoType x, SledProfileInfoType y, int column, SortOrder order)
        {
            var result = 0;

            var lstFuncs = new List<SortFunction>();

            // Add current column
            switch (column)
            {
                default: lstFuncs.Add(CompareFunctions); break;
                case 1: lstFuncs.Add(CompareNumCalls); break;
                case 2: lstFuncs.Add(CompareTotal); break;
                case 3: lstFuncs.Add(CompareAverage); break;
                case 4: lstFuncs.Add(CompareShortest); break;
                case 5: lstFuncs.Add(CompareLongest); break;
                case 6: lstFuncs.Add(CompareTotalInner); break;
                case 7: lstFuncs.Add(CompareAverageInner); break;
                case 8: lstFuncs.Add(CompareShortestInner); break;
                case 9: lstFuncs.Add(CompareLongestInner); break;
            }

            // Add others for tie breaking
            if (!lstFuncs.Contains(CompareFunctions))
                lstFuncs.Add(CompareFunctions);
            if (!lstFuncs.Contains(CompareNumCalls))
                lstFuncs.Add(CompareNumCalls);
            if (!lstFuncs.Contains(CompareTotal))
                lstFuncs.Add(CompareTotal);

            for (var i = 0; i < lstFuncs.Count; i++)
            {
                result = lstFuncs[i](x, y);
                if (result != 0)
                    break;
            }

            if (order == SortOrder.Descending)
                result *= -1;

            return result;
        }

        private bool CanBeLookedUp
        {
            get
            {
                if (NumFuncsCalled <= 0)
                    return false;

                return string.Compare(Function, LuaCFunc, StringComparison.Ordinal) != 0;
            }
        }

        #region ICloneable

        /// <summary>
        /// Clone the item
        /// </summary>
        /// <returns>A new cloned object</returns>
        public object Clone()
        {
            var copy = DomNode.Copy(new[] { DomNode });
            copy[0].InitializeExtensions();

            return copy[0].As<SledProfileInfoType>();
        }

        #endregion

        #region Sort Functions

        private static int CompareFunctions(SledProfileInfoType pi1, SledProfileInfoType pi2)
        {
            return string.Compare(pi1.Function, pi2.Function, StringComparison.CurrentCulture);
        }

        private static int CompareNumCalls(SledProfileInfoType pi1, SledProfileInfoType pi2)
        {
            var iNum1 = pi1.NumCalls;
            var iNum2 = pi2.NumCalls;

            if (iNum1 == iNum2)
                return 0;

            if (iNum1 < iNum2)
                return -1;

            return 1;
        }

        private static int CompareTotal(SledProfileInfoType pi1, SledProfileInfoType pi2)
        {
            var fl1 = pi1.TimeTotal;
            var fl2 = pi2.TimeTotal;

            if (fl1 == fl2)
                return 0;

            if (fl1 < fl2)
                return -1;

            return 1;
        }

        private static int CompareAverage(SledProfileInfoType pi1, SledProfileInfoType pi2)
        {
            var fl1 = pi1.TimeAverage;
            var fl2 = pi2.TimeAverage;

            if (fl1 == fl2)
                return 0;

            if (fl1 < fl2)
                return -1;

            return 1;
        }

        private static int CompareShortest(SledProfileInfoType pi1, SledProfileInfoType pi2)
        {
            var fl1 = pi1.TimeMin;
            var fl2 = pi2.TimeMin;

            if (fl1 == fl2)
                return 0;

            if (fl1 < fl2)
                return -1;

            return 1;
        }

        private static int CompareLongest(SledProfileInfoType pi1, SledProfileInfoType pi2)
        {
            var fl1 = pi1.TimeMax;
            var fl2 = pi2.TimeMax;

            if (fl1 == fl2)
                return 0;

            if (fl1 < fl2)
                return -1;

            return 1;
        }

        private static int CompareTotalInner(SledProfileInfoType pi1, SledProfileInfoType pi2)
        {
            var fl1 = pi1.TimeTotalInner;
            var fl2 = pi2.TimeTotalInner;

            if (fl1 == fl2)
                return 0;

            if (fl1 < fl2)
                return -1;

            return 1;
        }

        private static int CompareAverageInner(SledProfileInfoType pi1, SledProfileInfoType pi2)
        {
            var fl1 = pi1.TimeAverageInner;
            var fl2 = pi2.TimeAverageInner;

            if (fl1 == fl2)
                return 0;

            if (fl1 < fl2)
                return -1;

            return 1;
        }

        private static int CompareShortestInner(SledProfileInfoType pi1, SledProfileInfoType pi2)
        {
            var fl1 = pi1.TimeMinInner;
            var fl2 = pi2.TimeMinInner;

            if (fl1 == fl2)
                return 0;

            if (fl1 < fl2)
                return -1;

            return 1;
        }

        private static int CompareLongestInner(SledProfileInfoType pi1, SledProfileInfoType pi2)
        {
            var fl1 = pi1.TimeMaxInner;
            var fl2 = pi2.TimeMaxInner;

            if (fl1 == fl2)
                return 0;

            if (fl1 < fl2)
                return -1;

            return 1;
        }

        private delegate int SortFunction(SledProfileInfoType pi1, SledProfileInfoType pi2);

        #endregion

        private const string LuaCFunc = "=[c]";
    }
}
