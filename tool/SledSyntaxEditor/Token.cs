/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

using System;

using Sce.Atf;

namespace Sce.Sled.SyntaxEditor
{
    public struct Token
    {
        /// <summary>
        /// Construct a new instance of Token,
        /// </summary>
        /// <param name="startOffset">offset for this token</param>
        /// <param name="endOffset">offset for this token</param>
        /// <param name="id"></param>
        /// <param name="tokenType">token type</param>
        /// <param name="lexeme">lexeme</param>
        public Token(int startOffset, int endOffset, int id, string tokenType, string lexeme) : this()
        {
            if (StringUtil.IsNullOrEmptyOrWhitespace(tokenType))
                throw new Exception("tokenType cannot be null or empty or whitespace");

            if (lexeme == null)
                throw new ArgumentNullException();

            Id = id;
            TokenType = tokenType;
            Lexeme = lexeme;
            StartOffset = startOffset;
            EndOffset = endOffset;
        }

        /// <summary>
        /// Gets token type</summary>
        public string TokenType { get; private set; }

        /// <summary>
        /// Gets lexeme</summary>
        public string Lexeme { get; private set; }

        /// <summary>
        /// Gets token id </summary>
        public int Id { get; private set; }

        /// <summary>
        /// get starting offset for this token,
        /// relatvie to the whole document.
        /// </summary>
        public int StartOffset { get; private set; }

        /// <summary>
        /// get end offset for this token,
        /// relatvie to the whole document.
        /// </summary>
        public int EndOffset { get; private set; }
    }
}
