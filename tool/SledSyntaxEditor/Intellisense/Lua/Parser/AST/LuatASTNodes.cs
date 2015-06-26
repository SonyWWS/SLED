/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using ActiproSoftware.SyntaxEditor;

namespace Sce.Sled.SyntaxEditor.Intellisense.Lua.Parser.AST
{
    // ======================================================
    // =================== CompilationUnit ==================
    // ======================================================
    partial class CompilationUnit : LuatAstNodeBase, ICompilationUnit, ISemanticParseData
    {
        private List<LuatWarning> m_warnings = new List<LuatWarning>();
        public LuatWarning[] Warnings
        {
            get { return m_warnings.ToArray(); }
        }

        public void AddWarning(LuatWarning warning)
        {
            m_warnings.Add(warning);
        }

        public void ClearWarnings(LuatScript script)
        {
            m_warnings.RemoveAll(a => a.Script == script);
        }

        public void ClearWarnings(WarningType type)
        {
            m_warnings.RemoveAll(a => a.Type == type);
        }

        public override void GetScopedValues(LuatScript script, ref Dictionary<string, LuatValue> scopedValues)
        {
            foreach (var v in script.Table.Children)
            {
                if (scopedValues.ContainsKey(v.Key))
                {
                    continue;
                }

                scopedValues.Add(v.Key, v.Value);
            }
        }
    }

    // ======================================================
    // ===================== Identifier =====================
    // ======================================================
    partial class Identifier : LuatAstNodeBase
    {
        public override TextRange GetAutoCompleteTextRange(int offset)
        {
            return this.TextRange;
        }
    }

    // ======================================================
    // ===================== Expression =====================
    // ======================================================
    abstract partial class Expression : LuatAstNodeBase, IQuickInfoProvider
    {
        public bool IsLHSOfAssignment = false;

        protected ScriptValues m_resolvedValues;
        public ScriptValues ResolvedValues { get { return m_resolvedValues; } }
        public string Description;

        protected ScriptValue<LinkedListNode<Expression>> m_listNode;
        public ScriptValue<LinkedListNode<Expression>> ListNode { get { return m_listNode; } }

        // Contains a LuatValue for each LuatScript that uses this expression
        public class ScriptValues
        {
            private Dictionary<LuatScript, LuatValue> m_entries;
            private Expression m_owner;
            public IEnumerable<LuatScript> Scripts { get { return m_entries.Keys; } }
            public IEnumerable<LuatValue> Values { get { return m_entries.Values; } }

            public ScriptValues(Expression owner)
            {
                m_entries = new Dictionary<LuatScript, LuatValue>();
                m_owner = owner;
            }

            public int Count { get { return m_entries.Count; } }

            // Indexer
            public LuatValue this[LuatScript script]
            {
                get
                {
                    LuatValue value;
                    m_entries.TryGetValue(script, out value);
                    return value;
                }
                set
                {
                    if (null == value)
                    {
                        throw new Exception("Do not assign null to Value. Instead call Invalidate()");
                    }

                    if (m_entries.ContainsKey(script))
                    {
                        throw new Exception("Value can only be assigned once after creation or invalidation");
                    }

                    this.m_entries.Add(script, value);
                    value.AddReference(new Parser.AST.Expression.Reference(script, m_owner, m_owner.DisplayText));
                }
            }

            public void Invalidate(LuatScript script)
            {
                if (m_entries.ContainsKey(script))
                {
                    m_entries[script].RemoveReference(new Parser.AST.Expression.Reference(script, m_owner));
                    m_entries.Remove(script);
                }
            }

            // Provides an iterator over all of the entries
            public IEnumerable<KeyValuePair<LuatScript, LuatValue>> Entries
            {
                get
                {
                    return m_entries;
                }
            }

