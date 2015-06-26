/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

using System;
using System.ComponentModel.Composition;

using Sce.Atf;

using Sce.Sled.Shared.Services;

namespace Sce.Sled
{
    [Export(typeof(IInitializable))]
    [Export(typeof(ISledDebugFreezeService))]
    [Export(typeof(SledDebugFreezeService))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    class SledDebugFreezeService : IInitializable, ISledDebugFreezeService
    {
        #region IInitializable Interface

        void IInitializable.Initialize()
        {
            // Keep this here
        }

        #endregion

        #region ISledDebugFreezeService Interface

        public bool Frozen
        {
            get { lock (m_lock) return m_bFrozen; }
        }

        public event EventHandler Freezing;

        public event EventHandler Thawing;

        #endregion

        public void Freeze()
        {
            lock (m_lock)
            {
                if (m_bFrozen)
                    return;

                try
                {
                    Freezing.Raise(this, EventArgs.Empty);
                }
                finally
                {
                    m_bFrozen = true;
                }
            }
        }

        public void Thaw()
        {
            lock (m_lock)
            {
                if (!m_bFrozen)
                    return;

                try
                {
                    Thawing.Raise(this, EventArgs.Empty);
                }
                finally
                {
                    m_bFrozen = false;
                }
            }
        }

        private bool m_bFrozen;

        private volatile object m_lock =
            new object();
    }
}