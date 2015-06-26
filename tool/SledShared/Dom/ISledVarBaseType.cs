/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

using System;
using System.Collections.Generic;

using Sce.Atf.Applications;

namespace Sce.Sled.Shared.Dom
{
    /// <summary>
    /// SLED variable base type interface
    /// </summary>
    public interface ISledVarBaseType : IItemView, IDisposable, ICloneable
    {
        /// <summary>
        /// Gets or sets the name attribute
        /// <remarks>This is the variable's name</remarks>
        /// </summary>
        string Name
        {
            get;
            set;
        }

        /// <summary>
        /// Gets variable's locations
        /// <remarks>Returns a list of all the places the variable is
        /// referenced in various script files</remarks>
        /// </summary>
        IList<SledVarLocationType> Locations
        {
            get;
        }
    }
}
