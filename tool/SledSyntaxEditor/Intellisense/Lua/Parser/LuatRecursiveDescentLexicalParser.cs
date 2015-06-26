/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

using ActiproSoftware.SyntaxEditor.ParserGenerator;
using ActiproSoftware.SyntaxEditor;

namespace Sce.Sled.SyntaxEditor.Intellisense.Lua.Parser
{

	/// <summary>
	/// Represents a <c>Simple</c> recursive descent lexical parser implementation.
	/// </summary>
	class LuatRecursiveDescentLexicalParser : MergableRecursiveDescentLexicalParser {

        public string       PrecedingComment;
        protected const int m_maxPrecedingCommentLineDistance = 1;
        protected int       m_precedingCommentLineDistance;

		/////////////////////////////////////////////////////////////////////////////////////////////////////
		// OBJECT
		/////////////////////////////////////////////////////////////////////////////////////////////////////

		/// <summary>
		/// Initializes a new instance of the <c>LuatRecursiveDescentLexicalParser</c> class.
		/// </summary>
		/// <param name="language">The <see cref="SimpleSyntaxLanguage"/> to use.</param>
		/// <param name="manager">The <see cref="MergableLexicalParserManager"/> to use for coordinating merged languages.</param>
		public LuatRecursiveDescentLexicalParser(LuatSyntaxLanguage language, MergableLexicalParserManager manager) : base(language, manager) {}
		
		/////////////////////////////////////////////////////////////////////////////////////////////////////
		// PUBLIC PROCEDURES
		/////////////////////////////////////////////////////////////////////////////////////////////////////

		/// <summary>
		/// Returns the next <see cref="IToken"/> and seeks past it.
		/// </summary>
		/// <returns>The next <see cref="IToken"/>.</returns>
		protected override IToken GetNextTokenCore() {
			IToken token = null;
			int startOffset = this.TextBufferReader.Offset;

			while (!this.IsAtEnd) {
				// Get the next token
				token = this.Manager.GetNextToken();

				// Update whether there is non-whitespace since the last line start
				if (token.LexicalState == this.Language.DefaultLexicalState) {
					switch (token.ID) {
						case LuatTokenId.MultiLineComment:
                            PrecedingComment = this.TextBufferReader.GetSubstring( token.TextRange.StartOffset, token.TextRange.Length );
                            PrecedingComment = PrecedingComment.TrimStart( '-', '[' ).TrimEnd( '-', ']' );
                            m_precedingCommentLineDistance = 0;
                            break;
						case LuatTokenId.SingleLineComment:
                            PrecedingComment = this.TextBufferReader.GetSubstring( token.TextRange.StartOffset, token.TextRange.Length );
                            PrecedingComment = PrecedingComment.TrimStart( '-' );
                            m_precedingCommentLineDistance = 0;
                            break;
						case LuatTokenId.LineTerminator:
                            if ( m_precedingCommentLineDistance++ == m_maxPrecedingCommentLineDistance )
                            {
                                PrecedingComment = null;
                            }
                            break; // Consume non-significant token
                        case LuatTokenId.Whitespace:
							// Consume non-significant token
							break;
						default:
							// Return the significant token
							return token;
					}
				}
				else if (token.HasFlag(LexicalParseFlags.LanguageStart)) {
					// Return the significant token (which is in a different language)
					return token;
				}

				// Advance the start offset
				startOffset = this.TextBufferReader.Offset;
			}

			// Return an end of document token
			if (this.Token != null)
				return this.Language.CreateDocumentEndToken(startOffset, this.Token.LexicalState);
			else
				return this.Language.CreateDocumentEndToken(startOffset, this.Language.DefaultLexicalState);
		}

        public override bool IsAtEnd
        {
            get
            {
                return null != this.Token ? this.Token.IsDocumentEnd : false;
            }
        }
	}
}