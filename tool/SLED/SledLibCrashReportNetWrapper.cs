/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

using Sce.Atf;

using Sce.Sled.Shared.Services;

namespace Sce.Sled
{
    public static class SledLibCrashReportNetWrapper
    {
        public static ISledCrashReporter TryCreateCrashReporter()
        {
            const string dllName = "Sled.CrashReporter.dll";

            return FindAndInstantiate<ISledCrashReporter>(dllName);
        }

        public static ISledUsageStatistics TryCreateUsageStatistics()
        {
            const string dllName = "Sled.UsageStatistics.dll";

            return FindAndInstantiate<ISledUsageStatistics>(dllName);
        }

        private static T FindAndInstantiate<T>(string dllName) where T : class, IInitializable
        {
            try
            {
                var dllPath = GetFullPathToDll(dllName);
                if (string.IsNullOrEmpty(dllName))
                    return null;

                var type = FindTypeInAssemblyImplementingInterface(dllPath, typeof(T));
                if (type == null)
                    return null;

                var instance = (T)Activator.CreateInstance(type);
                
                instance.Initialize();
                
                return instance;
            }
            catch (Exception ex)
            {
                ex.ToString();
                return null;
            }
        }

        private static string GetFullPathToDll(string dllName)
        {
            try
            {
                if (string.IsNullOrEmpty(dllName))
                    throw new ArgumentNullException("dllName");

                var appDir = Path.GetDirectoryName(Application.StartupPath + Path.DirectorySeparatorChar);
                var dllPath = Path.Combine(appDir + Path.DirectorySeparatorChar, dllName);

                return !File.Exists(dllPath) ? null : dllPath;
            }
            catch (Exception ex)
            {
                ex.ToString();
                return null;
            }
        }

        private static Type FindTypeInAssemblyImplementingInterface(string assemblyPath, Type iface)
        {
            try
            {
                if (iface == null)
                    return null;

                if (string.IsNullOrEmpty(assemblyPath))
                    return null;

                if (!File.Exists(assemblyPath))
                    return null;

                var assembly =
                    Assembly.LoadFile(assemblyPath);

                if (assembly == null)
                    return null;

                // Grab all public types
                var types = assembly.GetExportedTypes();

                // Find type that uses the interface
                return types.FirstOrDefault(iface.IsAssignableFrom);
            }
            catch (Exception ex)
            {
                ex.ToString();
                return null;
            }
        }
    }
}