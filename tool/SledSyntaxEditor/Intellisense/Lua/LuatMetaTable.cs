/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

namespace Sce.Sled.SyntaxEditor.Intellisense.Lua
{
    /// <summary>
    /// An instance of a metatable
    /// </summary>
    public class LuatMetaTable
    {
        public void CreateIndex()
        {
            if (Index == null)
                Index = new LuatTable(null);
        }

        public void CreateNewIndex()
        {
            if (NewIndex == null)
                NewIndex = new LuatTable(null);
        }

        public LuatTable Index;
        public LuatTable NewIndex;
    }
}