/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Reflection;

using Sce.Atf;

using Sce.Sled.Shared;
using Sce.Sled.Shared.Plugin;
using Sce.Sled.Shared.Services;

namespace Sce.Sled
{
    [Export(typeof(IInitializable))]
    [Export(typeof(ISledDynamicPluginService))]
    [Export(typeof(SledDynamicPluginService))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    class SledDynamicPluginService : IInitializable, ISledDynamicPluginService
    {
        #region IInitializable Interface

        void IInitializable.Initialize()
        {
            using (new SledOutDevice.BreakBlock())
            {
                foreach (var plugin in m_lstPlugins)
                {
                    SledOutDevice.OutLine(
                        plugin.Loaded
                            ? SledMessageType.Info
                            : SledMessageType.Error,
                        GetString(plugin));
                }
            }
        }

        #endregion

        public IEnumerable<Assembly> GetDynamicAssemblies(ISledDirectoryInfoService directoryInfoService)
        {
            var dynamicAssemblies =
                GetDynamicAssemblies(
                    directoryInfoService.PluginDirectory,
                    new[]
                    {
                        typeof(AtfPluginAttribute),
                        typeof(SledNetworkPluginAttribute),
                        typeof(SledLanguagePluginAttribute)
                    });

            // Save plugin information
            m_lstPlugins.AddRange(dynamicAssemblies.Keys);

            return
                from kv in dynamicAssemblies
                where kv.Key.Loaded
                select kv.Value;
        }

        #region ISledDynamicPluginService Interface

        public IEnumerable<SledDynamicPluginInfo> Plugins
        {
            get { return m_lstPlugins; }
        }

        public IEnumerable<SledDynamicPluginInfo> FailedPlugins
        {
            get { return m_lstPlugins.Where(plugin => !plugin.Loaded); }
        }

        public IEnumerable<SledDynamicPluginInfo> SuccessfulPlugins
        {
            get { return m_lstPlugins.Where(plugin => plugin.Loaded); }
        }

        #endregion

        #region Member Methods

        private static Dictionary<SledDynamicPluginInfo, Assembly> GetDynamicAssemblies(string directory, Type[] attributeTypes)
        {
            var assemblies =
                new Dictionary<SledDynamicPluginInfo, Assembly>();

            try
            {
                var files =
                    Directory.GetFiles(directory, "*.dll", SearchOption.TopDirectoryOnly);

                foreach (var file in files)
                {
                    try
                    {
                        Assembly assembly;

                        try
                        {
                            assembly = Assembly.LoadFile(file);
                        }
                        catch (Exception) { continue; }

                        if (assembly == null)
                            continue;

                        // Check if assembly has any of the
                        // attributes we're looking for
                        var bAssemblyContainsAnyAttributes =
                            DoesAssemblyContainAnyAttributes(
                                assembly,
                                attributeTypes);

                        // No attributes; skip
                        if (!bAssemblyContainsAnyAttributes)
                            continue;

                        // Check if plugin can be instantiated
                        Exception exception;
                        var bCanPluginBeInstantiated =
                            CanAssemblyBeInstantiated(
                                assembly.GetExportedTypes(),
                                out exception);

                        // Create plugin info
                        var pluginInfo =
                            new SledDynamicPluginInfo(
                                Path.GetFileName(file),
                                file,
                                bCanPluginBeInstantiated,
                                exception);

                        // Add
                        assemblies.Add(pluginInfo, assembly);
                    }
                    catch (Exception ex)
                    {
                        SledOutDevice.OutLine(
                            SledMessageType.Error,
                            "Exception loading plugin \"{0}\": {1}",
                            file, ex.Message);
                    }
                }
            }
            catch (Exception ex)
            {
                SledOutDevice.OutLine(
                    SledMessageType.Error,
                    "Exception looking for plugins: {0}",
                    ex.Message);
            }

            return assemblies;
        }

        private static bool DoesAssemblyContainAnyAttributes(Assembly assembly, params Type[] attributeTypes)
        {
            var lstCustomAttributes =
                new List<object>(
                    assembly.GetCustomAttributes(true));

            var result =
                (from attributeType in attributeTypes
                 from customAttribute in lstCustomAttributes
                 let customType = customAttribute.GetType()
                 where attributeType.Equals(customType)
                 select attributeType).Any();

            return result;
        }

        private static bool CanAssemblyBeInstantiated(IEnumerable<Type> types, out Exception exception)
        {
            exception = null;

            try
            {
                var iface = typeof(ISledCanPluginBeInstantiated);

                // Go through types in the assembly. If one type
                // fails we reject the entire assembly.
                foreach (var type in types)
                {
                    // See if type implements the interface
                    if (!iface.IsAssignableFrom(type))
                        continue;

                    // Create an instance of the type
                    var plugin = (ISledCanPluginBeInstantiated)Activator.CreateInstance(type);

                    // Return whether the type can be instantiated
                    return plugin.CanPluginBeInstantiated;
                }

                return true;
            }
            catch (Exception ex)
            {
                exception = ex;
                return false;
            }
        }

        private static string GetString(SledDynamicPluginInfo info)
        {
            var name =
                Path.GetFileName(info.AbsolutePath);

            if (info.Loaded)
            {
                return
                    string.Format(
                        "Plugin finder found {0} " + 
                        "and it loaded successfully!",
                        name);
            }

            var exMessage =
                info.Exception == null
                    ? string.Empty
                    : string.Format(
                        "Exception: {0}",
                        info.Exception.Message);

            return
                string.Format(
                    "Plugin finder found {0} " + 
                    "but it failed to load! {1}",
                    name,
                    exMessage);
        }

        #endregion

        private readonly List<SledDynamicPluginInfo> m_lstPlugins =
            new List<SledDynamicPluginInfo>();
    }
}