            // Provides an iterator over the entries grouping scripts that resolve to the same
            // value together.
            public IEnumerable<KeyValuePair<LuatValue, LuatScript[]>> ScriptsByValue
            {
                get
                {
                    var grouped = m_entries.ToArray().GroupItems(c => c.Value, c => c.Key);

                    foreach (var entry in grouped)
                    {
                        yield return entry;
                    }
                }
            }
        }

        internal class Reference : LuatValue.IReference
        {
            public LuatScript Script { get { return m_script; } }
            public Expression Expression { get { return m_expression; } }

            protected LuatScript m_script;
            protected Expression m_expression;
            protected string m_displayText;

            public Reference(LuatScript script, Expression expression)
            {
                m_script = script;
                m_expression = expression;
                m_displayText = expression.DisplayText;
            }

            public Reference(LuatScript script, Expression expression, string displayText)
            {
                m_script = script;
                m_expression = expression;
                m_displayText = displayText;
            }

            #region Comparison
            public override bool Equals(object obj)
            {
                Reference other = obj as Reference;

                if (null == other)
                {
                    return false;
                }

                return (other.Expression == Expression &&
                         other.Script == Script);
            }

            public override int GetHashCode()
            {
                return Script.GetHashCode() ^ Expression.GetHashCode();
            }
            #endregion

            #region IReference

            public LuatValue Value
            {
                get
                {
                    return Expression.ResolvedValues[Script];
                }
            }

            public LuatValue.GotoReference Goto
            {
                get
                {
                    return OnGotoReference;
                }
            }

            private void OnGotoReference()
            {
                ILuaIntellisenseNavigator navigator = LuaTextEditorFactory.Get();
                if ((navigator != null) && (navigator.OpenAndSelectHandler != null))
                    navigator.OpenAndSelectHandler(Script.Path, new SyntaxEditorTextRange(Expression.TextRange));
            }

            public string DisplayText
            {
                get { return m_displayText; }
            }

            public string Context
            {
                get { return Script.Name; }
            }

            public string Path
            {
                get { return Script.Path; }
            }

            public int Line
            {
                get { return Helpers.GetLineNumber(Expression.CU.Source, Expression.StartOffset); }
            }

            public void OnValueInvalidated()
            {
                Expression.Invalidate(Script);
            }

            public void OnTypeInvalidated()
            {
                // Expression.InvalidateType( Script );
            }

            public void AddWarning(WarningType type, string message)
            {
                Expression.AddWarning(Script, type, message);
            }

            public ISyntaxEditorTextRange TextRange
            {
                get { return new SyntaxEditorTextRange(Expression.TextRange); }
            }
            #endregion
        }

        protected override void Initialise()
        {
            m_listNode = new ScriptValue<LinkedListNode<Expression>>(delegate() { return new LinkedListNode<Expression>(this); });
            m_resolvedValues = new ScriptValues(this);
            base.Initialise();
        }

        public virtual LuatValue Resolve(LuatScript script)
        {
            return this.ResolvedValues[script];
        }

        public override void Invalidate(LuatScript script)
        {
            if (null == this.ResolvedValues[script])
            {
                return;
            }

            // Add this expression to the needs-resolving list 
            Database.Instance.AddUnresolvedExpression(script, this);

            // Drop the value
            this.ResolvedValues.Invalidate(script);

            // Propagate invalidation up the syntax tree
            LuatAstNodeBase parent = this.ParentNode as LuatAstNodeBase;
            if (null != parent)
            {
                parent.Invalidate(script);
            }
        }

        public override string ToString()
        {
            return DisplayText;
        }

