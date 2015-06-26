/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

using Sce.Atf.Adaptation;
using Sce.Atf.Applications;
using Sce.Atf.Dom;

using Sce.Sled.Shared.Resources;
using Sce.Sled.SyntaxEditor;

namespace Sce.Sled.Shared.Dom
{
    /// <summary>
    /// Complex Type of a breakpoint
    /// </summary>
    public class SledProjectFilesBreakpointType : DomNodeAdapter, IItemView
    {
        /// <summary>
        /// Gets the parent as a SledProjectFilesFileType
        /// </summary>
        public SledProjectFilesFileType File
        {
            get { return DomNode.Parent.As<SledProjectFilesFileType>(); }
        }

        /// <summary>
        /// Gets or sets the line attribute
        /// </summary>
        public int Line
        {
            get
            {
                if (Breakpoint != null)
                    SetAttribute(SledSchema.SledProjectFilesBreakpointType.lineAttribute, Breakpoint.LineNumber);

                return GetAttribute<int>(SledSchema.SledProjectFilesBreakpointType.lineAttribute);
            }
            set { SetAttribute(SledSchema.SledProjectFilesBreakpointType.lineAttribute, value); }
        }

        /// <summary>
        /// Gets or sets the enabled attribute
        /// </summary>
        public bool Enabled
        {
            get
            {
                if (Breakpoint == null)
                    return GetAttribute<bool>(SledSchema.SledProjectFilesBreakpointType.enabledAttribute);

                return Breakpoint.Enabled;
            }
            set
            {
                if (Breakpoint != null)
                    Breakpoint.Enabled = value;

                SetAttribute(SledSchema.SledProjectFilesBreakpointType.enabledAttribute, value);
            }
        }

        /// <summary>
        /// Gets or sets the condition attribute
        /// </summary>
        public string Condition
        {
            get { return GetAttribute<string>(SledSchema.SledProjectFilesBreakpointType.conditionAttribute); }
            set { SetAttribute(SledSchema.SledProjectFilesBreakpointType.conditionAttribute, value); }
        }

        /// <summary>
        /// Gets or sets the condition enabled attribute
        /// </summary>
        public bool ConditionEnabled
        {
            get { return GetAttribute<bool>(SledSchema.SledProjectFilesBreakpointType.conditionenabledAttribute); }
            set
            {
                if (Breakpoint != null)
                    Breakpoint.Marker = value;

                SetAttribute(SledSchema.SledProjectFilesBreakpointType.conditionenabledAttribute, value);
            }
        }

        /// <summary>
        /// Gets or sets the result the condition is expecting
        /// </summary>
        public bool ConditionResult
        {
            get { return GetAttribute<bool>(SledSchema.SledProjectFilesBreakpointType.conditionresultAttribute); }
            set { SetAttribute(SledSchema.SledProjectFilesBreakpointType.conditionresultAttribute, value); }
        }

        /// <summary>
        /// Gets or sets whether to use the current function's environment or _G
        /// </summary>
        public bool UseFunctionEnvironment
        {
            get { return GetAttribute<bool>(SledSchema.SledProjectFilesBreakpointType.usefunctionenvironmentAttribute); }
            set { SetAttribute(SledSchema.SledProjectFilesBreakpointType.usefunctionenvironmentAttribute, value); }
        }

        /// <summary>
        /// Gets or sets the actual IBreakpoint this breakpoint references
        /// </summary>
        public IBreakpoint Breakpoint { get; set; }

        /// <summary>
        /// Gets the raw line number from the line attribute
        /// </summary>
        public int RawLine
        {
            get { return GetAttribute<int>(SledSchema.SledProjectFilesBreakpointType.lineAttribute); }
        }

        /// <summary>
        /// Gets or sets the line text
        /// </summary>
        /// <remarks>Value is string.Empty if the document isn't open</remarks>
        public string LineText { get; set; }

        private void Setup(IBreakpoint bp)
        {
            if (bp != null)
            {
                Line = bp.LineNumber;
                Enabled = bp.Enabled;
                LineText = bp.LineText;
                Breakpoint = bp;
            }

            // Assume true for now
            ConditionResult = true;
        }

        /// <summary>
        /// Adjust some properties based on the live IBreakpoint object
        /// </summary>
        public void Refresh()
        {
            // Update from live IBreakpoint
            if (Breakpoint == null)
                return;

            Line = Breakpoint.LineNumber;
            Enabled = Breakpoint.Enabled;
            LineText = Breakpoint.LineText;
        }

        #region IItemView Interface

        /// <summary>
        /// Fills in or modifies the given display info for the item</summary>
        /// <param name="item">Item</param>
        /// <param name="info">Display info to update</param>
        public void GetInfo(object item, ItemInfo info)
        {
            info.Label = Enabled.ToString();
            info.Properties =
                new[]
                {
                    File.Name,
                    Line.ToString(),
                    Condition,
                    ConditionEnabled.ToString(),
                    ConditionResult.ToString(),
                    (UseFunctionEnvironment ? "Environment" : "Global")
                };
            info.ImageIndex = info.GetImageIndex(Atf.Resources.DataImage);
            info.IsLeaf = true;
        }

        #endregion

        /// <summary>
        /// Create a SLED style breakpoint from a SyntaxEditorControl style breakpoint
        /// </summary>
        /// <param name="ibp">SyntaxEditorControl style breakpoint</param>
        /// <returns>SLED breakpoint</returns>
        public static SledProjectFilesBreakpointType Create(IBreakpoint ibp)
        {
            var node = new DomNode(SledSchema.SledProjectFilesBreakpointType.Type);
            var bp = node.As<SledProjectFilesBreakpointType>();
            bp.Setup(ibp);
            return bp;
        }

        /// <summary>
        /// Column names
        /// </summary>
        public static readonly string[] TheColumnNames =
        {
            Localization.SledSharedEnabled,
            Localization.SledSharedFile,
            Localization.SledSharedLine,
            Localization.SledSharedCondition,
            Localization.SledSharedConditionEnabled,
            Localization.SledSharedResult,
            "Environment"
        };
    }
}
