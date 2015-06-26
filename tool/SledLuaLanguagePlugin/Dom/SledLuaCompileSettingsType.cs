/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

using System.Collections.Generic;

using Sce.Sled.Shared.Dom;

namespace Sce.Sled.Lua.Dom
{
    public class SledLuaCompileSettingsType : SledProjectFilesUserSettingsType
    {
        /// <summary>
        /// Gets/sets the name attribute
        /// </summary>
        public override string Name
        {
            get { return GetAttribute<string>(SledLuaSchema.SledLuaCompileSettingsType.nameAttribute); }
            set { SetAttribute(SledLuaSchema.SledLuaCompileSettingsType.nameAttribute, value); }
        }

        /// <summary>
        /// Gets/sets expanded attribute
        /// </summary>
        public override bool Expanded
        {
            get { return GetAttribute<bool>(SledLuaSchema.SledLuaCompileSettingsType.expandedAttribute); }
            set { SetAttribute(SledLuaSchema.SledLuaCompileSettingsType.expandedAttribute, value); }
        }

        /// <summary>
        /// Gets the configurations element
        /// </summary>
        public IList<SledLuaCompileConfigurationType> Configurations
        {
            get { return GetChildList<SledLuaCompileConfigurationType>(SledLuaSchema.SledLuaCompileSettingsType.ConfigurationsChild); }
        }
    }
}
