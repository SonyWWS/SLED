/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

using System;
using System.Xml.Serialization;

using Sce.Atf.Applications;

using Sce.Sled.Shared.Resources;

namespace Sce.Sled.Shared.Dom
{
    /// <summary>
    /// Serializable memory trace entry
    /// </summary>
    [Serializable]
    [XmlRoot("SledMemoryTraceStream")]
    public class SledMemoryTraceStream : IItemView
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public SledMemoryTraceStream()
        {
            Order = 0;
            What = string.Empty;

            OldAddress = string.Empty;
            NewAddress = string.Empty;

            OldSize = 0;
            NewSize = 0;
        }

        /// <summary>
        /// Gets or sets order
        /// </summary>
        [XmlElement("Order")]
        public ulong Order { get; set; }

        /// <summary>
        /// Gets or sets what
        /// </summary>
        [XmlElement("What")]
        public string What { get; set; }

        /// <summary>
        /// Gets or sets old address
        /// </summary>
        [XmlElement("OldAddress")]
        public string OldAddress { get; set; }

        /// <summary>
        /// Gets or sets new address
        /// </summary>
        [XmlElement("NewAddress")]
        public string NewAddress { get; set; }

        /// <summary>
        /// Gets or sets old size
        /// </summary>
        [XmlElement("OldSize")]
        public int OldSize { get; set; }

        /// <summary>
        /// Gets or sets new size
        /// </summary>
        [XmlElement("NewSize")]
        public int NewSize { get; set; }

        #region IItemView Interface

        /// <summary>
        /// Fills in or modifies the given display info for the item</summary>
        /// <param name="item">Item</param>
        /// <param name="info">Display info to update</param>
        public void GetInfo(object item, ItemInfo info)
        {
            info.Label = Order.ToString();
            info.Properties =
                new[]
                {
                    What,
                    OldAddress,
                    NewAddress,
                    OldSize.ToString(),
                    NewSize.ToString()
                };
            info.ImageIndex = info.GetImageIndex(Atf.Resources.DataImage);
            info.IsLeaf = true;
        }

        #endregion

        /// <summary>
        /// Column names
        /// </summary>
        public static readonly string[] TheColumnNames =
        {
            Localization.SledSharedOrder,
            Localization.SledSharedWhat,
            Localization.SledSharedOldAddress,
            Localization.SledSharedNewAddress,
            Localization.SledSharedOldSize,
            Localization.SledSharedNewSize
        };
    }
}