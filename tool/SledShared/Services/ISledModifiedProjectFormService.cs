/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

using System;
using System.Collections.Generic;

namespace Sce.Sled.Shared.Services
{
    /// <summary>
    /// Enumeration for changes outside editors can make
    /// to a currently opened SLED project
    /// </summary>
    public enum SledModifiedProjectChangeType
    {
        /// <summary>
        /// Project name changed
        /// </summary>
        Name = 1,

        /// <summary>
        /// Asset directory changed
        /// </summary>
        AssetDir = 2,

        /// <summary>
        /// GUID changed
        /// </summary>
        Guid = 3,

        /// <summary>
        /// File added
        /// </summary>
        FileAdded = 4,

        /// <summary>
        /// File removed
        /// </summary>
        FileRemoved = 5,
    }

    /// <summary>
    /// SLED modified project change class
    /// </summary>
    public abstract class SledModifiedProjectChange
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="changeType">Change type</param>
        protected SledModifiedProjectChange(SledModifiedProjectChangeType changeType)
        {
            ChangeType = changeType;
        }

        /// <summary>
        /// Change type
        /// </summary>
        public readonly SledModifiedProjectChangeType ChangeType;
    }

    /// <summary>
    /// SLED modified project name change class
    /// </summary>
    public sealed class SledModifiedProjectNameChange : SledModifiedProjectChange
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="oldName">Old project name</param>
        /// <param name="newName">New project name</param>
        public SledModifiedProjectNameChange(string oldName, string newName)
            : base(SledModifiedProjectChangeType.Name)
        {
            OldName = oldName;
            NewName = newName;
        }

        /// <summary>
        /// Obtain a human readable string that represents the change
        /// </summary>
        /// <returns>Human readable string representing the change</returns>
        public override string ToString()
        {
            return
                string.Format(
                    "Name changed from \"{0}\" to \"{1}\"",
                    OldName,
                    NewName);
        }

        /// <summary>
        /// Old project name
        /// </summary>
        public readonly string OldName;

        /// <summary>
        /// New project name
        /// </summary>
        public readonly string NewName;
    }

    /// <summary>
    /// SLED modified project GUID change class
    /// </summary>
    public sealed class SledModifiedProjectGuidChange : SledModifiedProjectChange
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="oldGuid">Old project GUID</param>
        /// <param name="newGuid">New project GUID</param>
        public SledModifiedProjectGuidChange(Guid oldGuid, Guid newGuid)
            : base(SledModifiedProjectChangeType.Guid)
        {
            OldGuid = oldGuid;
            NewGuid = newGuid;
        }

        /// <summary>
        /// Obtain a human readable string that represents the change
        /// </summary>
        /// <returns>Human readable string representing the change</returns>
        public override string ToString()
        {
            return
                string.Format(
                    "Guid changed from \"{0}\" to \"{1}\"",
                    OldGuid,
                    NewGuid);
        }

        /// <summary>
        /// Old project GUID
        /// </summary>
        public readonly Guid OldGuid;

        /// <summary>
        /// New project GUID
        /// </summary>
        public readonly Guid NewGuid;
    }

    /// <summary>
    /// SLED modified project asset directory change class
    /// </summary>
    public sealed class SledModifiedProjectAssetDirChange : SledModifiedProjectChange
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="oldDir">Old project asset directory</param>
        /// <param name="newDir">New project asset directory</param>
        public SledModifiedProjectAssetDirChange(string oldDir, string newDir)
            : base(SledModifiedProjectChangeType.AssetDir)
        {
            OldDirectory = oldDir;
            NewDirectory = newDir;
        }

        /// <summary>
        /// Obtain a human readable string that represents the change
        /// </summary>
        /// <returns>Human readable string representing the change</returns>
        public override string ToString()
        {
            return
                string.Format(
                    "Asset directory changed from \"{0}\" to \"{1}\"",
                    OldDirectory,
                    NewDirectory);
        }

        /// <summary>
        /// Old project asset directory
        /// </summary>
        public readonly string OldDirectory;

        /// <summary>
        /// New project asset directory
        /// </summary>
        public readonly string NewDirectory;
    }

    /// <summary>
    /// SLED modified project file added change class
    /// </summary>
    public sealed class SledModifiedProjectFileAddedChange : SledModifiedProjectChange
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="absolutePath">Absolute path to added file</param>
        public SledModifiedProjectFileAddedChange(string absolutePath)
            : base(SledModifiedProjectChangeType.FileAdded)
        {
            AbsolutePath = absolutePath;
        }

        /// <summary>
        /// Obtain a human readable string that represents the change
        /// </summary>
        /// <returns>Human readable string representing the change</returns>
        public override string ToString()
        {
            return
                string.Format(
                    "File added: \"{0}\"",
                    AbsolutePath);
        }

        /// <summary>
        /// Absolute path to added file
        /// </summary>
        public readonly string AbsolutePath;
    }

    /// <summary>
    /// SLED modified project file removed change class
    /// </summary>
    public sealed class SledModifiedProjectFileRemovedChange : SledModifiedProjectChange
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="absolutePath">Absolute path to removed file</param>
        public SledModifiedProjectFileRemovedChange(string absolutePath)
            : base(SledModifiedProjectChangeType.FileRemoved)
        {
            AbsolutePath = absolutePath;
        }

        /// <summary>
        /// Obtain a human readable string that represents the change
        /// </summary>
        /// <returns>Human readable string representing the change</returns>
        public override string ToString()
        {
            return
                string.Format(
                    "File removed: \"{0}\"",
                    AbsolutePath);
        }

        /// <summary>
        /// Absolute path to removed file
        /// </summary>
        public readonly string AbsolutePath;
    }

    /// <summary>
    /// SLED modified project changes EventArgs class
    /// </summary>
    public sealed class SledModifiedProjectChangesEventArgs : EventArgs
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="acceptedChanges">Accepted user changes</param>
        /// <param name="ignoredChanges">Ignored user changes</param>
        public SledModifiedProjectChangesEventArgs(IList<SledModifiedProjectChange> acceptedChanges,
                                                  IList<SledModifiedProjectChange> ignoredChanges)
        {
            AcceptedChanges = acceptedChanges;
            IgnoredChanges = ignoredChanges;
        }

        /// <summary>
        /// List of changes the user accepted
        /// </summary>
        public readonly IList<SledModifiedProjectChange> AcceptedChanges;

        /// <summary>
        /// List of changes the user ignored
        /// </summary>
        public readonly IList<SledModifiedProjectChange> IgnoredChanges;
    }

    /// <summary>
    /// SLED modified project changes detected EventArgs class
    /// </summary>
    public sealed class SledModifiedProjectChangesDetectedEventArgs : EventArgs
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="showingGui">Whether the GUI is shown or not</param>
        public SledModifiedProjectChangesDetectedEventArgs(bool showingGui)
        {
            ShowingGui = showingGui;
        }

        /// <summary>
        /// Whether the GUI is shown or not
        /// </summary>
        public readonly bool ShowingGui;
    }

    /// <summary>
    /// SLED modified project form service interface
    /// </summary>
    public interface ISledModifiedProjectFormService
    {
        /// <summary>
        /// Event triggered when another program modifies the currently opened SLED project
        /// and SLED is determining whether or not the changes warrant user interaction
        /// </summary>
        event EventHandler<SledModifiedProjectChangesDetectedEventArgs> ChangesDetected;

        /// <summary>
        /// Event triggered when another program modifies the currently opened SLED project
        /// and the user has selected which changes to take and which changes to ignore
        /// </summary>
        event EventHandler<SledModifiedProjectChangesEventArgs> GuiChangesSubmitted;
    }
}
