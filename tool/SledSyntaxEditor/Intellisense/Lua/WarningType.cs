/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

namespace Sce.Sled.SyntaxEditor.Intellisense.Lua
{
    public enum WarningType
    {
        MultipleUsages,
        DuplicateLocal,
        DuplicateTableKey,
        UnresolvedVariable,
        UnknownType,
        MixedType,
        FixedType,
        MultilineReturn,
        WrongFunctionCall
    }
}