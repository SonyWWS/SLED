/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

using System.Collections.Generic;

using Sce.Sled.SyntaxEditor.Intellisense.Lua;

namespace Sce.Sled.Lua.Intellisense
{
    public static class LuaIntellisenseServiceHelpers
    {
        public static void RegisterProject(ILuaIntellisenseProject project, LuatTable stdLibs, ref List<LuatScript> scripts, ref List<ILuaIntellisenseDocument> documents)
        {
            if (project == null)
                return;

            var allDocuments = new List<ILuaIntellisenseDocument>(project.AllDocuments);
            documents.AddRange(allDocuments);

            foreach (var document in allDocuments)
            {
                var script = CreateLuatScript(document);
                script.Table.SetMetadataIndexTable(stdLibs);
                AddSledSpecificFunctions(script.Table);
                scripts.Add(script);
            }
        }

        private static LuatScript CreateLuatScript(ILuaIntellisenseDocument document)
        {
            var table = new LuatTable(null) { Description = Helpers.Decorate(document.Name, DecorationType.Code) };

            var luatScript = LuatScript.Create();
            luatScript.Table = table;
            luatScript.Path = document.Uri.LocalPath;
            luatScript.Reference = document;
            luatScript.Name = document.Name;

            return luatScript;
        }

        private static void AddSledSpecificFunctions(LuatTable table)
        {
            if (table == null)
                return;

            // add libsleddebugger table
            {
                var debuggerTable = new LuatTable { Description = "libsleddebugger injected functionality" };

                debuggerTable.AddChild("version", new LuatLiteral(LuatTypeString.Instance));
                debuggerTable.AddChild("instance", new LuatLiteral(LuatTypeNumber.Instance));

                table.AddChild("libsleddebugger", debuggerTable);
            }

            // add libsledluaplugin table
            {
                var luaPluginTable = new LuatTable { Description = "libsledluaplugin injected functionality" };

                luaPluginTable.AddChild("version", new LuatLiteral(LuatTypeString.Instance));
                luaPluginTable.AddChild("instance", new LuatLiteral(LuatTypeNumber.Instance));

                {
                    var retVal = new LuatVariable(null, LuatTypeNil.Instance, LuatVariableFlags.None);
                    var function = new LuatFunction(retVal, new[] { "message" }) { ExpectsSelf = false };
                    luaPluginTable.AddChild("tty", function);
                }

                {
                    var retVal = new LuatVariable(null, LuatTypeNil.Instance, LuatVariableFlags.None);
                    var function = new LuatFunction(retVal, new[] { "condition", "message" }) { ExpectsSelf = false };
                    luaPluginTable.AddChild("assert", function);
                }

                {
                    var retVal = new LuatVariable(null, LuatTypeNil.Instance, LuatVariableFlags.None);
                    var function = new LuatFunction(retVal, new[] { "error" }) { ExpectsSelf = false };
                    luaPluginTable.AddChild("errorhandler", function);
                }

                table.AddChild("libsledluaplugin", luaPluginTable);
            }
        }
    }
}