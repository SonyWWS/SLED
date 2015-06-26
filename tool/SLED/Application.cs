/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Globalization;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;

using Sce.Atf;
using Sce.Atf.Applications;
using Sce.Atf.Applications.WebServices;

using Sce.Sled.Project;
using Sce.Sled.Shared;
using Sce.Sled.Shared.Dom;
using Sce.Sled.Shared.Services;

namespace Sce.Sled
{
    public static class Program
    {
        /// <summary>
        /// Main entry point
        /// </summary>
        [STAThread]
        static void Main(string[] arg)
        {
            Thread.CurrentThread.CurrentUICulture = CultureInfo.CurrentCulture;

            // For testing localization
            //Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo("ja");

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.DoEvents();

            ShowBitNess();
            
            ISledCrashReporter crashReporter = null;
            ISledUsageStatistics usageStatistics = null;
            
            try
            {
                // Try and create crash reporter
                crashReporter = SledLibCrashReportNetWrapper.TryCreateCrashReporter();

                // Try and create usage statistics
                usageStatistics = SledLibCrashReportNetWrapper.TryCreateUsageStatistics();

                // Create 'master' catalog to hold all
                // other catalogs we might create
                var aggregateCatalog = new AggregateCatalog();

                // Add 'standard/default' parts to 'master' catalog
                aggregateCatalog.Catalogs.Add(
                    new TypeCatalog(

                        /* ATF related services */
                        typeof(SettingsService),
                        typeof(StatusService),
                        typeof(CommandService),
                        typeof(ControlHostService),
                        typeof(UnhandledExceptionService),
                        typeof(UserFeedbackService),
                        typeof(OutputService),
                        typeof(Outputs),
                        typeof(FileDialogService),
                        typeof(DocumentRegistry),
                        typeof(ContextRegistry),
                        typeof(StandardFileExitCommand),
                        typeof(RecentDocumentCommands),
                        typeof(StandardEditCommands),
                        typeof(StandardEditHistoryCommands),
                        typeof(TabbedControlSelector),
                        typeof(DefaultTabCommands),
                        typeof(WindowLayoutService),
                        typeof(WindowLayoutServiceCommands),
                        typeof(SkinService),

                        /* SLED related services */
                        typeof(SledOutDevice),
                        typeof(SledAboutService),
                        typeof(SledAboutDocumentService),
                        typeof(SledGotoService),
                        typeof(SledTitleBarTextService),
                        typeof(SledDocumentService),
                        typeof(SledProjectService),
                        typeof(SledProjectWatcherService),
                        typeof(SledModifiedProjectFormService),
                        typeof(SledProjectFileGathererService),
                        typeof(SledLanguageParserService),
                        typeof(SledProjectFilesTreeEditor),
                        typeof(SledProjectFilesDiskViewEditor),
                        typeof(SledProjectFileFinderService),
                        typeof(SledProjectFilesUtilityService),
                        typeof(SledProjectFileAdderService),
                        typeof(SledBreakpointService),
                        typeof(SledBreakpointEditor),
                        typeof(SledNetworkPluginService),
                        typeof(SledLanguagePluginService),
                        typeof(SledTargetService),
                        typeof(SledFindAndReplaceService),
                        typeof(SledFindAndReplaceService.SledFindResultsEditor1),
                        typeof(SledFindAndReplaceService.SledFindResultsEditor2),
                        typeof(SledDebugService),
                        typeof(SledDebugScriptCacheService),
                        typeof(SledDebugBreakpointService),
                        typeof(SledDebugFileService),
                        typeof(SledDebugHeartbeatService),
                        typeof(SledDebugNegotiationTimeoutService),
                        typeof(SledDebugFlashWindowService),
                        typeof(SledFileWatcherService),
                        typeof(SledModifiedFilesFormService),
                        typeof(SledFileExtensionService),
                        typeof(SledSyntaxCheckerService),
                        typeof(SledSyntaxErrorsEditor),
                        typeof(SledTtyService),
                        typeof(SledDebugFreezeService),
                        typeof(SledSourceControlService),
                        typeof(SledSharedSchemaLoader)
                        ));

                // Create directory information service
                var directoryInfoService = new SledDirectoryInfoService();

                // Create dynamic plugin service
                var dynamicPluginService = new SledDynamicPluginService();

                // Grab all dynamically loaded plugins
                // from the "SLED\Plugins" directory
                var dynamicAssemblies = dynamicPluginService.GetDynamicAssemblies(directoryInfoService);

                // Add dynamically obtained assemblies to
                // the master catalog
                AddAssembliesToMasterCatalog(aggregateCatalog, dynamicAssemblies);

                // Add 'master' catalog to container
                using (var container = new CompositionContainer(aggregateCatalog))
                {
                    // Create tool strip container
                    using (var toolStripContainer = new ToolStripContainer())
                    {
                        toolStripContainer.Dock = DockStyle.Fill;

                        // Grab SledShared.dll for resource loading
                        var assem = Assembly.GetAssembly(typeof(SledShared));

                        // Create main form & set up some properties
                        var mainForm =
                            new MainForm(toolStripContainer)
                                {
                                    AllowDrop = true,
                                    Text = Resources.Resource.SLED,
                                    Icon = GdiUtil.GetIcon(
                                        assem,
                                        SledShared.IconPathBase + ".Sled.ico")
                                };

                        // Load all the icons and images SLED will need
                        SledShared.RegisterImages();

                        directoryInfoService.MainForm = mainForm;

                        // Create batch and add any manual parts
                        var batch = new CompositionBatch();
                        batch.AddPart(mainForm);
                        batch.AddPart(directoryInfoService);
                        batch.AddPart(dynamicPluginService);
                        SetupDebugEventFiringWatching(batch);
                        container.Compose(batch);

                        // Set this one time
                        SledServiceReferenceCompositionContainer.SetCompositionContainer(container);

                        // Initialize all IInitializable interfaces
                        try
                        {
                            try
                            {
                                foreach (var initializable in container.GetExportedValues<IInitializable>())
                                    initializable.Initialize();
                            }
                            catch (CompositionException ex)
                            {
                                foreach (var error in ex.Errors)
                                    MessageBox.Show(error.Description, MefCompositionExceptionText);

                                throw;
                            }
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.ToString(), ExceptionDetailsText);
                            Environment.Exit(-1);
                        }

                        // Send usage data to the server now that everything is loaded
                        if (usageStatistics != null)
                            usageStatistics.PhoneHome();

                        SledOutDevice.OutBreak();

                        // Notify
                        directoryInfoService.LoadingFinished();

                        // Let ATF's UnhandledExceptionService know about our ICrashLogger
                        if (crashReporter != null)
                        {
                            var unhandledExceptionService = SledServiceInstance.TryGet<UnhandledExceptionService>();
                            if (unhandledExceptionService != null)
                                unhandledExceptionService.CrashLogger = crashReporter;
                        }

                        // Show main form finally
                        Application.Run(mainForm);
                    }
                }
            }
            finally
            {
                //
                // Cleanup
                //

                if (crashReporter != null)
                    crashReporter.Dispose();

                if (usageStatistics != null)
                    usageStatistics.Dispose();
            }
        }

