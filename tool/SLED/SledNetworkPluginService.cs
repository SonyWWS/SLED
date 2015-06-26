/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;

using Sce.Atf;

using Sce.Sled.Resources;
using Sce.Sled.Shared;
using Sce.Sled.Shared.Plugin;
using Sce.Sled.Shared.Services;
using Sce.Sled.Shared.Utilities;

namespace Sce.Sled
{
    /// <summary>
    /// SledNetworkPluginService Class
    /// </summary>
    [Export(typeof(IInitializable))]
    [Export(typeof(ISledNetworkPluginService))]
    [Export(typeof(SledNetworkPluginService))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    sealed class SledNetworkPluginService : IInitializable, ISledNetworkPluginService
    {
        #region IInitializable Interface

        void IInitializable.Initialize()
        {
            if (IsInitialized)
                return;

            var networkPlugins = SledServiceInstance.GetAll<ISledNetworkPlugin>();

            m_lstPlugins.Clear();

            using (new SledOutDevice.BreakBlock())
            {
                foreach (var netPlugin in networkPlugins)
                {
                    // Add plugin to list
                    m_lstPlugins.Add(netPlugin);

                    // Report the plugin was found
                    SledOutDevice.OutLine(
                        SledMessageType.Info,
                        SledUtil.TransSub(Localization.SledNetworkPluginLoaded, netPlugin.Protocol, netPlugin.Name));
                }
            }

            try
            {
                Initialized.Raise(this, EventArgs.Empty);
            }
            finally
            {
                IsInitialized = true;
            }
        }

        #endregion

        #region ISledNetworkPluginService Interface

        /// <summary>
        /// Gets plugins
        /// </summary>
        public IEnumerable<ISledNetworkPlugin> NetworkPlugins
        {
            get { return m_lstPlugins; }
        }

        /// <summary>
        /// Returns number of network plugins
        /// </summary>
        public int Count
        {
            get { return m_lstPlugins.Count; }
        }

        /// <summary>
        /// Gets whether the service has run its initialization method yet
        /// </summary>
        public bool IsInitialized { get; private set; }

        /// <summary>
        /// Event fired when the service runs its initialization method
        /// </summary>
        public event EventHandler Initialized;

        #endregion

        private readonly List<ISledNetworkPlugin> m_lstPlugins =
            new List<ISledNetworkPlugin>();
    }
}
