/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

using System;
using System.ComponentModel.Composition;

using Sce.Atf;
using Sce.Atf.Applications;

using Sce.Sled.Shared.Services;
using Sce.Sled.Shared.Utilities;

namespace Sce.Sled
{
    [Export(typeof(IInitializable))]
    [Export(typeof(SledDebugFlashWindowService))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    class SledDebugFlashWindowService : IInitializable
    {
        [ImportingConstructor]
        public SledDebugFlashWindowService(MainForm mainForm)
        {
            m_mainForm = mainForm;
        }

        #region IInitializable Interface

        void IInitializable.Initialize()
        {
            m_debugService.BreakpointHit += DebugServiceBreakpointHit;
        }

        #endregion

        #region ISledDebugService Events

        private void DebugServiceBreakpointHit(object sender, SledDebugServiceBreakpointEventArgs e)
        {
            try
            {
                var hwnd = SledUser32.GetForegroundWindow();
                if (hwnd == m_mainForm.Handle)
                    return;

                SledUser32.FlashWindow(m_mainForm.Handle);
            }
            catch (Exception ex)
            {
                ex.ToString();
            }
        }

        #endregion

#pragma warning disable 649 // Field is never assigned to and will always have its default value null

        [Import]
        private ISledDebugService m_debugService;

#pragma warning restore 649

        private readonly MainForm m_mainForm;
    }
}