/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

using System;
using System.Collections.Generic;

namespace Sce.Sled.SyntaxEditor.Intellisense.Lua
{
    /// <summary>
    /// A function instance
    /// </summary>
    public class LuatFunction : LuatValue
    {
        public LuatFunction(LuatValue returnValue, string[] arguments)
            : base(null)
        {
            m_type = new LuatTypeFunction(arguments);
            ReturnValue = returnValue;
        }

        public override LuatType Type
        {
            get { return m_type; }
            set { throw new Exception("Function type cannot be changed"); }
        }

        public LuatValue ReturnValue { get; private set; }

        public bool ExpectsSelf { get; set; }

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