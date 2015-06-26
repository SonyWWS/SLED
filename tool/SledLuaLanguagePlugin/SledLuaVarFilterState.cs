/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

using System.Collections.Generic;

namespace Sce.Sled.Lua
{
    public class SledLuaVariableFilterState
    {
        public bool[] LocalFilterTypes =
        {
            false,          // LUA_TNIL
            false,          // LUA_TBOOLEAN
            false,          // LUA_TLIGHTUSERDATA
            false,          // LUA_TNUMBER
            false,          // LUA_TSTRING
            false,          // LUA_TTABLE
            false,          // LUA_TFUNCTION
            false,          // LUA_TUSERDATA
            false           // LUA_TTHREAD
        };

        public bool[] NetFilterTypes =
        {
            false,          // LUA_TNIL
            false,          // LUA_TBOOLEAN
            false,          // LUA_TLIGHTUSERDATA
            false,          // LUA_TNUMBER
            false,          // LUA_TSTRING
            false,          // LUA_TTABLE
            false,          // LUA_TFUNCTION
            false,          // LUA_TUSERDATA
            false           // LUA_TTHREAD
        };

        public List<string> LocalFilterNames = new List<string>();
        public List<string> NetFilterNames = new List<string>();

        /// <summary>
        /// Returns true if any filters are set
        /// </summary>
        public bool FiltersActive
        {
            get
            {
                if ((LocalFilterNames.Count > 0) || (NetFilterNames.Count > 0))
                    return true;

                for (var i = 0; i < LocalFilterTypes.Length; i++)
                {
                    if (LocalFilterTypes[i])
                        return true;
                }

                for (var i = 0; i < NetFilterTypes.Length; i++)
                {
                    if (NetFilterTypes[i])
                        return true;
                }

                return false;
            }
        }
    }
}
