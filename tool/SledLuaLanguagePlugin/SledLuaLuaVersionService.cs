/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

using System;
using System.ComponentModel.Composition;
using System.Windows.Forms;

using Sce.Atf;
using Sce.Atf.Applications;

using Sce.Sled.Shared;

namespace Sce.Sled.Lua
{
    [Export(typeof(IInitializable))]
    [Export(typeof(ISledLuaLuaVersionService))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    sealed class SledLuaLuaVersionService : IInitializable, ISledLuaLuaVersionService
    {
        [ImportingConstructor]
        public SledLuaLuaVersionService(ICommandService commandService)
        {
            m_luaVerBox = new ToolStripComboBox("Lua Version");
            {
                var ver = new VersionWrapper(LuaVersion.Lua51, "Lua 5.1.4");
                m_luaVerBox.Items.Add(ver);
                m_luaVerBox.Items.Add(new VersionWrapper(LuaVersion.Lua52, "Lua 5.2.3"));
                m_luaVerBox.SelectedItem = ver;
                CurrentLuaVersion = ver.Version;
            }
            m_luaVerBox.SelectedIndexChanged += LuaVerBoxSelectedIndexChanged;
        }

        #region Implementation of IInitializable

        void IInitializable.Initialize()
        {
            SledLuaMenuShared.MenuInfo.GetToolStrip().Items.Add(m_luaVerBox);
        }

        #endregion

        #region Implementation of ISledLuaLuaVersionService

        public LuaVersion CurrentLuaVersion
        {
            get { return m_currentLuaVersion; }

            private set
            {
                if (m_currentLuaVersion == value)
                    return;

                m_currentLuaVersion = value;

                SledOutDevice.OutLine(SledMessageType.Info, "[Lua]: Now using {0}", m_luaVerBox.SelectedItem);

                CurrentLuaVersionChanged.Raise(this, EventArgs.Empty);
            }
        }

        public event EventHandler CurrentLuaVersionChanged;

        #endregion

        private void LuaVerBoxSelectedIndexChanged(object sender, EventArgs e)
        {
            var wrapper = (VersionWrapper)m_luaVerBox.SelectedItem;
            CurrentLuaVersion = wrapper.Version;
        }

        #region Private Classes

        private class VersionWrapper
        {
            public VersionWrapper(LuaVersion version, string verString)
            {
                Version = version;
                m_verString = verString;
            }

            public LuaVersion Version { get; private set; }

            public override string ToString()
            {
                return m_verString;
            }

            private readonly string m_verString;
        }

        #endregion
        
        private LuaVersion m_currentLuaVersion;
        
        private readonly ToolStripComboBox m_luaVerBox;
    }

    public interface ISledLuaLuaVersionService
    {
        LuaVersion CurrentLuaVersion { get; }

        event EventHandler CurrentLuaVersionChanged;
    }

    public enum LuaVersion
    {
        Lua51,
        Lua52
    }
}