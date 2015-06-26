/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

using System;
using System.Collections.Generic;

namespace Sce.Sled.SyntaxEditor.Intellisense.Lua
{
    /// <summary>
    /// A literal value
    /// </summary>
    public class LuatLiteral : LuatValue
    {
        public LuatLiteral(LuatType type)
            : base(null)
        {
            m_type = type;
        }

        public override LuatType Type
        {
            get { return m_type; }
            set { throw new Exception("Literal type cannot be changed"); }
        }

        public override LuatValue Index(string index, bool bAssignment, ref HashSet<LuatValue> visited)
        {
            return null;
        }

        public override IEnumerable<KeyValuePair<string, LuatValue>> GetChildren(ref HashSet<LuatValue> visited)
        {
            return new KeyValuePair<string, LuatValue>[0];
        }

        private readonly LuatType m_type;
    }
}