/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

using ActiproSoftware.SyntaxEditor;

namespace Sce.Sled.SyntaxEditor.Intellisense.Lua.Parser
{
    class LuatToken : MergableToken
    {
		/////////////////////////////////////////////////////////////////////////////////////////////////////
		// OBJECT
		/////////////////////////////////////////////////////////////////////////////////////////////////////
		
		/// <summary>
		/// Initializes a new instance of the <c>LuatToken</c> class.
		/// </summary>
		/// <param name="startOffset">The start offset of the token.</param>
		/// <param name="length">The length of the token.</param>
		/// <param name="lexicalParseFlags">The <see cref="LexicalParseFlags"/> for the token.</param>
		/// <param name="parentToken">The <see cref="IToken"/> that starts the current state scope specified by the <see cref="IToken.LexicalState"/> property.</param>
		/// <param name="lexicalParseData">The <see cref="ITokenLexicalParseData"/> that contains lexical parse information about the token.</param>
		public LuatToken(int startOffset, int length, LexicalParseFlags lexicalParseFlags, IToken parentToken, ITokenLexicalParseData lexicalParseData) : 
			base(startOffset, length, lexicalParseFlags, parentToken, lexicalParseData) {}
		
		/////////////////////////////////////////////////////////////////////////////////////////////////////
		// PUBLIC PROCEDURES
		/////////////////////////////////////////////////////////////////////////////////////////////////////
		
		/// <summary>
		/// Clones the data in the <see cref="IToken"/>.
		/// </summary>
		/// <param name="startOffset">The <see cref="IToken.StartOffset"/> of the cloned object.</param>
		/// <param name="length">The length of the cloned object.</param>
		/// <returns>The <see cref="IToken"/> that was created.</returns>
		public override IToken Clone(int startOffset, int length) {
			return new LuatToken(startOffset, length, this.LexicalParseFlags, this.ParentToken, this.LexicalParseData);
		}
		
		/// <summary>
		/// Gets whether the token represents a comment.
		/// </summary>
		/// <value>
		/// <c>true</c> if the token represents a comment; otherwise <c>false</c>.
		/// </value>
		public override bool IsComment { 
			get {
				switch (this.ID) {
					case LuatTokenId.SingleLineComment:
					case LuatTokenId.MultiLineComment:
						return true;
					default:
						return false;
				}
			}
		}

		/// <summary>
		/// Gets whether the token marks the end of the document.
		/// </summary>
		/// <value>
		/// <c>true</c> if the token marks the end of the document; otherwise <c>false</c>.
		/// </value>
		public override bool IsDocumentEnd { 
			get {
				return (this.ID == LuatTokenId.DocumentEnd);
			}
		}
		
		/// <summary>
		/// Gets whether the token marks an invalid range of text.
		/// </summary>
		/// <value>
		/// <c>true</c> if the token marks invalid range of text; otherwise <c>false</c>.
		/// </value>
		public override bool IsInvalid { 
			get {
				return (this.ID == LuatTokenId.Invalid);
			}
		}
		
		/// <summary>
		/// Gets whether the <see cref="IToken"/> is the end <see cref="IToken"/> of an <see cref="IToken"/> pair.
		/// </summary>
		/// <value>
		/// <c>true</c> if the <see cref="IToken"/> is the end <see cref="IToken"/> of an <see cref="IToken"/> pair; otherwise, <c>false</c>.
		/// </value>
		/// <remarks>
		/// A token pair is generally a pair of brackets.
		/// </remarks>
		public override bool IsPairedEnd {
			get {
				switch (this.ID) {
					case LuatTokenId.CloseParenthesis:
					case LuatTokenId.CloseCurlyBrace:
						return true;
					default:
						return false;
				}
			}
		}

		/// <summary>
		/// Gets whether the <see cref="IToken"/> is the start <see cref="IToken"/> of an <see cref="IToken"/> pair.
		/// </summary>
		/// <value>
		/// <c>true</c> if the <see cref="IToken"/> is the start <see cref="IToken"/> of an <see cref="IToken"/> pair; otherwise, <c>false</c>.
		/// </value>
		/// <remarks>
		/// A token pair is generally a pair of brackets.
		/// </remarks>
		public override bool IsPairedStart {
			get {
				switch (this.ID) {
					case LuatTokenId.OpenParenthesis:
					case LuatTokenId.OpenCurlyBrace:
						return true;
					default:
						return false;
				}
			}
		}

		/// <summary>
		/// Gets whether the token represents whitespace.
		/// </summary>
		/// <value>
		/// <c>true</c> if the token represents whitespace; otherwise <c>false</c>.
		/// </value>
		public override bool IsWhitespace { 
			get {
				switch (this.ID) {
					case LuatTokenId.Whitespace:
					case LuatTokenId.LineTerminator:
					case LuatTokenId.DocumentEnd:
						return true;
					default:
						return false;
				}
			}
		}

		/// <summary>
		/// Gets the key assigned to the token.
		/// </summary>
		/// <value>The key assigned to the token.</value>
		public override string Key { 
			get {
				return LuatTokenId.GetTokenKey(this.ID);
			}
		}
		
		/// <summary>
		/// Gets the ID of the <see cref="IToken"/> that matches this <see cref="IToken"/> if this token is paired.
		/// </summary>
		/// <value>The ID of the <see cref="IToken"/> that matches this <see cref="IToken"/> if this token is paired.</value>
		public override int MatchingTokenID { 
			get {
				switch (this.ID) {
					case LuatTokenId.OpenParenthesis:
						return (int)LuatTokenId.CloseParenthesis;
					case LuatTokenId.CloseParenthesis:
						return (int)LuatTokenId.OpenParenthesis;
					case LuatTokenId.OpenCurlyBrace:
						return (int)LuatTokenId.CloseCurlyBrace;
					case LuatTokenId.CloseCurlyBrace:
						return (int)LuatTokenId.OpenCurlyBrace;
					default:
						return (int)LuatTokenId.Invalid;
				}
			}
		}
		
		/// <summary>
		/// Creates and returns a string representation of the current object.
		/// </summary>
		/// <returns>A string representation of the current object.</returns>
		public override string ToString() {
			return this.ToString("Simple Token");
		}
	}
}