        string IQuickInfoProvider.QuickInfo
        {
            get
            {
                StringBuilder sb = new StringBuilder();

                var descriptions = new List<KeyValuePair<string, LuatScript>>();
                foreach (var entry in ResolvedValues.Entries)
                {
                    StringBuilder descSB = new StringBuilder();

                    LuatValue value = entry.Value;
                    LuatScript script = entry.Key;

                    descSB.Append("Type: ");
                    descSB.Append(value.Type.Description);

                    if (null != value.Description)
                    {
                        descSB.Append("\n");
                        descSB.Append(value.Description);
                    }

                    descriptions.Add(new KeyValuePair<string, LuatScript>(descSB.ToString(), script));
                }

                var grouped = descriptions.GroupItems(a => a.Key, a => a.Value);

                foreach (var entry in grouped)
                {
                    string typeDesc = entry.Key;
                    LuatScript[] scripts = entry.Value;

                    if (sb.Length > 0)
                    {
                        sb.Append("<br/>");
                    }

                    if (ResolvedValues.Count > 0)
                    {
                        sb.Append("Context: <b>");
                        sb.Append(scripts.ToCommaSeperatedList(script => script.Name));
                        sb.Append("</b>");
                        sb.Append("<br/>");
                    }

                    sb.Append(typeDesc);
                }

                return sb.ToString();
            }
        }
    }

    // ======================================================
    // ================= VariableExpression =================
    // ======================================================
    partial class VariableExpression : Expression
    {
        public string Type;
        public bool IsLocal = false;

        public override LuatValue Resolve(LuatScript script)
        {
            LuatValue value = base.Resolve(script);
            if (null != value)
            {
                return value;
            }

            string name = Name.Text;

            BlockStatement block = this.FindAncestor(typeof(BlockStatement)) as BlockStatement;

            if (IsLocal)
            {
                value = block.AddLocal(script, Name, Type);
            }
            else
            {
                // Start by searching locals of the ancestor blocks 
                while (block != null)
                {
                    value = block.Locals[script].Index(name);
                    if (null != value)
                    {
                        ResolvedValues[script] = value;
                        return value;
                    }

                    block = block.FindAncestor(typeof(BlockStatement)) as BlockStatement;
                }

                // Now try globals
                value = script.Table.Index(name, IsLHSOfAssignment);
            }

            if (null != value)
            {
                this.ResolvedValues[script] = value;
            }

            return value;
        }

        public override void Invalidate(LuatScript script)
        {
            base.Invalidate(script);
        }
    }

    // ======================================================
    // =================== IndexExpression ==================
    // ======================================================
    partial class IndexExpression : Expression
    {
        public override LuatValue Resolve(LuatScript script)
        {
            LuatValue value = base.Resolve(script);
            if (null != value)
            {
                return value;
            }

            LuatValue lhsValue = LHS.Resolve(script);
            if (null == lhsValue)
            {
                return null;
            }

            if (null == RHS)
            {
                return null;
            }

            string index;
            if (false == GetString(RHS, out index))
            {
                return null;
            }

            value = lhsValue.Index(index, IsLHSOfAssignment);
            if (null == value)
            {
                return null;
            }

            this.ResolvedValues[script] = value;
            return value;
        }

        protected bool GetString(IAstNode node, out string str)
        {
            str = "";

            if (node is Identifier)
            {
                str = (node as Identifier).Text;
                return true;
            }

            if (node is StringExpression)
            {
                str = (node as StringExpression).String;
                return true;
            }

            if (node is NumberExpression)
            {
                str = (node as NumberExpression).Number.ToString();
                return true;
            }

            return false;
        }

        public override IEnumerable<AutoCompleteItem> GetAutoCompleteList(LuatScript script, int offset)
        {
            if (offset > this.LHS.EndOffset)
            {
                LuatValue value = LHS.ResolvedValues[script];
                if (null != value)
                {
                    foreach (KeyValuePair<string, LuatValue> child in value.Children)
                    {
                        yield return new AutoCompleteItem(child.Key, child.Value);
                    }
                }
            }
            else
            {
                foreach (AutoCompleteItem item in base.GetAutoCompleteList(script, offset))
                {
                    yield return item;
                }
            }
        }

