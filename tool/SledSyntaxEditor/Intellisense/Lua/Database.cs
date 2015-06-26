/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

using ActiproSoftware.SyntaxEditor;

using Sce.Atf;

using Sce.Sled.SyntaxEditor.Intellisense.Lua.Parser.AST;

namespace Sce.Sled.SyntaxEditor.Intellisense.Lua
{
    class Database
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="taskQueue"></param>
        /// <param name="luaSyntaxLanguage"></param>
        /// <param name="broker"></param>
        public Database(TaskQueue taskQueue, LuatSyntaxLanguage luaSyntaxLanguage, ILuaIntellisenseBroker broker)
        {
            Instance = this;

            m_taskQueue = taskQueue;
            m_language = luaSyntaxLanguage;
            m_broker = broker;

            Status = new StatusImpl();
        }

        /// <summary>
        /// Status
        /// </summary>
        public ILuaintellisenseStatus Status { get; private set; }

        /// <summary>
        /// Queues the processing of the CompilationUnit cu
        /// </summary>
        /// <param name="cu"></param>
        public void Process(CompilationUnit cu)
        {
            m_taskQueue.AddTask(() => DoProcess(cu), TaskQueue.Thread.Worker);
        }

        /// <summary>
        /// Processes the CompilationUnit cu immediately
        /// </summary>
        /// <param name="cu"></param>
        public void ProcessSynchronous(CompilationUnit cu)
        {
            DoProcess(cu);
        }

        /// <summary>
        /// Queues a resolve of all symbols
        /// </summary>
        public void Resolve()
        {
            m_taskQueue.AddTask(DoResolve, TaskQueue.Thread.Worker);
        }

        /// <summary>
        /// Queues a clear of the entire database
        /// </summary>
        public void Clear()
        {
            m_taskQueue.AddTask(DoClear, TaskQueue.Thread.Worker);
        }

        /// <summary>
        /// Queues a reload of the standard library
        /// </summary>
        public void LoadStandardLibrary()
        {
            m_taskQueue.AddTask(DoLoadStandardLibrary, TaskQueue.Thread.Worker);
        }

        /// <summary>
        /// Queues a reload of the entire database
        /// </summary>
        /// <param name="project"></param>
        public void Rebuild(ILuaIntellisenseProject project)
        {
            m_taskQueue.AddTask(() => DoRebuild(project), TaskQueue.Thread.Worker);
        }

        /// <summary>
        /// Queues a process of all warnings
        /// </summary>
        public void ProcessWarnings()
        {
            m_taskQueue.AddTask(DoProcessWarnings, TaskQueue.Thread.Worker);
        }

        /// <summary>
        /// Returns an enumerable list of auto-complete suggestions for the given node and caret position
        /// </summary>
        /// <param name="node"></param>
        /// <param name="offset"></param>
        /// <returns>An enumerable list of auto-complete suggestions for the given node and caret position</returns>
        public IEnumerable<AutoCompleteItem> GetAutoCompleteList(LuatAstNodeBase node, int offset)
        {
            lock (this)
            {
                return GetAutoCompleteListNoLock(node, offset);
            }
        }

        public void RegisterReflectedTypeResolver(ReflectedTypeResolver resolver)
        {
            m_reflectedTypeResolvers.Add(resolver);
        }

        public LuatVariable CreateReflectedVariable(string strTypeName, LuatValue parent)
        {
            foreach (ReflectedTypeResolver resolver in m_reflectedTypeResolvers)
            {
                LuatVariable variable;
                if (resolver(strTypeName, parent, out variable))
                {
                    return variable;
                }
            }

            return new LuatVariable(parent, new LuatTypeReflected(strTypeName), LuatVariableFlags.FixedType);
        }

        /// <summary>
        /// Adds an expression to the unresolved list
        /// </summary>
        /// <param name="script"></param>
        /// <param name="expression"></param>
        /// <returns></returns>
        public bool AddUnresolvedExpression(LuatScript script, Expression expression)
        {
            LinkedListNode<Expression> node = expression.ListNode[script];
            if (node.List == script.UnresolvedExpressions)
            {
                return false;
            }

            if (null != node.List)
            {
                if (node.List != script.ResolvedExpressions)
                {
                    throw new Exception("Expression is part of an unknown list");
                }

                node.List.Remove(node);
            }

            script.UnresolvedExpressions.AddFirst(node);
            return true;
        }

