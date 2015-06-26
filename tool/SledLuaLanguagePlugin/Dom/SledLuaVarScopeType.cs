/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

using System;

namespace Sce.Sled.Lua.Dom
{
    public enum SledLuaVarScopeType
    {
        Global      = 0,
        Local       = 1,
        Upvalue     = 2,
        Environment = 3
    }

    public static class SledLuaVarScopeTypeString
    {
        public static string ToString(SledLuaVarScopeType value)
        {
            return Enum.GetName(typeof(SledLuaVarScopeType), value);
        }
    }
}