/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

using Sce.Atf.Applications;
using Sce.Atf.Dom;

using Sce.Sled.Shared.Plugin;
using Sce.Sled.Shared.Resources;

namespace Sce.Sled.Shared.Dom
{
    /// <summary>
    /// Complex Type for a syntax error
    /// </summary>
    public class SledSyntaxErrorType : DomNodeAdapter, IItemView
    {
        /// <summary>
        /// Get or set the name attribute
        /// </summary>
        public string Name
        {
            get { return Error; }
            set { Error = value; }
        }

        /// <summary>
        /// Get or set the line attribute
        /// </summary>
        public int Line
        {
            get { return GetAttribute<int>(SledSchema.SledSyntaxErrorType.lineAttribute); }
            set { SetAttribute(SledSchema.SledSyntaxErrorType.lineAttribute, value); }
        }

        /// <summary>
        /// Get or set the error attribute
        /// </summary>
        public string Error
        {
            get { return GetAttribute<string>(SledSchema.SledSyntaxErrorType.errorAttribute); }
            set { SetAttribute(SledSchema.SledSyntaxErrorType.errorAttribute, value); }
        }

        /// <summary>
        /// Get or set the file
        /// </summary>
        public SledProjectFilesFileType File { get; set; }

        /// <summary>
        /// Get or set the language
        /// </summary>
        public ISledLanguagePlugin Language { get; set; }

        #region IItemView Interface

        /// <summary>
        /// Fills in or modifies the given display info for the item</summary>
        /// <param name="item">Item</param>
        /// <param name="info">Display info to update</param>
        public void GetInfo(object item, ItemInfo info)
        {
            info.Label = Language == null ? @"?" : Language.LanguageName;
            info.Properties = new[] { File == null ? @"?" : File.Path, Line.ToString(), Error };
            info.IsLeaf = true;
            info.ImageIndex = info.GetImageIndex(Atf.Resources.DataImage);
        }

        #endregion

        /// <summary>
        /// Get the column names array
        /// </summary>
        public static readonly string[] TheColumnNames =
        {
            Localization.SledSharedLanguage,
            Localization.SledSharedFile,
            Localization.SledSharedLine,
            Localization.SledSharedError
        };
    }
}
