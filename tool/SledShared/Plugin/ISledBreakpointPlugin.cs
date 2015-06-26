/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

using Sce.Sled.Shared.Document;

namespace Sce.Sled.Shared.Plugin
{
    /// <summary>
    /// SLED breakpoint plugin interface
    /// </summary>
    public interface ISledBreakpointPlugin
    {
        /// <summary>
        /// Determine whether breakpoint can be added
        /// </summary>
        /// <param name="sd">Document (if any, can be null)</param>
        /// <param name="lineNumber">Line number of breakpoint</param>
        /// <param name="lineText">Text on line of breakpoint</param>
        /// <param name="numLanguagePluginBreakpoints">The current number of breakpoints that belong to the language plugin</param>
        /// <returns>True iff can add breakpoint</returns>
        bool CanAdd(ISledDocument sd, int lineNumber, string lineText, int numLanguagePluginBreakpoints);

        /// <summary>
        /// Get the maximum number of breakpoints allowed
        /// </summary>
        /// <param name="maxBreakpoints">Maximum number of breakpoints allowed</param>
        /// <returns>True iff there's a maximum number of breakpoints allowed</returns>
        bool TryGetMax(out int maxBreakpoints);
    }
}
