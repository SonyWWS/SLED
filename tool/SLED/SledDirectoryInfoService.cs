/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

using System;
using System.ComponentModel.Composition;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

using Sce.Atf;
using Sce.Atf.Applications;

using Sce.Sled.Shared;
using Sce.Sled.Shared.Services;

namespace Sce.Sled
{
    /// <summary>
    /// SledDirectoryInfoService Class
    /// </summary>
    [Export(typeof(ISledDirectoryInfoService))]
    [Export(typeof(IInitializable))]
    [Export(typeof(SledDirectoryInfoService))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    class SledDirectoryInfoService : ISledDirectoryInfoService, IInitializable
    {
        public SledDirectoryInfoService()
        {
            ((IInitializable)this).Initialize();
        }

        public MainForm MainForm { get; set; }

        public void LoadingFinished()
        {
            m_loading = false;
        }

        #region ISledDirectoryInfoService Interface

        public string ExePath { get; private set; }

        public string ExeDirectory { get; private set; }

        public string PluginDirectory { get; private set; }

        #endregion

        #region IInitializable Interface

        void IInitializable.Initialize()
        {
            if (m_bInitialized)
                return;

            ExePath = Application.ExecutablePath;

            ExeDirectory = EnsureDirEndsWithDirectorySeparator(Application.StartupPath);

            PluginDirectory =
                Path.Combine(
                    ExeDirectory,
                    "Plugins" + Path.DirectorySeparatorChar);

            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomainAssemblyResolve;

            m_bInitialized = true;
        }

        #endregion

        private static string EnsureDirEndsWithDirectorySeparator(string path)
        {
            return
                path.EndsWith(Path.DirectorySeparatorChar.ToString())
                    ? path
                    : path + Path.DirectorySeparatorChar;
        }

        Assembly CurrentDomainAssemblyResolve(object sender, ResolveEventArgs args)
        {
            // Try exe directory then plugin directory

            var assemblyName = new AssemblyName(args.Name);
            var assem =
                (Resolve(assemblyName, ExeDirectory, "*.dll", false, MainForm) ??
                 Resolve(assemblyName, PluginDirectory, "*.dll", false, MainForm)) ??
                (Resolve(assemblyName, ExeDirectory, "*.dll", true, MainForm) ??
                 Resolve(assemblyName, PluginDirectory, "*.dll", true, MainForm));

            if (assem == null)
            {
                if (args.Name.Contains("resources"))
                    return null;

                if (m_loading)
                {
                    MessageBox.Show(
                        MainForm,
                        string.Format(
                            "Failed to find the assembly \"{0}\" in the " +
                            "SLED executable and plugin directories!{1}{1}" +
                            "SLED will be unable to continue!",
                            args.Name,
                            Environment.NewLine),
                        AssemblyResoveErrorText,
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);

                    Environment.Exit(-1);
                }
                else
                {
                    SledOutDevice.OutLine(
                        SledMessageType.Error,
                        "SledDirectoryInfoService: Failed to find assembly \"{0}\"!",
                        args.Name);
                }
            }

            return assem;
        }

        private static Assembly Resolve(AssemblyName name, string directory, string searchPattern, bool partialMatch, MainForm mainForm)
        {
            try
            {
                string[] files = null;

                try
                {
                    files = Directory.GetFiles(directory, searchPattern, SearchOption.TopDirectoryOnly);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        mainForm,
                        string.Format(
                            "Exception enumerating directory \"{0}\"!{2}{2}" +
                            "Exception: {1}{2}{2}SLED will be unable to continue!",
                            directory,
                            ex.Message,
                            Environment.NewLine),
                        AssemblyResoveErrorText,
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);

                    Environment.Exit(-1);
                }

                if (files == null)
                    return null;

                foreach (var file in files)
                {
                    try
                    {
                        var assembly = Assembly.LoadFrom(file);
                        var result =
                            string.Compare(
                                partialMatch
                                    ? assembly.GetName().Name
                                    : assembly.FullName,
                                partialMatch
                                    ? name.Name
                                    : name.FullName,
                                true);

                        if (result == 0)
                            return assembly;
                    }
                    catch (Exception) { continue; }
                }
            }
            catch (Exception ex)
            {
                SledOutDevice.OutLine(
                    SledMessageType.Error,
                    "DirectoryInfoService: Exception in " +
                    "resolving assembly \"{0}\": {1}",
                    name.FullName, ex.Message);
            }

            return null;
        }
        
        private bool m_bInitialized;
        private bool m_loading = true;

        private const string AssemblyResoveErrorText = "Assembly Resolve Error";
    }
}