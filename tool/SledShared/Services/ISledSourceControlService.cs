/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

using System;

using Sce.Atf;
using Sce.Atf.Applications;

namespace Sce.Sled.Shared.Services
{
    /// <summary>
    /// SLED source control service interface
    /// </summary>
    public interface ISledSourceControlService
    {
        /// <summary>
        /// Get whether source control is being used or not
        /// </summary>
        bool CanUseSourceControl { get; }

        /// <summary>
        /// Get (cached) status of an item
        /// </summary>
        /// <param name="sc">Item to check status of</param>
        /// <returns>Source control status</returns>
        SourceControlStatus GetStatus(IResource sc);

        /// <summary>
        /// Can a source control command be performed on the context?
        /// </summary>
        /// <param name="command">Source control command</param>
        /// <param name="context">Source control context</param>
        /// <returns>True iff command can be performed</returns>
        bool CanDoCommand(SledSourceControlCommand command, ISourceControlContext context);

        /// <summary>
        /// Perform a source control command on a context
        /// </summary>
        /// <param name="command">Source control command</param>
        /// <param name="context">Source control context</param>
        /// <returns>True iff command performed</returns>
        bool DoCommand(SledSourceControlCommand command, ISourceControlContext context);

        /// <summary>
        /// Event triggered when an item's status changes
        /// </summary>
        event EventHandler<SourceControlEventArgs> StatusChanged;
    }

    /// <summary>
    /// Source control commands enumeration
    /// </summary>
    public enum SledSourceControlCommand
    {
        /// <summary>
        /// Add an item to source control
        /// </summary>
        Add,

        /// <summary>
        /// Revert changes to an item under source control
        /// </summary>
        Revert,

        /// <summary>
        /// Check in an item to source control
        /// </summary>
        CheckIn,

        /// <summary>
        /// Check out an item under source control
        /// </summary>
        CheckOut,

        /// <summary>
        /// Refresh an item under source control
        /// </summary>
        Refresh,

        /// <summary>
        /// Show history for an item under source control
        /// </summary>
        History,
    }
}