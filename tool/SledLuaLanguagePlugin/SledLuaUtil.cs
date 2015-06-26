/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

using System;
using System.Collections.Generic;
using System.Threading;

using Sce.Atf.Adaptation;
using Sce.Atf.Dom;

using Sce.Sled.Lua.Dom;
using Sce.Sled.Lua.Resources;
using Sce.Sled.Shared.Document;
using Sce.Sled.Shared.Utilities;
using Sce.Sled.SyntaxEditor;

namespace Sce.Sled.Lua
{
    public static class SledLuaUtil
    {
        static SledLuaUtil()
        {
            // Generate hashes for SyntaxEditor tokens
            foreach (var token in s_syntaxEditorTokenStrings)
                s_syntaxEditorTokenHashes.Add(token.GetHashCode(), token);
        }

        /// <summary>
        /// Path in the assembly to the Lua specific icons
        /// </summary>
        public const string LuaIconPath = "Sce.Sled.Lua.Resources.Icons";

        /// <summary>
        /// Path in the assembly to the Lua XML schema
        /// </summary>
        public const string LuaSchemaPath = "Sce.Sled.Lua.Schemas.SledLuaProjectFiles.xsd";

        public static ISledLuaVarBaseType GetRootLevelVar(ISledLuaVarBaseType luaVar)
        {
            // Lineage starts with the current node and heads
            // toward the root (instead of starting with root
            // and heading toward current node)
            var lineage = new List<DomNode>(luaVar.DomNode.Lineage);

            // Return the 2nd to last item (if any)
            return lineage.Count <= 1 ? luaVar : lineage[lineage.Count - 2].As<ISledLuaVarBaseType>();
        }

        /// <summary>
        /// Convert a Lua type string (like LUA_TNIL) to
        /// its corresponding Lua type integer value
        /// </summary>
        /// <param name="szLuaType"></param>
        /// <returns></returns>
        public static int LuaTypeStringToInt(string szLuaType)
        {
            int iRetval;

            switch (szLuaType)
            {
                case "LUA_TNIL":
                    iRetval = (int)LuaType.LUA_TNIL;
                    break;

                case "LUA_TBOOLEAN":
                    iRetval = (int)LuaType.LUA_TBOOLEAN;
                    break;

                case "LUA_TLIGHTUSERDATA":
                    iRetval = (int)LuaType.LUA_TLIGHTUSERDATA;
                    break;

                case "LUA_TNUMBER":
                    iRetval = (int)LuaType.LUA_TNUMBER;
                    break;

                case "LUA_TSTRING":
                    iRetval = (int)LuaType.LUA_TSTRING;
                    break;

                case "LUA_TTABLE":
                    iRetval = (int)LuaType.LUA_TTABLE;
                    break;

                case "LUA_TFUNCTION":
                    iRetval = (int)LuaType.LUA_TFUNCTION;
                    break;

                case "LUA_TUSERDATA":
                    iRetval = (int)LuaType.LUA_TUSERDATA;
                    break;

                case "LUA_TTHREAD":
                    iRetval = (int)LuaType.LUA_TTHREAD;
                    break;

                default:
                    throw new ArgumentOutOfRangeException("szLuaType");
            }

            return iRetval;
        }

        /// <summary>
        /// Convert string to LuaType
        /// (like "LUA_TNIL" to LuaType.LUA_TNIL)
        /// </summary>
        /// <param name="szLuaType"></param>
        /// <returns></returns>
        public static LuaType StringToLuaType(string szLuaType)
        {
            var lRetval = LuaType.LUA_TNONE;

            switch (szLuaType)
            {
                case "LUA_TNIL":
                    lRetval = LuaType.LUA_TNIL;
                    break;

                case "LUA_TBOOLEAN":
                    lRetval = LuaType.LUA_TBOOLEAN;
                    break;

                case "LUA_TLIGHTUSERDATA":
                    lRetval = LuaType.LUA_TLIGHTUSERDATA;
                    break;

                case "LUA_TNUMBER":
                    lRetval = LuaType.LUA_TNUMBER;
                    break;

                case "LUA_TSTRING":
                    lRetval = LuaType.LUA_TSTRING;
                    break;

                case "LUA_TTABLE":
                    lRetval = LuaType.LUA_TTABLE;
                    break;

                case "LUA_TFUNCTION":
                    lRetval = LuaType.LUA_TFUNCTION;
                    break;

                case "LUA_TUSERDATA":
                    lRetval = LuaType.LUA_TUSERDATA;
                    break;

                case "LUA_TTHREAD":
                    lRetval = LuaType.LUA_TTHREAD;
                    break;
            }

            return lRetval;
        }

