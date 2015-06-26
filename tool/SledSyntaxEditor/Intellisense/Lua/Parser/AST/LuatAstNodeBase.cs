/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

using System.Collections.Generic;

using ActiproSoftware.SyntaxEditor;

namespace Sce.Sled.SyntaxEditor.Intellisense.Lua.Parser.AST
{
    abstract class LuatAstNodeBase : AstNode
    {
        protected const byte LuatAstNodeBaseContextIDBase = AstNode.AstNodeContextIDBase;

        protected virtual void Initialise() { }
        protected LuatAstNodeBase() { Initialise(); }
        public LuatAstNodeBase(TextRange textRange) : base(textRange) { Initialise(); }

        public virtual void Invalidate(LuatScript script)
        {
            CU.ClearWarnings(script);
        }

        public CompilationUnit CU
        {
            get
            {
                return this.RootNode as CompilationUnit;
            }
        }

        public string NodeSource
        {
            get { return this.CU.Source.Substring(this.StartOffset, this.Length); }
        }

        public void AddError(string message)
        {
            foreach (SyntaxError error in CU.SyntaxErrors)
            {
                if (error.TextRange == TextRange)
                {
                    return;
                }
            }

            CU.SyntaxErrors.Add(new SyntaxError(this.TextRange, message));
        }

        public void AddWarning(LuatScript script, WarningType type, string message)
        {
            LuatWarning warning = new LuatWarning(script, type, this.TextRange, message);
            CU.AddWarning(warning);
        }

        public T FindAncestor<T>() where T : class
        {
            return this.FindAncestor(typeof(T)) as T;
        }

        public T FindNodeRecursive<T>(int offset) where T : class
        {
            IAstNode node = this;
            IAstNode next = node;

            while (null != next)
            {
                node = next;
                next = node.FindDescendant(typeof(T), offset);
            }

            return node as T;
        }

        public virtual void GetScopedValues(LuatScript script, ref Dictionary<string, LuatValue> scopedValues)
        {
            LuatAstNodeBase parent = this.ParentNode as LuatAstNodeBase;

            if (null != parent)
            {
                parent.GetScopedValues(script, ref scopedValues);
            }
        }

        public virtual IEnumerable<AutoCompleteItem> GetAutoCompleteList(LuatScript script, int offset)
        {
            LuatAstNodeBase parent = this.ParentNode as LuatAstNodeBase;

            return (null != parent) ? parent.GetAutoCompleteList(script, offset) : new AutoCompleteItem[0];
        }

        public virtual TextRange GetAutoCompleteTextRange(int offset)
        {
            return new TextRange(offset);
        }

        public class ScriptValue<T> where T : class
        {
            public delegate T ValueConstructor();

            protected Dictionary<LuatScript, T> m_entries = new Dictionary<LuatScript, T>();
            protected ValueConstructor m_valueConstructor;

            public ScriptValue(ValueConstructor valueConstructor)
            {
                m_valueConstructor = valueConstructor;
            }

            public T this[LuatScript script]
            {
                get
                {
                    return Add(script);
                }
            }

            public bool Contains(LuatScript script)
            {
                return m_entries.ContainsKey(script);
            }

            public T Add(LuatScript script)
            {
                T value;
                if (false == m_entries.TryGetValue(script, out value))
                {
                    value = m_valueConstructor();
                    m_entries.Add(script, value);
                }
                return value;
            }

            public void Remove(LuatScript script)
            {
                m_entries.Remove(script);
            }

            public IEnumerable<KeyValuePair<LuatScript, T>> Entries { get { return m_entries; } }

        }
    }
}