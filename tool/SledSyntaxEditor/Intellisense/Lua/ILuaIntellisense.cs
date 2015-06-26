/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

using System;
using System.Collections.Generic;

namespace Sce.Sled.SyntaxEditor.Intellisense.Lua
{
    /// <summary>
    /// Interface for a document
    /// </summary>
    public interface ILuaIntellisenseDocument
    {
        /// <summary>
        /// Name of the document (in case it differs from its filename)
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Document's conents (ie. the code/script)
        /// </summary>
        string Contents { get; }

        /// <summary>
        /// Absolute path to the document
        /// </summary>
        Uri Uri { get; }

        /// <summary>
        /// Document's syntax editor control (can be null)
        /// </summary>
        ISyntaxEditorControl SyntaxEditorControl { get; }

        /// <summary>
        /// Project the document is in
        /// </summary>
        ILuaIntellisenseProject Project { get; }
    }

    /// <summary>
    /// Interface for a project
    /// </summary>
    public interface ILuaIntellisenseProject
    {
        /// <summary>
        /// Gets the document that has the uri
        /// </summary>
        /// <param name="uri"></param>
        /// <returns>Document with uri or null</returns>
        ILuaIntellisenseDocument this[Uri uri] { get; }

        /// <summary>
        /// Gets all documents in the project
        /// </summary>
        IEnumerable<ILuaIntellisenseDocument> AllDocuments { get; }

        /// <summary>
        /// Gets all opened documents in the project
        /// </summary>
        IEnumerable<ILuaIntellisenseDocument> OpenDocuments { get; }
    }

    /// <summary>
    /// Delegate for doing custom script registration and setup
    /// </summary>
    /// <param name="stdLibs">Table containing any standard libraries</param>
    /// <param name="scripts">All currently registered LuatScripts plus any you need to create or add</param>
    /// <param name="documents">Any documents that need to be parsed after the registration/setup</param>
    public delegate void LuaIntellisenseCustomScriptRegistrationHandler(LuatTable stdLibs, ref List<LuatScript> scripts, ref List<ILuaIntellisenseDocument> documents);

    /// <summary>
    /// Interface for notifying the intellisense internals details about your project
    /// </summary>
    /// <remarks>Singleton; grab this through the <see cref="Sce.Sled.SyntaxEditor.LuaTextEditorFactory"/></remarks>
    public interface ILuaIntellisenseBroker : ILuaIntellisenseNavigator
    {
        /// <summary>
        /// Method to call when your project is opened
        /// </summary>
        /// <param name="project"></param>
        void ProjectOpened(ILuaIntellisenseProject project);

        /// <summary>
        /// Method to call when your project is modified (file added/removed, etc.)
        /// </summary>
        /// <param name="project"></param>
        void ProjectModified(ILuaIntellisenseProject project);

        /// <summary>
        /// Method to call when your project is closed
        /// </summary>
        void ProjectClosed();

        /// <summary>
        /// Method to call when your project opens a document
        /// </summary>
        /// <param name="document"></param>
        void DocumentOpened(ILuaIntellisenseDocument document);

        /// <summary>
        /// Method to call when your project closes a document
        /// </summary>
        /// <param name="document"></param>
        void DocumentClosed(ILuaIntellisenseDocument document);

        /// <summary>
        /// The way to set a custom script registration/setup handler
        /// </summary>
        LuaIntellisenseCustomScriptRegistrationHandler CustomScriptRegistrationHandler { get; set; }

        /// <summary>
        /// Gets the intellisense status
        /// </summary>
        ILuaintellisenseStatus Status { get; }
    }
}