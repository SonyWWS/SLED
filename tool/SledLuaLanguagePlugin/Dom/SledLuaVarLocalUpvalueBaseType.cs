/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

namespace Sce.Sled.Lua.Dom
{
    public abstract class SledLuaVarLocalUpvalueBaseType : SledLuaVarBaseType
    {
        /// <summary>
        /// Gets/sets the level attribute
        /// <remarks>This represents the stack level the local/upvalue resides at</remarks>
        /// </summary>
        public abstract int Level { get; set; }

        /// <summary>
        /// Gets/sets the index attribute
        /// <remarks>This represents the index at a particular stack level where the local/upvalue resides</remarks>
        /// </summary>
        public abstract int Index { get; set; }

        /// <summary>
        /// Gets/sets the function name attribute
        /// <remarks>This represents the function the variable is used in</remarks>
        /// </summary>
        public abstract string FunctionName { get; set; }

        /// <summary>
        /// Gets/sets the function line defined attribute
        /// <remarks>This represents the line the function is defined on</remarks>
        /// </summary>
        public abstract int FunctionLineDefined { get; set; }
    }
}
