/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

using Sce.Atf.Applications;
using Sce.Atf.Dom;

using Sce.Sled.Shared.Utilities;

namespace Sce.Sled.Shared.Dom
{
    /// <summary>
    /// Complex Type for a call stack entry
    /// </summary>
    public class SledCallStackType : DomNodeAdapter, IItemView
    {
        /// <summary>
        /// Get or set the name
        /// </summary>
        public string Name
        {
            get { return Function; }
            set { Function = value; }
        }

        /// <summary>
        /// Get or set the function
        /// </summary>
        public string Function
        {
            get { return GetAttribute<string>(SledSchema.SledCallStackType.functionAttribute); }
            set { SetAttribute(SledSchema.SledCallStackType.functionAttribute, value); }
        }

        /// <summary>
        /// Get or set the file 
        /// </summary>
        public string File
        {
            get { return GetAttribute<string>(SledSchema.SledCallStackType.fileAttribute); }
            set { SetAttribute(SledSchema.SledCallStackType.fileAttribute, value); }
        }

        /// <summary>
        /// Get or set the line the function call is on
        /// </summary>
        public int CurrentLine
        {
            get { return GetAttribute<int>(SledSchema.SledCallStackType.currentlineAttribute); }
            set { SetAttribute(SledSchema.SledCallStackType.currentlineAttribute, value); }
        }

        /// <summary>
        /// Get or set the line defined
        /// </summary>
        public int LineDefined
        {
            get { return GetAttribute<int>(SledSchema.SledCallStackType.linedefinedAttribute); }
            set { SetAttribute(SledSchema.SledCallStackType.linedefinedAttribute, value); }
        }

        /// <summary>
        /// Get or set the line end
        /// </summary>
        public int LineEnd
        {
            get { return GetAttribute<int>(SledSchema.SledCallStackType.lineendAttribute); }
            set { SetAttribute(SledSchema.SledCallStackType.lineendAttribute, value); }
        }

        /// <summary>
        /// Get or set the level 
        /// </summary>
        public int Level
        {
            get { return GetAttribute<int>(SledSchema.SledCallStackType.levelAttribute); }
            set { SetAttribute(SledSchema.SledCallStackType.levelAttribute, value); }
        }

        /// <summary>
        /// Get or set whether the cursor is in the function
        /// </summary>
        public bool IsCursorInFunction { get; set; }

        #region IItemView Interface

        /// <summary>
        /// Fills in or modifies the given display info for the item
        /// </summary>
        /// <param name="item">Item</param>
        /// <param name="info">Display info to update</param>
        public void GetInfo(object item, ItemInfo info)
        {
            // Cache value
            if (s_iBpCsiImgIndex == -1)
                s_iBpCsiImgIndex = info.GetImageIndex(SledIcon.BreakpointCsi);

            info.Label = string.Empty;
            info.Properties = new[] { Function, File, CurrentLine.ToString() };
            info.Description = Function;
            if (IsCursorInFunction)
                info.ImageIndex = s_iBpCsiImgIndex;
            info.IsLeaf = true;
        }

        #endregion

        /// <summary>
        /// Get or set whether node can be looked up
        /// </summary>
        public bool NodeCanBeLookedUp { get; set; }

        /// <summary>
        /// Get or set whether node needs to be looked up
        /// </summary>
        public bool NodeNeedsLookedUp { get; set; }

        private static int s_iBpCsiImgIndex = -1;
    }
}
