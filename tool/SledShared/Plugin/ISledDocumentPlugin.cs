/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

using System.Collections.Generic;

using Sce.Sled.Shared.Document;

namespace Sce.Sled.Shared.Plugin
{
    /// <summary>
    /// SLED document plugin interface
    /// <remarks>Abstraction to supply a SledDocument's context menu with command tags</remarks>
    /// </summary>
    public interface ISledDocumentPlugin
    {
        /// <summary>
        /// Get context menu command tags for the target SledDocument
        /// </summary>
        /// <param name="args">Arguments (document, region clicked, line number clicked)</param>
        /// <returns>List of context menu command tags for the target SledDocument</returns>
        IList<object> GetPopupCommandTags(SledDocumentContextMenuArgs args);

        /// <summary>
        /// Get values for hovered over tokens
        /// </summary>
        /// <param name="args">Arguments (document, token, line number)</param>
        /// <returns>List of strings representing possible values for the hovered over token</returns>
        IList<string> GetMouseHoverOverTokenValues(SledDocumentHoverOverTokenArgs args);
    }
}
