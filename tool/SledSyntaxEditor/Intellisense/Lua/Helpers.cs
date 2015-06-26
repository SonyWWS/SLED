/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

using System;
using System.Collections.Generic;
using System.Text;

namespace Sce.Sled.SyntaxEditor.Intellisense.Lua
{
    public static class Helpers
    {
        /// <summary>
        /// Applies a decoration to the given string.
        /// </summary>
        /// <param name="str"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static string Decorate(string str, DecorationType type)
        {
            switch (type)
            {
                case DecorationType.Code: return Monospace(str);
                case DecorationType.Comment: return Color(str, System.Drawing.Color.Green);
                default: return str;
            }
        }

        /// <summary>
        /// Applies a mono-space font span style to the given string.
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string Monospace(string str)
        {
            var sb = new StringBuilder();
            sb.Append("<span style=\"font-family: Consolas;\">");
            sb.Append(str);
            sb.Append("</span>");
            return sb.ToString();
        }

        /// <summary>
        /// Applies a color span style to the given string.
        /// </summary>
        /// <param name="str"></param>
        /// <param name="color"></param>
        /// <returns></returns>
        public static string Color(string str, System.Drawing.Color color)
        {
            var sb = new StringBuilder();
            sb.Append("<span style=\"color: ");
            sb.Append(string.Format("#{0:X2}{1:X2}{2:X2}\"", color.R, color.G, color.B));
            sb.Append(">");
            sb.Append(str);
            sb.Append("</span>");
            return sb.ToString();
        }

        /// <summary>
        /// Calculates the line number for the offset in the given string
        /// </summary>
        /// <param name="str"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        public static int GetLineNumber(string str, int offset)
        {
            int line = 1;
            while (offset-- > 0)
            {
                if (str[offset] == '\n')
                {
                    ++line;
                }
            }
            return line;
        }

        /// <summary>
        /// Indents the string by the given depth
        /// </summary>
        /// <param name="str"></param>
        /// <param name="depth"></param>
        /// <returns></returns>
        public static string Tab(string str, int depth)
        {
            var sb = new StringBuilder();
            while (depth-- > 0)
            {
                sb.Append("&nbsp;");
            }
            string padding = Monospace(sb.ToString());
            string formatted = padding + str;
            formatted = formatted.Replace("\n", "\n" + padding);
            formatted = formatted.Replace("<br/>", "<br/>" + padding);
            formatted = formatted.Replace("<br />", "<br />" + padding);
            return formatted;
        }

        /// <summary>
        /// The method takes two things: an item, and a relation that produces the set of
        /// everything that is adjacent to the item.
        /// </summary>
        /// <remarks>
        /// This marvelous function was written by Eric Lippert and posted here:
        /// http://stackoverflow.com/a/2209155
        /// He writes:
        /// The method takes two things: an item, and a relation that produces the set of 
        /// everything that is adjacent to the item. It produces a depth-first traversal 
        /// of the transitive and reflexive closure of the adjacency relation on the item. 
        /// Let the number of items in the graph be n, and the maximum depth be 1 &lt;= d &lt;= n, 
        /// assuming the branching factor is not bounded. This algorithm uses an explicit 
        /// stack rather than recursion because (1) recursion in this case turns what should 
        /// be an O(n) algorithm into O(nd), which is then something between O(n) and O(n^2), 
        /// and (2) excessive recursion can blow the stack if the d is more than a few 
        /// hundred nodes.
        ///
        /// Note that the peak memory usage of this algorithm is of course O(n + d) = O(n).
        /// 
        /// So, for example:
        ///
        /// foreach(Node node in Traversal(myGraph.Root, n => n.Children))
        ///     Console.WriteLine(node.Name);
        ///
        /// [Edit]: Changed from IEnumerable&lt;T&gt; to IEnumerable.
        /// </remarks>
        /// <typeparam name="T"></typeparam>
        /// <param name="item"></param>
        /// <param name="children"></param>
        /// <returns></returns>
        public static IEnumerable<T> Traversal<T>(T item, Func<T, System.Collections.IEnumerable> children)
        {
            var seen = new HashSet<T>();
            var stack = new Stack<T>();
            seen.Add(item);
            stack.Push(item);
            yield return item;
            while (stack.Count > 0)
            {
                T current = stack.Pop();
                foreach (T newItem in children(current))
                {
                    if (!seen.Contains(newItem))
                    {
                        seen.Add(newItem);
                        stack.Push(newItem);
                        yield return newItem;
                    }
                }
            }
        }
    }
}