        /// <summary>
        /// Adds a statement to the unresolved list
        /// </summary>
        /// <param name="script"></param>
        /// <param name="statement"></param>
        /// <returns></returns>
        public bool AddUnresolvedStatement(LuatScript script, Statement statement)
        {
            LinkedListNode<Statement> node = statement.ListNode[script];
            if (node.List == script.UnresolvedStatements)
            {
                return false;
            }

            if (null != node.List)
            {
                if (node.List != script.ResolvedStatements)
                {
                    throw new Exception("Statement is part of an unknown list");
                }

                node.List.Remove(node);
            }

            script.UnresolvedStatements.AddFirst(node);
            return true;
        }

        /// <summary>
        /// Adds a variable to the unresolved type list
        /// </summary>
        /// <param name="variable"></param>
        /// <returns></returns>
        public bool AddUnresolvedVariableType(LuatVariable variable)
        {
            if (m_unresolvedVariableTypes.Contains(variable))
                return false;

            if (!variable.IsFixedType)
                variable.Type = LuatTypeUnknown.Instance;

            m_unresolvedVariableTypes.Add(variable);

            foreach (LuatValue.IReference reference in variable.References)
            {
                reference.OnTypeInvalidated();
            }

            return true;
        }

        public static Database Instance { get; private set; }

        /// <summary>
        /// A delegate that can resolve a reflected type from string
        /// </summary>
        /// <param name="strTypeName"></param>
        /// <param name="parent"></param>
        /// <param name="variable"></param>
        /// <returns></returns>
        public delegate bool ReflectedTypeResolver(string strTypeName, LuatValue parent, out LuatVariable variable);

        /// <summary>
        /// Remove the LuatScript from the database
        /// </summary>
        /// <param name="script"></param>
        private void Drop(LuatScript script)
        {
            // Invalidate all nodes
            foreach (LuatAstNodeBase node in Helpers.Traversal<IAstNode>(script.CU, n => n.ChildNodes))
            {
                node.Invalidate(script);
            }

            script.UnresolvedExpressions.Clear();
            script.UnresolvedStatements.Clear();
        }

        /// <summary>
        /// Attempts to resolve (or infer) the variable type
        /// </summary>
        /// <param name="variable"></param>
        /// <returns>true if the type was resolved or false if the type could not be resolved</returns>
        private bool ResolveVariableType(LuatVariable variable)
        {
            LuatType type = variable.Type;

            foreach (LuatValue.IReference assignment in variable.Assignments)
            {
                if (null == assignment.Value)
                {
                    return false;
                }

                LuatType rhsType = assignment.Value.Type;

                if (rhsType is LuatTypeUnknown)
                {
                    // Being assigned an unknown results in an unknown
                    return false;
                }

                if (type is LuatTypeUnknown || type is LuatTypeNil)
                {
                    type = rhsType;
                    continue;
                }

                if (rhsType is LuatTypeNil)
                {
                    continue;
                }

                if (type.Equals(rhsType))
                {
                    continue;
                }

                if (variable.IsFixedType)
                {
                    assignment.AddWarning(WarningType.FixedType, string.Format("Type is fixed to {0}", type));
                    continue;
                }

                // Mixed types.
                var mixed = type as LuatTypeMixed;
                if (null == mixed)
                {
                    mixed = new LuatTypeMixed();
                    mixed.AddType(type);
                    type = mixed;
                }

                mixed.AddType(rhsType);
            }

            if (null == type)
            {
                return false;
            }

            variable.Type = type;

            // Propagate resolving of variable types through expressions
            foreach (LuatValue.IReference reference in variable.References)
            {
                reference.OnTypeInvalidated();
            }

            return true;
        }

