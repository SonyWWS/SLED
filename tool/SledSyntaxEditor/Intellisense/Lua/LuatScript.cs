/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

using System.Collections.Generic;

using Sce.Sled.SyntaxEditor.Intellisense.Lua.Parser.AST;

namespace Sce.Sled.SyntaxEditor.Intellisense.Lua
{
    /// <summary>
    /// LuatScript describes an instance of a luat script file
    /// </summary>
    public class LuatScript
    {
        private LuatScript()
        {
        }

        /// <summary>
        /// Name of the script (may differ from the file name)
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Absoluate path to the script
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        /// Table
        /// </summary>
        public LuatTable Table { get; set; }

        /// <summary>
        /// Document (if any)
        /// </summary>
        public ILuaIntellisenseDocument Reference { get; set; }

        /// <summary>
        /// Create a new LuatScript
        /// </summary>
        /// <returns></returns>
        public static LuatScript Create()
        {
            return new LuatScript();
        }

        internal CompilationUnit CU;
        internal LinkedList<Expression> UnresolvedExpressions = new LinkedList<Expression>();
        internal LinkedList<Expression> ResolvedExpressions = new LinkedList<Expression>();
        internal LinkedList<Statement> UnresolvedStatements = new LinkedList<Statement>();
        internal LinkedList<Statement> ResolvedStatements = new LinkedList<Statement>();
    }
}