        public override TextRange GetAutoCompleteTextRange(int offset)
        {
            if (null != RHS && offset >= RHS.StartOffset)
            {
                return RHS.TextRange;
            }
            else
            {
                return new TextRange(this.IndexToken.EndOffset);
            }
        }
    }

    // ======================================================
    // ================== TableConstructor ==================
    // ======================================================
    partial class TableConstructor : Expression
    {
        public override LuatValue Resolve(LuatScript script)
        {
            LuatValue value = base.Resolve(script);
            if (null != value)
            {
                return value;
            }

            LuatTable table = new LuatTable(null);
            int fieldIndex = 1;
            foreach (Field field in Fields)
            {
                string key;
                if (null == field.Key)
                {
                    key = (fieldIndex++).ToString();
                }
                else if (field.Key is StringExpression)
                {
                    key = (field.Key as StringExpression).String;
                }
                else if (field.Key is NumberExpression)
                {
                    key = (field.Key as NumberExpression).Number.ToString();
                }
                else if (field.Key is Identifier)
                {
                    key = (field.Key as Identifier).Text;
                }
                else
                {
                    continue;
                }

                if (null != table.Index(key, false))
                {
                    field.AddWarning(script, WarningType.DuplicateTableKey, string.Format("Table already contains key '{0}'", key));
                    continue;
                }

                LuatVariable entry = table.AddChild(key, new LuatVariable(table));
                entry.Description = Description;

                if (null != field.Value)
                {
                    entry.AddAssignment(new Parser.AST.Expression.Reference(script, field.Value, this.DisplayText));
                }
            }

            this.ResolvedValues[script] = table;
            return table;
        }
    }

    // ======================================================
    // ====================== Function ======================
    // ======================================================
    partial class Function : Expression, ICollapsibleNode
    {
        public override LuatValue Resolve(LuatScript script)
        {
            LuatValue value = base.Resolve(script);
            if (null != value)
            {
                return value;
            }

            int index = 1;
            foreach (Identifier identifier in this.Parameters)
            {
                LuatVariable local = this.Block.AddLocal(script, identifier, null);
                local.Description = string.Format("parameter {0}", index++);
            }

            List<string> args = new List<string>();
            foreach (Identifier param in this.Parameters)
            {
                args.Add(param.Text);
            }

            LuatValue returnValue = this.Block.ReturnValues[script];
            LuatFunction function = new LuatFunction(returnValue, args.ToArray());
            function.Description = this.Description;
            function.ExpectsSelf = this.ExpectsSelf;
            value = function;

            this.ResolvedValues[script] = value;

            return value;
        }

        public override string DisplayText
        {
            get
            {
                StringBuilder text = new StringBuilder();
                text.Append("function( ");
                text.Append(Parameters.ToArray().ToCommaSeperatedList(a => a.DisplayText));
                text.Append(" )");
                return text.ToString();
            }
        }
    }

    // ======================================================
    // ==================== FunctionCall ====================
    // ======================================================
    partial class FunctionCall : Expression
    {
        protected Dictionary<LuatScript, LuatFunction> m_resolvedFunctions = new Dictionary<LuatScript, LuatFunction>();
        public IEnumerable<LuatFunction> ResolvedFunctions
        {
            get { return m_resolvedFunctions.Values; }
        }