        /// <summary>
        /// Convert LUA_T&lt;type&gt; integer to its string value
        /// (like -1 to "LUA_TNONE")
        /// </summary>
        /// <param name="iLuaType"></param>
        /// <returns></returns>
        public static string LuaTypeIntToString(int iLuaType)
        {
            return s_luaTypeStrings[iLuaType + 1];
        }

        /// <summary>
        /// Convert LUA_T&lt;type&gt; LuaType to its string value
        /// (like LuaType.LUA_TNIL to "LUA_TNIL")
        /// </summary>
        /// <param name="luaType"></param>
        /// <returns></returns>
        public static string LuaTypeToString(LuaType luaType)
        {
            return LuaTypeIntToString((int)luaType);
        }

        /// <summary>
        /// Convert LUA_T&lt;type&gt; int to LuaType
        /// (like -1 to LuaType.LUA_TNONE)
        /// </summary>
        /// <param name="iLuaType"></param>
        /// <returns></returns>
        public static LuaType IntToLuaType(int iLuaType)
        {
            return (LuaType)iLuaType;
        }

        /// <summary>
        /// Convert LuaType to int
        /// (like LuaType.LUA_TNONE to -1)
        /// </summary>
        /// <param name="luaType"></param>
        /// <returns></returns>
        public static int LuaTypeToInt(LuaType luaType)
        {
            return (int)luaType;
        }

        private readonly static string[] s_luaTypeStrings =
        {
            Resource.LUA_TNONE,
            Resource.LUA_TNIL,
            Resource.LUA_TBOOLEAN,
            Resource.LUA_TLIGHTUSERDATA,
            Resource.LUA_TNUMBER,
            Resource.LUA_TSTRING,
            Resource.LUA_TTABLE,
            Resource.LUA_TFUNCTION,
            Resource.LUA_TUSERDATA,
            Resource.LUA_TTHREAD
        };

        public static bool IsEditableLuaType(ISledLuaVarBaseType luaVar)
        {
            return IsEditableLuaType(luaVar.LuaType) && IsEditableLuaType((LuaType)luaVar.KeyType);
        }

        public static string GetFullHoverOvenToken(SledDocumentHoverOverTokenArgs args)
        {
            var bUseCache =
                (s_cachedArgs == args) &&
                !string.IsNullOrEmpty(s_cachedFullToken);

            // Get full token string
            if (!bUseCache)
                s_cachedFullToken = FindAndConcatTokens(args);

            // Store for next run
            s_cachedArgs = args;

            return s_cachedFullToken;
        }

        private static bool IsEditableLuaType(LuaType type)
        {
            return
                (type == LuaType.LUA_TSTRING) ||
                (type == LuaType.LUA_TBOOLEAN) ||
                (type == LuaType.LUA_TNUMBER);
        }

        private static string FindAndConcatTokens(SledDocumentHoverOverTokenArgs args)
        {
            // Get full token string
            var szFullToken =
                FindAndConcatTokens(args.Args.Token,
                                    args.Document.Editor.GetTokens(args.Args.LineNumber));

            if (string.IsNullOrEmpty(szFullToken))
                return null;

            // Replace quotations
            szFullToken =
                szFullToken.Replace("\"", string.Empty);

            return szFullToken;
        }

        private static string s_cachedFullToken;
        private static SledDocumentHoverOverTokenArgs s_cachedArgs;

