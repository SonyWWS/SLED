/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

using System.ComponentModel.Composition;

using Sce.Atf;

using Sce.Sled.Shared.Services;

namespace Sce.Sled.Lua
{
    [Export(typeof(IInitializable))]
    [Export(typeof(SledLuaTtyService))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    sealed class SledLuaTtyService : IInitializable
    {
        #region IInitializable Interface

        void IInitializable.Initialize()
        {
            var luaLanguagePlugin = SledServiceInstance.Get<SledLuaLanguagePlugin>();

            var ttyService = SledServiceInstance.Get<ISledTtyService>();
            ttyService.RegisterLanguage(luaLanguagePlugin);
        }

        #endregion
    }
}
