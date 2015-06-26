/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

using Sce.Atf.Applications;

using Sce.Atf.Dom;

using Sce.Sled.Lua.Resources;
using Sce.Sled.Shared.Utilities;

namespace Sce.Sled.Lua.Dom
{
    /// <summary>
    /// Complex Type for a Lua state
    /// </summary>
    public class SledLuaStateType : DomNodeAdapter, IItemView
    {
        /// <summary>
        /// Gets/sets the name attribute
        /// </summary>
        public string Name
        {
            get { return GetAttribute<string>(SledLuaSchema.SledLuaStateType.nameAttribute); }
            set { SetAttribute(SledLuaSchema.SledLuaStateType.nameAttribute, value); }
        }

        /// <summary>
        /// Gets/sets the address attribute
        /// </summary>
        public string Address
        {
            get { return GetAttribute<string>(SledLuaSchema.SledLuaStateType.addressAttribute); }
            set { SetAttribute(SledLuaSchema.SledLuaStateType.addressAttribute, value); }
        }

        /// <summary>
        /// Gets/sets the enabled attribute
        /// </summary>
        public bool Checked
        {
            get { return GetAttribute<bool>(SledLuaSchema.SledLuaStateType.checkedAttribute); }
            set { SetAttribute(SledLuaSchema.SledLuaStateType.checkedAttribute, value); }
        }

        #region IItemView Interface

        /// <summary>
        /// Fill in contents for displaying on a GUI
        /// </summary>
        /// <param name="item"></param>
        /// <param name="info">structure to hold display information</param>
        public void GetInfo(object item, ItemInfo info)
        {
            info.Label = Address;
            info.Checked = Checked;
            info.Properties = new[] { Name };
            info.IsLeaf = false;
            info.Description = SledUtil.TransSub(Localization.SledLuaLuaState, Address);
            info.ImageIndex = info.GetImageIndex(Atf.Resources.DataImage);
        }

        #endregion
    }
}