        public override LuatValue Resolve(LuatScript script)
        {
            LuatValue value = base.Resolve(script);
            if (null != value)
            {
                return value;
            }

            LuatValue funcValue = Owner.Resolve(script);
            if (null == funcValue)
            {
                return null;
            }

            if (null != Name)
            {
                // funcValue isn't actually the function, but the owner.
                funcValue = funcValue.Index(Name.Text);
                if (null == funcValue)
                {
                    return null;
                }
            }

            // Infer return type for value
            LuatFunction function = funcValue as LuatFunction;
            if (null == function)
            {
                LuatVariable variable = funcValue as LuatVariable;
                if (null != variable)
                {
                    foreach (LuatValue.IReference reference in variable.AssignmentsRecursive)
                    {
                        function = reference.Value as LuatFunction;

                        if (null != function)
                        {
                            break;
                        }
                    }
                }
            }

            if (null == function)
            {
                return null;
            }

            if (function.ExpectsSelf != this.PassesSelf)
            {
                string warning = string.Format("Function expects to be called using '{0}' not '{1}'",
                                                 function.ExpectsSelf ? ':' : '.',
                                                 this.PassesSelf ? ':' : '.');
                AddWarning(script, WarningType.WrongFunctionCall, warning);
            }

            this.m_resolvedFunctions.Add(script, function);
            this.ResolvedValues[script] = function.ReturnValue;
            return function;
        }

        public override void Invalidate(LuatScript script)
        {
            this.m_resolvedFunctions.Remove(script);

            base.Invalidate(script);
        }

        public override IEnumerable<AutoCompleteItem> GetAutoCompleteList(LuatScript script, int offset)
        {
            if (offset > this.Owner.EndOffset)
            {
                LuatValue value = Owner.ResolvedValues[script];
                if (null != value)
                {
                    foreach (KeyValuePair<string, LuatValue> child in value.Children)
                    {
                        if (child.Value.Type is LuatTypeFunction)
                        {
                            yield return new AutoCompleteItem(child.Key, child.Value);
                        }
                    }
                }
            }
            else
            {
                foreach (AutoCompleteItem item in base.GetAutoCompleteList(script, offset))
                {
                    yield return item;
                }
            }
        }
    }

    // ======================================================
    // ================= LiteralExpression ==================
    // ======================================================
    partial class LiteralExpression : Expression
    {
        protected abstract LuatType Type { get; }

        public override LuatValue Resolve(LuatScript script)
        {
            LuatValue value = base.Resolve(script);
            if (null == value)
            {
                value = new LuatLiteral(Type);
                value.Description = this.Description;
                this.ResolvedValues[script] = value;
            }

            return value;
        }

        public override IEnumerable<AutoCompleteItem> GetAutoCompleteList(LuatScript script, int offset)
        {
            return new AutoCompleteItem[0];
        }
    }

    // ======================================================
    // ================== NumberExpression ==================
    // ======================================================
    partial class NumberExpression : LiteralExpression
    {
        protected override LuatType Type { get { return LuatTypeNumber.Instance; } }
    }

    // ======================================================
    // ==================== NilExpression ===================
    // ======================================================
    partial class NilExpression : LiteralExpression
    {
        protected override LuatType Type { get { return LuatTypeNil.Instance; } }
    }

    // ======================================================
    // ================== BooleanExpression ==================
    // ======================================================
    partial class BooleanExpression : LiteralExpression
    {
        protected override LuatType Type { get { return LuatTypeBoolean.Instance; } }
    }

    // ======================================================
    // ================== StringExpression ==================
    // ======================================================
    partial class StringExpression : LiteralExpression
    {
        protected override LuatType Type { get { return LuatTypeString.Instance; } }
    }

    // ======================================================
    // ====================== Statement =====================
    // ======================================================
    abstract partial class Statement : LuatAstNodeBase
    {
        protected ScriptValue<LinkedListNode<Statement>> m_listNode;
        public ScriptValue<LinkedListNode<Statement>> ListNode { get { return m_listNode; } }

        protected delegate void UninstallAction();
        private class UninstallActions : List<UninstallAction> { }
        private ScriptValue<UninstallActions> m_uninstallScriptActions = new ScriptValue<UninstallActions>(delegate() { return new UninstallActions(); });

        protected override void Initialise()
        {
            m_listNode = new ScriptValue<LinkedListNode<Statement>>(delegate() { return new LinkedListNode<Statement>(this); });
        }

        public virtual bool Install(LuatScript script)
        {
            return true;
        }

