/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

using Sce.Atf.Dom;

namespace Sce.Sled.Shared.Dom
{
    /// <summary>
    /// Complex Type for a variable location
    /// </summary>
    public class SledVarLocationType : DomNodeAdapter
    {
        /// <summary>
        /// Get or set the name
        /// </summary>
        public string Name
        {
            get { return File; }
            set { File = value; }
        }

        /// <summary>
        /// Get or set the file
        /// </summary>
        public string File
        {
            get { return GetAttribute<string>(SledSchema.SledVarLocationType.fileAttribute); }
            set { SetAttribute(SledSchema.SledVarLocationType.fileAttribute, value); }
        }

        /// <summary>
        /// Get or set the line attribute
        /// <remarks>The line number in the script file the variable can
        /// be found on</remarks>
        /// </summary>
        public int Line
        {
            get { return GetAttribute<int>(SledSchema.SledVarLocationType.lineAttribute); }
            set { SetAttribute(SledSchema.SledVarLocationType.lineAttribute, value); }
        }

        /// <summary>
        /// Get or set the occurence attribute
        /// <remarks>Sometimes a variable may be mentioned several times on
        /// the same line in a script file, so this represents which occurrence
        /// of the variable name is the actual variable</remarks>
        /// </summary>
        public int Occurence
        {
            get { return GetAttribute<int>(SledSchema.SledVarLocationType.occurenceAttribute); }
            set { SetAttribute(SledSchema.SledVarLocationType.occurenceAttribute, value); }
        }
    }
}
