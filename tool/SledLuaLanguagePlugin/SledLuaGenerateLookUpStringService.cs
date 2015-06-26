/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

//using System.ComponentModel.Composition;

//using Sce.Atf;

//namespace Sce.Sled.Lua
//{
//    [Export(typeof(IInitializable))]
//    [Export(typeof(SledLuaGenerateLookUpStringService))]
//    [PartCreationPolicy(CreationPolicy.Shared)]
//    class SledLuaGenerateLookUpStringService : IInitializable
//    {
//        #region IInitializable Interface

//        void IInitializable.Initialize()
//        {
//            // Keep this here
//        }

//        #endregion

//        //#region ISledLuaGenerateLookUpStringService Interface

//        //public SledLuaVarLookUp GenerateLookUp(ISledLuaVarBaseType luaVar)
//        //{
//        //    var lookUp = SledLuaVarLookUp.FromLuaVar(luaVar);

//        //    if ((lookUp == null) && System.Diagnostics.Debugger.IsAttached)
//        //        System.Diagnostics.Debugger.Break();

//        //    return lookUp;
//        //}

//        //public string GenerateLookUpString(SledProfileInfoType pi)
//        //{
//        //    var sb = new StringBuilder();
//        //    sb.Append("{pi_lookup:");
//        //    sb.Append(pi.Function);     // function
//        //    sb.Append(',');
//        //    sb.Append('f');             // t or f (t for root table)
//        //    sb.Append(',');
//        //    sb.Append(pi.Line);         // line #
//        //    sb.Append(',');
//        //    sb.Append(pi.File.Replace(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));         // file
//        //    sb.Append('}');
//        //    return sb.ToString();
//        //}

//        //#endregion
//    }

//    //interface ISledLuaGenerateLookUpStringService
//    //{
//    //    SledLuaVarLookUp GenerateLookUp(ISledLuaVarBaseType luaVar);

//    //    string GenerateLookUpString(SledProfileInfoType pi);
//    //}
//}