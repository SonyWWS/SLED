/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

using System;
using System.Collections.Generic;

using Sce.Atf.Adaptation;
using Sce.Atf.Dom;

namespace Sce.Sled.Shared.Dom
{
    /// <summary>
    /// SLED DOM utilities
    /// </summary>
    public static class SledDomUtil
    {
        /// <summary>
        /// Find the first occurrence of an object convertible to type T
        /// in a collection of type TU objects
        /// </summary>
        /// <typeparam name="T">Type to search for</typeparam>
        /// <typeparam name="TU">Type of collection to search in</typeparam>
        /// <param name="objects">Objects to search in</param>
        /// <returns>Type searched for or null if not found</returns>
        public static T GetFirstAs<T, TU>(IEnumerable<TU> objects)
            where T : class
            where TU : class
        {
            if (objects == null)
                return null;

            foreach (var obj in objects)
            {
                if (obj.Is<T>())
                    return obj.As<T>();
            }

            return null;
        }

        /// <summary>
        /// Recursively gather type T convertible objects from a specific
        /// root DomNode and its children
        /// </summary>
        /// <typeparam name="T">Type to convert to</typeparam>
        /// <param name="domRootNode">Root DomNode</param>
        /// <param name="lstObjects">List of objects</param>
        public static void GatherAllAs<T>(DomNode domRootNode, List<T> lstObjects) where T : class
        {
            if (domRootNode == null)
                return;

            if (domRootNode.Is<T>())
                lstObjects.Add(domRootNode.As<T>());

            foreach (var domNode in domRootNode.Children)
            {
                GatherAllAs(domNode, lstObjects);
            }
        }

        /// <summary>
        /// Find first item in a hierarchy using a predicate
        /// </summary>
        /// <typeparam name="T">Type to convert to</typeparam>
        /// <param name="domRootNode">Root item to look in</param>
        /// <param name="predicate">Predicate to use</param>
        /// <returns>Found item or null if not found</returns>
        public static T FindFirstInWhere<T>(DomNode domRootNode, Predicate<T> predicate) where T : class
        {
            if (domRootNode == null)
                return null;

            if (predicate == null)
                return null;

            if (domRootNode.Is<T>())
            {
                var converted = domRootNode.As<T>();
                if (predicate(converted))
                    return converted;
            }

            foreach (var domNode in domRootNode.Children)
            {
                var result = FindFirstInWhere(domNode, predicate);
                if (result != null)
                    return result;
            }

            return null;
        }

        /// <summary>
        /// Helper method for getting an enum value from a DOM attribute
        /// </summary>
        /// <typeparam name="T">Type to return enum as</typeparam>
        /// <param name="adapter">Adapter to use</param>
        /// <param name="attribute">Attribute to obtain enum for</param>
        /// <returns>Enum value</returns>
        public static T GetEnumValue<T>(this DomNodeAdapter adapter, AttributeInfo attribute)
        {
            var stringValue = (string)adapter.DomNode.GetAttribute(attribute);
            return (T)Enum.Parse(typeof(T), stringValue);
        }

        /// <summary>
        /// Helper method for setting a string value on a DOM attribute from a given value
        /// </summary>
        /// <typeparam name="T">Type of value</typeparam>
        /// <param name="adapter">Adapter to use</param>
        /// <param name="attribute">Attribute to set</param>
        /// <param name="value">New value to set on attribute</param>
        public static void SetEnumValue<T>(this DomNodeAdapter adapter, AttributeInfo attribute, T value)
        {
            var stringValue = Enum.GetName(typeof(T), value);
            adapter.DomNode.SetAttribute(attribute, stringValue);
        }
    }
}
