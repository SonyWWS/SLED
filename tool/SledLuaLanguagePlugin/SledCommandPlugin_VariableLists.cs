/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using Sce.Atf;
using Sce.Atf.Adaptation;
using Sce.Atf.Applications;

using Sce.Sled.Lua.Dom;
using Sce.Sled.Shared.Controls;
using Sce.Sled.Shared.Services;

namespace Sce.Sled.Lua
{
    [Export(typeof(IInitializable))]
    [Export(typeof(IContextMenuCommandProvider))]
    [Export(typeof(SledCommandPluginVariableLists))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    class SledCommandPluginVariableLists : IInitializable, ICommandClient, IContextMenuCommandProvider
    {
        [ImportingConstructor]
        public SledCommandPluginVariableLists(ICommandService commandService)
        {
            ICommandService commandService1 = commandService;

            var menu =
                commandService1.RegisterMenu(
                    Menu.VarList,
                    "Lua Variable List",
                    "Lua Variable List");

            commandService1.RegisterCommand(
                Command.Goto,
                Menu.VarList,
                Group.NavigateCommands,
                "Goto",
                "Goto variable",
                Keys.None,
                Atf.Resources.ZoomInImage,
                CommandVisibility.ContextMenu,
                this);

            commandService1.RegisterCommand(
                Command.AddWatch,
                Menu.VarList,
                Group.WatchCommands,
                "Add Watch",
                "Add varible to watch list",
                Keys.None,
                null,
                CommandVisibility.ContextMenu,
                this);

            commandService1.RegisterCommand(
                Command.RemoveWatch,
                Menu.VarList,
                Group.WatchCommands,
                "Remove Watch",
                "Remove variable from watch list",
                Keys.None,
                null,
                CommandVisibility.ContextMenu,
                this);

            commandService1.RegisterCommand(
                Command.CopyName,
                Menu.VarList,
                Group.CopyCommands,
                "Copy Name",
                "Copy the variable's name to the clipboard",
                Atf.Input.Keys.None,
                null,
                CommandVisibility.ContextMenu,
                this);

            commandService1.RegisterCommand(
                Command.CopyValue,
                Menu.VarList,
                Group.CopyCommands,
                "Copy Value",
                "Copy the variable's value to the clipboard",
                Atf.Input.Keys.None,
                null,
                CommandVisibility.ContextMenu,
                this);

            // Don't show the menu
            menu.GetMenuItem().Visible = false;
        }

        #region IInitializable Interface

        void IInitializable.Initialize()
        {
        }

        #endregion

        #region ICommandClient Interface

        public bool CanDoCommand(object commandTag)
        {
            return
                (commandTag is Command) &&
                (m_selection.Count > 0);
        }

        public void DoCommand(object commandTag)
        {
            if (!(commandTag is Command))
                return;

            if (m_selection.Count <= 0)
                return;

            try
            {
                var cmd = (Command)commandTag;
                switch (cmd)
                {
                    case Command.Goto:
                        m_gotoService.GotoVariable(m_selection[0]);
                        break;

                    case Command.AddWatch:
                        HandleAddWatch(m_selection);
                        break;

                    case Command.RemoveWatch:
                        HandleRemoveWatch(m_selection);
                        break;

                    case Command.CopyName:
                    case Command.CopyValue:
                        HandleCopyCommands(cmd, m_selection);
                        break;
                }
            }
            finally
            {
                m_selection.Clear();
            }
        }

        public void UpdateCommand(object commandTag, CommandState state)
        {
        }

        #endregion

        #region IContextMenuCommandProvider Overrides

        public IEnumerable<object> GetCommands(object context, object target)
        {
            m_selection.Clear();

            var clicked = target.As<ISledLuaVarBaseType>();
            if (clicked == null)
                return s_emptyCommands;

            {
                var registry = SledServiceInstance.TryGet<IContextRegistry>();
                if (registry != null)
                {
                    // Check if a GUI editor is the active context
                    var editor = registry.ActiveContext.As<SledTreeListViewEditor>();
                    if (editor != null)
                    {
                        // Add selection from editor to saved selection
                        foreach (var item in editor.Selection)
                        {
                            if (item.Is<ISledLuaVarBaseType>())
                                m_selection.Add(item.As<ISledLuaVarBaseType>());
                        }
                    }
                }
            }

            // Add clicked item to selection
            if (!m_selection.Contains(clicked))
                m_selection.Add(clicked);

            //
            // Check what commands can be issued
            //

            var lstCommands = new List<object>();

            // If only one item we can jump to it
            if (m_selection.Count == 1)
                lstCommands.Add(Command.Goto);

            var bWatchGui =
                clicked.DomNode.GetRoot().Type ==
                SledLuaSchema.SledLuaVarWatchListType.Type;

            if (bWatchGui)
                lstCommands.Add(Command.RemoveWatch);
            else
            {
                // Check if there are any items in the
                // selection that aren't being watched
                var bAnyItemsNotWatched =
                    m_selection.Any(
                        luaVar => !m_luaWatchedVariableService.IsLuaVarWatched(luaVar));

                if (bAnyItemsNotWatched)
                    lstCommands.Add(Command.AddWatch);
            }

            lstCommands.Add(StandardCommand.EditCopy);
            lstCommands.Add(Command.CopyName);
            lstCommands.Add(Command.CopyValue);

            return lstCommands;
        }

        #endregion

        #region Commands

        enum Command
        {
            Goto,

            AddWatch,
            RemoveWatch,

            CopyName,
            CopyValue,
        }

        enum Menu
        {
            VarList,
        }

        enum Group
        {
            NavigateCommands,
            WatchCommands,
            CopyCommands,
        }

        #endregion

        private void HandleAddWatch(IEnumerable<ISledLuaVarBaseType> selection)
        {
            foreach (var luaVar in selection)
            {
                if (m_luaWatchedVariableService.IsLuaVarWatched(luaVar))
                    continue;

                m_luaWatchedVariableService.AddWatchedLuaVar(luaVar);
            }
        }

        private void HandleRemoveWatch(IEnumerable<ISledLuaVarBaseType> selection)
        {
            foreach (var luaVar in selection)
            {
                var rootLevelLuaVar = SledLuaUtil.GetRootLevelVar(luaVar);
                if (!m_luaWatchedVariableService.IsLuaVarWatched(rootLevelLuaVar))
                    continue;

                m_luaWatchedVariableService.RemoveWatchedLuaVar(rootLevelLuaVar);
            }
        }

        private void HandleCopyCommands(Command cmd, IEnumerable<ISledLuaVarBaseType> selection)
        {
            var sb = new StringBuilder();

            foreach (var luaVar in selection)
            {
                switch (cmd)
                {
                    case Command.CopyName: sb.Append(luaVar.Name); break;
                    case Command.CopyValue: sb.Append(luaVar.Value); break;
                }

                sb.Append(Environment.NewLine);
            }

            try
            {
                StandardEditCommands.UseSystemClipboard = true;

                var dataObject = new DataObject(DataFormats.Text, sb.ToString());
                m_standardEditCommands.Clipboard = dataObject;
            }
            finally
            {
                StandardEditCommands.UseSystemClipboard = false;
            }
        }

#pragma warning disable 649 // Field is never assigned

        [Import]
        private StandardEditCommands m_standardEditCommands;

        [Import]
        private ISledGotoService m_gotoService;

        [Import]
        private ISledLuaWatchedVariableService m_luaWatchedVariableService;

#pragma warning restore 649
        
        private readonly List<ISledLuaVarBaseType> m_selection =
            new List<ISledLuaVarBaseType>();

        private static readonly IEnumerable<object> s_emptyCommands =
            EmptyEnumerable<object>.Instance;
    }
}