        public virtual bool Uninstall(LuatScript script)
        {
            foreach (UninstallAction uninstallAction in m_uninstallScriptActions[script])
            {
                uninstallAction();
            }

            m_uninstallScriptActions.Remove(script);

            return true;
        }

        public override void Invalidate(LuatScript script)
        {
            if (Database.Instance.AddUnresolvedStatement(script, this))
            {
                Uninstall(script);
            }
        }

        protected void AddUninstallAction(LuatScript script, UninstallAction action)
        {
            m_uninstallScriptActions[script].Add(action);
        }
    }

    // ======================================================
    // =================== BlockStatement ===================
    // ======================================================
    partial class BlockStatement : Statement
    {
        public ScriptValue<LuatTable> Locals { get { return m_locals; } }
        public ScriptValue<LuatVariable> ReturnValues { get { return m_returnValues; } }

        protected ScriptValue<LuatTable> m_locals = new ScriptValue<LuatTable>(delegate() { return new LuatTable(); });
        protected ScriptValue<LuatVariable> m_returnValues = new ScriptValue<LuatVariable>(delegate() { return new LuatVariable(); });

        public override void Invalidate(LuatScript script)
        {
            this.Locals.Remove(script);

            base.Invalidate(script);
        }

        public override void GetScopedValues(LuatScript script, ref Dictionary<string, LuatValue> scopedValues)
        {
            foreach (var v in Locals[script].Children)
            {
                if (scopedValues.ContainsKey(v.Key))
                {
                    continue;
                }

                scopedValues.Add(v.Key, v.Value);
            }

            base.GetScopedValues(script, ref scopedValues);
        }

        public LuatVariable AddLocal(LuatScript script, Identifier Name, string type)
        {
            string name = Name.Text;

            LuatVariable variable = Locals[script].Index(name, false) as LuatVariable;
            if (null == variable)
            {
                LuatTable localTable = Locals[script];

                if (null != type)
                {
                    variable = Database.Instance.CreateReflectedVariable(type, localTable);
                }

                if (null == variable)
                {
                    variable = new LuatVariable(localTable);
                }

                localTable.AddChild(name, variable);
            }
            else
            {
                Name.AddWarning(script, WarningType.DuplicateLocal, "Warning: Local '" + name + "' already defined");
            }

            return variable;
        }

        public override IEnumerable<AutoCompleteItem> GetAutoCompleteList(LuatScript script, int offset)
        {
            Dictionary<string, LuatValue> scopedValues = new Dictionary<string, LuatValue>();
            GetScopedValues(script, ref scopedValues);
            foreach (KeyValuePair<string, LuatValue> v in scopedValues)
            {
                yield return new AutoCompleteItem(v.Key, v.Value);
            }

            yield return new AutoCompleteItem("function", null, LuaIntellisenseIconType.Keyword);
            yield return new AutoCompleteItem("local", null, LuaIntellisenseIconType.Keyword);
            yield return new AutoCompleteItem("end", null, LuaIntellisenseIconType.Keyword);
            yield return new AutoCompleteItem("for", null, LuaIntellisenseIconType.Keyword);
            yield return new AutoCompleteItem("if", null, LuaIntellisenseIconType.Keyword);
            yield return new AutoCompleteItem("do", null, LuaIntellisenseIconType.Keyword);
            yield return new AutoCompleteItem("while", null, LuaIntellisenseIconType.Keyword);
            yield return new AutoCompleteItem("repeat", null, LuaIntellisenseIconType.Keyword);
            yield return new AutoCompleteItem("return", null, LuaIntellisenseIconType.Keyword);
        }
    }

    // ======================================================
    // =================== ReturnStatement ==================
    // ======================================================
    partial class ReturnStatement : Statement
    {
        public bool IsMultiline = false;

