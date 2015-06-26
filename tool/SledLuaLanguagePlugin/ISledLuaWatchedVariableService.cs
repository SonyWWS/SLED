/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

using System;
using System.Collections.Generic;
using System.Drawing;

using Sce.Atf.Controls;

using Sce.Sled.Lua.Dom;
using Sce.Sled.Shared.Services;

namespace Sce.Sled.Lua
{
    public interface ISledLuaWatchedVariableService
    {
        bool ReceivingWatchedVariables { get; }
        bool IsLuaVarWatched(ISledLuaVarBaseType luaVar);
        void AddWatchedLuaVar(ISledLuaVarBaseType luaVar);
        void RemoveWatchedLuaVar(ISledLuaVarBaseType luaVar);

        bool ReceivingWatchedCustomVariables { get; }
        bool IsCustomWatchedVariable(ISledLuaVarBaseType luaVar);
        //void AddCustomWatchedVariable(SledLuaWatchedCustomVariable variable);
    }

    public class SledLuaWatchedCustomVariableRendererArgs
    {
        public SledLuaWatchedCustomVariableRendererArgs(TreeListView control, TreeListView.Node node, Graphics gfx, Rectangle bounds, int column, ISledLuaVarBaseType luaVar, SledLuaWatchedCustomVariable customVar)
        {
            Control = control;
            Node = node;
            Graphics = gfx;
            Bounds = bounds;
            Column = column;
            LuaVariable = luaVar;
            CustomVariable = customVar;
            DrawDefault = true;
        }

        public TreeListView Control { get; private set; }
        public TreeListView.Node Node { get; private set; }

        public Graphics Graphics { get; private set; }
        public Rectangle Bounds { get; private set; }
        public int Column { get; private set; }

        public ISledLuaVarBaseType LuaVariable { get; private set; }
        public SledLuaWatchedCustomVariable CustomVariable { get; private set; }

        public bool DrawDefault { get; set; }
    }

    public interface ISledLuaWatchedCustomVariableRenderer
    {
        void DrawBackground(SledLuaWatchedCustomVariableRendererArgs e);
        void DrawLabel(SledLuaWatchedCustomVariableRendererArgs e);
        void DrawImage(SledLuaWatchedCustomVariableRendererArgs e);
        void DrawStateImage(SledLuaWatchedCustomVariableRendererArgs e);
    }

    public class SledLuaWatchedCustomVariable
    {
        public SledLuaWatchedCustomVariable(string alias, SledLuaVarScopeType scope, List<KeyValuePair<string, int>> pairs)
            : this(alias, scope, pairs, Guid.NewGuid())
        {
        }

        public SledLuaWatchedCustomVariable(string alias, SledLuaVarScopeType scope, List<KeyValuePair<string, int>> pairs, Guid guid)
            : this(alias, scope, pairs, guid, null)
        {
        }

        public SledLuaWatchedCustomVariable(string alias, SledLuaVarScopeType scope, List<KeyValuePair<string, int>> pairs, Guid guid, ISledLuaWatchedCustomVariableRenderer renderer)
        {
            Alias = alias;
            Scope = scope;
            NamesAndTypes = new List<KeyValuePair<string, int>>(pairs);
            Guid = guid;
            Renderer = renderer;
        }

        public string Alias { get; private set; }

        public SledLuaVarScopeType Scope { get; private set; }

        public List<KeyValuePair<string, int>> NamesAndTypes { get; private set; }

        public Guid Guid { get; private set; }

        public ISledLuaWatchedCustomVariableRenderer Renderer { get; private set; }
    }
    
    public interface ISledLuaWatchedVariableProvider
    {
        IEnumerable<SledLuaWatchedCustomVariable> GetVariables(SledDebugServiceBreakpointEventArgs args);
    }
}