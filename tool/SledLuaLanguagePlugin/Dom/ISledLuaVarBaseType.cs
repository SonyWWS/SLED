/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

using System;
using System.Collections.Generic;
using System.Globalization;

using Sce.Atf.Dom;

using Sce.Sled.Shared.Dom;

namespace Sce.Sled.Lua.Dom
{
    public interface ISledLuaVarBaseType : ISledVarBaseType
    {
        /// <summary>
        /// Gets the name sort key
        /// </summary>
        SortKey NameSortKey { get; }

        /// <summary>
        /// Gets the underlying DomNode
        /// </summary>
        DomNode DomNode { get; }

        /// <summary>
        /// Gets the display name
        /// </summary>
        string DisplayName { get; set; }

        /// <summary>
        /// Gets/sets the unique name
        /// </summary>
        string UniqueName { get; set; }

        /// <summary>
        /// Generate a MD5 hash from the variables' unique name
        /// </summary>
        Int64 UniqueNameMd5Hash { get; }

        /// <summary>
        /// Gets/sets the what attribute
        /// <remarks>What the actual variable is (ie. the LUA_T type of the 
        /// object - LUA_TSTRING, LUA_TNUMBER, LUA_TTHREAD, etc...)</remarks>
        /// </summary>
        string What { get; set; }

        /// <summary>
        /// Gets the what sort key
        /// </summary>
        SortKey WhatSortKey { get; }

        /// <summary>
        /// Gets/sets the value attribute
        /// <remarks>The actual value of the variable</remarks>
        /// </summary>
        string Value { get; set; }

        /// <summary>
        /// Gets the value sort key
        /// </summary>
        SortKey ValueSortKey { get; }

        /// <summary>
        /// Gets/sets the keytype attribute
        /// <remarks>This is used when changing the value of the variable
        /// while debugging as the variable has to be pushed on the stack so
        /// knowing what type the name is to push is appropriately is very
        /// important</remarks>
        /// </summary>
        int KeyType { get; set; }

        /// <summary>
        /// Gets/sets whether the item is expanded
        /// </summary>
        bool Expanded { get; set; }

        /// <summary>
        /// Gets/sets whether the item is visible
        /// </summary>
        bool Visible { get; set; }

        /// <summary>
        /// Get the element's type as a LUA_T&lt;&gt; type caching the result
        /// </summary>
        LuaType LuaType { get; }

        /// <summary>
        /// Get the element's children variables
        /// </summary>
        IEnumerable<ISledLuaVarBaseType> Variables { get; }

        /// <summary>
        /// Try and generate a "unique" name for the variable
        /// </summary>
        void GenerateUniqueName();

        /// <summary>
        /// Gets the variable 'scope' - global / local / upvalue
        /// </summary>
        SledLuaVarScopeType Scope { get; }

        /// <summary>
        /// Gets the table hierarchy on the target, so we know where to insert items as they come in during the lookup phase
        /// </summary>
        IList<SledLuaVarNameTypePairType> TargetHierarchy { get; }
    }
}