        private static string FindAndConcatTokens(Token hoverToken, Token[] lineTokens)
        {
            string szFullToken = null;

            // Check if the token the mouse is hovering over is valid or not
            if (IsInvalidToken(hoverToken))
                return null;

            // Ignore whitespace the mouse is on
            if (hoverToken.TokenType == "WhitespaceToken")
                return null;

            var iIndex = -1;

            // Find the actual token the mouse is hovering over on the line
            for (var i = 0; (i < lineTokens.Length) && (iIndex == -1); i++)
            {
                if ((lineTokens[i].StartOffset == hoverToken.StartOffset) &&
                    (lineTokens[i].EndOffset == hoverToken.EndOffset))
                    iIndex = i;
            }

            // Concatenate together valid tokens
            if (iIndex != -1)
            {
                szFullToken = lineTokens[iIndex].Lexeme;

                //OutDevice.OutLine(MessageType.Info, "- [Index: " + iIndex.ToString() + "] of [Total: " + lineTokens.Length + "] [" + lineTokens[iIndex].Lexeme + "] [" + lineTokens[iIndex].TokenType + "]");

                var bStop = false;

                for (var i = iIndex + 1; (i < lineTokens.Length) && !bStop; i++)
                {
                    if (!IsInvalidToken(lineTokens[i]))
                        szFullToken = szFullToken + lineTokens[i].Lexeme;
                    else
                        bStop = true;
                }

                bStop = false;

                for (var i = iIndex - 1; (i >= 0) && !bStop; i--)
                {
                    if (!IsInvalidToken(lineTokens[i]))
                        szFullToken = lineTokens[i].Lexeme + szFullToken;
                    else
                        bStop = true;
                }

                // Remove spaces
                szFullToken = szFullToken.Trim();
                // Convert [ to .
                szFullToken = szFullToken.Replace('[', '.');
                // Remove ]
                szFullToken = szFullToken.Replace("]", String.Empty);
            }

            return szFullToken;
        }

        /// <summary>
        /// Returns true or false if the token is valid or not (ie. it can
        /// be part of a Lua variable)
        /// <remarks>This isn't 100% accurate but returns a good enough
        /// result since the returned concatenated result still has to
        /// be looked up as a valid variable anyways.</remarks>
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        private static bool IsInvalidToken(Token token)
        {
            var iHashCode = token.TokenType.GetHashCode();

            // Special case for PunctuationToken
            if (iHashCode == -1613044712)
            {
                if (token.Lexeme != ".")
                    return true;
            }

            return s_syntaxEditorTokenHashes.ContainsKey(iHashCode);
        }

        /// <summary>
        /// Holds hash codes for SyntaxEditor tokens
        /// </summary>
        private static readonly SortedList<int, string> s_syntaxEditorTokenHashes =
            new SortedList<int, string>();

        /*  Hash codes for tokens
            1077052185  : LineTerminatorToken
            -561050543  : OpenParenthesisToken
            1991237168  : CloseParenthesisToken
            667020772   : OpenCurlyBraceToken
            1139962090  : CloseCurlyBraceToken
            -1975386304 : ReservedWordToken
            -2076233560 : FunctionToken
            1154245194  : OperatorToken
            537949335   : RealNumberToken
            -1613044712 : PunctuationToken
            -1136125023 : SingleQuoteStringDefaultToken
            1537402127  : SingleQuoteStringEscapedCharacterToken
            1069267250  : SingleQuoteStringWhitespaceToken
            1678979394  : SingleQuoteStringWordToken
            -63739884   : DoubleQuoteStringEscapedCharacterToken
            1522906717  : LongBracketStringDefaultToken
            615317892   : LongBracketStringStartToken
            -734326990  : LongBracketStringEndToken
            201584444   : LongBracketStringWhitespaceToken
            -264903966  : LongBracketStringWordToken
            -1416924211 : CommentDefaultToken
            -1119709654 : CommentStartToken
            -674770501  : CommentStringEndToken
            -1611404732 : MultiLineCommentDefaultToken
            -1364531305 : MultiLineCommentStartToken
            -267688178  : MultiLineCommentEndToken
            -83712800   : MultiLineCommentWhitespaceToken
            10191556    : MultiLineCommentLineTerminatorToken
            -1385349543 : MultiLineCommentWordToken
        */