        public override bool Install(LuatScript script)
        {
            if (false == base.Install(script))
            {
                return false;
            }

            BlockStatement block;

            Function func = this.FindAncestor<Function>();
            if (null != func)
            {
                block = func.Block;
            }
            else
            {
                block = this.FindAncestor<BlockStatement>();
            }

            if (null == block)
            {
                throw new Exception("ReturnStatement requires a BlockStatement as ancestor");
            }

            List<LuatValue> values = new List<LuatValue>();
            foreach (Expression expression in Values)
            {
                // TODO: not correct for multiple return values
                LuatVariable returnValue = block.ReturnValues[script];
                LuatValue.IReference reference = new Parser.AST.Expression.Reference(script, expression, "return " + expression.DisplayText);
                returnValue.AddAssignment(reference);
                AddUninstallAction(script, delegate() { returnValue.RemoveAssignment(reference); });
            }

            if (IsMultiline)
            {
                AddWarning(script, WarningType.MultilineReturn, string.Format("Returning value from next line"));
            }

            return true;
        }
    }

    // ======================================================
    // ================= AssignmentStatement ================
    // ======================================================
    partial class AssignmentStatement : Statement
    {
        public override bool Install(LuatScript script)
        {
            if (false == base.Install(script))
            {
                return false;
            }

            if (false == IsLocal && Values.Count == 0)
            {
                // Illegal assignment statement, assumes parser has already emitted error.
                return false;
            }

            int variableCount = Variables.Count;
            for (int variableIndex = 0; variableIndex < variableCount; ++variableIndex)
            {
                Expression lhs = Variables[variableIndex] as Expression;
                Expression rhs;

                if (variableIndex < Values.Count)
                {
                    rhs = Values[variableIndex] as Expression;
                }
                else
                {
                    rhs = new NilExpression(lhs.TextRange);
                    rhs.Resolve(script);
                    this.ChildNodes.Add(rhs);
                }

                LuatVariable variable = lhs.Resolve(script) as LuatVariable;
                bool bValid = false;

                do
                {
                    if (null == variable)
                    {
                        break;
                    }

                    if (variable.IsReadOnly)
                    {
                        this.AddError(string.Format("{0} is read-only", lhs.DisplayText));
                        break;
                    }

                    bValid = true;
                }
                while (false);

                if (false == bValid)
                {
                    // Failed to resolve or create the name.
                    // Undo all the assignments we've done and return incomplete.
                    Uninstall(script);
                    return false;
                }

                string displayText = string.Format("{0} = {1}", lhs.DisplayText, rhs.DisplayText);
                LuatValue.IReference reference = new Parser.AST.Expression.Reference(script, rhs, displayText);
                variable.AddAssignment(reference);
                AddUninstallAction(script, delegate() { variable.RemoveAssignment(reference); });
            }

            return true;
        }
    }

    // ======================================================
    // ==================== ForStatement ====================
    // ======================================================
    partial class ForStatement : Statement
    {
        public override bool Install(LuatScript script)
        {
            if (false == base.Install(script))
            {
                return false;
            }

            if (null == this.Body)
            {
                return false;
            }

            if (null == this.Start)
            {
                return false;
            }

            LuatVariable iterator = Body.AddLocal(script, Iterator.Name, null);
            LuatValue.IReference reference = new Parser.AST.Expression.Reference(script, this.Start);
            iterator.AddAssignment(reference);
            AddUninstallAction(script, delegate() { iterator.RemoveAssignment(reference); });

            return base.Install(script);
        }
    }

    // ======================================================
    // =================== ForInStatement ===================
    // ======================================================
    partial class ForInStatement : Statement
    {
        public override bool Install(LuatScript script)
        {
            if (false == base.Install(script))
            {
                return false;
            }

            if (null == this.Body)
            {
                return false;
            }

            foreach (Identifier iterator in this.Iterators)
            {
                Body.AddLocal(script, iterator, null);
            }

            return base.Install(script);
        }
    }
}