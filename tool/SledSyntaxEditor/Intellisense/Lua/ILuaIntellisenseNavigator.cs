/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

namespace Sce.Sled.SyntaxEditor.Intellisense.Lua
{
    /// <summary>
    /// Delegate for opening and then going to a specific spot in a file
    /// </summary>
    /// <param name="absFilePath">Absolute path to a file</param>
    /// <param name="textRange">Spot in file to select</param>
    public delegate void LuaIntellisenseOpenAndSelectDelegate(string absFilePath, ISyntaxEditorTextRange textRange);

    /// <summary>
    /// Interface for navigating to specific spots in a file
    /// </summary>
    public interface ILuaIntellisenseNavigator
    {
        /// <summary>
        /// Gets the function that navigates to the specific file and location
        /// </summary>
        LuaIntellisenseOpenAndSelectDelegate OpenAndSelectHandler { get; set; }

        /// <summary>
        /// Gets whether the goto previous position functionality is valid
        /// </summary>
        bool CanGotoPreviousPosition { get; }

        /// <summary>
        /// Moves the caret to the previous visited location
        /// </summary>
        void GotoPreviousPosition();

        /// <summary>
        /// Gets whether the goto next position functionality is valid
        /// </summary>
        bool CanGotoNextPosition { get; }

        /// <summary>
        /// Moves the caret to the next visited location
        /// </summary>
        void GotoNextPosition();

        /// <summary>
        /// Gets whether the can goto definition functionality is valid
        /// </summary>
        /// <param name="document"></param>
        /// <returns></returns>
        bool CanGotoDefinition(ILuaIntellisenseDocument document);

        /// <summary> 
        /// Goto to the definition of the selected variable
        /// </summary>
        /// <param name="document"></param>
        void GotoDefinition(ILuaIntellisenseDocument document);

        /// <summary>
        /// Gets whether the goto reference position functionality is valid
        /// </summary>
        /// <param name="document"></param>
        /// <returns></returns>
        bool CanGotoReference(ILuaIntellisenseDocument document);

        /// <summary>
        /// Goto the references of the selected variable
        /// </summary>
        /// <param name="document"></param>
        void GotoReference(ILuaIntellisenseDocument document);

        /// <summary>
        /// Gets whether the selected variable can be renamed
        /// </summary>
        /// <param name="document"></param>
        /// <returns></returns>
        bool CanRenameVariable(ILuaIntellisenseDocument document);

        /// <summary>
        /// Rename the selected variable
        /// </summary>
        /// <param name="document"></param>
        void RenameVariable(ILuaIntellisenseDocument document);

        /// <summary>
        /// Gets or sets whether to add the navigation bar to documents
        /// </summary>
        bool UseNavigationBar { get; set; }
    }
}