        /// <summary>
        /// List of all SyntaxEditor tokens SLED cares about
        /// </summary>
        private static readonly string[] s_syntaxEditorTokenStrings =
        {
            "LineTerminatorToken",
            "OpenParenthesisToken",
            "CloseParenthesisToken",
            "OpenCurlyBraceToken",
            "CloseCurlyBraceToken",
            "ReservedWordToken",
            "FunctionToken",
            "OperatorToken",
            "RealNumberToken",
            "PunctuationToken",
            "SingleQuoteStringDefaultToken",
            "SingleQuoteStringEscapedCharacterToken",
            "SingleQuoteStringWhitespaceToken",
            "SingleQuoteStringWordToken",
            "DoubleQuoteStringEscapedCharacterToken",
            "LongBracketStringDefaultToken",
            "LongBracketStringStartToken",
            "LongBracketStringEndToken",
            "LongBracketStringWhitespaceToken",
            "LongBracketStringWordToken",
            "CommentDefaultToken",
            "CommentStartToken",
            "CommentStringEndToken",
            "MultiLineCommentDefaultToken",
            "MultiLineCommentStartToken",
            "MultiLineCommentEndToken",
            "MultiLineCommentWhitespaceToken",
            "MultiLineCommentLineTerminatorToken",
            "MultiLineCommentWordToken"
        };
    }

    static class SledLuaIcon
    {
        public const string ResourcePath = "Sled.Lua.Icons.";

        public const string Compile = ResourcePath + "Compile";
    }

    class ProducerConsumerQueue : IDisposable
    {
        static ProducerConsumerQueue()
        {
            WorkerCount = Environment.ProcessorCount;
        }

        public ProducerConsumerQueue(int workerCount, SledUtil.BoolWrapper shouldCancel)
        {
            m_workers = new Thread[workerCount];
            m_cancel = shouldCancel;

            for (var i = 0; i < workerCount; i++)
            {
                m_workers[i] =
                    new Thread(ThreadRun)
                    {
                        Name = string.Format("SLED - PCQueue Thread:{0}", i),
                        IsBackground = true,
                        CurrentCulture = Thread.CurrentThread.CurrentCulture,
                        CurrentUICulture = Thread.CurrentThread.CurrentUICulture
                    };

                m_workers[i].SetApartmentState(ApartmentState.STA);
                m_workers[i].Start();
            }
        }

        #region Implementation of IDisposable

        public void Dispose()
        {
            // Signal for thread run funcs to quit
            foreach (var worker in m_workers)
                Enqueue(null);

            foreach (var worker in m_workers)
                worker.Join();
        }

        #endregion

        public void EnqueueWorkItems(IEnumerable<IWork> items)
        {
            foreach (var item in items)
            {
                if (m_cancel.Value)
                    return;

                Enqueue(item.WorkCallback);
            }
        }

        public interface IWork
        {
            void WorkCallback();
        }

        public static int WorkerCount { get; private set; }

        #region Member Methods

        private void Enqueue(ThreadStart item)
        {
            lock (m_lock)
            {
                m_queue.Enqueue(item);
                Monitor.Pulse(m_lock);
            }
        }

        private void ThreadRun()
        {
            while (!m_cancel.Value)
            {
                ThreadStart item;

                lock (m_lock)
                {
                    while (m_queue.Count == 0)
                        Monitor.Wait(m_lock);

                    item = m_queue.Dequeue();
                }

                if (item == null)
                    return;

                if (m_cancel.Value)
                    return;

                item();
            }
        }

        #endregion

        private volatile object m_lock =
            new object();

        private readonly Thread[] m_workers;
        private readonly SledUtil.BoolWrapper m_cancel;

        private readonly Queue<ThreadStart> m_queue =
            new Queue<ThreadStart>();
    }
}
