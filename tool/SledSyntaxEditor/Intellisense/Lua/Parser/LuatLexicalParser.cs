/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

using System;
using System.Collections;

using ActiproSoftware.SyntaxEditor;

namespace Sce.Sled.SyntaxEditor.Intellisense.Lua.Parser
{
	/// <summary>
	/// Represents a <c>Simple</c> lexical parser implementation.
	/// </summary>
	class LuatLexicalParser : IMergableLexicalParser {

		private Hashtable				keywords		= new Hashtable();
		
		/////////////////////////////////////////////////////////////////////////////////////////////////////
		// OBJECT
		/////////////////////////////////////////////////////////////////////////////////////////////////////

		/// <summary>
		/// Initializes a new instance of the <c>LuatLexicalParser</c> class.
		/// </summary>

		public LuatLexicalParser() {
			// Initialize keywords
			for (int index = (int)LuatTokenId.KeywordStart + 1; index < (int)LuatTokenId.KeywordEnd; index++)
				keywords.Add(LuatTokenId.GetTokenKey(index).ToLowerInvariant(), index);
		}
		
		/////////////////////////////////////////////////////////////////////////////////////////////////////
		// PUBLIC PROCEDURES
		/////////////////////////////////////////////////////////////////////////////////////////////////////
		
		/// <summary>
		/// Returns a single-character <see cref="ITokenLexicalParseData"/> representing the lexical parse data for the
		/// default token in the <see cref="ILexicalState"/> and seeks forward one position in the <see cref="ITextBufferReader"/>
		/// </summary>
		/// <param name="reader">An <see cref="ITextBufferReader"/> that is reading a text source.</param>
		/// <param name="lexicalState">The <see cref="ILexicalState"/> that specifies the current context.</param>
		/// <returns>The <see cref="ITokenLexicalParseData"/> for default text in the <see cref="ILexicalState"/>.</returns>
		public ITokenLexicalParseData GetLexicalStateDefaultTokenLexicalParseData(ITextBufferReader reader, ILexicalState lexicalState) {
			reader.Read();
			return new LexicalStateAndIDTokenLexicalParseData(lexicalState, (byte)lexicalState.DefaultTokenID);
		}

