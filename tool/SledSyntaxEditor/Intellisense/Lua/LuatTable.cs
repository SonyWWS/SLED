/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

using System;
using System.Collections.Generic;

namespace Sce.Sled.SyntaxEditor.Intellisense.Lua
{
    /// <summary>
    /// An instance of a table
    /// </summary>
    public class LuatTable : LuatValue
    {
        public LuatTable()
            : base(null)
        {
        }

        public LuatTable(LuatValue parent)
            : base(parent)
        {
        }

        public override LuatType Type
        {
            get { return LuatTypeTable.Instance; }
            set { throw new Exception("LuatTable.Type is not assignable"); }
        }

        /// <summary>
        /// Adds an item to the table
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="index"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public T AddChild<T>(string index, T value) where T : LuatValue
        {
            m_children.Add(index, value);
            return value;
        }

        /// <summary>
        /// Removes an item from the table by name
        /// </summary>
        /// <param name="name"></param>
        public void RemoveChild(string name)
        {
            m_children.Remove(name);
        }

        // Removes an item from the table by value
        public void RemoveChild(LuatValue value)
        {
            var names = new List<string>();
            foreach (KeyValuePair<string, LuatValue> child in m_children)
            {
                if (child.Value == value)
                    names.Add(child.Key);
            }

            if (names.Count == 0)
                throw new Exception("Child not found");

            foreach (string name in names)
            {
                m_children.Remove(name);
            }
        }

        public void AddAlias(string name, LuatValue value)
        {
            Metatable.CreateIndex();
            Metatable.Index.AddChild(name, value);
        }

        public void SetMetadataIndexTable(LuatTable table)
        {
            // Skip a meta-table so we don't start adding
            // entries into table with calls to AddAlias()
            Metatable.CreateIndex();
            Metatable.Index.Metatable.Index = table;
        }

        // Lookup the value in the table
        public override LuatValue Index(string index, bool bAssignment, ref HashSet<LuatValue> visited)
        {
            if (visited.Contains(this))
                return null;

            visited.Add(this);

            LuatValue res;
            if (m_children.TryGetValue(index, out res))
                return res;

            if (bAssignment)
            {
                if (null != m_metatable && null != m_metatable.NewIndex)
                    return m_metatable.NewIndex.Index(index, true, ref visited);
                
                return AddChild(index, new LuatVariable(this));
            }
            
            if (null != m_metatable && null != m_metatable.Index)
                return m_metatable.Index.Index(index, false, ref visited);
                
            return null;
        }

        // Returns an enumerable collection of string-value pairs of all the items in the table.
        public override IEnumerable<KeyValuePair<string, LuatValue>> GetChildren(ref HashSet<LuatValue> visited)
        {
            visited.Add(this);

            if ((m_metatable == null) ||
                (m_metatable.Index == null) ||
                (visited.Contains(m_metatable.Index)))
            {
                // No metatable Index - simply return the children
                return m_children;
            }

            // Merge children with the metatable Index children
            var children = new Dictionary<string, LuatValue>(m_children);

            foreach (KeyValuePair<string, LuatValue> child in m_metatable.Index.GetChildren(ref visited))
            {
                if (!children.ContainsKey(child.Key))
                    children.Add(child.Key, child.Value);
            }

            return children;
        }

        protected LuatMetaTable Metatable
        {
            get { return m_metatable ?? (m_metatable = new LuatMetaTable()); }
        }

        private LuatMetaTable m_metatable;

        private readonly Dictionary<string, LuatValue> m_children =
            new Dictionary<string, LuatValue>();
    }
}