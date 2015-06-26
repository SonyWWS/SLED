/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sce.Sled.SyntaxEditor.Intellisense.Lua
{
    /// <summary>
    /// Value type
    /// </summary>
    public abstract class LuatType
    {
        public virtual LuaIntellisenseIconType Icon { get { return LuaIntellisenseIconType.Unknown; } }
        public override string ToString() { return "unknown"; }
        public override bool Equals(object obj) { return obj.GetType() == GetType(); }
        public override int GetHashCode() { return base.GetHashCode(); }
        public virtual string Description
        {
            get
            {
                var sb = new StringBuilder();
                AppendIcon(sb);
                sb.Append(ToString());
                return sb.ToString();
            }
        }

        protected void AppendIcon(StringBuilder sb)
        {
            sb.Append("<img src=\"");
            sb.Append(Icon.ToString());
            sb.Append("\"/>");
        }
    }

    /// <summary>
    /// Nil type
    /// </summary>
    public sealed class LuatTypeNil : LuatType
    {
        public static LuatTypeNil Instance = new LuatTypeNil();
        public override LuaIntellisenseIconType Icon { get { return LuaIntellisenseIconType.Nil; } }
        public override string ToString() { return "nil"; }
        private LuatTypeNil() { }
    }

    /// <summary>
    /// Boolean type
    /// </summary>
    public sealed class LuatTypeBoolean : LuatType
    {
        public static LuatTypeBoolean Instance = new LuatTypeBoolean();
        public override LuaIntellisenseIconType Icon { get { return LuaIntellisenseIconType.Boolean; } }
        public override string ToString() { return "bool"; }
        private LuatTypeBoolean() { }
    }

    /// <summary>
    /// Table type
    /// </summary>
    public sealed class LuatTypeTable : LuatType
    {
        public static LuatTypeTable Instance = new LuatTypeTable();
        public override LuaIntellisenseIconType Icon { get { return LuaIntellisenseIconType.Table; } }
        public override string ToString() { return "table"; }
        private LuatTypeTable() { }
    }

    /// <summary>
    /// Number type
    /// </summary>
    public sealed class LuatTypeNumber : LuatType
    {
        public static LuatTypeNumber Instance = new LuatTypeNumber();
        public override LuaIntellisenseIconType Icon { get { return LuaIntellisenseIconType.Number; } }
        public override string ToString() { return "number"; }
        private LuatTypeNumber() { }
    }

    /// <summary>
    /// String type
    /// </summary>
    public sealed class LuatTypeString : LuatType
    {
        public static LuatTypeString Instance = new LuatTypeString();
        public override LuaIntellisenseIconType Icon { get { return LuaIntellisenseIconType.String; } }
        public override string ToString() { return "string"; }
        private LuatTypeString() { }
    }

    /// <summary>
    /// Unknown type
    /// </summary>
    public sealed class LuatTypeUnknown : LuatType
    {
        public static LuatTypeUnknown Instance = new LuatTypeUnknown();
        public override LuaIntellisenseIconType Icon { get { return LuaIntellisenseIconType.Unknown; } }
        public override string ToString() { return "unknown"; }
        private LuatTypeUnknown() { }
    }

    /// <summary>
    /// Function type
    /// </summary>
    public sealed class LuatTypeFunction : LuatType
    {
        public override LuaIntellisenseIconType Icon { get { return LuaIntellisenseIconType.Function; } }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("function");
            sb.Append(Arguments.ToArray().ToCommaSeperatedList().Parenthesize());
            sb.Append("");
            return sb.ToString();
        }

        public override string Description
        {
            get
            {
                var sb = new StringBuilder();
                AppendIcon(sb);
                sb.Append(ToString());
                return sb.ToString();
            }
        }

        public string[] Arguments { get { return m_arguments; } }

        public LuatTypeFunction(string[] arguments)
        {
            m_arguments = arguments;
        }

        public override bool Equals(object obj)
        {
            var other = obj as LuatTypeFunction;
            if (null == other)
                return false;

            if ((null == other.Arguments) != (null == Arguments))
                return false;

            if ((null != Arguments) && (false == other.Arguments.Equals(Arguments)))
                return false;

            return true;
        }

        public override int GetHashCode()
        {
            return Arguments.GetHashCode();
        }

        private readonly string[] m_arguments;
    }

    /// <summary>
    /// Reflected type
    /// </summary>
    public sealed class LuatTypeReflected : LuatType
    {
        public override LuaIntellisenseIconType Icon { get { return LuaIntellisenseIconType.Reflected; } }
        public override string ToString() { return "[" + ReflectedType + "]"; }

        public string ReflectedType;

        public LuatTypeReflected(string reflectedType)
        {
            ReflectedType = reflectedType;
        }

        public override bool Equals(object obj)
        {
            if (obj.GetType() != GetType())
                return false;

            var other = obj as LuatTypeReflected;
            if (other == null)
                return false;

            return ReflectedType == other.ReflectedType;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }

    /// <summary>
    /// A mix of types
    /// </summary>
    public sealed class LuatTypeMixed : LuatType
    {
        public override LuaIntellisenseIconType Icon { get { return LuaIntellisenseIconType.Mixed; } }
        public LuatType[] Types { get { return m_types.ToArray(); } }

        public override string ToString()
        {
            return string.Format("mixed: ({0})", Types.ToCommaSeperatedList());
        }

        public void AddType(LuatType type)
        {
            if (false == m_types.Contains(type))
            {
                m_types.Add(type);
            }
        }

        private readonly List<LuatType> m_types = new List<LuatType>();
    }
}
