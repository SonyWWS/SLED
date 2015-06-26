/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

using Sce.Atf;
using Sce.Atf.Adaptation;
using Sce.Atf.Dom;

using Sce.Sled.Shared.Services;
using Sce.Sled.Shared.Utilities;

namespace Sce.Sled.Shared.Dom
{
    /// <summary>
    /// Complex Type of a root item
    /// </summary>
    public class SledProjectFilesRootType : DomNodeAdapter
    {
        /// <summary>
        /// Gets or sets the directory
        /// </summary>
        public string Directory
        {
            get
            {
                // Want to return an absolute path

                var directory = GetAttribute<string>(SledSchema.SledProjectFilesRootType.directoryAttribute);
                
                if (PathUtil.IsRelative(directory))
                {
                    var project = TryGetProject();
                    if (project != null)
                        directory = SledUtil.GetAbsolutePath(directory, project.ProjectDirectory);
                }

                return directory;
            }

            set
            {
                // Want to store as relative path from project directory
                
                var directory = value;

                if (!PathUtil.IsRelative(value))
                {
                    var project = TryGetProject();
                    if (project != null)
                        directory = SledUtil.GetRelativePath(value, project.ProjectDirectory);
                }

                SetAttribute(SledSchema.SledProjectFilesRootType.directoryAttribute, directory);
            }
        }

        private SledProjectFilesType TryGetProject()
        {
            if (DomNode.Parent != null)
                return DomNode.Parent.As<SledProjectFilesType>();

            return s_projectService.Get == null ? null : s_projectService.Get.ActiveProject;
        }

        private static readonly SledServiceReference<ISledProjectService> s_projectService =
            new SledServiceReference<ISledProjectService>();
    }
}