        /// <summary>
        /// Populates the warnings for each compilation unit registered with the database
        /// </summary>
        private void DoProcessWarnings()
        {
            lock (this)
            {
                // Remove warnings that are created from this function
                foreach (LuatScript script in m_scripts)
                {
                    if (script.CU == null)
                        continue;

                    script.CU.ClearWarnings(WarningType.UnresolvedVariable);
                    script.CU.ClearWarnings(WarningType.MixedType);
                    script.CU.ClearWarnings(WarningType.UnknownType);

                    foreach (Expression expression in script.UnresolvedExpressions)
                    {
                        var variable = expression as VariableExpression;
                        if (variable != null)
                        {
                            expression.AddWarning(script, WarningType.UnresolvedVariable, "Unknown variable '" + variable.Name.Text + "'");
                            continue;
                        }

                        var index = expression as IndexExpression;
                        if ((index != null) && (index.RHS != null))
                        {
                            index.RHS.AddWarning(script, WarningType.UnresolvedVariable, "Unknown index '" + index.RHS.DisplayText + "'");
                            continue;
                        }
                    }
                }

                foreach (LuatVariable variable in m_unresolvedVariableTypes)
                {
                    var types = new List<LuatType>();
                    foreach (LuatValue assignment in variable.ValueAssignments)
                    {
                        types.Merge(null != assignment ? assignment.Type : LuatTypeUnknown.Instance);
                    }

                    int typeCount = types.Count;
                    if (typeCount > 1)
                    {
                        var sb = new StringBuilder();
                        sb.Append("Assignment of different types ");
                        sb.Append("( ");
                        sb.Append(types.ToArray().ToCommaSeperatedList(type => type.ToString()));
                        sb.Append(" )");

                        foreach (LuatValue.IReference assignment in variable.Assignments)
                        {
                            string warning = string.Format(sb.ToString());
                            assignment.AddWarning(WarningType.MixedType, warning);
                        }
                    }

                    //                 foreach ( LuatValue.IReference reference in variable.References )
                    //                 {
                    //                     reference.AddWarning( WarningType.UnknownType, "Unknown type '" + reference.DisplayText + "'" );
                    //                 }
                }
            }
        }

        /// <summary>
        /// The main processing function for the database
        /// </summary>
        /// <param name="cu"></param>
        private void DoProcess(CompilationUnit cu)
        {
            lock (this)
            {
                // Gather all the scripts that use this lua file
                var scripts = FindScripts(cu.SourceKey);

                foreach (LuatScript script in scripts)
                {
                    ((StatusImpl)Status).Action = string.Format("Dropping stale symbols from '{0}'", script.Name);

                    if (null != script.CU)
                    {
                        // Remove any table variables that were declared in the old compilation unit
                        Drop(script);
                    }

                    ((StatusImpl)Status).Action = string.Format("Adding symbols for {0} ('{1}')", script.Name, Path.GetFileName(cu.SourceKey));

                    script.CU = cu;

                    // Gather up all the expressions and statements and add them to the pending process lists
                    foreach (LuatAstNodeBase node in Helpers.Traversal<IAstNode>(cu, n => n.ChildNodes))
                    {
                        var expression = node as Expression;
                        if (expression != null)
                            AddUnresolvedExpression(script, expression);

                        var statement = node as Statement;
                        if (statement != null)
                            AddUnresolvedStatement(script, statement);
                    }
                }
            }

            // Resolve all the symbols
            DoResolve();

            // Process all the warnings
            DoProcessWarnings();

            // Done
            ((StatusImpl)Status).Finish();
        }

        /// <summary>
        /// Clears the entire database
        /// </summary>
        private void DoClear()
        {
            lock (this)
            {
                m_stdLibs = new LuatTable(null);
                m_unresolvedVariableTypes = new List<LuatVariable>();
                m_scripts = new List<LuatScript>();
            }
        }

        /// <summary>
        /// Rebuilds the entire database
        /// </summary>
        /// <param name="project"></param>
        private void DoRebuild(ILuaIntellisenseProject project)
        {
            DoClear();

            DoLoadStandardLibrary();

            lock (this)
            {
                // use default or user-supplied method for script registration(s)

                if (m_broker.CustomScriptRegistrationHandler != null)
                {
                    var documents = new List<ILuaIntellisenseDocument>();

                    m_broker.CustomScriptRegistrationHandler(m_stdLibs, ref m_scripts, ref documents);

                    foreach (var document in documents)
                        ParseScript(document);
                }
                else
                {
                    foreach (ILuaIntellisenseDocument document in project.AllDocuments)
                    {
                        RegisterScript(document, m_stdLibs);
                        ParseScript(document);
                    }
                }
            }
        }

