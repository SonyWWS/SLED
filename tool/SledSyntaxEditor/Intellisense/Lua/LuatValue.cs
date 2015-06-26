/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

using System;
using System.Collections.Generic;

namespace Sce.Sled.SyntaxEditor.Intellisense.Lua
{
    /// <summary>
    /// An instance of a luat value
    /// </summary>
    [System.Diagnostics.DebuggerDisplay("{Description}")]
    public abstract class LuatValue
    {
        protected LuatValue(LuatValue parent)
        {
            Parent = parent;
        }

        public string Description
        {
            get
            {
                var visited = new HashSet<LuatValue>();
                return GetDescription(ref visited);
            }
            set { InternalDescription = value; }
        }

        public abstract LuatType Type
        {
            get;
            set;
        }

        public IReference[] References { get { return m_references.ToArray(); } }

        public LuatValue Parent { get; private set; }

        public void AddReference(IReference reference)
        {
            if (m_references.Contains(reference))
            {
                throw new Exception("Reference already added!");
            }

            m_references.Add(reference);
        }

        public void RemoveReference(IReference reference)
        {
            if (false == m_references.Remove(reference))
            {
                throw new Exception("Reference not found");
            }
        }

        public T Index<T>(string index) where T : LuatValue
        {
            return Index(index) as T;
        }

        public virtual LuatValue Index(string index)
        {
            return Index(index, false);
        }

        public T Index<T>(string index, bool bAssignment) where T : LuatVariable
        {
            return Index(index, bAssignment) as T;
        }

        public LuatValue Index(string index, bool bAssignment)
        {
            var visited = new HashSet<LuatValue>();
            return Index(index, bAssignment, ref visited);
        }

        public IEnumerable<KeyValuePair<string, LuatValue>> Children
        {
            get
            {
                var visited = new HashSet<LuatValue>();
                return GetChildren(ref visited);
            }
        }

        public virtual void Invalidate()
        {
            foreach (IReference reference in References)
            {
                reference.OnValueInvalidated();
            }
        }

        public virtual string GetDescription(ref HashSet<LuatValue> visited)
        {
            return InternalDescription;
        }

        public abstract LuatValue Index(string index, bool bAssignment, ref HashSet<LuatValue> visited);

        public abstract IEnumerable<KeyValuePair<string, LuatValue>> GetChildren(ref HashSet<LuatValue> visited);

        public delegate void GotoReference();

        public interface IReference
        {
            LuatValue Value { get; }
            GotoReference Goto { get; }
            string DisplayText { get; }
            string Context { get; }
            string Path { get; }
            int Line { get; }
            ISyntaxEditorTextRange TextRange { get; }
            void OnValueInvalidated();
            void OnTypeInvalidated();
            void AddWarning(WarningType type, string message);
        }

        protected string InternalDescription;
        
        private readonly List<IReference> m_references =
            new List<IReference>();
    }
}