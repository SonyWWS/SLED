/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

using Sce.Atf.Dom;

namespace Sce.Sled.Shared.Dom
{
    /// <summary>
    /// SledProjectFilesLanguageType
    /// </summary>
    public class SledProjectFilesLanguageType : DomNodeAdapter
    {
        /// <summary>
        /// Get or set the name attribute
        /// </summary>
        public string Name
        {
            get { return Language; }
            set { Language = value; }
        }

        /// <summary>
        /// Get or set the language attribute
        /// </summary>
        public string Language
        {
            get { return GetAttribute<string>(SledSchema.SledProjectFilesLanguageType.languageAttribute); }
            set { SetAttribute(SledSchema.SledProjectFilesLanguageType.languageAttribute, value); }
        }

        /// <summary>
        /// Get or set the version attribute
        /// </summary>
        public string Version
        {
            get { return GetAttribute<string>(SledSchema.SledProjectFilesLanguageType.versionAttribute); }
            set { SetAttribute(SledSchema.SledProjectFilesLanguageType.versionAttribute, value); }
        }
    }
}
