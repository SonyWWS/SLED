/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

using System;
using System.Collections.Generic;

using Sce.Atf.Adaptation;

using Sce.Sled.Shared.Dom;

namespace Sce.Sled.Shared.Services
{
    /// <summary>
    /// SLED project service project EventArgs class
    /// </summary>
    public class SledProjectServiceProjectEventArgs : EventArgs
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="project">Project</param>
        public SledProjectServiceProjectEventArgs(SledProjectFilesType project)
        {
            Project = project;
        }

        /// <summary>
        /// Project
        /// </summary>
        public readonly SledProjectFilesType Project;
    }

    /// <summary>
    /// SLED project service project name EventArgs class
    /// </summary>
    public sealed class SledProjectServiceProjectNameEventArgs : SledProjectServiceProjectEventArgs
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="project">Project</param>
        /// <param name="oldName">Old project name</param>
        /// <param name="newName">New project name</param>
        public SledProjectServiceProjectNameEventArgs(SledProjectFilesType project, string oldName, string newName)
            : base(project)
        {
            OldName = oldName;
            NewName = newName;
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
    /// SLED project service project save-as EventArgs class
    /// </summary>
    public sealed class SledProjectServiceProjectSaveAsEventArgs : SledProjectServiceProjectEventArgs
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="project">Project</param>
        /// <param name="oldName">Old project name</param>
        /// <param name="newName">New project name</param>
        /// <param name="oldProjDir">Old project directory</param>
        /// <param name="newProjDir">New project directory</param>
        /// <param name="oldAssetDir">Old asset directory</param>
        /// <param name="newAssetDir">New asset directory</param>
        public SledProjectServiceProjectSaveAsEventArgs(SledProjectFilesType project, string oldName, string newName, string oldProjDir, string newProjDir, string oldAssetDir, string newAssetDir)
            : base(project)
        {
            OldName = oldName;
            NewName = newName;
            OldProjectDir = oldProjDir;
            NewProjectDir = newProjDir;
            OldAssetDir = oldAssetDir;
            NewAssetDir = newAssetDir;
        }

        /// <summary>
        /// Old project name
        /// </summary>
        public readonly string OldName;

        /// <summary>
        /// New project name
        /// </summary>
        public readonly string NewName;

        /// <summary>
        /// Old project directory
        /// </summary>
        public readonly string OldProjectDir;

        /// <summary>
        /// New project directory
        /// </summary>
        public readonly string NewProjectDir;

        /// <summary>
        /// Old asset directory
        /// </summary>
        public readonly string OldAssetDir;

        /// <summary>
        /// New asset directory
        /// </summary>
        public readonly string NewAssetDir;
    }

    /// <summary>
    /// SLED project service project GUID EventArgs class
    /// </summary>
    public sealed class SledProjectServiceProjectGuidEventArgs : SledProjectServiceProjectEventArgs
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="project">Project</param>
        /// <param name="oldGuid">Old project GUID</param>
        /// <param name="newGuid">New project GUID</param>
        public SledProjectServiceProjectGuidEventArgs(SledProjectFilesType project, Guid oldGuid, Guid newGuid)
            : base(project)
        {
            OldGuid = oldGuid;
            NewGuid = newGuid;
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
    /// SLED project service asset directory EventArgs class
    /// </summary>
    public sealed class SledProjectServiceAssetDirEventArgs : SledProjectServiceProjectEventArgs
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="project">Project</param>
        /// <param name="oldDirectory">Old project asset directory</param>
        /// <param name="newDirectory">New project asset directory</param>
        public SledProjectServiceAssetDirEventArgs(SledProjectFilesType project, string oldDirectory, string newDirectory)
            : base(project)
        {
            OldDirectory = oldDirectory;
            NewDirectory = newDirectory;
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
    /// SLED project service file EventArgs class
    /// </summary>
    public sealed class SledProjectServiceFileEventArgs : SledProjectServiceProjectEventArgs
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="project">Project</param>
        /// <param name="file">File</param>
        public SledProjectServiceFileEventArgs(SledProjectFilesType project, SledProjectFilesFileType file)
            : this(project, file, string.Empty, string.Empty)
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="project">Project</param>
        /// <param name="file">File</param>
        /// <param name="oldPath">Old path (used when file path is changing)</param>
        /// <param name="newPath">New path (used when file path is changing)</param>
        public SledProjectServiceFileEventArgs(SledProjectFilesType project, SledProjectFilesFileType file, string oldPath, string newPath)
            : base(project)
        {
            File = file;
            OldPath = oldPath;
            NewPath = newPath;
        }

        /// <summary>
        /// File
        /// </summary>
        public readonly SledProjectFilesFileType File;

        /// <summary>
        /// Old path (used when file path is changing)
        /// </summary>
        public readonly string OldPath;

        /// <summary>
        /// New Path (used when file path is changing)
        /// </summary>
        public readonly string NewPath;
    }

    /// <summary>
    /// SLED project service folder EventArgs class
    /// </summary>
    public sealed class SledProjectServiceFolderEventArgs : SledProjectServiceProjectEventArgs
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="project">Project</param>
        /// <param name="folder">Folder</param>
        public SledProjectServiceFolderEventArgs(SledProjectFilesType project, SledProjectFilesFolderType folder)
            : this(project, folder, string.Empty, string.Empty)
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="project">Project</param>
        /// <param name="folder">Folder</param>
        /// <param name="oldName">Old name (used when folder name is changed)</param>
        /// <param name="newName">New name (used when folder name is changed)</param>
        public SledProjectServiceFolderEventArgs(SledProjectFilesType project, SledProjectFilesFolderType folder, string oldName, string newName)
            : base(project)
        {
            Folder = folder;
            OldName = oldName;
            NewName = newName;
        }

        /// <summary>
        /// Folder
        /// </summary>
        public readonly SledProjectFilesFolderType Folder;

        /// <summary>
        /// Old name (used when folder name is changed)
        /// </summary>
        public readonly string OldName;

        /// <summary>
        /// New name (used when folder name is changed)
        /// </summary>
        public readonly string NewName;
    }

    /// <summary>
    /// SLED project service interface
    /// </summary>
    public interface ISledProjectService : ISledLessProjectSerivce, IAdaptable
    {
        /// <summary>
        /// Get name of the active project
        /// </summary>
        string ProjectName { get; }

        /// <summary>
        /// Get directory the active project is in
        /// </summary>
        string ProjectDirectory { get; }

        /// <summary>
        /// Get directory assets are held in (such as a scripts directory)
        /// </summary>
        string AssetDirectory { get; }

        /// <summary>
        /// Get project unique identifier (GUID)
        /// </summary>
        Guid ProjectGuid { get; }

        /// <summary>
        /// New project created event
        /// </summary>
        event EventHandler<SledProjectServiceProjectEventArgs> Created;

        /// <summary>
        /// Existing project opened event
        /// </summary>
        event EventHandler<SledProjectServiceProjectEventArgs> Opened;

        /// <summary>
        /// Project closing event
        /// </summary>
        event EventHandler<SledProjectServiceProjectEventArgs> Closing;

        /// <summary>
        /// Project closed event
        /// </summary>
        event EventHandler<SledProjectServiceProjectEventArgs> Closed;

        /// <summary>
        /// Project saved event
        /// </summary>
        event EventHandler<SledProjectServiceProjectEventArgs> Saved;

        /// <summary>
        /// Project saved-as-ing event
        /// </summary>
        event EventHandler<SledProjectServiceProjectSaveAsEventArgs> SavedAsing;

        /// <summary>
        /// Project saved-as-ed event
        /// </summary>
        event EventHandler<SledProjectServiceProjectSaveAsEventArgs> SavedAsed;

        /// <summary>
        /// Project settings saving event
        /// </summary>
        event EventHandler<SledProjectServiceProjectEventArgs> SavingSettings;

        /// <summary>
        /// Project settings saved event
        /// </summary>
        event EventHandler<SledProjectServiceProjectEventArgs> SavedSettings;

        /// <summary>
        /// File in the project being renamed
        /// </summary>
        event EventHandler<SledProjectServiceFileEventArgs> FileRenaming;

        /// <summary>
        /// File in the project renamed event
        /// </summary>
        event EventHandler<SledProjectServiceFileEventArgs> FileRenamed;

        /// <summary>
        /// File added to project event
        /// </summary>
        event EventHandler<SledProjectServiceFileEventArgs> FileAdded;

        /// <summary>
        /// File being removed from project event
        /// </summary>
        event EventHandler<SledProjectServiceFileEventArgs> FileRemoving;

        /// <summary>
        /// File removed from project event
        /// </summary>
        event EventHandler<SledProjectServiceFileEventArgs> FileRemoved;

        /// <summary>
        /// Folder in the project being renamed event
        /// </summary>
        event EventHandler<SledProjectServiceFolderEventArgs> FolderRenaming;

        /// <summary>
        /// Folder in the project renamed event
        /// </summary>
        event EventHandler<SledProjectServiceFolderEventArgs> FolderRenamed;

        /// <summary>
        /// Folder added to project event
        /// </summary>
        event EventHandler<SledProjectServiceFolderEventArgs> FolderAdded;

        /// <summary>
        /// Folder being removed from project event
        /// </summary>
        event EventHandler<SledProjectServiceFolderEventArgs> FolderRemoving;

        /// <summary>
        /// Folder removed from project event
        /// </summary>
        event EventHandler<SledProjectServiceFolderEventArgs> FolderRemoved;

        /// <summary>
        /// Project file opened in the editor event
        /// </summary>
        event EventHandler<SledProjectServiceFileEventArgs> FileOpened;

        /// <summary>
        /// Project file closing in the editor event
        /// </summary>
        event EventHandler<SledProjectServiceFileEventArgs> FileClosing;

        /// <summary>
        /// Project file closed in the editor event
        /// </summary>
        event EventHandler<SledProjectServiceFileEventArgs> FileClosed;

        /// <summary>
        /// Event triggered when project name is changing
        /// </summary>
        event EventHandler<SledProjectServiceProjectNameEventArgs> NameChanging;

        /// <summary>
        /// Event triggered when project name changed
        /// </summary>
        event EventHandler<SledProjectServiceProjectNameEventArgs> NameChanged;

        /// <summary>
        /// Event triggered when project GUID is changing
        /// </summary>
        event EventHandler<SledProjectServiceProjectGuidEventArgs> GuidChanging;

        /// <summary>
        /// Event triggered when project GUID changed
        /// </summary>
        event EventHandler<SledProjectServiceProjectGuidEventArgs> GuidChanged;

        /// <summary>
        /// Event triggered when asset directory is changing
        /// </summary>
        event EventHandler<SledProjectServiceAssetDirEventArgs> AssetDirChanging;

        /// <summary>
        /// Event triggered when asset directory changed
        /// </summary>
        event EventHandler<SledProjectServiceAssetDirEventArgs> AssetDirChanged;

        /// <summary>
        /// Get whether there is a project open and it has no files in it
        /// </summary>
        bool Empty { get; } 

        /// <summary>
        /// Get whether underlying collection is dirty
        /// </summary>
        bool Dirty { get; }

        /// <summary>
        /// Get whether a project is open
        /// </summary>
        bool Active { get; }

        /// <summary>
        /// Return the underlying collection as a SLED project (if the underlying collection isn't null)
        /// </summary>
        /// <returns>SLED project</returns>
        [Obsolete("Use ActiveProject instead")]
        SledProjectFilesType Get();

        /// <summary>
        /// Get the active project, or null if no active project
        /// </summary>
        SledProjectFilesType ActiveProject { get; }

        /// <summary>
        /// Get a collection of all files in the project
        /// </summary>
        ICollection<SledProjectFilesFileType> AllFiles { get; }

        /// <summary>
        /// Create a new project (display the new project dialog)
        /// </summary>
        /// <returns>True iff project is created</returns>
        bool New();

        /// <summary>
        /// Create a new project with given criteria
        /// </summary>
        /// <param name="name">Name of project</param>
        /// <param name="projectDir">Project directory</param>
        /// <param name="assetDir">Asset directory</param>
        /// <param name="guid">GUID</param>
        /// <returns>True iff project is created</returns>
        bool New(string name, string projectDir, string assetDir, Guid guid);

        /// <summary>
        /// Open a project
        /// </summary>
        /// <param name="absPath">Absolute path to project file</param>
        /// <returns>True iff project is opened</returns>
        bool Open(string absPath);

        /// <summary>
        /// Save the project
        /// </summary>
        void Save();

        /// <summary>
        /// Save-as the project
        /// </summary>
        void SaveAs();

        /// <summary>
        /// Write settings to disk immediately, with no queuing or extra delay
        /// </summary>
        void SaveSettings();

        /// <summary>
        /// Write settings to disk
        /// </summary>
        /// <param name="canQueue">True iff multiple calls can be queued and then, some
        /// milliseconds time later, be turned into one save settings call</param>
        void SaveSettings(bool canQueue);

        /// <summary>
        /// Close the project
        /// </summary>
        void Close();

        /// <summary>
        /// Add file to project
        /// </summary>
        /// <param name="absPath">Absolute path to file</param>
        /// <param name="file">Handle to file in project</param>
        /// <returns>True iff file added (or already in the project)</returns>
        bool AddFile(string absPath, out SledProjectFilesFileType file);

        /// <summary>
        /// Add file to project with specified folder
        /// </summary>
        /// <param name="absPath">Absolute path to file</param>
        /// <param name="folder">Folder to add the file to</param>
        /// <param name="file">Handle to file in project</param>
        /// <returns>True iff file added (or already in the project)</returns>
        bool AddFile(string absPath, SledProjectFilesFolderType folder, out SledProjectFilesFileType file);

        /// <summary>
        /// Remove a file from the project
        /// </summary>
        /// <param name="absPath">Absolute path to file</param>
        /// <returns>True if file removed, or false if not in project and therefore not removed</returns>
        bool RemoveFile(string absPath);

        /// <summary>
        /// Add folder to project
        /// </summary>
        /// <param name="name">Name of the folder</param>
        /// <param name="folder">Handle to folder in project</param>
        /// <returns>True iff folder added</returns>
        bool AddFolder(string name, out SledProjectFilesFolderType folder);

        /// <summary>
        /// Add folder to project
        /// </summary>
        /// <param name="name">Name of the folder</param>
        /// <param name="parent">Folder to add the new folder to</param>
        /// <param name="folder">Handle to folder in project</param>
        /// <returns>True iff folder added</returns>
        bool AddFolder(string name, SledProjectFilesFolderType parent, out SledProjectFilesFolderType folder);
    }

    /// <summary>
    /// Interface to use if accessing the project service when SLED is not running (i.e. using SledProject.dll only)
    /// </summary>
    public interface ISledLessProjectSerivce
    {
        /// <summary>
        /// Create a project
        /// </summary>
        /// <param name="name">Name of project</param>
        /// <param name="projDir">Project directory</param>
        /// <param name="assetDir">Asset directory</param>
        /// <param name="absPathFiles">List of absolute paths of files to add to the project</param>
        /// <returns>True iff project created</returns>
        bool CreateProject(string name, string projDir, string assetDir, IEnumerable<string> absPathFiles);
    }
}