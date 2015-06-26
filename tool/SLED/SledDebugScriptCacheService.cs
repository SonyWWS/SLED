/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

using System;
using System.ComponentModel.Composition;
using System.IO;

using Sce.Atf;

using Sce.Sled.Resources;
using Sce.Sled.Shared;
using Sce.Sled.Shared.Dom;
using Sce.Sled.Shared.Scmp;
using Sce.Sled.Shared.Services;
using Sce.Sled.Shared.Utilities;

namespace Sce.Sled
{
    /// <summary>
    /// SledDebugScriptCacheService Class
    /// </summary>
    [Export(typeof(IInitializable))]
    [Export(typeof(SledDebugScriptCacheService))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    class SledDebugScriptCacheService : IInitializable
    {
        #region IInitializable Interface

        void IInitializable.Initialize()
        {
            m_debugService =
                SledServiceInstance.Get<ISledDebugService>();

            m_debugService.DataReady += DebugServiceDataReady;

            m_projectService =
                SledServiceInstance.Get<ISledProjectService>();
        }

        #endregion

        #region ISledDebugService Events

        private void DebugServiceDataReady(object sender, SledDebugServiceEventArgs e)
        {
            if (e.Scmp.TypeCode != (UInt16)TypeCodes.ScriptCache)
                return;

            var scriptCache = m_debugService.GetScmpBlob<ScriptCache>();

            var szAbsFilePath = SledUtil.GetAbsolutePath(scriptCache.RelScriptPath, m_projectService.AssetDirectory);
            if (!File.Exists(szAbsFilePath))
            {
                SledOutDevice.OutLine(
                    SledMessageType.Error,
                    SledUtil.TransSub(Localization.SledRemoteTargetErrorScriptCacheFileNotExist, scriptCache.RelScriptPath, szAbsFilePath));
            }
            else
            {
                SledProjectFilesFileType file;
                m_projectService.AddFile(szAbsFilePath, out file);
            }
        }

        #endregion

        private ISledDebugService m_debugService;
        private ISledProjectService m_projectService;
    }
}
