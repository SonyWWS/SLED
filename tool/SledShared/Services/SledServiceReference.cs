/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Linq;

using Sce.Atf;

namespace Sce.Sled.Shared.Services
{
    /// <summary>
    /// SLED service reference class
    /// <remarks>Performs an on demand query of the CompositionContainer to obtain a reference to a particular export</remarks>
    /// </summary>
    /// <typeparam name="T">Export type</typeparam>
    public class SledServiceReference<T> where T : class
    {
        /// <summary>
        /// Return reference to a particular plugin or service
        /// <remarks>Cache the reference for future accesses.</remarks>
        /// </summary>
        public T Get
        {
            get
            {
                try
                {
                    var container = SledServiceReferenceCompositionContainer.Get;
                    if (container == null)
                        throw new NullReferenceException("container is null");

                    var lazy = container.GetExport<T>();
                    if (lazy == null)
                        throw new NullReferenceException("lazy is null");

                    return m_service ?? (m_service = lazy.Value);
                }
                catch (Exception ex)
                {
                    SledOutDevice.OutLine(
                        SledMessageType.Error,
                        "{0}: Type \"{1}\" does not exist in the CompositionContainer! Exception: {2}",
                        typeof(SledServiceReference<T>), typeof(T), ex.Message);

                    return null;
                }
            }
        }

        private T m_service;

        /// <summary>
        /// Return all references to a particular plugin or service
        /// </summary>
        public IEnumerable<T> GetAll
        {
            get
            {
                try
                {
                    var container = SledServiceReferenceCompositionContainer.Get;
                    return container.GetExports<T>().Select(lazy => lazy.Value);
                }
                catch (Exception ex)
                {
                    SledOutDevice.OutLine(
                        SledMessageType.Error,
                        "{0}: Type \"{1}\" does not exist in the CompositionContainer! Exception: {2}",
                        typeof(SledServiceReference<T>), typeof(T), ex.Message);

                    return EmptyEnumerable<T>.Instance;
                }
            }
        }
    }

    /// <summary>
    /// SLED service instance class
    /// </summary>
    public static class SledServiceInstance
    {
        /// <summary>
        /// Obtain a particular type from the MEF composition container (if it's in there)
        /// <remarks>This method doesn't report exceptions from types that don't exist.</remarks>
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        /// <returns>Type from the MEF composition container or null</returns>
        public static T TryGet<T>() where T : class
        {
            try
            {
                var container = SledServiceReferenceCompositionContainer.Get;
                if (container == null)
                    throw new NullReferenceException("container is null");

                var lazy = container.GetExport<T>();
                if (lazy == null)
                    throw new NullReferenceException("lazy is null");

                return lazy.Value;
            }
            catch (Exception ex)
            {
                SledOutDevice.OutLineDebug(
                    SledMessageType.Warning,
                    "{0}: Exception in TryGet<{1}>(): {2}",
                    typeof(SledServiceInstance), typeof(T), ex.Message);
                
                return null;
            }
        }

        /// <summary>
        /// Obtain a particular type from the MEF composition container (if it's in there)
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        /// <returns>Type from the MEF composition container or null</returns>
        public static T Get<T>() where T : class
        {
            try
            {
                var container = SledServiceReferenceCompositionContainer.Get;
                if (container == null)
                    throw new NullReferenceException("container is null");

                var lazy = container.GetExport<T>();
                if (lazy == null)
                    throw new NullReferenceException("lazy is null");

                return lazy.Value;
            }
            catch (Exception ex)
            {
                SledOutDevice.OutLine(
                    SledMessageType.Error,
                    "{0}: Type \"{1}\" does not exist in the CompositionContainer! Exception: {2}",
                    typeof(SledServiceInstance), typeof(T), ex.Message);

                return null;
            }
        }

        /// <summary>
        /// Get all instances of a particular type from the MEF composition container
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        /// <returns>All instances of a particular type from the MEF composition container</returns>
        public static IEnumerable<T> GetAll<T>() where T : class
        {
            try
            {
                var container = SledServiceReferenceCompositionContainer.Get;
                return container.GetExports<T>().Select(lazy => lazy.Value);
            }
            catch (Exception ex)
            {
                SledOutDevice.OutLine(
                    SledMessageType.Error,
                    "SledServiceInstance: Type \"{0}\" does not " +
                    "exist in the CompositionContainer! Exception: {1}",
                    typeof(T), ex.Message);

                return EmptyEnumerable<T>.Instance;
            }
        }
    }

    /// <summary>
    /// SLED service reference CompositionContainer class
    /// </summary>
    public static class SledServiceReferenceCompositionContainer
    {
        /// <summary>
        /// Set CompositionContainer
        /// </summary>
        /// <param name="container">Composition container</param>
        public static void SetCompositionContainer(CompositionContainer container)
        {
            if (Get == null)
                Get = container;
        }

        /// <summary>
        /// Get the underlying CompositionContainer
        /// </summary>
        public static CompositionContainer Get { get; private set; }

        /// <summary>
        /// Wrapper for CompositionContainer and its ComposeParts method
        /// </summary>
        /// <param name="attributedParts">Parts to compose</param>
        public static void ComposeParts(params object[] attributedParts)
        {
            if (Get != null)
                Get.ComposeParts(attributedParts);
        }
    }
}
