/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

using Sce.Atf.Applications;

using Sce.Sled.Lua.Resources;
using Sce.Sled.Shared.Services;

namespace Sce.Sled.Lua
{
    /// <summary>
    /// We want to put all Lua plugin related items into the same toolbar menu
    /// and we also only want to register the Lua toolbar menu one time.
    /// </summary>
    static class SledLuaMenuShared
    {
        private enum Menu
        {
            Lua,
        }

        private enum CommandGroup
        {
            Lua,
        }

        public static object MenuTag
        {
            get
            {
                if (!s_bSetup)
                    Setup();

                return Menu.Lua;
            }
        }

        public static MenuInfo MenuInfo
        {
            get
            {
                if (!s_bSetup)
                    Setup();

                return s_menuInfo;
            }
        }

        public static object CommandGroupTag
        {
            get { return CommandGroup.Lua; }
        }

        private static void Setup()
        {
            if (s_bSetup)
                return;

            // Find plugin dictionary
            var commandService = SledServiceInstance.Get<ICommandService>();
            
            // Register menu for all Lua plugin items to use
            s_menuInfo = 
                commandService.RegisterMenu(
                    Menu.Lua,
                    Localization.SledLuaTag,
                    Localization.SledLuaPluginOptions);

            s_bSetup = true;
        }

        private static bool s_bSetup;
        private static MenuInfo s_menuInfo;
    }
}
