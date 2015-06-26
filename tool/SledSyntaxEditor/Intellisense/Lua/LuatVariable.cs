/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

using System;
using System.Collections.Generic;
using System.Text;

namespace Sce.Sled.SyntaxEditor.Intellisense.Lua
{
    /// <summary>
    /// Luat variable flags
    /// </summary>
    [Flags]
    public enum LuatVariableFlags
    {
        None,
        ReadOnly,
        FixedType
    }

    /// <summary>
    /// A luat variable instance
    /// </summary>
    public class LuatVariable : LuatValue
    {
        public LuatVariable()
            : base(null)
        {
        }

        public LuatVariable(LuatValue parent)
            : base(parent)
        {
        }

        public LuatVariable(LuatValue parent, LuatVariableFlags flags)
            : base(parent)
        {
            m_flags = flags;
        }

        public LuatVariable(LuatValue parent, LuatType type, LuatVariableFlags flags)
            : base(parent)
        {
            m_flags = flags;
            m_type = type;
        }

        public override string GetDescription(ref HashSet<LuatValue> visited)
        {
            visited.Add(this);

            var sb = new StringBuilder();
            if (null != InternalDescription) { return InternalDescription; }

            bool bFirst = true;
            foreach (IReference reference in Assignments)
            {
                if (false == bFirst) { sb.AppendLine(); }
                bFirst = false;
                sb.Append(Helpers.Decorate(reference.DisplayText, DecorationType.Code));

                if (null == reference.Value)
                {
                    continue;
                }

                if (visited.Contains(reference.Value))
                {
                    continue;
                }

                string description = reference.Value.GetDescription(ref visited);
                if (null != description)
                {
                    sb.AppendLine();
                    sb.Append(Helpers.Tab(description, 2));
                }
            }
            return sb.ToString();
        }

        public override LuatType Type
        {
            get { return m_type; }
            set
            {
                if (value == null)
                {
                    throw new Exception("Variable Type cannot be assigned null");
                }

                if (IsFixedType && false == m_type.Equals(value))
                {
                    throw new Exception("Variable Type is fixed");
                }

                m_type = value;
            }
        }

        public IReference[] Assignments { get { return m_assignments.ToArray(); } }
        public bool IsReadOnly { get { return (m_flags & LuatVariableFlags.ReadOnly) == LuatVariableFlags.ReadOnly; } }
        public bool IsFixedType { get { return (m_flags & LuatVariableFlags.FixedType) == LuatVariableFlags.FixedType; } }

        public void SetInitialiser(IReference initialiser)
        {
            if (m_assignments.Contains(initialiser))
            {
                throw new Exception("Already initialised");
            }

            m_assignments.Add(initialiser);

            // Type has possibly changed
            Database.Instance.AddUnresolvedVariableType(this);
        }

        public void AddAssignment(IReference assignment)
        {
            if (IsReadOnly)
            {
                throw new Exception("Cannot assign to read only variable");
            }

            if (m_assignments.Contains(assignment))
            {
                throw new Exception("Already assigned");
            }

            m_assignments.Add(assignment);

            // Type has possibly changed
            Database.Instance.AddUnresolvedVariableType(this);
        }

        public void RemoveAssignment(IReference assignment)
        {
            if (false == m_assignments.Remove(assignment))
            {
                throw new Exception("Assignment not found");
            }

            if (0 == m_assignments.Count)
            {
                // Nothing assigns to this variable any more.
                Destroy();
            }
            else
            {
                // Type has possibly changed
                Database.Instance.AddUnresolvedVariableType(this);
            }
        }

        // Returns an enumerator of all direct and indirect assignments of this variable
        public IEnumerable<IReference> AssignmentsRecursive
        {
            get
            {
                var visited = new HashSet<LuatValue>();
                return GetAssignmentsRecursive(ref visited);
            }
        }

        // Returns a list of all direct and indirect assignments of this variable
        protected IReference[] GetAssignmentsRecursive(ref HashSet<LuatValue> visited)
        {
            visited.Add(this);

            var assignments = new List<IReference>();
            assignments.AddRange(Assignments);

            foreach (IReference reference in Assignments)
            {
                var variable = reference.Value as LuatVariable;
                if (variable != null && visited.Contains(variable))
                    continue;

                if (variable != null)
                    assignments.Merge(variable.GetAssignmentsRecursive(ref visited));
            }

            return assignments.ToArray();
        }

        public void Destroy()
        {
            // Remove it from the table.
            if (Parent is LuatTable)
            {
                var table = Parent as LuatTable;
                table.RemoveChild(this);
            }

            // Invalidate all expressions using this variable
            foreach (IReference reference in References)
            {
                reference.OnValueInvalidated();
            }
        }

        // Returns an enumerator of all direct assignments of this variable
        public IEnumerable<LuatValue> ValueAssignments
        {
            get
            {
                foreach (IReference assignment in m_assignments)
                {
                    if (assignment.Value != null)
                        yield return assignment.Value;
                }
            }
        }

        // Attempts to index the given variable by examining the assigned values
        public override LuatValue Index(string index, bool bAssignment, ref HashSet<LuatValue> visited)
        {
            visited.Add(this);

            foreach (LuatValue value in ValueAssignments)
            {
                if (visited.Contains(value))
                {
                    continue;
                }

                LuatValue var = value.Index(index, bAssignment, ref visited);
                if (null != var)
                {
                    return var;
                }
            }

            return null;
        }

        // Returns an enumerator of all the children of all the assigned values
        public override IEnumerable<KeyValuePair<string, LuatValue>> GetChildren(ref HashSet<LuatValue> visited)
        {
            visited.Add(this);

            var children = new Dictionary<string, LuatValue>();
            foreach (LuatValue value in ValueAssignments)
            {
                if (visited.Contains(value))
                    continue;

                foreach (KeyValuePair<string, LuatValue> child in value.GetChildren(ref visited))
                {
                    children.Add(child.Key, child.Value);
                }
            }

            return children;
        }

        private LuatType m_type = LuatTypeUnknown.Instance;

        private readonly LuatVariableFlags m_flags;

        private readonly List<IReference> m_assignments =
            new List<IReference>();
    }
}