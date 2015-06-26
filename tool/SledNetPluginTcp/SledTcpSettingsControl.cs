/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

using System.Windows.Forms;

using Sce.Sled.Shared.Plugin;
using Sce.Sled.Shared.Services;

namespace Sce.Sled.Net.Tcp
{
    internal partial class SledTcpSettingsControl : SledNetworkPluginTargetFormSettings
    {
        public SledTcpSettingsControl(ISledTarget target)
        {
            InitializeComponent();
        }

        #region SledNetworkPluginTargetFormSettings Overrides

        public override bool ContainsErrors(out string errorMsg, out Control errorControl)
        {
            errorMsg = string.Empty;
            errorControl = null;

            // No errors as there are no custom settings
            return false;
        }

        public override object[] GetDataBlob()
        {
            return null;
        }

        public override int DefaultPort
        {
            get { return SledNetPluginTcp.DefaultPort; }
        }

        #endregion
    }
}