		/// <summary>
		/// Performs a lexical parse to return the next <see cref="ITokenLexicalParseData"/> 
		/// from a <see cref="ITextBufferReader"/> and seeks past it if there is a match.
		/// </summary>
		/// <param name="reader">An <see cref="ITextBufferReader"/> that is reading a text source.</param>
		/// <param name="lexicalState">The <see cref="ILexicalState"/> that specifies the current context.</param>
		/// <param name="lexicalParseData">Returns the next <see cref="ITokenLexicalParseData"/> from a <see cref="ITextBufferReader"/>.</param>
		/// <returns>A <see cref="MatchType"/> indicating the type of match that was made.</returns>
		public MatchType GetNextTokenLexicalParseData(ITextBufferReader reader, ILexicalState lexicalState, ref ITokenLexicalParseData lexicalParseData) {
			// Initialize
			int tokenID = LuatTokenId.Invalid;

            if ( reader.IsAtEnd )
            {
				lexicalParseData = new LexicalStateAndIDTokenLexicalParseData(lexicalState, (byte)LuatTokenId.DocumentEnd);
                return MatchType.ExactMatch;
            }

			// Get the next character
			char ch = reader.Read();

			// If the character is a letter or digit...
			if ((Char.IsLetter(ch) || (ch == '_'))) {
				// Parse the identifier
				tokenID = this.ParseIdentifier(reader, ch);
			}
			else if ((ch != '\n') && (Char.IsWhiteSpace(ch))) {
				while ((reader.Peek() != '\n') && (Char.IsWhiteSpace(reader.Peek()))) 
					reader.Read();
				tokenID = LuatTokenId.Whitespace;
			}
			else {
				tokenID = LuatTokenId.Invalid;
				switch (ch) {
					case ',':
						tokenID = LuatTokenId.Comma;
						break;
					case '(':
						tokenID = LuatTokenId.OpenParenthesis;
						break;
					case ')':
						tokenID = LuatTokenId.CloseParenthesis;
						break;
					case ';':
						tokenID = LuatTokenId.SemiColon;
                        break;
                    case ':':
                        tokenID = LuatTokenId.Colon;
                        break;
					case '\n':
                    case '\r':
						// Line terminator
						tokenID = LuatTokenId.LineTerminator;
						break;
					case '{':
						tokenID = LuatTokenId.OpenCurlyBrace;
						break;
					case '}':
						tokenID = LuatTokenId.CloseCurlyBrace;
						break;
                    case '\"':
                        tokenID = this.ParseString( reader, '\"' );
                        break;
                    case '\'':
                        tokenID = this.ParseString( reader, '\'' );
                        break;
					case '-':						
						if ( reader.Peek(1) != '-' ) 
                        {
                            tokenID = LuatTokenId.Subtraction;
                            break;
                        }

                        reader.Read();

						if ( reader.Peek(1) != '[' ||
                             reader.Peek(2) != '[' ) 
                        {
                            tokenID = this.ParseSingleLineComment(reader);
                        }
                        else
                        {
                            reader.Read();
                            reader.Read();
                            tokenID = this.ParseMultiLineComment( reader );
                        }
						break;
                    case '<':
                        if (reader.Peek() == '=') {
							reader.Read();
							tokenID = LuatTokenId.LessThanEqual;
						}
                        else
                        {
                            tokenID = LuatTokenId.LessThan;
                        }
                        break;
                    case '>':
                        if (reader.Peek() == '=') {
							reader.Read();
							tokenID = LuatTokenId.GreaterThanEqual;
						}
                        else
                        {
                            tokenID = LuatTokenId.GreaterThan;
                        }
                        break;
                    case '~':
                        if (reader.Peek() == '=') {
							reader.Read();
							tokenID = LuatTokenId.Inequality;
						}
                        break;
					case '=':
						if (reader.Peek() == '=') {
							reader.Read();
							tokenID = LuatTokenId.Equality;
						}
						else
                        {
							tokenID = LuatTokenId.Assignment;
                        }
						break;
					case '!':
						if (reader.Peek() == '=') {
							reader.Read();
							tokenID = LuatTokenId.Inequality;
						}
						break;
					case '+':
						tokenID = LuatTokenId.Addition;
						break;
                    case '/':
                        tokenID = LuatTokenId.Division;
                        break;
					case '*':
						tokenID = LuatTokenId.Multiplication;
                        break;
                    case '^':
                        tokenID = LuatTokenId.Hat;
                        break;
                    case '#':
                        tokenID = LuatTokenId.Hash;
                        break;
                    case '%':
                        tokenID = LuatTokenId.Modulus;
                        break;
                    case '.':
                        tokenID = LuatTokenId.Dot;
                        
                        if (reader.Peek() == '.')
                        {
                            reader.Read();
                            tokenID = LuatTokenId.DoubleDot;
                        }
                        
                        if (reader.Peek() == '.')
                        {
                            reader.Read();
                            tokenID = LuatTokenId.TripleDot;
                        }

                        break;
                    case '[':
                        tokenID = LuatTokenId.OpenSquareBracket;
                        break;
                    case ']':
                        tokenID = LuatTokenId.CloseSquareBracket;
                        break;
					default:
						if ((ch >= '0') && (ch <= '9')) {
							// Parse the number
							tokenID = this.ParseNumber(reader, ch);
						}
						break;
				}
			}

			if (tokenID != LuatTokenId.Invalid) {
				lexicalParseData = new LexicalStateAndIDTokenLexicalParseData(lexicalState, (byte)tokenID);
				return MatchType.ExactMatch;
			}
			else {
				reader.ReadReverse();
				return MatchType.NoMatch;
			}
		}

