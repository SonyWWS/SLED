/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

using System;

namespace Sce.Sled.SyntaxEditor.Intellisense.Lua
{
    /// <summary>
    /// Interface for the status of the Intellisense
    /// </summary>
    public interface ILuaintellisenseStatus
    {
        bool Active { get; }
        string Action { get; }
        float Progress { get; }
        event EventHandler Changed;
    }
}