/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using Sce.Atf.Adaptation;

using Sce.Sled.Project.Resources;
using Sce.Sled.Shared;
using Sce.Sled.Shared.Dom;
using Sce.Sled.Shared.Services;
using Sce.Sled.Shared.Utilities;

namespace Sce.Sled.Project
{
    public static class SledProjectUtilities
    {
        /// <summary>
        /// Open the project file and make sure the namespace is correct
        /// </summary>
        /// <param name="szAbsPath"></param>
        public static void CleanupProjectFileNamespace(string szAbsPath)
        {
            if (!File.Exists(szAbsPath))
                return;

            if (SledUtil.IsFileReadOnly(szAbsPath))
                return;

            try
            {
                Encoding encoding;
                string szFileContents;

                using (Stream stream = new FileStream(szAbsPath, FileMode.Open, FileAccess.Read))
                {
                    using (var reader = new StreamReader(stream, true))
                    {
                        // Store encoding & read contents
                        encoding = reader.CurrentEncoding;
                        szFileContents = reader.ReadToEnd();
                    }
                }

                if (!string.IsNullOrEmpty(szFileContents))
                {
                    const string szOldXmlNs = "xmlns=\"lua\"";
                    const string szNewXmlNs = "xmlns=\"sled\"";

                    if (szFileContents.Contains(szOldXmlNs))
                    {
                        szFileContents = szFileContents.Replace(szOldXmlNs, szNewXmlNs);

                        using (Stream stream = new FileStream(szAbsPath, FileMode.Create, FileAccess.Write))
                        {
                            using (var writer = new StreamWriter(stream, encoding))
                            {
                                writer.Write(szFileContents);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                SledOutDevice.OutLine(
                    SledMessageType.Error,
                    SledUtil.TransSub(Localization.SledProjectFilesErrorVerifyingNamespace, ex.Message, szAbsPath));
            }
        }

        /// <summary>
        /// Open the project file and remove any duplicates
        /// </summary>
        /// <param name="szAbsPath"></param>
        public static void CleanupProjectFileDuplicates(string szAbsPath)
        {
            if (!File.Exists(szAbsPath))
                return;

            if (SledUtil.IsFileReadOnly(szAbsPath))
                return;

            try
            {
                var schemaLoader = SledServiceInstance.TryGet<SledSharedSchemaLoader>();
                if (schemaLoader == null)
                    return;

                var uri = new Uri(szAbsPath);
                var reader = new SledSpfReader(schemaLoader);

                var root = reader.Read(uri, false);
                if (root == null)
                    return;

                var lstProjFiles = new List<SledProjectFilesFileType>();

                // Gather up all project files in the project
                SledDomUtil.GatherAllAs(root, lstProjFiles);

                if (lstProjFiles.Count <= 1)
                    return;

                var uniquePaths = new Dictionary<string, SledProjectFilesFileType>(StringComparer.CurrentCultureIgnoreCase);
                var lstDuplicates = new List<SledProjectFilesFileType>();

                foreach (var projFile in lstProjFiles)
                {
                    if (uniquePaths.ContainsKey(projFile.Path))
                        lstDuplicates.Add(projFile);
                    else
                        uniquePaths.Add(projFile.Path, projFile);
                }

                if (lstDuplicates.Count <= 0)
                    return;

                foreach (var projFile in lstDuplicates)
                    projFile.DomNode.RemoveFromParent();

                var writer = new SledSpfWriter(schemaLoader.TypeCollection);

                // Write changes back to disk
                writer.Write(root, uri, false);
            }
            catch (Exception ex)
            {
                SledOutDevice.OutLine(
                    SledMessageType.Error,
                    SledUtil.TransSub(Localization.SledProjectFilesErrorRemovingDuplicates, ex.Message, szAbsPath));
            }
        }

        /// <summary>
        /// Try and obtain project information from a project file on disk
        /// </summary>
        /// <param name="absPath"></param>
        /// <param name="name"></param>
        /// <param name="projectDir"></param>
        /// <param name="assetDir"></param>
        /// <param name="guid"></param>
        /// <param name="files"></param>
        /// <returns></returns>
        public static bool TryGetProjectDetails(
            string absPath,
            out string name,
            out string projectDir,
            out string assetDir,
            out Guid guid,
            out List<string> files)
        {
            name = null;
            projectDir = null;
            assetDir = null;
            guid = Guid.Empty;
            files = null;

            try
            {
                // ATF 3's DOM makes this so much easier now!

                var schemaLoader =
                    SledServiceInstance.TryGet<SledSharedSchemaLoader>();

                if (schemaLoader == null)
                    return false;

                var uri = new Uri(absPath);
                var reader =
                    new SledSpfReader(schemaLoader);

                var root = reader.Read(uri, false);
                if (root == null)
                    return false;

                var project =
                    root.As<SledProjectFilesType>();

                project.Uri = uri;
                
                // Pull out project details
                name = project.Name;
                guid = project.Guid;
                assetDir = project.AssetDirectory;
                projectDir = Path.GetDirectoryName(absPath);

                var lstProjFiles =
                    new List<SledProjectFilesFileType>();

                SledDomUtil.GatherAllAs(root, lstProjFiles);

                var assetDirTemp = assetDir;
                files =
                    (from projFile in lstProjFiles
                     let absFilePath =
                        SledUtil.GetAbsolutePath(
                            projFile.Path,
                            assetDirTemp)
                     select absFilePath).ToList();

                return true;
            }
            catch (Exception ex)
            {
                SledOutDevice.OutLine(
                    SledMessageType.Error,
                    "Exception encountered obtaining project " +
                    "details. Project file: \"{0}\". Exception: {1}",
                    absPath, ex.Message);

                return false;
            }
        }
    }
}