		/// <summary>
		/// Parses an identifier.
		/// </summary>
		/// <param name="reader">An <see cref="ITextBufferReader"/> that is reading a text source.</param>
		/// <param name="ch">The first character of the identifier.</param>
		/// <returns>The ID of the token that was matched.</returns>
		protected virtual int ParseIdentifier(ITextBufferReader reader, char ch) {
			// Get the entire word
			int startOffset = reader.Offset - 1;
			while (!reader.IsAtEnd) {
                // Accept namespaced identifiers
                if ( reader.Peek( 1 ) == ':' &&
                     reader.Peek( 2 ) == ':' &&
                     char.IsLetterOrDigit( reader.Peek( 3 ) ) )
                {
                    reader.Read();
                    reader.Read();
                    reader.Read();
                    continue;
                }

                char ch2 = reader.Read();

				// NOTE: This could be improved by supporting \u escape sequences
				if ((!char.IsLetterOrDigit(ch2)) && (ch2 != '_')) {
					reader.ReadReverse();
					break;
				}
			}

			// Determine if the word is a keyword
			if (Char.IsLetter(ch)) {
				object value = keywords[reader.GetSubstring(startOffset, reader.Offset - startOffset)];

				if (value != null)
					return (int)value;
				else
					return LuatTokenId.Identifier;
			}
			else
				return LuatTokenId.Identifier;
		}

		/// <summary>
		/// Parses a multiple line comment.
		/// </summary>
		/// <param name="reader">An <see cref="ITextBufferReader"/> that is reading a text source.</param>
		/// <returns>The ID of the token that was matched.</returns>
		protected virtual int ParseMultiLineComment(ITextBufferReader reader) {
			while (reader.Offset + 2 < reader.Length) {
				if ( ( reader.Peek(1) == ']' &&
                       reader.Peek(2) == ']' ) )
                {
					reader.Read();
					reader.Read();
                    break;
				}
				reader.Read();
			}
			return LuatTokenId.MultiLineComment;
		}
		
		/// <summary>
		/// Parses a number.
		/// </summary>
		/// <param name="reader">An <see cref="ITextBufferReader"/> that is reading a text source.</param>
		/// <param name="ch">The first character of the number.</param>
		/// <returns>The ID of the token that was matched.</returns>
		protected virtual int ParseNumber(ITextBufferReader reader, char ch) {
            Char c = reader.Peek();
            bool bParsedDot = false;
            while (Char.IsNumber(c) || c == '.')
            {
                if (c == '.')
                {
                    // Only handle a single '.'
                    if (bParsedDot)
                    {
                        return LuatTokenId.Number;
                    }

                    bParsedDot = true;
                }

                reader.Read();
                c = reader.Peek();
            }
			return LuatTokenId.Number;
		}

		/// <summary>
		/// Parses a single line comment.
		/// </summary>
		/// <param name="reader">An <see cref="ITextBufferReader"/> that is reading a text source.</param>
		/// <returns>The ID of the token that was matched.</returns>
		protected virtual int ParseSingleLineComment(ITextBufferReader reader) {
			while ((!reader.IsAtEnd) && (reader.Peek() != '\n'))
				reader.Read();
			return LuatTokenId.SingleLineComment;
		}
		
        /// <summary>
		/// Parses a string.
		/// </summary>
		/// <param name="reader">An <see cref="ITextBufferReader"/> that is reading a text source.</param>
		/// <returns>The ID of the token that was matched.</returns>
		protected virtual int ParseString(ITextBufferReader reader, char quote ) {
            Char c0 = '\0';
            Char c1 = reader.Read();
            while ( !( reader.IsAtEnd || ( c1 == quote && c0 != '\\' ) ) )
            {
                c0 = c1;
                c1 = reader.Read();
            }
			return LuatTokenId.String;
		}
		
	}
}