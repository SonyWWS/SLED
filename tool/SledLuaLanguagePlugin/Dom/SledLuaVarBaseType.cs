/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

using Sce.Atf.Adaptation;
using Sce.Atf.Dom;

using Sce.Sled.Lua.Resources;
using Sce.Sled.Shared.Dom;
using Sce.Sled.Shared.Utilities;

namespace Sce.Sled.Lua.Dom
{
    public abstract class SledLuaVarBaseType : SledVarBaseType, ISledLuaVarBaseType
    {
        #region ISledLuaVarBaseType Interface

        public SortKey NameSortKey { get; private set; }

        public abstract string DisplayName { get; set; }

        public abstract string UniqueName { get; set; }

        public Int64 UniqueNameMd5Hash
        {
            get
            {
                if (m_uniqueNameHash == 0)
                    m_uniqueNameHash = SledUtil.GetMd5HashForText(UniqueName);

                return m_uniqueNameHash;
            }
        }

        public abstract string What { get; set; }

        public SortKey WhatSortKey { get; private set; }

        public abstract string Value { get; set; }

        public SortKey ValueSortKey { get; private set; }

        public abstract int KeyType { get; set; }

        public abstract bool Expanded { get; set; }

        public abstract bool Visible { get; set; }

        public LuaType LuaType
        {
            get
            {
                if (m_luaType == LuaType.LUA_TNONE)
                    m_luaType = SledLuaUtil.StringToLuaType(What);

                return m_luaType;
            }
        }

        public abstract IEnumerable<ISledLuaVarBaseType> Variables { get; }

        public abstract void GenerateUniqueName();

        public abstract SledLuaVarScopeType Scope { get; }
        
        public abstract IList<SledLuaVarNameTypePairType> TargetHierarchy { get; }

        #endregion

        #region IDisposable Interface

        public override void Dispose()
        {
        }

        #endregion

        #region ICloneable Interface

        public override object Clone()
        {
            var copy = DomNode.Copy(new[] { DomNode });
            copy[0].InitializeExtensions();

            var luaVar = copy[0].As<SledLuaVarBaseType>();
            SetSortKeys(luaVar);

            return luaVar;
        }

        #endregion

        public static string CreateFlattenedHierarchyName(string name, List<KeyValuePair<string, int>> hierarchy, string separator)
        {
            s_builder.Clear();

            foreach (var kv in hierarchy)
            {
                s_builder.Append(kv.Key);
                s_builder.Append(separator);
            }

            s_builder.Append(name);

            return s_builder.ToString();
        }

        public static string CreateFlattenedHierarchyName(string name, IList<SledLuaVarNameTypePairType> hierarchy, string separator)
        {
            s_builder.Clear();

            foreach (var kv in hierarchy)
            {
                s_builder.Append(kv.Name);
                s_builder.Append(separator);
            }

            s_builder.Append(name);

            return s_builder.ToString();
        }

        public static void SetupTargetHierarchyFromRuntimeData(ISledLuaVarBaseType luaVar, List<KeyValuePair<string, int>> hierarchy)
        {
            foreach (var kv in hierarchy)
            {
                var nameAndType =
                    new DomNode(SledLuaSchema.SledLuaVarNameTypePairType.Type)
                    .As<SledLuaVarNameTypePairType>();

                nameAndType.Name = kv.Key;
                nameAndType.NameType = kv.Value;

                luaVar.TargetHierarchy.Add(nameAndType);
            }
        }

        /// <summary>
        /// Column names
        /// </summary>
        public static string[] ColumnNames =
        {
            Localization.SledLuaName,
            Localization.SledLuaType,
            Localization.SledLuaValue
        };

        /// <summary>
        /// Watched variable list column names
        /// </summary>
        public static string[] WatchColumnNames =
        {
            Localization.SledLuaName,
            Localization.SledLuaType,
            Localization.SledLuaValue,
            "Variable Type"
        };

        public const string InvalidVarWhat = "N/A";

        protected void SetSortKey()
        {
            NameSortKey = GetSortKey(Name);
        }

        protected void SetWhatSortKey()
        {
            WhatSortKey = GetSortKey(What);
        }

        protected void SetValueSortKey()
        {
            ValueSortKey = GetSortKey(Value);
        }

        private static void SetSortKeys(SledLuaVarBaseType luaVar)
        {
            luaVar.SetSortKey();
            luaVar.SetWhatSortKey();
            luaVar.SetValueSortKey();

            foreach (var variable in luaVar.Variables)
                SetSortKeys(variable.As<SledLuaVarBaseType>());
        }

        private static SortKey GetSortKey(string source)
        {
            if (s_compareInfo == null)
                s_compareInfo = CultureInfo.CurrentCulture.CompareInfo;

            return s_compareInfo.GetSortKey(source);
        }

        private Int64 m_uniqueNameHash;
        private LuaType m_luaType = LuaType.LUA_TNONE;

        private static CompareInfo s_compareInfo;
        private static readonly StringBuilder s_builder =
            new StringBuilder();
    }
}
