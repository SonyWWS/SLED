/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

using System.Drawing;
using System.Reflection;
using System.Windows.Forms;

using Sce.Atf;

namespace Sce.Sled.SyntaxEditor.Intellisense.Lua
{
    public enum LuaIntellisenseIconType
    {
        Unknown,
        Nil,
        Boolean,
        Table,
        Number,
        String,
        Function,
        Reflected,
        Mixed = Unknown,
        Keyword = Unknown
    }

    public class LuaIntellisenseIcons
    {
        public static ImageList GetImageList()
        {
            if (s_imageList == null)
            {
                var assembly = Assembly.GetAssembly(typeof(LuaIntellisenseIcons));
                var icons = GdiUtil.GetImage(assembly, IconImagePath);

                s_imageList = new ImageList { ImageSize = new Size(icons.Height, icons.Height) };
                s_imageList.Images.AddStrip(icons);
            }

            return s_imageList;
        }

        private static ImageList s_imageList;

        private const string IconImagePath =
            "Sce.Sled.SyntaxEditor.Intellisense.Lua.Icons.icons.png";
    }
}