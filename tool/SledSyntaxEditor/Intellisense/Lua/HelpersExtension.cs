/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

using System;
using System.Collections.Generic;
using System.Text;

namespace Sce.Sled.SyntaxEditor.Intellisense.Lua
{
    internal static class HelpersExtension
    {
        /// <summary>
        /// Parenthesizes the given string.
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        internal static string Parenthesize(this string str)
        {
            return str.Length == 0 ? "()" : string.Format("( {0} )", str);
        }

        /// <summary>
        /// Returns a string containing a comma-separated list of the array's items by invoking ToString() for each item.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="items"></param>
        /// <returns></returns>
        internal static string ToCommaSeperatedList<T>(this T[] items)
        {
            return ToCommaSeperatedList(items, a => a.ToString());
        }

        /// <summary>
        /// Returns a string containing a comma-separated list of the array's items by invoking getname for each item.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="items"></param>
        /// <param name="getname"></param>
        /// <returns></returns>
        internal static string ToCommaSeperatedList<T>(this T[] items, Func<T, string> getname)
        {
            var sb = new StringBuilder();

            for (int i = 0; i < items.Length; ++i)
            {
                if (i > 0) { sb.Append(", "); }
                sb.Append(getname(items[i]));
            }

            return sb.ToString();
        }

        /// <summary>
        /// Merges the elements in items into this List
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <param name="items"></param>
        internal static void Merge<T>(this List<T> list, T[] items)
        {
            foreach (T item in items)
            {
                list.Merge(item);
            }
        }

        /// <summary>
        /// Merges the single item into this List
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <param name="item"></param>
        internal static void Merge<T>(this List<T> list, T item)
        {
            foreach (T cmp in list)
            {
                if (cmp.Equals(item))
                {
                    return;
                }
            }

            list.Add(item);
        }

        /// <summary>
        /// Returns a the subset of items in the list using a predicate
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <param name="predicate"></param>
        /// <returns></returns>
        internal static T[] Filter<T>(this T[] list, Predicate<T> predicate)
        {
            var res = new List<T>(list.Length);

            foreach (T item in list)
            {
                if (predicate(item))
                {
                    res.Add(item);
                }
            }

            return res.ToArray();
        }

        /// <summary>
        /// Removes all the elements in the linked list that matches the predicate
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <param name="predicate"></param>
        /// <returns>Returns the number of elements removed</returns>
        internal static int RemoveAll<T>(this LinkedList<T> list, Predicate<T> predicate)
        {
            int count = 0;

            LinkedListNode<T> node = list.Last;
            while (null != node)
            {
                LinkedListNode<T> next = node.Previous;

                if (predicate(node.Value))
                {
                    list.Remove(node);
                    ++count;
                }

                node = next;
            }

            return count;
        }

        /// <summary>
        /// Groups the items returned by getValue by common data returned by getGroup
        /// </summary>
        /// <typeparam name="GroupType"></typeparam>
        /// <typeparam name="ValueType"></typeparam>
        /// <typeparam name="IncomingType"></typeparam>
        /// <param name="data"></param>
        /// <param name="getGroup"></param>
        /// <param name="getValue"></param>
        /// <returns></returns>
        internal static KeyValuePair<GroupType, ValueType[]>[] GroupItems<GroupType, ValueType, IncomingType>(
            this IncomingType[] data,
            Func<IncomingType, GroupType> getGroup,
            Func<IncomingType, ValueType> getValue)
        {
            var groups = new Dictionary<GroupType, List<ValueType>>();
            foreach (IncomingType input in data)
            {
                GroupType group = getGroup(input);
                ValueType value = getValue(input);

                List<ValueType> values;
                if (false == groups.TryGetValue(group, out values))
                {
                    values = new List<ValueType>();
                    groups.Add(group, values);
                }

                values.Add(value);
            }

            int i = 0;
            var res = new KeyValuePair<GroupType, ValueType[]>[groups.Count];
            foreach (KeyValuePair<GroupType, List<ValueType>> entry in groups)
            {
                res[i++] = new KeyValuePair<GroupType, ValueType[]>(entry.Key, entry.Value.ToArray());
            }

            return res;
        }

        /// <summary>
        /// Groups the items returned by getValue by common data returned by getGroup
        /// </summary>
        /// <typeparam name="GroupType"></typeparam>
        /// <typeparam name="ValueType"></typeparam>
        /// <typeparam name="IncomingType"></typeparam>
        /// <param name="data"></param>
        /// <param name="getGroup"></param>
        /// <param name="getValue"></param>
        /// <returns></returns>
        internal static KeyValuePair<GroupType, ValueType[]>[] GroupItems<GroupType, ValueType, IncomingType>(
            this List<IncomingType> data,
            Func<IncomingType, GroupType> getGroup,
            Func<IncomingType, ValueType> getValue)
        {
            return GroupItems(data.ToArray(), getGroup, getValue);
        }
    }
}