        private IEnumerable<AutoCompleteItem> GetAutoCompleteListNoLock(LuatAstNodeBase node, int offset)
        {
            var table = new Hashtable();

            foreach (LuatScript script in m_scripts)
            {
                if (script.CU != node.CU)
                    continue;

                foreach (AutoCompleteItem item in node.GetAutoCompleteList(script, offset))
                {
                    if (!table.ContainsKey(item.Name))
                        table.Add(item.Name, item);
                }
            }

            foreach (AutoCompleteItem item in table.Values)
            {
                yield return item;
            }
        }

        /// <summary>
        /// Scans the standard library folder for *.lua files, loading each into the m_stdLibs table
        /// </summary>
        private void DoLoadStandardLibrary()
        {
            var exePath = Assembly.GetEntryAssembly().Location;
            var exeFolder = Path.GetDirectoryName(exePath);
            if (string.IsNullOrEmpty(exeFolder))
                return;

            var stdFolder = Path.Combine(exeFolder, "StandardLibrary");
            if (!Directory.Exists(stdFolder) && !m_attemptedStandardLibraryInstall)
            {
                m_attemptedStandardLibraryInstall = true;
                try
                {
                    Directory.CreateDirectory(stdFolder);

                    var assembly = Assembly.GetAssembly(typeof(Database));
                    var resources =
                        assembly.GetManifestResourceNames()
                            .Where(name => name.StartsWith(StandardLibraryAssemblyPathPrefix));

                    foreach (var resource in resources)
                    {
                        using (var stream = assembly.GetManifestResourceStream(resource))
                        {
                            if ((stream == null) || (stream == Stream.Null))
                                continue;

                            var name = resource.Substring(StandardLibraryAssemblyPathPrefix.Length);
                            using (var reader = new StreamReader(stream))
                            {
                                try
                                {
                                    var diskPath = Path.Combine(stdFolder, name);
                                    File.WriteAllText(diskPath, reader.ReadToEnd(), reader.CurrentEncoding);
                                }
                                catch (Exception ex)
                                {
                                    ex.ToString();
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    ex.ToString();
                }
            }

            var stdFiles = Directory.GetFiles(stdFolder, "*.lua");
            foreach (var stdFile in stdFiles)
            {
                using (var code = new StreamReader(stdFile))
                {
                    var luatScript = LuatScript.Create();
                    luatScript.Name = Path.GetFileNameWithoutExtension(stdFile);
                    luatScript.Table = m_stdLibs;
                    luatScript.Path = stdFile;
                    lock (this)
                    {
                        m_scripts.Add(luatScript);
                    }

                    var document = new Document();
                    document.Text = code.ReadToEnd();
                    document.Filename = stdFile;
                    document.Language = m_language; // triggers a reparse
                }
            }
        }

        /// <summary>
        /// Finds all scripts that refer to the given filepath
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        private IEnumerable<LuatScript> FindScripts(string path)
        {
            var scripts = new List<LuatScript>();
            foreach (LuatScript script in m_scripts)
            {
                if (script.Path == path)
                    scripts.Add(script);
            }

            return scripts;
        }

        private void RegisterScript(ILuaIntellisenseDocument script, LuatTable parentPublicTable)
        {
            var table = new LuatTable(null);
            var publicTable = new LuatTable(table);

            // Chain the public tables using metatable indices so that a child script can
            // access the content of the parent's public table at global scope.
            publicTable.SetMetadataIndexTable(parentPublicTable);

            // Expose the content of the public table at global scope
            table.SetMetadataIndexTable(publicTable);

            // Register the script
            var luatScript = LuatScript.Create();
            luatScript.Name = script.Name;
            luatScript.Table = table;
            luatScript.Path = script.Uri.LocalPath;
            luatScript.Reference = script;
            m_scripts.Add(luatScript);

            table.Description = Helpers.Decorate(script.Name, DecorationType.Code);
        }

        /// <summary>
        /// Triggers process of the given ILuaIntellisenseDocument
        /// </summary>
        /// <param name="script"></param>
        private void ParseScript(ILuaIntellisenseDocument script)
        {
            if (script == null)
                return;

            // don't use script.SyntaxEditorControl in case the file isn't open in the editor

            var document = new Document();
            document.Text = script.Contents;
            document.Filename = script.Uri.LocalPath;
            document.Language = m_language; // triggers a reparse
        }

        /// <summary>
        /// Attempts to resolve the statements and expressions for all scripts registered with the database
        /// </summary>
        private void DoResolve()
        {
            ((StatusImpl)Status).Action = string.Format("Resolving...");

            lock (this)
            {
                // Churn through the expressions and statements
                // processing them until no more processing can be done
                bool bPassResolved = true;

                int passes = 0;

                while (bPassResolved)
                {
                    bPassResolved = false;

                    // Process statements first - Assignments need to be processed before Expressions!
                    foreach (LuatScript script in m_scripts)
                    {
                        LinkedList<Statement> statements = script.UnresolvedStatements;

                        var localScript = script;
                        bPassResolved |= 0 != statements.RemoveAll(s => s.Install(localScript));
                    }

                    foreach (LuatScript script in m_scripts)
                    {
                        LinkedList<Expression> expressions = script.UnresolvedExpressions;

                        var localScript = script;
                        bPassResolved |= 0 != expressions.RemoveAll(e => null != e.Resolve(localScript));
                    }

                    // ((StatusImpl)Status).Progress = UnresolvedCount / initialUnresolved;

                    if (passes++ > 10000)
                    {
                        // Looks like we've become stuck...
                        System.Windows.Forms.MessageBox.Show(@"Lua Intellisense hang detected while resolving statements and expressions.\nPlease report this message, preferably with a reproduction case.");
                        break;
                    }
                }

                // Statements and expressions have been resolved (as much as possible)
                // Now try resolving (or infering) variable types
                passes = 0;

                bPassResolved = true;
                while (bPassResolved)
                {
                    bPassResolved = false;
                    bPassResolved |= 0 != m_unresolvedVariableTypes.RemoveAll(ResolveVariableType);

                    // ((StatusImpl)Status).Progress = UnresolvedCount / initialUnresolved;

                    if (passes++ > 10000)
                    {
                        // Looks like we've become stuck...
                        System.Windows.Forms.MessageBox.Show(@"Lua Intellisense hang detected while resolving types.\nPlease report this message, preferably with a reproduction case.");
                        break;
                    }
                }
            }
        }

        internal struct UnresolvedExpression
        {
            public UnresolvedExpression(LuatScript script, Expression expression)
            {
                Script = script;
                Expression = expression;
            }

            public LuatScript Script;
            public Expression Expression;
        }

        #region Private Classes

        internal class StatusImpl : ILuaintellisenseStatus
        {
            #region ILuaintellisenseStatus Implementation

            public bool Active
            {
                get { return m_active; }
            }

            public string Action
            {
                get { return m_action; }
                set
                {
                    m_action = value;
                    m_active = true;
                    OnChanged();
                }
            }

            public float Progress
            {
                get { return m_progress; }
                set
                {
                    m_progress = value;
                    m_active = true;
                    OnChanged();
                }
            }

            public event EventHandler Changed;

            #endregion

            public void Finish()
            {
                m_active = false;
                m_action = null;
                m_progress = 0;
                OnChanged();
            }

            private void OnChanged()
            {
                Changed.Raise(this, EventArgs.Empty);
            }

            private bool m_active;
            private string m_action;
            private float m_progress;
        }

        #endregion

        private bool m_attemptedStandardLibraryInstall;
        private LuatTable m_stdLibs;
        private List<LuatVariable> m_unresolvedVariableTypes;
        private List<LuatScript> m_scripts;
        
        private readonly TaskQueue m_taskQueue;
        private readonly LuatSyntaxLanguage m_language;
        private readonly ILuaIntellisenseBroker m_broker;

        private readonly List<ReflectedTypeResolver> m_reflectedTypeResolvers =
            new List<ReflectedTypeResolver>();

        public const string StandardLibraryAssemblyPathPrefix =
            "Sce.Sled.SyntaxEditor.Intellisense.Lua.StandardLibrary.";
    }
}