        private static void AddAssembliesToMasterCatalog(AggregateCatalog catalog, IEnumerable<Assembly> assemblies)
        {
            foreach (var assembly in assemblies)
            {
                try
                {
                    var assemblyCatalog = new AssemblyCatalog(assembly);
                    catalog.Catalogs.Add(assemblyCatalog);
                }
                catch (Exception ex)
                {
                    SledOutDevice.OutLine(
                        SledMessageType.Error,
                        "Exception creating MEF AssemblyCatalog " +
                        "from assembly \"{0}\" ({1}): {2}",
                        assembly.FullName, assembly.FullName, ex.Message);
                }
            }
        }

        private static void ShowBitNess()
        {
            SledOutDevice.OutLine(
                SledMessageType.Info,
                "SLED is running as a {0} bit application!", IntPtr.Size * 8);
        }

        [System.Diagnostics.Conditional("DEBUG")]
        private static void SetupDebugEventFiringWatching(CompositionBatch batch)
        {
            const SledCheckEventsFiredService.Events eventsToWatch =
                SledCheckEventsFiredService.Events.None;

            batch.AddPart(new SledCheckEventsFiredService(eventsToWatch));
        }

        private const string ExceptionDetailsText = "Exception Details";
        private const string MefCompositionExceptionText = "MEF Composition Exception";
    }
}
