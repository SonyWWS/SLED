/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

using System;

using Sce.Atf.Adaptation;
using Sce.Atf.Dom;

namespace Sce.Sled.Lua.Dom
{
    public class SledLuaVarNameTypePairType : DomNodeAdapter, ICloneable
    {
        #region Implementation of ICloneable

        public object Clone()
        {
            var copy = DomNode.Copy(new[] { DomNode });
            copy[0].InitializeExtensions();

            return copy[0].As<SledLuaVarNameTypePairType>();
        }

        #endregion

        /// <summary>
        /// Gets/sets the name attribute
        /// </summary>
        public string Name
        {
            get { return GetAttribute<string>(SledLuaSchema.SledLuaVarNameTypePairType.nameAttribute); }
            set { SetAttribute(SledLuaSchema.SledLuaVarNameTypePairType.nameAttribute, value); }
        }

        /// <summary>
        /// Gets/sets the name type attribute
        /// </summary>
        public int NameType
        {
            get { return GetAttribute<int>(SledLuaSchema.SledLuaVarNameTypePairType.name_typeAttribute); }
            set { SetAttribute(SledLuaSchema.SledLuaVarNameTypePairType.name_typeAttribute, value); }
        }

        /// <summary>
        /// Create the pair easily
        /// </summary>
        /// <param name="name"></param>
        /// <param name="nameType"></param>
        /// <returns></returns>
        public static SledLuaVarNameTypePairType Create(string name, int nameType)
        {
            var nameAndType =
                new DomNode(SledLuaSchema.SledLuaVarNameTypePairType.Type)
                .As<SledLuaVarNameTypePairType>();

            nameAndType.Name = name;
            nameAndType.NameType = nameType;

            return nameAndType;
        }
    }
}