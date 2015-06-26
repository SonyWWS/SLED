/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

using System;
using System.Collections;
using System.Text;

using ActiproSoftware.SyntaxEditor;

using Sce.Sled.SyntaxEditor.Intellisense.Lua.Parser.AST;

namespace Sce.Sled.SyntaxEditor.Intellisense.Lua.Parser
{
	#region Token Ids

	/// <summary>
	/// Contains the token IDs for the <c>Luat</c> language.
	/// </summary>
    class LuatTokenId
    {
		/// <summary>
		/// Returns the string-based key for the specified token ID.
		/// </summary>
		/// <param name="id">The token ID to examine.</param>
		public static string GetTokenKey(int id) {
			System.Reflection.FieldInfo[] fields = typeof(LuatTokenId).GetFields();
			foreach (System.Reflection.FieldInfo field in fields) {
				if ((field.IsStatic) && (field.IsLiteral) && (id.Equals(field.GetValue(null))))
					return field.Name;
			}
			return null;
		}

		/// <summary>
		/// The Invalid token ID.
		/// </summary>
		public const int Invalid = 0;

		/// <summary>
		/// The DocumentEnd token ID.
		/// </summary>
		public const int DocumentEnd = 1;

		/// <summary>
		/// The LanguageTransitionStart token ID.
		/// </summary>
		public const int LanguageTransitionStart = 2;

		/// <summary>
		/// The LanguageTransitionEnd token ID.
		/// </summary>
		public const int LanguageTransitionEnd = 3;

		/// <summary>
		/// The Whitespace token ID.
		/// </summary>
		public const int Whitespace = 4;

		/// <summary>
		/// The LineTerminator token ID.
		/// </summary>
		public const int LineTerminator = 5;

		/// <summary>
		/// The SingleLineComment token ID.
		/// </summary>
		public const int SingleLineComment = 6;

		/// <summary>
		/// The MultiLineComment token ID.
		/// </summary>
		public const int MultiLineComment = 7;

		/// <summary>
		/// The Number token ID.
		/// </summary>
		public const int Number = 8;

		/// <summary>
		/// The Identifier token ID.
		/// </summary>
		public const int Identifier = 9;

		/// <summary>
		/// The String token ID.
		/// </summary>
		public const int String = 10;

		/// <summary>
		/// The KeywordStart token ID.
		/// </summary>
		public const int KeywordStart = 11;

		/// <summary>
		/// The And token ID.
		/// </summary>
		public const int And = 12;

		/// <summary>
		/// The Break token ID.
		/// </summary>
		public const int Break = 13;

		/// <summary>
		/// The Do token ID.
		/// </summary>
		public const int Do = 14;

		/// <summary>
		/// The Else token ID.
		/// </summary>
		public const int Else = 15;

		/// <summary>
		/// The Elseif token ID.
		/// </summary>
		public const int Elseif = 16;

		/// <summary>
		/// The End token ID.
		/// </summary>
		public const int End = 17;

		/// <summary>
		/// The True token ID.
		/// </summary>
		public const int True = 18;

		/// <summary>
		/// The False token ID.
		/// </summary>
		public const int False = 19;

		/// <summary>
		/// The For token ID.
		/// </summary>
		public const int For = 20;

		/// <summary>
		/// The Function token ID.
		/// </summary>
		public const int Function = 21;

		/// <summary>
		/// The If token ID.
		/// </summary>
		public const int If = 22;

		/// <summary>
		/// The In token ID.
		/// </summary>
		public const int In = 23;

		/// <summary>
		/// The Local token ID.
		/// </summary>
		public const int Local = 24;

		/// <summary>
		/// The Nil token ID.
		/// </summary>
		public const int Nil = 25;

		/// <summary>
		/// The Not token ID.
		/// </summary>
		public const int Not = 26;

		/// <summary>
		/// The Or token ID.
		/// </summary>
		public const int Or = 27;

		/// <summary>
		/// The Repeat token ID.
		/// </summary>
		public const int Repeat = 28;

		/// <summary>
		/// The Return token ID.
		/// </summary>
		public const int Return = 29;

		/// <summary>
		/// The Then token ID.
		/// </summary>
		public const int Then = 30;

		/// <summary>
		/// The Until token ID.
		/// </summary>
		public const int Until = 31;

		/// <summary>
		/// The While token ID.
		/// </summary>
		public const int While = 32;

		/// <summary>
		/// The KeywordEnd token ID.
		/// </summary>
		public const int KeywordEnd = 33;

		/// <summary>
		/// The OperatorOrPunctuatorStart token ID.
		/// </summary>
		public const int OperatorOrPunctuatorStart = 34;

		/// <summary>
		/// The Addition token ID.
		/// </summary>
		public const int Addition = 35;

		/// <summary>
		/// The Subtraction token ID.
		/// </summary>
		public const int Subtraction = 36;

		/// <summary>
		/// The Multiplication token ID.
		/// </summary>
		public const int Multiplication = 37;

		/// <summary>
		/// The Division token ID.
		/// </summary>
		public const int Division = 38;

		/// <summary>
		/// The Modulus token ID.
		/// </summary>
		public const int Modulus = 39;

		/// <summary>
		/// The Hat token ID.
		/// </summary>
		public const int Hat = 40;

		/// <summary>
		/// The Hash token ID.
		/// </summary>
		public const int Hash = 41;

		/// <summary>
		/// The Equality token ID.
		/// </summary>
		public const int Equality = 42;

		/// <summary>
		/// The Inequality token ID.
		/// </summary>
		public const int Inequality = 43;

		/// <summary>
		/// The LessThanEqual token ID.
		/// </summary>
		public const int LessThanEqual = 44;

		/// <summary>
		/// The GreaterThanEqual token ID.
		/// </summary>
		public const int GreaterThanEqual = 45;

		/// <summary>
		/// The LessThan token ID.
		/// </summary>
		public const int LessThan = 46;

		/// <summary>
		/// The GreaterThan token ID.
		/// </summary>
		public const int GreaterThan = 47;

		/// <summary>
		/// The Assignment token ID.
		/// </summary>
		public const int Assignment = 48;

		/// <summary>
		/// The OpenParenthesis token ID.
		/// </summary>
		public const int OpenParenthesis = 49;

		/// <summary>
		/// The CloseParenthesis token ID.
		/// </summary>
		public const int CloseParenthesis = 50;

		/// <summary>
		/// The OpenCurlyBrace token ID.
		/// </summary>
		public const int OpenCurlyBrace = 51;

		/// <summary>
		/// The CloseCurlyBrace token ID.
		/// </summary>
		public const int CloseCurlyBrace = 52;

		/// <summary>
		/// The OpenSquareBracket token ID.
		/// </summary>
		public const int OpenSquareBracket = 53;

		/// <summary>
		/// The CloseSquareBracket token ID.
		/// </summary>
		public const int CloseSquareBracket = 54;

		/// <summary>
		/// The SemiColon token ID.
		/// </summary>
		public const int SemiColon = 55;

		/// <summary>
		/// The Colon token ID.
		/// </summary>
		public const int Colon = 56;

		/// <summary>
		/// The Comma token ID.
		/// </summary>
		public const int Comma = 57;

		/// <summary>
		/// The Dot token ID.
		/// </summary>
		public const int Dot = 58;

		/// <summary>
		/// The DoubleDot token ID.
		/// </summary>
		public const int DoubleDot = 59;

		/// <summary>
		/// The TripleDot token ID.
		/// </summary>
		public const int TripleDot = 60;

		/// <summary>
		/// The OperatorOrPunctuatorEnd token ID.
		/// </summary>
		public const int OperatorOrPunctuatorEnd = 61;

		/// <summary>
		/// The MaxTokenID token ID.
		/// </summary>
		public const int MaxTokenID = 62;

	}

	#endregion

	#region Lexical State Ids

	/// <summary>
	/// Contains the lexical state IDs for the <c>Luat</c> language.
	/// </summary>
    class LuatLexicalStateId
    {
		/// <summary>
		/// Returns the string-based key for the specified lexical state ID.
		/// </summary>
		/// <param name="id">The lexical state ID to examine.</param>
		public static string GetLexicalStateKey(int id) {
			System.Reflection.FieldInfo[] fields = typeof(LuatLexicalStateId).GetFields();
			foreach (System.Reflection.FieldInfo field in fields) {
				if ((field.IsStatic) && (field.IsLiteral) && (id.Equals(field.GetValue(null))))
					return field.Name;
			}
			return null;
		}

		/// <summary>
		/// The Default lexical state ID.
		/// </summary>
		public const int Default = 0;
	}

	#endregion

	#region Semantic Parser

	/// <summary>
	/// Provides a semantic parser for the <c>Luat</c> language.
	/// </summary>
	class LuatSemanticParser : ActiproSoftware.SyntaxEditor.ParserGenerator.RecursiveDescentSemanticParser {

		private CompilationUnit	compilationUnit;

		/// <summary>
		/// Initializes a new instance of the <c>LuatSemanticParser</c> class.
		/// </summary>
		/// <param name="lexicalParser">The <see cref="ActiproSoftware.SyntaxEditor.ParserGenerator.IRecursiveDescentLexicalParser"/> to use for lexical parsing.</param>
		public LuatSemanticParser(ActiproSoftware.SyntaxEditor.ParserGenerator.IRecursiveDescentLexicalParser lexicalParser) : base(lexicalParser) {}

		/// <summary>
		/// Gets the <see cref="CompilationUnit"/> that was parsed.
		/// </summary>
		/// <value>The <see cref="CompilationUnit"/> that was parsed.</value>
		public CompilationUnit CompilationUnit {
			get {
				return compilationUnit;
			}
		}
	
		/// <summary>
		/// Reports a syntax error.
		/// </summary>
		/// <param name="textRange">The <see cref="TextRange"/> of the error.</param>
		/// <param name="message">The error message.</param>
		protected override void ReportSyntaxError(TextRange textRange, System.String message) {
			// Don't allow multiple errors at the same offset
			if ((compilationUnit.SyntaxErrors.Count > 0) && (((SyntaxError)compilationUnit.SyntaxErrors[compilationUnit.SyntaxErrors.Count - 1]).TextRange.StartOffset == textRange.StartOffset))
				return;
			
			compilationUnit.SyntaxErrors.Add(new SyntaxError(textRange, message));
		}
		
		protected bool RaiseError( int startOffset, string message )
		{
            int endOffset = (this.Token == null) ? this.LookAheadToken.StartOffset : this.Token.EndOffset;
            ReportSyntaxError(new TextRange(startOffset, endOffset), message);
			return false;
		}
		protected bool IsIdentifierAndAssignment()
		{
			bool v = true;
			StartPeek();
			v &= Peek().ID == LuatTokenId.Identifier;
			v &= Peek().ID == LuatTokenId.Assignment;
			StopPeek();
			return v;
		}
		
		protected bool IsVariable( Expression expression )
		{
			return expression is VariableExpression ||
			       expression is IndexExpression;
		}
		
		protected bool IsFunctionCall( Expression expression )
		{
			return expression is FunctionCall;
		}

		public string GetDescription()
		{
			string description = (this.LexicalParser as LuatRecursiveDescentLexicalParser).PrecedingComment;
			
			// Use the preceding comment as the function description
			description = (null != description) 
							? Helpers.Decorate( description.Trim( '\n', '\r' ), DecorationType.Comment )
							: null;
							
			return description;
		}

		/// <summary>
		/// Parses the document and generates a document object model.
		/// </summary>
		public void Parse() {
			this.MatchCompilationUnit();
		}

		/// <summary>
		/// Matches a <c>CompilationUnit</c> non-terminal.
		/// </summary>
		/// <returns><c>true</c> if the <c>CompilationUnit</c> was matched successfully; otherwise, <c>false</c>.</returns>
		/// <remarks>
		/// The non-terminal can start with: <c>Do</c>, <c>While</c>, <c>Repeat</c>, <c>If</c>, <c>For</c>, <c>Local</c>, <c>Function</c>, <c>Return</c>, <c>Break</c>, <c>OpenParenthesis</c>, <c>OpenCurlyBrace</c>, <c>Identifier</c>, <c>TripleDot</c>, <c>Nil</c>, <c>False</c>, <c>True</c>, <c>Number</c>, <c>Subtraction</c>, <c>Not</c>, <c>Hash</c>, <c>String</c>.
		/// </remarks>
		protected virtual bool MatchCompilationUnit() {
			compilationUnit = new CompilationUnit();
			compilationUnit.StartOffset = 0;
			BlockStatement block;
			if (!this.MatchCUBlock(out block))
				return false;
			compilationUnit.Block = block;
			compilationUnit.EndOffset = this.LookAheadToken.EndOffset;
			return true;
		}

		/// <summary>
		/// Matches a <c>CUBlock</c> non-terminal.
		/// </summary>
		/// <returns><c>true</c> if the <c>CUBlock</c> was matched successfully; otherwise, <c>false</c>.</returns>
		/// <remarks>
		/// The non-terminal can start with: <c>Do</c>, <c>While</c>, <c>Repeat</c>, <c>If</c>, <c>For</c>, <c>Local</c>, <c>Function</c>, <c>Return</c>, <c>Break</c>, <c>OpenParenthesis</c>, <c>OpenCurlyBrace</c>, <c>Identifier</c>, <c>TripleDot</c>, <c>Nil</c>, <c>False</c>, <c>True</c>, <c>Number</c>, <c>Subtraction</c>, <c>Not</c>, <c>Hash</c>, <c>String</c>.
		/// </remarks>
		protected virtual bool MatchCUBlock(out BlockStatement block) {
			block = new BlockStatement();
			block.StartOffset = 0;
			
			while( !this.IsAtEnd )
				{
				IToken prevToken      = this.Token;
				bool   bChunkError    = true;
				int    prevErrorCount = compilationUnit.SyntaxErrors.Count;
				if (this.MatchChunk(block)) {
					bChunkError = false;
				}
				// Did the Block non-terminal raise its own error message?
				bool bErrorsReported = compilationUnit.SyntaxErrors.Count > prevErrorCount;
				
				// Should we display an error for the text range of this block?
				bool bDisplayError = bChunkError && !bErrorsReported;
				
				if ( bDisplayError )
					{
					int  errorStart = ( block != null && block.FirstUnconsumedToken != null ) ? block.FirstUnconsumedToken.StartOffset : 0;
					int  errorEnd   = ( this.Token != null ) ? this.Token.EndOffset : this.LookAheadToken.EndOffset;
					
					this.ReportSyntaxError( new TextRange( errorStart, errorEnd ), "Invalid statement" );
				}
				
				if ( this.Token == prevToken )
					{
					// Token did not advance, probably due to error.
					// Move it along.
					this.AdvanceToNext();
				}
				};
				block.EndOffset = this.LookAheadToken.StartOffset;
			return true;
		}

		/// <summary>
		/// Matches a <c>Block</c> non-terminal.
		/// </summary>
		/// <returns><c>true</c> if the <c>Block</c> was matched successfully; otherwise, <c>false</c>.</returns>
		/// <remarks>
		/// The non-terminal can start with: <c>Do</c>, <c>While</c>, <c>Repeat</c>, <c>If</c>, <c>For</c>, <c>Local</c>, <c>Function</c>, <c>Return</c>, <c>Break</c>, <c>OpenParenthesis</c>, <c>OpenCurlyBrace</c>, <c>Identifier</c>, <c>TripleDot</c>, <c>Nil</c>, <c>False</c>, <c>True</c>, <c>Number</c>, <c>Subtraction</c>, <c>Not</c>, <c>Hash</c>, <c>String</c>.
		/// </remarks>
		protected virtual bool MatchBlock(out BlockStatement block) {
			block = new BlockStatement();
			block.StartOffset = this.LookAheadToken.StartOffset;
			if (this.IsInMultiMatchSet(0, this.LookAheadToken)) {
				this.MatchChunk(block);
			}
			block.EndOffset = this.LookAheadToken.StartOffset;
			return true;
		}

		/// <summary>
		/// Matches a <c>Chunk</c> non-terminal.
		/// </summary>
		/// <returns><c>true</c> if the <c>Chunk</c> was matched successfully; otherwise, <c>false</c>.</returns>
		/// <remarks>
		/// The non-terminal can start with: <c>Do</c>, <c>While</c>, <c>Repeat</c>, <c>If</c>, <c>For</c>, <c>Local</c>, <c>Function</c>, <c>Return</c>, <c>Break</c>, <c>OpenParenthesis</c>, <c>OpenCurlyBrace</c>, <c>Identifier</c>, <c>TripleDot</c>, <c>Nil</c>, <c>False</c>, <c>True</c>, <c>Number</c>, <c>Subtraction</c>, <c>Not</c>, <c>Hash</c>, <c>String</c>.
		/// </remarks>
		protected virtual bool MatchChunk(BlockStatement block) {
			while (this.IsInMultiMatchSet(1, this.LookAheadToken)) {
				if (!this.MatchStatement(block))
					return false;
				if (this.TokenIs(this.LookAheadToken, LuatTokenId.SemiColon)) {
					this.Match(LuatTokenId.SemiColon);
				}
				block.FirstUnconsumedToken = this.LookAheadToken;
			}
			if (((this.TokenIs(this.LookAheadToken, LuatTokenId.Return)) || (this.TokenIs(this.LookAheadToken, LuatTokenId.Break)))) {
				if (!this.MatchLastStatement(block))
					return false;
				if (this.TokenIs(this.LookAheadToken, LuatTokenId.SemiColon)) {
					this.Match(LuatTokenId.SemiColon);
				}
				block.FirstUnconsumedToken = this.LookAheadToken;
			}
			return true;
		}

		/// <summary>
		/// Matches a <c>Statement</c> non-terminal.
		/// </summary>
		/// <returns><c>true</c> if the <c>Statement</c> was matched successfully; otherwise, <c>false</c>.</returns>
		/// <remarks>
		/// The non-terminal can start with: <c>Do</c>, <c>While</c>, <c>Repeat</c>, <c>If</c>, <c>For</c>, <c>Local</c>, <c>Function</c>, <c>OpenParenthesis</c>, <c>OpenCurlyBrace</c>, <c>Identifier</c>, <c>TripleDot</c>, <c>Nil</c>, <c>False</c>, <c>True</c>, <c>Number</c>, <c>Subtraction</c>, <c>Not</c>, <c>Hash</c>, <c>String</c>.
		/// </remarks>
		protected virtual bool MatchStatement(BlockStatement block) {
			Statement statement = null;
			int start = this.LookAheadToken.StartOffset;
			if (!this.MatchStatementInner(out statement)) {
				statement = new IncompleteStatement( statement, new TextRange( start, this.Token.EndOffset ) );
			}
			if ( statement != null )
				{
				block.Statements.Add( statement );
			}
			return true;
		}

		/// <summary>
		/// Matches a <c>StatementInner</c> non-terminal.
		/// </summary>
		/// <returns><c>true</c> if the <c>StatementInner</c> was matched successfully; otherwise, <c>false</c>.</returns>
		/// <remarks>
		/// The non-terminal can start with: <c>Do</c>, <c>While</c>, <c>Repeat</c>, <c>If</c>, <c>For</c>, <c>Local</c>, <c>Function</c>, <c>OpenParenthesis</c>, <c>OpenCurlyBrace</c>, <c>Identifier</c>, <c>TripleDot</c>, <c>Nil</c>, <c>False</c>, <c>True</c>, <c>Number</c>, <c>Subtraction</c>, <c>Not</c>, <c>Hash</c>, <c>String</c>.
		/// </remarks>
		protected virtual bool MatchStatementInner(out Statement statement) {
			statement = null;
			if (this.IsInMultiMatchSet(2, this.LookAheadToken)) {
				if (!this.MatchAssignmentOrFunctionCall(out statement))
					return false;
			}
			else if (this.TokenIs(this.LookAheadToken, LuatTokenId.Do)) {
				if (!this.MatchDoStatement(out statement))
					return false;
			}
			else if (this.TokenIs(this.LookAheadToken, LuatTokenId.While)) {
				if (!this.MatchWhileStatement(out statement))
					return false;
			}
			else if (this.TokenIs(this.LookAheadToken, LuatTokenId.Repeat)) {
				if (!this.MatchRepeatStatement(out statement))
					return false;
			}
			else if (this.TokenIs(this.LookAheadToken, LuatTokenId.If)) {
				if (!this.MatchIfStatement(out statement))
					return false;
			}
			else if (this.TokenIs(this.LookAheadToken, LuatTokenId.For)) {
				if (!this.MatchForStatement(out statement))
					return false;
			}
			else if (this.TokenIs(this.LookAheadToken, LuatTokenId.Function)) {
				if (!this.MatchFunctionDeclaration(out statement))
					return false;
			}
			else if (this.TokenIs(this.LookAheadToken, LuatTokenId.Local)) {
				if (!this.MatchLocalDeclaration(out statement))
					return false;
			}
			else
				return false;
			return true;
		}

		/// <summary>
		/// Matches a <c>LastStatement</c> non-terminal.
		/// </summary>
		/// <returns><c>true</c> if the <c>LastStatement</c> was matched successfully; otherwise, <c>false</c>.</returns>
		/// <remarks>
		/// The non-terminal can start with: <c>Return</c>, <c>Break</c>.
		/// </remarks>
		protected virtual bool MatchLastStatement(BlockStatement block) {
			Statement statement = null;
			int start = this.LookAheadToken.StartOffset;
			if (!this.MatchLastStatementInner(out statement)) {
				statement = new IncompleteStatement( statement, new TextRange( start, this.Token.EndOffset ) );
			}
			if ( statement != null )
				{
				block.Statements.Add( statement );
			}
			return true;
		}

		/// <summary>
		/// Matches a <c>LastStatementInner</c> non-terminal.
		/// </summary>
		/// <returns><c>true</c> if the <c>LastStatementInner</c> was matched successfully; otherwise, <c>false</c>.</returns>
		/// <remarks>
		/// The non-terminal can start with: <c>Return</c>, <c>Break</c>.
		/// </remarks>
		protected virtual bool MatchLastStatementInner(out Statement statement) {
			statement = null;
			if (this.TokenIs(this.LookAheadToken, LuatTokenId.Return)) {
				if (!this.MatchReturnStatement(out statement))
					return false;
			}
			else if (this.TokenIs(this.LookAheadToken, LuatTokenId.Break)) {
				if (!this.MatchBreakStatement(out statement))
					return false;
			}
			else
				return false;
			return true;
		}

		/// <summary>
		/// Matches a <c>AssignmentOrFunctionCall</c> non-terminal.
		/// </summary>
		/// <returns><c>true</c> if the <c>AssignmentOrFunctionCall</c> was matched successfully; otherwise, <c>false</c>.</returns>
		/// <remarks>
		/// The non-terminal can start with: <c>OpenParenthesis</c>, <c>OpenCurlyBrace</c>, <c>Identifier</c>, <c>TripleDot</c>, <c>Nil</c>, <c>False</c>, <c>True</c>, <c>Number</c>, <c>Subtraction</c>, <c>Not</c>, <c>Hash</c>, <c>String</c>.
		/// </remarks>
		protected virtual bool MatchAssignmentOrFunctionCall(out Statement statement) {
			Expression expression = null;
			statement             = null;
			int start = this.LookAheadToken.StartOffset;
			if (!this.MatchExpressionNoFunction(out expression))
				return false;
			if ( IsFunctionCall( expression ) )
				{
				statement = new ExpressionStatement( expression );
				return true;
			}
			
			AssignmentStatement assignment = new AssignmentStatement();
			statement = assignment;
			
			assignment.Variables.Add( expression );
			while (this.TokenIs(this.LookAheadToken, LuatTokenId.Comma)) {
				if (!this.Match(LuatTokenId.Comma))
					return false;
				if (!this.MatchExpressionNoFunction(out expression))
					return false;
				assignment.Variables.Add( expression );
			}
			if (!this.Match(LuatTokenId.Assignment))
				return false;
			if (!this.MatchExpressionList(assignment.Values))
				return false;
			foreach( Expression variable in assignment.Variables )
				{
				// Mark the LHS expression as being assigned.
				// This alters the way certain variables are resolved.
				variable.IsLHSOfAssignment = true;
				
				if( false == IsVariable( variable ) )
					{
					// First expression was not a variable or a function call.
					// Invalid statement
					string message = "The left-hand side of an assignment must be a variable";
					compilationUnit.SyntaxErrors.Add(new SyntaxError( variable.TextRange, message ) );
				}
			}
			
			assignment.StartOffset = start;
			assignment.EndOffset   = this.Token.EndOffset;
			return true;
		}

		/// <summary>
		/// Matches a <c>CallSuffix</c> non-terminal.
		/// </summary>
		/// <returns><c>true</c> if the <c>CallSuffix</c> was matched successfully; otherwise, <c>false</c>.</returns>
		/// <remarks>
		/// The non-terminal can start with: <c>Colon</c>, <c>OpenParenthesis</c>, <c>OpenCurlyBrace</c>, <c>String</c>.
		/// </remarks>
		protected virtual bool MatchCallSuffix(ref Expression expression) {
			Identifier name = null;
			bool bPassesSelf = false;
			if (this.TokenIs(this.LookAheadToken, LuatTokenId.Colon)) {
				if (!this.Match(LuatTokenId.Colon))
					return false;
				bPassesSelf = true;
				this.MatchIdentifier(out name);
			}
			FunctionCall functionCall = new FunctionCall( expression, name );
			LuatAstNodeBase arguments;
			this.MatchArguments(out arguments);
			functionCall.Arguments = arguments;
			functionCall.EndOffset = this.Token.EndOffset;
			functionCall.PassesSelf = bPassesSelf;
			expression = functionCall;
			return true;
		}

		/// <summary>
		/// Matches a <c>DoStatement</c> non-terminal.
		/// </summary>
		/// <returns><c>true</c> if the <c>DoStatement</c> was matched successfully; otherwise, <c>false</c>.</returns>
		/// <remarks>
		/// The non-terminal can start with: <c>Do</c>.
		/// </remarks>
		protected virtual bool MatchDoStatement(out Statement statement) {
			statement = null;
			DoStatement doStatement = new DoStatement();
			doStatement.StartOffset = this.Token.StartOffset;
			statement = doStatement;
			if (!this.Match(LuatTokenId.Do))
				return false;
			BlockStatement body;
			if (!this.MatchBlock(out body))
				return false;
			doStatement.Body = body;
			if (!this.Match(LuatTokenId.End))
				return false;
			doStatement.EndOffset = this.Token.EndOffset;
			return true;
		}

		/// <summary>
		/// Matches a <c>WhileStatement</c> non-terminal.
		/// </summary>
		/// <returns><c>true</c> if the <c>WhileStatement</c> was matched successfully; otherwise, <c>false</c>.</returns>
		/// <remarks>
		/// The non-terminal can start with: <c>While</c>.
		/// </remarks>
		protected virtual bool MatchWhileStatement(out Statement statement) {
			statement = null;
			if (!this.Match(LuatTokenId.While))
				return false;
			WhileStatement whileStatement = new WhileStatement();
			whileStatement.StartOffset = this.Token.StartOffset;
			statement = whileStatement;
			Expression conditional;
			if (!this.MatchExpression(out conditional))
				return false;
			whileStatement.Conditional = conditional;
			if (!this.Match(LuatTokenId.Do))
				return false;
			BlockStatement Block;
			if (!this.MatchBlock(out Block))
				return false;
			whileStatement.Block = Block;
			if (!this.Match(LuatTokenId.End))
				return false;
			whileStatement.EndOffset = this.Token.EndOffset;
			return true;
		}

		/// <summary>
		/// Matches a <c>RepeatStatement</c> non-terminal.
		/// </summary>
		/// <returns><c>true</c> if the <c>RepeatStatement</c> was matched successfully; otherwise, <c>false</c>.</returns>
		/// <remarks>
		/// The non-terminal can start with: <c>Repeat</c>.
		/// </remarks>
		protected virtual bool MatchRepeatStatement(out Statement statement) {
			statement = null;
			RepeatStatement repeatStatement = new RepeatStatement();
			repeatStatement.StartOffset = this.Token.StartOffset;
			statement = repeatStatement;
			if (!this.Match(LuatTokenId.Repeat))
				return false;
			BlockStatement Block;
			if (!this.MatchBlock(out Block))
				return false;
			repeatStatement.Block = Block;
			if (!this.Match(LuatTokenId.Until))
				return false;
			Expression conditional;
			if (!this.MatchExpression(out conditional))
				return false;
			repeatStatement.Conditional = conditional;
			repeatStatement.EndOffset = this.Token.EndOffset;
			return true;
		}

		/// <summary>
		/// Matches a <c>IfStatement</c> non-terminal.
		/// </summary>
		/// <returns><c>true</c> if the <c>IfStatement</c> was matched successfully; otherwise, <c>false</c>.</returns>
		/// <remarks>
		/// The non-terminal can start with: <c>If</c>.
		/// </remarks>
		protected virtual bool MatchIfStatement(out Statement statement) {
			IfStatement ifStatement = new IfStatement();
			statement = ifStatement;
			Expression       conditional;
			BlockStatement   block;
			ifStatement.StartOffset = this.LookAheadToken.StartOffset;
			if (!this.Match(LuatTokenId.If))
				return false;
			if (!this.MatchExpression(out conditional))
				return false;
			ifStatement.Conditional = conditional;
			if (!this.Match(LuatTokenId.Then))
				return false;
			if (!this.MatchBlock(out block))
				return false;
			ifStatement.Block       = block;
			while (this.TokenIs(this.LookAheadToken, LuatTokenId.Elseif)) {
				if (!this.Match(LuatTokenId.Elseif))
					return false;
				ConditionalBlock conditionalBlock = new ConditionalBlock();
				if (!this.MatchExpression(out conditional))
					return false;
				conditionalBlock.Conditional = conditional;
				if (!this.Match(LuatTokenId.Then))
					return false;
				if (!this.MatchBlock(out block))
					return false;
				conditionalBlock.Block = block;
				ifStatement.ElseIfs.Add( conditionalBlock );
			}
			if (this.TokenIs(this.LookAheadToken, LuatTokenId.Else)) {
				if (!this.Match(LuatTokenId.Else))
					return false;
				if (!this.MatchBlock(out block))
					return false;
				ifStatement.Else = block;
			}
			if (!this.Match(LuatTokenId.End))
				return false;
			ifStatement.EndOffset = this.Token.EndOffset;
			return true;
		}

		/// <summary>
		/// Matches a <c>ForStatement</c> non-terminal.
		/// </summary>
		/// <returns><c>true</c> if the <c>ForStatement</c> was matched successfully; otherwise, <c>false</c>.</returns>
		/// <remarks>
		/// The non-terminal can start with: <c>For</c>.
		/// </remarks>
		protected virtual bool MatchForStatement(out Statement statement) {
			statement = null;
			int startOffset = this.LookAheadToken.StartOffset;
			if (!this.Match(LuatTokenId.For))
				return false;
			Identifier firstIdent = null;
			if (!this.MatchIdentifier(out firstIdent))
				return false;
			if (this.TokenIs(this.LookAheadToken, LuatTokenId.Assignment)) {
				// for a = b, c, d do ... end
				VariableExpression firstVar = new VariableExpression( firstIdent );
				ForStatement forStatement = new ForStatement();
				statement = forStatement;
				forStatement.Iterator = firstVar as VariableExpression;
				if (!this.Match(LuatTokenId.Assignment))
					return false;
				Expression start = null;
				if (!this.MatchExpression(out start))
					return false;
				forStatement.Start = start;
				if (!this.Match(LuatTokenId.Comma))
					return false;
				Expression end = null;
				if (!this.MatchExpression(out end))
					return false;
				forStatement.End = end;
				if (this.TokenIs(this.LookAheadToken, LuatTokenId.Comma)) {
					if (!this.Match(LuatTokenId.Comma))
						return false;
					Expression step = null;
					if (!this.MatchExpression(out step))
						return false;
					forStatement.Step = step;
				}
				if (!this.Match(LuatTokenId.Do))
					return false;
				BlockStatement body;
				if (!this.MatchBlock(out body))
					return false;
				forStatement.Body = body;
				if (!this.Match(LuatTokenId.End))
					return false;
				forStatement.StartOffset = startOffset; forStatement.EndOffset = this.Token.EndOffset;
			}
			else if (((this.TokenIs(this.LookAheadToken, LuatTokenId.Comma)) || (this.TokenIs(this.LookAheadToken, LuatTokenId.In)))) {
				// for a,b in c,d do ... end
				ForInStatement forInStatement = new ForInStatement();
				statement = forInStatement;
				forInStatement.Iterators.Add( firstIdent );
				if (this.TokenIs(this.LookAheadToken, LuatTokenId.Comma)) {
					if (!this.Match(LuatTokenId.Comma))
						return false;
					if (!this.MatchIdentifierList(forInStatement.Iterators))
						return false;
				}
				if (!this.Match(LuatTokenId.In))
					return false;
				if (!this.MatchExpressionList(forInStatement.Tables))
					return false;
				if (!this.Match(LuatTokenId.Do))
					return false;
				BlockStatement body;
				if (!this.MatchBlock(out body))
					return false;
				forInStatement.Body = body;
				if (!this.Match(LuatTokenId.End))
					return false;
				forInStatement.StartOffset = startOffset; forInStatement.EndOffset = this.Token.EndOffset;
			}
			else
				return false;
			return true;
		}

		/// <summary>
		/// Matches a <c>LocalDeclaration</c> non-terminal.
		/// </summary>
		/// <returns><c>true</c> if the <c>LocalDeclaration</c> was matched successfully; otherwise, <c>false</c>.</returns>
		/// <remarks>
		/// The non-terminal can start with: <c>Local</c>.
		/// </remarks>
		protected virtual bool MatchLocalDeclaration(out Statement statement) {
			statement = null;
			int start = this.LookAheadToken.StartOffset;
			if (!this.Match(LuatTokenId.Local))
				return false;
			if (!this.MatchLocalSuffix(out statement)) {
				return this.RaiseError( start, "Expected identifier or function declaration." );
			}
			else {
				statement.StartOffset = start;
				statement.EndOffset = this.Token.EndOffset;
			}
			return true;
		}

		/// <summary>
		/// Matches a <c>LocalSuffix</c> non-terminal.
		/// </summary>
		/// <returns><c>true</c> if the <c>LocalSuffix</c> was matched successfully; otherwise, <c>false</c>.</returns>
		/// <remarks>
		/// The non-terminal can start with: <c>Function</c>, <c>Identifier</c>.
		/// </remarks>
		protected virtual bool MatchLocalSuffix(out Statement statement) {
			statement = null;
			AssignmentStatement assignment = new AssignmentStatement();
			assignment.IsLocal = true;
			Identifier type = null;
			if (this.TokenIs(this.LookAheadToken, LuatTokenId.Identifier)) {
				if (!this.MatchVariableList(assignment.Variables))
					return false;
				if (this.TokenIs(this.LookAheadToken, LuatTokenId.Colon)) {
					if (!this.Match(LuatTokenId.Colon))
						return false;
					if (!this.MatchIdentifier(out type))
						return false;
				}
				if (this.TokenIs(this.LookAheadToken, LuatTokenId.Assignment)) {
					if (!this.Match(LuatTokenId.Assignment))
						return false;
					if (!this.MatchExpressionList(assignment.Values))
						return false;
				}
			}
			else if (this.TokenIs(this.LookAheadToken, LuatTokenId.Function)) {
				if (!this.Match(LuatTokenId.Function))
					return false;
				Expression variableNode;
				if (!this.MatchVariable(out variableNode))
					return false;
				Function function;
				if (!this.MatchFunctionBody(out function))
					return false;
				VariableExpression variableExpression = variableNode as VariableExpression;
				assignment.Variables.Add( variableNode );
				assignment.Values.Add( function );
			}
			else
				return false;
			foreach ( VariableExpression v in assignment.Variables )
				{
				v.IsLHSOfAssignment = true;
				v.IsLocal           = true;
				v.Type              = type == null ? null : type.Text;
			}
			
			statement = assignment;
			return true;
		}

		/// <summary>
		/// Matches a <c>FunctionName</c> non-terminal.
		/// </summary>
		/// <returns><c>true</c> if the <c>FunctionName</c> was matched successfully; otherwise, <c>false</c>.</returns>
		/// <remarks>
		/// The non-terminal can start with: <c>Identifier</c>.
		/// </remarks>
		protected virtual bool MatchFunctionName(out Expression expression, out bool expectsSelf) {
			expression = null;
			Identifier identifier;
			IToken     indexToken;
			expectsSelf = false;
			if (!this.MatchIdentifier(out identifier))
				return false;
			expression = new VariableExpression( identifier );
			while (this.TokenIs(this.LookAheadToken, LuatTokenId.Dot)) {
				if (!this.Match(LuatTokenId.Dot))
					return false;
				indexToken = this.Token;
				if (!this.MatchIdentifier(out identifier))
					return false;
				expression = new IndexExpression( expression, indexToken, identifier );
			}
			if (this.TokenIs(this.LookAheadToken, LuatTokenId.Colon)) {
				if (!this.Match(LuatTokenId.Colon))
					return false;
				indexToken = this.Token;
				expectsSelf = true;
				if (!this.MatchIdentifier(out identifier))
					return false;
				expression = new IndexExpression( expression, indexToken, identifier );
			}
			return true;
		}

		/// <summary>
		/// Matches a <c>FunctionDeclaration</c> non-terminal.
		/// </summary>
		/// <returns><c>true</c> if the <c>FunctionDeclaration</c> was matched successfully; otherwise, <c>false</c>.</returns>
		/// <remarks>
		/// The non-terminal can start with: <c>Function</c>.
		/// </remarks>
		protected virtual bool MatchFunctionDeclaration(out Statement statement) {
			statement = null;
			int start = this.LookAheadToken.StartOffset;
			string description = this.GetDescription();
			bool expectsSelf;
			if (!this.Match(LuatTokenId.Function))
				return false;
			Expression name;
			if (!this.MatchFunctionName(out name, out expectsSelf))
				return false;
			Function function;
			if (!this.MatchFunctionBody(out function))
				return false;
			AssignmentStatement assignment = new AssignmentStatement();
			assignment.Variables.Add( name );
			assignment.Values.Add( function );
			
			// Use the preceding comment as the function description
			function.Description = (null != description)
				? description.Trim( '\n', '\r' )
				: null;
			
			function.ExpectsSelf = expectsSelf;
			
			// Mark the LHS expression as being assigned.
			// This alters the way certain variables are resolved.
			name.IsLHSOfAssignment = true;
			
			assignment.StartOffset = start; assignment.EndOffset = this.Token.EndOffset;
			statement = assignment;
			return true;
		}

		/// <summary>
		/// Matches a <c>ReturnStatement</c> non-terminal.
		/// </summary>
		/// <returns><c>true</c> if the <c>ReturnStatement</c> was matched successfully; otherwise, <c>false</c>.</returns>
		/// <remarks>
		/// The non-terminal can start with: <c>Return</c>.
		/// </remarks>
		protected virtual bool MatchReturnStatement(out Statement statement) {
			statement = null;
			int start = this.LookAheadToken.StartOffset;
			if (!this.Match(LuatTokenId.Return))
				return false;
			ReturnStatement returnStatement = new ReturnStatement();
			returnStatement.IsMultiline     = this.LexicalParser.LookAheadTokenIsOnDifferentLine;
			if (this.IsInMultiMatchSet(3, this.LookAheadToken)) {
				if (!this.MatchExpressionList(returnStatement.Values))
					return false;
			}
			returnStatement.StartOffset = start; returnStatement.EndOffset = this.Token.EndOffset;
		if ( returnStatement.Values.Count == 0 ) { returnStatement.IsMultiline = false; }
		statement = returnStatement;
			return true;
		}

		/// <summary>
		/// Matches a <c>BreakStatement</c> non-terminal.
		/// </summary>
		/// <returns><c>true</c> if the <c>BreakStatement</c> was matched successfully; otherwise, <c>false</c>.</returns>
		/// <remarks>
		/// The non-terminal can start with: <c>Break</c>.
		/// </remarks>
		protected virtual bool MatchBreakStatement(out Statement statement) {
			statement = null;
			if (!this.Match(LuatTokenId.Break))
				return false;
			statement = new BreakStatement( this.Token.TextRange );
			return true;
		}

		/// <summary>
		/// Matches a <c>FunctionBody</c> non-terminal.
		/// </summary>
		/// <returns><c>true</c> if the <c>FunctionBody</c> was matched successfully; otherwise, <c>false</c>.</returns>
		/// <remarks>
		/// The non-terminal can start with: <c>OpenParenthesis</c>.
		/// </remarks>
		protected virtual bool MatchFunctionBody(out Function function) {
			function  = null;
			int start = this.LookAheadToken.StartOffset;
			function  = new Function();
			if (!this.Match(LuatTokenId.OpenParenthesis))
				return false;
			if (((this.TokenIs(this.LookAheadToken, LuatTokenId.Identifier)) || (this.TokenIs(this.LookAheadToken, LuatTokenId.TripleDot)))) {
				if (!this.MatchParameterList(function.Parameters))
					return false;
			}
			if (!this.Match(LuatTokenId.CloseParenthesis))
				return false;
			BlockStatement block;
			int collapsibleStart = this.Token.EndOffset;
			if (!this.MatchBlock(out block))
				return false;
			function.Block = block;
			if (!this.Match(LuatTokenId.End)) {
				this.RaiseError( start, "Missing end" );
			}
			function.StartOffset   	        = start;
			function.EndOffset     	        = this.Token.EndOffset;
			function.CollapsibleStartOffset = collapsibleStart;
			function.CollapsibleEndOffset   = this.Token.EndOffset;
			return true;
		}

		/// <summary>
		/// Matches a <c>Arguments</c> non-terminal.
		/// </summary>
		/// <returns><c>true</c> if the <c>Arguments</c> was matched successfully; otherwise, <c>false</c>.</returns>
		/// <remarks>
		/// The non-terminal can start with: <c>OpenParenthesis</c>, <c>OpenCurlyBrace</c>, <c>String</c>.
		/// </remarks>
		protected virtual bool MatchArguments(out LuatAstNodeBase arguments) {
			arguments = null;
			if (this.TokenIs(this.LookAheadToken, LuatTokenId.OpenParenthesis)) {
				ArgumentList arglist;
				if (!this.MatchArgumentList(out arglist))
					return false;
				arguments = arglist;
			}
			else if (this.TokenIs(this.LookAheadToken, LuatTokenId.OpenCurlyBrace)) {
				Expression table;
				if (!this.MatchTableConstructor(out table))
					return false;
				arguments = table;
			}
			else if (this.TokenIs(this.LookAheadToken, LuatTokenId.String)) {
				Expression stringExp;
				if (!this.MatchStringExpression(out stringExp))
					return false;
				arguments = stringExp;
			}
			else
				return false;
			return true;
		}

		/// <summary>
		/// Matches a <c>ArgumentList</c> non-terminal.
		/// </summary>
		/// <returns><c>true</c> if the <c>ArgumentList</c> was matched successfully; otherwise, <c>false</c>.</returns>
		/// <remarks>
		/// The non-terminal can start with: <c>OpenParenthesis</c>.
		/// </remarks>
		protected virtual bool MatchArgumentList(out ArgumentList arglist) {
			arglist = new ArgumentList();
			arglist.StartOffset = this.LookAheadToken.StartOffset;
			if (!this.Match(LuatTokenId.OpenParenthesis))
				return false;
			int listStart = this.Token.EndOffset;
			if (this.IsInMultiMatchSet(3, this.LookAheadToken)) {
				this.MatchExpressionList(arglist.Arguments);
			}
			arglist.ListTextRange = new TextRange( listStart, this.LookAheadToken.StartOffset );
			if (!this.Match(LuatTokenId.CloseParenthesis)) {
				this.ReportSyntaxError( "Missing argument" );
			}
			else {
				arglist.IsClosed = true;
			}
			arglist.EndOffset = this.Token.EndOffset;
			return true;
		}

		/// <summary>
		/// Matches a <c>Function</c> non-terminal.
		/// </summary>
		/// <returns><c>true</c> if the <c>Function</c> was matched successfully; otherwise, <c>false</c>.</returns>
		/// <remarks>
		/// The non-terminal can start with: <c>Function</c>.
		/// </remarks>
		protected virtual bool MatchFunction(out Expression expression) {
			expression = null;
			if (!this.Match(LuatTokenId.Function))
				return false;
			Function function;
			if (!this.MatchFunctionBody(out function))
				return false;
			expression = function;
			return true;
		}

		/// <summary>
		/// Matches a <c>TableConstructor</c> non-terminal.
		/// </summary>
		/// <returns><c>true</c> if the <c>TableConstructor</c> was matched successfully; otherwise, <c>false</c>.</returns>
		/// <remarks>
		/// The non-terminal can start with: <c>OpenCurlyBrace</c>.
		/// </remarks>
		protected virtual bool MatchTableConstructor(out Expression expression) {
			expression = null;
			int start = this.LookAheadToken.StartOffset;
			if (!this.Match(LuatTokenId.OpenCurlyBrace))
				return false;
			TableConstructor table = new TableConstructor();
			if (this.IsInMultiMatchSet(4, this.LookAheadToken)) {
				if (!this.MatchFieldList(table.Fields))
					return false;
			}
			if (!this.Match(LuatTokenId.CloseCurlyBrace))
				return false;
			table.StartOffset = start; table.EndOffset = this.Token.EndOffset;
			expression = table;
			return true;
		}

		/// <summary>
		/// Matches a <c>FieldList</c> non-terminal.
		/// </summary>
		/// <returns><c>true</c> if the <c>FieldList</c> was matched successfully; otherwise, <c>false</c>.</returns>
		/// <remarks>
		/// The non-terminal can start with: <c>Function</c>, <c>OpenParenthesis</c>, <c>OpenCurlyBrace</c>, <c>OpenSquareBracket</c>, <c>Identifier</c>, <c>TripleDot</c>, <c>Nil</c>, <c>False</c>, <c>True</c>, <c>Number</c>, <c>Subtraction</c>, <c>Not</c>, <c>Hash</c>, <c>String</c>.
		/// </remarks>
		protected virtual bool MatchFieldList(IAstNodeList fields) {
			Field field;
			if (!this.MatchField(out field))
				return false;
			fields.Add( field );
			if (((this.TokenIs(this.LookAheadToken, LuatTokenId.SemiColon)) || (this.TokenIs(this.LookAheadToken, LuatTokenId.Comma)))) {
				if (!this.MatchFieldSep())
					return false;
				if (this.IsInMultiMatchSet(4, this.LookAheadToken)) {
					if (!this.MatchFieldList(fields))
						return false;
				}
			}
			return true;
		}

		/// <summary>
		/// Matches a <c>Field</c> non-terminal.
		/// </summary>
		/// <returns><c>true</c> if the <c>Field</c> was matched successfully; otherwise, <c>false</c>.</returns>
		/// <remarks>
		/// The non-terminal can start with: <c>Function</c>, <c>OpenParenthesis</c>, <c>OpenCurlyBrace</c>, <c>OpenSquareBracket</c>, <c>Identifier</c>, <c>TripleDot</c>, <c>Nil</c>, <c>False</c>, <c>True</c>, <c>Number</c>, <c>Subtraction</c>, <c>Not</c>, <c>Hash</c>, <c>String</c>.
		/// </remarks>
		protected virtual bool MatchField(out Field field) {
			field = null;
			int start = this.LookAheadToken.StartOffset;
			IAstNode   key   = null;
			Expression value = null;
			if (this.TokenIs(this.LookAheadToken, LuatTokenId.OpenSquareBracket)) {
				if (!this.Match(LuatTokenId.OpenSquareBracket))
					return false;
				Expression keyExpr;
				if (!this.MatchExpression(out keyExpr))
					return false;
				key = keyExpr;
				if (!this.Match(LuatTokenId.CloseSquareBracket))
					return false;
				if (!this.Match(LuatTokenId.Assignment))
					return false;
				if (!this.MatchExpression(out value))
					return false;
			}
			else if (this.IsIdentifierAndAssignment()) {
				Identifier keyIdent;
				if (!this.MatchIdentifier(out keyIdent))
					return false;
				key = keyIdent;
				if (!this.Match(LuatTokenId.Assignment))
					return false;
				if (!this.MatchExpression(out value))
					return false;
			}
			else if (this.IsInMultiMatchSet(5, this.LookAheadToken)) {
				if (!this.MatchExpression(out value))
					return false;
			}
			else
				return false;
			field = new Field( key, value, new TextRange( start, this.Token.EndOffset ) );
			return true;
		}

		/// <summary>
		/// Matches a <c>FieldSep</c> non-terminal.
		/// </summary>
		/// <returns><c>true</c> if the <c>FieldSep</c> was matched successfully; otherwise, <c>false</c>.</returns>
		/// <remarks>
		/// The non-terminal can start with: <c>SemiColon</c>, <c>Comma</c>.
		/// </remarks>
		protected virtual bool MatchFieldSep() {
			if (this.TokenIs(this.LookAheadToken, LuatTokenId.Comma)) {
				if (!this.Match(LuatTokenId.Comma))
					return false;
			}
			else if (this.TokenIs(this.LookAheadToken, LuatTokenId.SemiColon)) {
				if (!this.Match(LuatTokenId.SemiColon))
					return false;
			}
			else
				return false;
			return true;
		}

		/// <summary>
		/// Matches a <c>Identifier</c> non-terminal.
		/// </summary>
		/// <returns><c>true</c> if the <c>Identifier</c> was matched successfully; otherwise, <c>false</c>.</returns>
		/// <remarks>
		/// The non-terminal can start with: <c>Identifier</c>.
		/// </remarks>
		protected virtual bool MatchIdentifier(out Identifier identifier) {
			identifier = null;
			if (!this.Match(LuatTokenId.Identifier))
				return false;
			identifier = new Identifier( this.TokenText, this.Token.TextRange );
			return true;
		}

		/// <summary>
		/// Matches a <c>ParameterList</c> non-terminal.
		/// </summary>
		/// <returns><c>true</c> if the <c>ParameterList</c> was matched successfully; otherwise, <c>false</c>.</returns>
		/// <remarks>
		/// The non-terminal can start with: <c>Identifier</c>, <c>TripleDot</c>.
		/// </remarks>
		protected virtual bool MatchParameterList(IAstNodeList list) {
			if (this.TokenIs(this.LookAheadToken, LuatTokenId.Identifier)) {
				Identifier identifier = null;
				if (!this.MatchIdentifier(out identifier))
					return false;
				list.Add( identifier );
				while (this.TokenIs(this.LookAheadToken, LuatTokenId.Comma)) {
					if (!this.Match(LuatTokenId.Comma))
						return false;
					if (this.TokenIs(this.LookAheadToken, LuatTokenId.Identifier)) {
						if (!this.MatchIdentifier(out identifier))
							return false;
						list.Add( identifier );
					}
					else if (this.TokenIs(this.LookAheadToken, LuatTokenId.TripleDot)) {
						if (!this.Match(LuatTokenId.TripleDot))
							return false;
					}
					else
						return false;
				}
			}
			else if (this.TokenIs(this.LookAheadToken, LuatTokenId.TripleDot)) {
				if (!this.Match(LuatTokenId.TripleDot))
					return false;
			}
			else
				return false;
			return true;
		}

		/// <summary>
		/// Matches a <c>IdentifierList</c> non-terminal.
		/// </summary>
		/// <returns><c>true</c> if the <c>IdentifierList</c> was matched successfully; otherwise, <c>false</c>.</returns>
		/// <remarks>
		/// The non-terminal can start with: <c>Identifier</c>.
		/// </remarks>
		protected virtual bool MatchIdentifierList(IAstNodeList list) {
			Identifier identifier = null;
			if (!this.MatchIdentifier(out identifier))
				return false;
			list.Add( identifier );
			while (this.TokenIs(this.LookAheadToken, LuatTokenId.Comma)) {
				if (!this.Match(LuatTokenId.Comma))
					return false;
				if (!this.MatchIdentifier(out identifier))
					return false;
				list.Add( identifier );
			}
			return true;
		}

		/// <summary>
		/// Matches a <c>ExpressionNoFunction</c> non-terminal.
		/// </summary>
		/// <returns><c>true</c> if the <c>ExpressionNoFunction</c> was matched successfully; otherwise, <c>false</c>.</returns>
		/// <remarks>
		/// The non-terminal can start with: <c>OpenParenthesis</c>, <c>OpenCurlyBrace</c>, <c>Identifier</c>, <c>TripleDot</c>, <c>Nil</c>, <c>False</c>, <c>True</c>, <c>Number</c>, <c>Subtraction</c>, <c>Not</c>, <c>Hash</c>, <c>String</c>.
		/// </remarks>
		protected virtual bool MatchExpressionNoFunction(out Expression expression) {
			expression = null;
			int start = this.LookAheadToken.StartOffset;
			if (!this.MatchExpressionNoFunctionInner(out expression)) {
				expression = new IncompleteExpression( expression, new TextRange( start, this.Token.EndOffset ) );
			}
			return true;
		}

		/// <summary>
		/// Matches a <c>ExpressionNoFunctionInner</c> non-terminal.
		/// </summary>
		/// <returns><c>true</c> if the <c>ExpressionNoFunctionInner</c> was matched successfully; otherwise, <c>false</c>.</returns>
		/// <remarks>
		/// The non-terminal can start with: <c>OpenParenthesis</c>, <c>OpenCurlyBrace</c>, <c>Identifier</c>, <c>TripleDot</c>, <c>Nil</c>, <c>False</c>, <c>True</c>, <c>Number</c>, <c>Subtraction</c>, <c>Not</c>, <c>Hash</c>, <c>String</c>.
		/// </remarks>
		protected virtual bool MatchExpressionNoFunctionInner(out Expression expression) {
			expression = null;
			if (this.IsInMultiMatchSet(6, this.LookAheadToken)) {
				if (!this.MatchPrimary(out expression))
					return false;
			}
			else if (this.TokenIs(this.LookAheadToken, LuatTokenId.TripleDot)) {
				if (!this.Match(LuatTokenId.TripleDot))
					return false;
				expression = new NilExpression( this.Token.TextRange );
			}
			else if (((this.TokenIs(this.LookAheadToken, LuatTokenId.OpenParenthesis)) || (this.TokenIs(this.LookAheadToken, LuatTokenId.OpenCurlyBrace)) || (this.TokenIs(this.LookAheadToken, LuatTokenId.Identifier)))) {
				if (this.TokenIs(this.LookAheadToken, LuatTokenId.Identifier)) {
					if (!this.MatchVariable(out expression))
						return false;
				}
				else if (this.TokenIs(this.LookAheadToken, LuatTokenId.OpenCurlyBrace)) {
					if (!this.MatchTableConstructor(out expression))
						return false;
				}
				else if (this.TokenIs(this.LookAheadToken, LuatTokenId.OpenParenthesis)) {
					if (!this.MatchParenthesis(out expression))
						return false;
				}
				else
					return false;
				while (this.IsInMultiMatchSet(7, this.LookAheadToken)) {
					if (((this.TokenIs(this.LookAheadToken, LuatTokenId.Dot)) || (this.TokenIs(this.LookAheadToken, LuatTokenId.OpenSquareBracket)))) {
						if (!this.MatchIndex(ref expression))
							return false;
					}
					else if (this.IsInMultiMatchSet(8, this.LookAheadToken)) {
						if (!this.MatchCallSuffix(ref expression))
							return false;
					}
					else
						return false;
				}
			}
			else if (((this.TokenIs(this.LookAheadToken, LuatTokenId.Subtraction)) || (this.TokenIs(this.LookAheadToken, LuatTokenId.Not)) || (this.TokenIs(this.LookAheadToken, LuatTokenId.Hash)))) {
				int start = this.LookAheadToken.StartOffset;
				OperatorType operatorType;
				if (!this.MatchUnaryOp(out operatorType))
					return false;
				Expression rhs = null;
				if (!this.MatchExpression(out rhs))
					return false;
				expression = new UnaryExpression( operatorType, rhs, new TextRange( start, this.Token.EndOffset) );
			}
			else
				return false;
			while (this.IsInMultiMatchSet(9, this.LookAheadToken)) {
				OperatorType operatorType;
				if (!this.MatchBinaryOp(out operatorType))
					return false;
				Expression rhs = null;
				if (!this.MatchExpression(out rhs))
					return false;
				expression = new BinaryExpression( operatorType, expression, rhs );
			}
			return true;
		}

		/// <summary>
		/// Matches a <c>Expression</c> non-terminal.
		/// </summary>
		/// <returns><c>true</c> if the <c>Expression</c> was matched successfully; otherwise, <c>false</c>.</returns>
		/// <remarks>
		/// The non-terminal can start with: <c>Function</c>, <c>OpenParenthesis</c>, <c>OpenCurlyBrace</c>, <c>Identifier</c>, <c>TripleDot</c>, <c>Nil</c>, <c>False</c>, <c>True</c>, <c>Number</c>, <c>Subtraction</c>, <c>Not</c>, <c>Hash</c>, <c>String</c>.
		/// </remarks>
		protected virtual bool MatchExpression(out Expression expression) {
			expression = null;
			int start = this.LookAheadToken.StartOffset;
			if (!this.MatchExpressionInner(out expression)) {
				expression = new IncompleteExpression( expression, new TextRange( start, this.Token.EndOffset ) );
			}
			return true;
		}

		/// <summary>
		/// Matches a <c>ExpressionInner</c> non-terminal.
		/// </summary>
		/// <returns><c>true</c> if the <c>ExpressionInner</c> was matched successfully; otherwise, <c>false</c>.</returns>
		/// <remarks>
		/// The non-terminal can start with: <c>Function</c>, <c>OpenParenthesis</c>, <c>OpenCurlyBrace</c>, <c>Identifier</c>, <c>TripleDot</c>, <c>Nil</c>, <c>False</c>, <c>True</c>, <c>Number</c>, <c>Subtraction</c>, <c>Not</c>, <c>Hash</c>, <c>String</c>.
		/// </remarks>
		protected virtual bool MatchExpressionInner(out Expression expression) {
			expression = null;
			string description = this.GetDescription();
			if (this.IsInMultiMatchSet(2, this.LookAheadToken)) {
				if (!this.MatchExpressionNoFunction(out expression))
					return false;
			}
			else if (this.TokenIs(this.LookAheadToken, LuatTokenId.Function)) {
				if (!this.MatchFunction(out expression))
					return false;
			}
			else
				return false;
			expression.Description = description;
			return true;
		}

		/// <summary>
		/// Matches a <c>Primary</c> non-terminal.
		/// </summary>
		/// <returns><c>true</c> if the <c>Primary</c> was matched successfully; otherwise, <c>false</c>.</returns>
		/// <remarks>
		/// The non-terminal can start with: <c>Nil</c>, <c>False</c>, <c>True</c>, <c>Number</c>, <c>String</c>.
		/// </remarks>
		protected virtual bool MatchPrimary(out Expression expression) {
			expression = null;
			if (this.TokenIs(this.LookAheadToken, LuatTokenId.Nil)) {
				if (!this.Match(LuatTokenId.Nil))
					return false;
				expression = new NilExpression( this.Token.TextRange );
			}
			else if (this.TokenIs(this.LookAheadToken, LuatTokenId.False)) {
				if (!this.Match(LuatTokenId.False))
					return false;
				expression = new BooleanExpression( false, this.Token.TextRange);
			}
			else if (this.TokenIs(this.LookAheadToken, LuatTokenId.True)) {
				if (!this.Match(LuatTokenId.True))
					return false;
				expression = new BooleanExpression( true,  this.Token.TextRange);
			}
			else if (this.TokenIs(this.LookAheadToken, LuatTokenId.Number)) {
				if (!this.Match(LuatTokenId.Number))
					return false;
				expression = new NumberExpression( Convert.ToDouble(this.TokenText), this.Token.TextRange );
			}
			else if (this.TokenIs(this.LookAheadToken, LuatTokenId.String)) {
				if (!this.MatchStringExpression(out expression))
					return false;
			}
			else
				return false;
			return true;
		}

		/// <summary>
		/// Matches a <c>Parenthesis</c> non-terminal.
		/// </summary>
		/// <returns><c>true</c> if the <c>Parenthesis</c> was matched successfully; otherwise, <c>false</c>.</returns>
		/// <remarks>
		/// The non-terminal can start with: <c>OpenParenthesis</c>.
		/// </remarks>
		protected virtual bool MatchParenthesis(out Expression expression) {
			expression = null;
			if (!this.Match(LuatTokenId.OpenParenthesis))
				return false;
			int start = this.Token.StartOffset;
			if (!this.MatchExpression(out expression))
				return false;
			if (!this.Match(LuatTokenId.CloseParenthesis))
				return false;
			TextRange textRange = new TextRange( start, this.Token.EndOffset );
			expression = new ParenthesizedExpression( expression, textRange );
			return true;
		}

		/// <summary>
		/// Matches a <c>Variable</c> non-terminal.
		/// </summary>
		/// <returns><c>true</c> if the <c>Variable</c> was matched successfully; otherwise, <c>false</c>.</returns>
		/// <remarks>
		/// The non-terminal can start with: <c>Identifier</c>.
		/// </remarks>
		protected virtual bool MatchVariable(out Expression expression) {
			expression = null;
			Identifier identifier;
			if (!this.MatchIdentifier(out identifier))
				return false;
			expression = new VariableExpression( identifier );
			return true;
		}

		/// <summary>
		/// Matches a <c>VariableList</c> non-terminal.
		/// </summary>
		/// <returns><c>true</c> if the <c>VariableList</c> was matched successfully; otherwise, <c>false</c>.</returns>
		/// <remarks>
		/// The non-terminal can start with: <c>Identifier</c>.
		/// </remarks>
		protected virtual bool MatchVariableList(IAstNodeList variables) {
			Expression variable;
			if (!this.MatchVariable(out variable))
				return false;
			variables.Add( variable );
			while (this.TokenIs(this.LookAheadToken, LuatTokenId.Comma)) {
				if (!this.Match(LuatTokenId.Comma))
					return false;
				if (!this.MatchVariable(out variable))
					return false;
				variables.Add( variable );
			}
			return true;
		}

		/// <summary>
		/// Matches a <c>Index</c> non-terminal.
		/// </summary>
		/// <returns><c>true</c> if the <c>Index</c> was matched successfully; otherwise, <c>false</c>.</returns>
		/// <remarks>
		/// The non-terminal can start with: <c>Dot</c>, <c>OpenSquareBracket</c>.
		/// </remarks>
		protected virtual bool MatchIndex(ref Expression expression) {
			LuatAstNodeBase index;
			IToken          indexToken;
			int start = expression.StartOffset;
			if (this.TokenIs(this.LookAheadToken, LuatTokenId.OpenSquareBracket)) {
				if (!this.Match(LuatTokenId.OpenSquareBracket))
					return false;
				indexToken = this.Token;
				Expression indexExpression = null;
				if (!this.MatchExpression(out indexExpression))
					return false;
				index = indexExpression;
				if (!this.Match(LuatTokenId.CloseSquareBracket))
					return false;
				expression.EndOffset = this.Token.EndOffset; // Expand to encompass ']'
			}
			else if (this.TokenIs(this.LookAheadToken, LuatTokenId.Dot)) {
				if (!this.Match(LuatTokenId.Dot))
					return false;
				indexToken = this.Token;
				Identifier indexIdentifier = null;
				if (!this.MatchIdentifier(out indexIdentifier)) {
					this.RaiseError( this.Token.EndOffset - 1, "Expected identifier." );
				}
				index = indexIdentifier;
			}
			else
				return false;
			expression = new IndexExpression( expression, indexToken, index, new TextRange( start, this.Token.EndOffset ) );
			return true;
		}

		/// <summary>
		/// Matches a <c>ExpressionList</c> non-terminal.
		/// </summary>
		/// <returns><c>true</c> if the <c>ExpressionList</c> was matched successfully; otherwise, <c>false</c>.</returns>
		/// <remarks>
		/// The non-terminal can start with: <c>Function</c>, <c>OpenParenthesis</c>, <c>OpenCurlyBrace</c>, <c>Identifier</c>, <c>TripleDot</c>, <c>Nil</c>, <c>False</c>, <c>True</c>, <c>Number</c>, <c>Subtraction</c>, <c>Not</c>, <c>Hash</c>, <c>String</c>.
		/// </remarks>
		protected virtual bool MatchExpressionList(IAstNodeList expressions) {
			Expression expression = null;
			if (!this.MatchExpression(out expression))
				return false;
			expressions.Add( expression );
			while (this.TokenIs(this.LookAheadToken, LuatTokenId.Comma)) {
				if (!this.Match(LuatTokenId.Comma))
					return false;
				if (!this.MatchExpression(out expression))
					return false;
				expressions.Add( expression );
			}
			return true;
		}

		/// <summary>
		/// Matches a <c>UnaryOp</c> non-terminal.
		/// </summary>
		/// <returns><c>true</c> if the <c>UnaryOp</c> was matched successfully; otherwise, <c>false</c>.</returns>
		/// <remarks>
		/// The non-terminal can start with: <c>Subtraction</c>, <c>Not</c>, <c>Hash</c>.
		/// </remarks>
		protected virtual bool MatchUnaryOp(out OperatorType operatorType) {
			operatorType = OperatorType.None;
			if (this.TokenIs(this.LookAheadToken, LuatTokenId.Subtraction)) {
				if (!this.Match(LuatTokenId.Subtraction))
					return false;
				else {
					operatorType = OperatorType.Negate;
				}
			}
			else if (this.TokenIs(this.LookAheadToken, LuatTokenId.Not)) {
				if (!this.Match(LuatTokenId.Not))
					return false;
				else {
					operatorType = OperatorType.Not;
				}
			}
			else if (this.TokenIs(this.LookAheadToken, LuatTokenId.Hash)) {
				if (!this.Match(LuatTokenId.Hash))
					return false;
				else {
					operatorType = OperatorType.Length;
				}
			}
			else
				return false;
			return true;
		}

		/// <summary>
		/// Matches a <c>BinaryOp</c> non-terminal.
		/// </summary>
		/// <returns><c>true</c> if the <c>BinaryOp</c> was matched successfully; otherwise, <c>false</c>.</returns>
		/// <remarks>
		/// The non-terminal can start with: <c>Subtraction</c>, <c>Addition</c>, <c>Multiplication</c>, <c>Division</c>, <c>Modulus</c>, <c>Hat</c>, <c>Equality</c>, <c>Inequality</c>, <c>LessThanEqual</c>, <c>GreaterThanEqual</c>, <c>LessThan</c>, <c>GreaterThan</c>, <c>And</c>, <c>Or</c>, <c>DoubleDot</c>.
		/// </remarks>
		protected virtual bool MatchBinaryOp(out OperatorType operatorType) {
			operatorType = OperatorType.None;
			if (this.TokenIs(this.LookAheadToken, LuatTokenId.Addition)) {
				if (!this.Match(LuatTokenId.Addition))
					return false;
				else {
					operatorType = OperatorType.Addition;
				}
			}
			else if (this.TokenIs(this.LookAheadToken, LuatTokenId.Subtraction)) {
				if (!this.Match(LuatTokenId.Subtraction))
					return false;
				else {
					operatorType = OperatorType.Subtraction;
				}
			}
			else if (this.TokenIs(this.LookAheadToken, LuatTokenId.Multiplication)) {
				if (!this.Match(LuatTokenId.Multiplication))
					return false;
				else {
					operatorType = OperatorType.Multiplication;
				}
			}
			else if (this.TokenIs(this.LookAheadToken, LuatTokenId.Division)) {
				if (!this.Match(LuatTokenId.Division))
					return false;
				else {
					operatorType = OperatorType.Division;
				}
			}
			else if (this.TokenIs(this.LookAheadToken, LuatTokenId.Modulus)) {
				if (!this.Match(LuatTokenId.Modulus))
					return false;
				else {
					operatorType = OperatorType.Modulus;
				}
			}
			else if (this.TokenIs(this.LookAheadToken, LuatTokenId.Hat)) {
				if (!this.Match(LuatTokenId.Hat))
					return false;
				else {
					operatorType = OperatorType.Power;
				}
			}
			else if (this.TokenIs(this.LookAheadToken, LuatTokenId.Equality)) {
				if (!this.Match(LuatTokenId.Equality))
					return false;
				else {
					operatorType = OperatorType.Equality;
				}
			}
			else if (this.TokenIs(this.LookAheadToken, LuatTokenId.Inequality)) {
				if (!this.Match(LuatTokenId.Inequality))
					return false;
				else {
					operatorType = OperatorType.Inequality;
				}
			}
			else if (this.TokenIs(this.LookAheadToken, LuatTokenId.LessThanEqual)) {
				if (!this.Match(LuatTokenId.LessThanEqual))
					return false;
				else {
					operatorType = OperatorType.LessThanEqual;
				}
			}
			else if (this.TokenIs(this.LookAheadToken, LuatTokenId.GreaterThanEqual)) {
				if (!this.Match(LuatTokenId.GreaterThanEqual))
					return false;
				else {
					operatorType = OperatorType.GreaterThanEqual;
				}
			}
			else if (this.TokenIs(this.LookAheadToken, LuatTokenId.LessThan)) {
				if (!this.Match(LuatTokenId.LessThan))
					return false;
				else {
					operatorType = OperatorType.LessThan;
				}
			}
			else if (this.TokenIs(this.LookAheadToken, LuatTokenId.GreaterThan)) {
				if (!this.Match(LuatTokenId.GreaterThan))
					return false;
				else {
					operatorType = OperatorType.GreaterThan;
				}
			}
			else if (this.TokenIs(this.LookAheadToken, LuatTokenId.And)) {
				if (!this.Match(LuatTokenId.And))
					return false;
				else {
					operatorType = OperatorType.And;
				}
			}
			else if (this.TokenIs(this.LookAheadToken, LuatTokenId.Or)) {
				if (!this.Match(LuatTokenId.Or))
					return false;
				else {
					operatorType = OperatorType.Or;
				}
			}
			else if (this.TokenIs(this.LookAheadToken, LuatTokenId.DoubleDot)) {
				if (!this.Match(LuatTokenId.DoubleDot))
					return false;
				else {
					operatorType = OperatorType.Concatenate;
				}
			}
			else
				return false;
			return true;
		}

		/// <summary>
		/// Matches a <c>StringExpression</c> non-terminal.
		/// </summary>
		/// <returns><c>true</c> if the <c>StringExpression</c> was matched successfully; otherwise, <c>false</c>.</returns>
		/// <remarks>
		/// The non-terminal can start with: <c>String</c>.
		/// </remarks>
		protected virtual bool MatchStringExpression(out Expression expression) {
			expression = null;
			if (!this.Match(LuatTokenId.String))
				return false;
			string text = this.TokenText;
		if ( text.StartsWith("\"") ) { text = text.Substring( 1 ); }
	if ( text.EndsWith("\"") )   { text = text.Substring( 0, text.Length - 1 ); }
	expression = new StringExpression(text, this.Token.TextRange);
			return true;
		}

		/// <summary>
		/// Gets the multi-match sets array.
		/// </summary>
		/// <value>The multi-match sets array.</value>
		protected override bool[,] MultiMatchSets {
			get {
				return multiMatchSets;
			}
		}

		private const bool Y = true;
		private const bool n = false;
		private static bool[,] multiMatchSets = {
			// 0: Do While Repeat If For Local Function Return Break OpenParenthesis OpenCurlyBrace Identifier TripleDot Nil False True Number Subtraction Not Hash String 
			{n,n,n,n,n,n,n,n,Y,Y,Y,n,n,Y,Y,n,n,n,Y,Y,Y,Y,Y,n,Y,Y,Y,n,Y,Y,n,n,Y,n,n,n,Y,n,n,n,n,Y,n,n,n,n,n,n,n,Y,n,Y,n,n,n,n,n,n,n,n,Y,n,n},
			// 1: Do While Repeat If For Local Function OpenParenthesis OpenCurlyBrace Identifier TripleDot Nil False True Number Subtraction Not Hash String 
			{n,n,n,n,n,n,n,n,Y,Y,Y,n,n,n,Y,n,n,n,Y,Y,Y,Y,Y,n,Y,Y,Y,n,Y,n,n,n,Y,n,n,n,Y,n,n,n,n,Y,n,n,n,n,n,n,n,Y,n,Y,n,n,n,n,n,n,n,n,Y,n,n},
			// 2: OpenParenthesis OpenCurlyBrace Identifier TripleDot Nil False True Number Subtraction Not Hash String 
			{n,n,n,n,n,n,n,n,Y,Y,Y,n,n,n,n,n,n,n,Y,Y,n,n,n,n,n,Y,Y,n,n,n,n,n,n,n,n,n,Y,n,n,n,n,Y,n,n,n,n,n,n,n,Y,n,Y,n,n,n,n,n,n,n,n,Y,n,n},
			// 3: Function OpenParenthesis OpenCurlyBrace Identifier TripleDot Nil False True Number Subtraction Not Hash String 
			{n,n,n,n,n,n,n,n,Y,Y,Y,n,n,n,n,n,n,n,Y,Y,n,Y,n,n,n,Y,Y,n,n,n,n,n,n,n,n,n,Y,n,n,n,n,Y,n,n,n,n,n,n,n,Y,n,Y,n,n,n,n,n,n,n,n,Y,n,n},
			// 4: Function OpenParenthesis OpenCurlyBrace OpenSquareBracket Identifier TripleDot Nil False True Number Subtraction Not Hash String 
			{n,n,n,n,n,n,n,n,Y,Y,Y,n,n,n,n,n,n,n,Y,Y,n,Y,n,n,n,Y,Y,n,n,n,n,n,n,n,n,n,Y,n,n,n,n,Y,n,n,n,n,n,n,n,Y,n,Y,n,Y,n,n,n,n,n,n,Y,n,n},
			// 5: Function OpenParenthesis OpenCurlyBrace Identifier TripleDot Nil False True Number Subtraction Not Hash String 
			{n,n,n,n,n,n,n,n,Y,Y,Y,n,n,n,n,n,n,n,Y,Y,n,Y,n,n,n,Y,Y,n,n,n,n,n,n,n,n,n,Y,n,n,n,n,Y,n,n,n,n,n,n,n,Y,n,Y,n,n,n,n,n,n,n,n,Y,n,n},
			// 6: Nil False True Number String 
			{n,n,n,n,n,n,n,n,Y,n,Y,n,n,n,n,n,n,n,Y,Y,n,n,n,n,n,Y,n,n,n,n,n,n,n,n,n,n,n,n,n,n,n,n,n,n,n,n,n,n,n,n,n,n,n,n,n,n,n,n,n,n,n,n,n},
			// 7: Colon Dot OpenParenthesis OpenCurlyBrace OpenSquareBracket String 
			{n,n,n,n,n,n,n,n,n,n,Y,n,n,n,n,n,n,n,n,n,n,n,n,n,n,n,n,n,n,n,n,n,n,n,n,n,n,n,n,n,n,n,n,n,n,n,n,n,n,Y,n,Y,n,Y,n,n,Y,n,Y,n,n,n,n},
			// 8: Colon OpenParenthesis OpenCurlyBrace String 
			{n,n,n,n,n,n,n,n,n,n,Y,n,n,n,n,n,n,n,n,n,n,n,n,n,n,n,n,n,n,n,n,n,n,n,n,n,n,n,n,n,n,n,n,n,n,n,n,n,n,Y,n,Y,n,n,n,n,Y,n,n,n,n,n,n},
			// 9: Subtraction Addition Multiplication Division Modulus Hat Equality Inequality LessThanEqual GreaterThanEqual LessThan GreaterThan And Or DoubleDot 
			{n,n,n,n,n,n,n,n,n,n,n,n,Y,n,n,n,n,n,n,n,n,n,n,n,n,n,n,Y,n,n,n,n,n,n,n,Y,Y,Y,Y,Y,Y,n,Y,Y,Y,Y,Y,Y,n,n,n,n,n,n,n,n,n,n,n,Y,n,n,n}
		};

	}

	#endregion
}

#region Generated AST Node Classes

namespace Sce.Sled.SyntaxEditor.Intellisense.Lua.Parser.AST
{
	/// <summary>
	/// Specifies the type of an <see cref="AstNode"/>.
	/// </summary>
    enum LuatNodeType
    {
		/// <summary>
		/// A Luat language compilation unit.
		/// </summary>
		CompilationUnit,

		/// <summary>
		/// An incomplete statement.
		/// </summary>
		IncompleteStatement,

		/// <summary>
		/// A return statement.
		/// </summary>
		BreakStatement,

		/// <summary>
		/// An assignment statement.
		/// </summary>
		AssignmentStatement,

		/// <summary>
		/// An statement that is formed from an expression.
		/// </summary>
		ExpressionStatement,

		/// <summary>
		/// A do statement.
		/// </summary>
		DoStatement,

		/// <summary>
		/// A conditional block.
		/// </summary>
		ConditionalBlock,

		/// <summary>
		/// A while statement.
		/// </summary>
		WhileStatement,

		/// <summary>
		/// A repeat statement.
		/// </summary>
		RepeatStatement,

		/// <summary>
		/// An if statement.
		/// </summary>
		IfStatement,

		/// <summary>
		/// An assignment statement.
		/// </summary>
		ForStatement,

		/// <summary>
		/// An assignment statement.
		/// </summary>
		ForInStatement,

		/// <summary>
		/// A block of statements.
		/// </summary>
		BlockStatement,

		/// <summary>
		/// A return statement.
		/// </summary>
		ReturnStatement,

		/// <summary>
		/// An incomplete statement.
		/// </summary>
		IncompleteExpression,

		/// <summary>
		/// A function call.
		/// </summary>
		FunctionCall,

		/// <summary>
		/// A parenthesised list of arguments.
		/// </summary>
		ArgumentList,

		/// <summary>
		/// A unary expression.
		/// </summary>
		UnaryExpression,

		/// <summary>
		/// A binary expression.
		/// </summary>
		BinaryExpression,

		/// <summary>
		/// A function declaration.
		/// </summary>
		Function,

		/// <summary>
		/// Indexer.
		/// </summary>
		IndexExpression,

		/// <summary>
		/// A variable.
		/// </summary>
		VariableExpression,

		/// <summary>
		/// A number expression.
		/// </summary>
		NumberExpression,

		/// <summary>
		/// The value of nil.
		/// </summary>
		NilExpression,

		/// <summary>
		/// A number expression.
		/// </summary>
		BooleanExpression,

		/// <summary>
		/// A string expression.
		/// </summary>
		StringExpression,

		/// <summary>
		/// A parenthesized expression.
		/// </summary>
		ParenthesizedExpression,

		/// <summary>
		/// A table constructor expression.
		/// </summary>
		TableConstructor,

		/// <summary>
		/// An identifier.
		/// </summary>
		Identifier,

		/// <summary>
		/// A field.
		/// </summary>
		Field,

	}

	/// <summary>
	/// Provides the base class for an AST node.
	/// </summary>
	/// <remarks>
	/// <para>
	/// Each AST node has a single parent and optional children.
	/// The nodes may be navigated by parent to child or child to parent.
	/// When a node is created, it initially has no parent node.
	/// </para>
	/// <para>
	/// AST nodes implement the visitor pattern.
	/// </para>
	/// </remarks>
    abstract partial class AstNode : AstNodeBase
    {
		private byte contextID;
		
		/// <summary>
		/// Gets the minimum context ID that should be used in your code for AST nodes inheriting this class.
		/// </summary>
		/// <remarks>
		/// Base all your context ID constants off of this value.
		/// </remarks>
		protected const byte AstNodeContextIDBase = AstNode.ContextIDBase;

		/////////////////////////////////////////////////////////////////////////////////////////////////////
		// OBJECT
		/////////////////////////////////////////////////////////////////////////////////////////////////////
		
		/// <summary>
		/// Initializes a new instance of the <c>AstNode</c> class. 
		/// </summary>
		public AstNode() {}

		/// <summary>
		/// Initializes a new instance of the <c>AstNode</c> class. 
		/// </summary>
		/// <param name="textRange">The <see cref="TextRange"/> of the AST node.</param>
		public AstNode(TextRange textRange) : base(textRange) {}
		
		/////////////////////////////////////////////////////////////////////////////////////////////////////
		// PUBLIC PROCEDURES
		/////////////////////////////////////////////////////////////////////////////////////////////////////
		
		/// <summary>
		/// Gets or sets a context value identifying the context of the AST node within its parent node.
		/// </summary>
		/// <remarks>
		/// The context ID value is typically defined on the parent AST node as a constant.
		/// </remarks>
		public override int ContextID { 
				get {
					return contextID;
				}
				set {
					contextID = (byte)value;
				}
			}

		/// <summary>
		/// Gets the <see cref="LuatNodeType"/> that identifies the type of node.
		/// </summary>
		/// <value>The <see cref="LuatNodeType"/> that identifies the type of node.</value>
		public abstract LuatNodeType NodeType { get; }

		/// <summary>
		/// Gets the image index that is applicable for displaying this node in a user interface control.
		/// </summary>
		/// <value>The image index that is applicable for displaying this node in a user interface control.</value>
		public override int ImageIndex {
			get {
				return (int)ActiproSoftware.Products.SyntaxEditor.IconResource.Keyword;
			}
		}
	}

	/// <summary>
	/// Represents a number expression.
	/// </summary>
    partial class BooleanExpression : LiteralExpression
    {
		private System.Boolean	boolValue;

		/// <summary>
		/// Gets the minimum context ID that should be used in your code for AST nodes inheriting this class.
		/// </summary>
		/// <remarks>
		/// Base all your context ID constants off of this value.
		/// </remarks>
        protected const byte BooleanExpressionContextIDBase = LiteralExpression.LiteralExpressionContextIDBase;

		/////////////////////////////////////////////////////////////////////////////////////////////////////
		// OBJECT
		/////////////////////////////////////////////////////////////////////////////////////////////////////

		public BooleanExpression( bool value, TextRange textRange) : this(textRange) {
			this.BoolValue = value;
		}

		/// <summary>
		/// Initializes a new instance of the <c>BooleanExpression</c> class. 
		/// </summary>
		public BooleanExpression() {}

		/// <summary>
		/// Initializes a new instance of the <c>BooleanExpression</c> class. 
		/// </summary>
		/// <param name="textRange">The <see cref="TextRange"/> of the AST node.</param>
		public BooleanExpression(TextRange textRange) : base(textRange) {}

		/////////////////////////////////////////////////////////////////////////////////////////////////////
		// PUBLIC PROCEDURES
		/////////////////////////////////////////////////////////////////////////////////////////////////////

		/// <summary>
		/// Gets the <see cref="LuatNodeType"/> that identifies the type of node.
		/// </summary>
		/// <value>The <see cref="LuatNodeType"/> that identifies the type of node.</value>
		public override LuatNodeType NodeType { 
			get {
				return LuatNodeType.BooleanExpression;
			}
		}

		/// <summary>
		/// Gets or sets the value.
		/// </summary>
		/// <value>The value.</value>
		public System.Boolean BoolValue {
			get {
				return boolValue;
			}
			set {
				boolValue = value;
			}
		}

		public override string DisplayText {
			get {
				return this.BoolValue.ToString();
			}
		}
	}

	/// <summary>
	/// Represents a function call.
	/// </summary>
    partial class FunctionCall : Expression
    {
		private bool	passesSelf;

		/// <summary>
		/// Gets the context ID for the function owner.
		/// </summary>
        public const byte OwnerContextID = Sce.Sled.SyntaxEditor.Intellisense.Lua.Parser.AST.Expression.ExpressionContextIDBase;

		/// <summary>
		/// Gets the context ID for the function name.
		/// </summary>
        public const byte NameContextID = Sce.Sled.SyntaxEditor.Intellisense.Lua.Parser.AST.Expression.ExpressionContextIDBase + 1;

		/// <summary>
		/// Gets the context ID for the arguments to the call.
		/// </summary>
        public const byte ArgumentsContextID = Sce.Sled.SyntaxEditor.Intellisense.Lua.Parser.AST.Expression.ExpressionContextIDBase + 2;

		/// <summary>
		/// Gets the minimum context ID that should be used in your code for AST nodes inheriting this class.
		/// </summary>
		/// <remarks>
		/// Base all your context ID constants off of this value.
		/// </remarks>
        protected const byte FunctionCallContextIDBase = Sce.Sled.SyntaxEditor.Intellisense.Lua.Parser.AST.Expression.ExpressionContextIDBase + 3;

		/////////////////////////////////////////////////////////////////////////////////////////////////////
		// OBJECT
		/////////////////////////////////////////////////////////////////////////////////////////////////////

		public FunctionCall(Expression owner, Identifier name)
			: this(new TextRange( owner.StartOffset, owner == null ? name.EndOffset : owner.EndOffset ) )
			{
			// Initialize parameters
			this.Owner		= owner;
			this.Name		= name;
		}

		/// <summary>
		/// Initializes a new instance of the <c>FunctionCall</c> class. 
		/// </summary>
		public FunctionCall() {}

		/// <summary>
		/// Initializes a new instance of the <c>FunctionCall</c> class. 
		/// </summary>
		/// <param name="textRange">The <see cref="TextRange"/> of the AST node.</param>
		public FunctionCall(TextRange textRange) : base(textRange) {}

		/////////////////////////////////////////////////////////////////////////////////////////////////////
		// PUBLIC PROCEDURES
		/////////////////////////////////////////////////////////////////////////////////////////////////////

		/// <summary>
		/// Gets the <see cref="LuatNodeType"/> that identifies the type of node.
		/// </summary>
		/// <value>The <see cref="LuatNodeType"/> that identifies the type of node.</value>
		public override LuatNodeType NodeType { 
			get {
				return LuatNodeType.FunctionCall;
			}
		}

		/// <summary>
		/// Gets or sets the function owner.
		/// </summary>
		/// <value>The function owner.</value>
		public Expression Owner {
			get {
				return this.GetChildNode(FunctionCall.OwnerContextID) as Expression;
			}
			set {
				this.ChildNodes.Replace(value, FunctionCall.OwnerContextID);
			}
		}

		/// <summary>
		/// Gets or sets the function name.
		/// </summary>
		/// <value>The function name.</value>
		public Identifier Name {
			get {
				return this.GetChildNode(FunctionCall.NameContextID) as Identifier;
			}
			set {
				this.ChildNodes.Replace(value, FunctionCall.NameContextID);
			}
		}

		/// <summary>
		/// Gets or sets the arguments to the call.
		/// </summary>
		/// <value>The arguments to the call.</value>
		public LuatAstNodeBase Arguments {
			get {
				return this.GetChildNode(FunctionCall.ArgumentsContextID) as LuatAstNodeBase;
			}
			set {
				this.ChildNodes.Replace(value, FunctionCall.ArgumentsContextID);
			}
		}

		/// <summary>
		/// Gets or sets does the call use ':' instead of '.'?
		/// </summary>
		/// <value>Does the call use ':' instead of '.'?</value>
		public bool PassesSelf {
			get {
				return passesSelf;
			}
			set {
				passesSelf = value;
			}
		}

		public override string DisplayText {
			get {
				StringBuilder sb = new StringBuilder();
				
				sb.Append( Owner.DisplayText );
				
				if ( null != Name )
					{
					sb.Append( ":" );
					sb.Append( Name.DisplayText );
				}
				
				if ( null != Arguments )
					{
					sb.Append( Arguments.DisplayText );
				}
				
				return sb.ToString();
			}
		}

	}

	/// <summary>
	/// Represents an incomplete statement.
	/// </summary>
    partial class IncompleteStatement : Statement
    {

		/// <summary>
		/// Gets the context ID for the partial statement.
		/// </summary>
        public const byte BodyContextID = Sce.Sled.SyntaxEditor.Intellisense.Lua.Parser.AST.Statement.StatementContextIDBase;

		/// <summary>
		/// Gets the minimum context ID that should be used in your code for AST nodes inheriting this class.
		/// </summary>
		/// <remarks>
		/// Base all your context ID constants off of this value.
		/// </remarks>
        protected const byte IncompleteStatementContextIDBase = Sce.Sled.SyntaxEditor.Intellisense.Lua.Parser.AST.Statement.StatementContextIDBase + 1;

		/////////////////////////////////////////////////////////////////////////////////////////////////////
		// OBJECT
		/////////////////////////////////////////////////////////////////////////////////////////////////////

		public IncompleteStatement( IAstNode body, TextRange textTange ) : this( textTange )
			{
			if ( null != body )
				{
				body.StartOffset = textTange.StartOffset;
				body.EndOffset   = textTange.EndOffset;
			}
			this.Body = body;
		}

		/// <summary>
		/// Initializes a new instance of the <c>IncompleteStatement</c> class. 
		/// </summary>
		public IncompleteStatement() {}

		/// <summary>
		/// Initializes a new instance of the <c>IncompleteStatement</c> class. 
		/// </summary>
		/// <param name="textRange">The <see cref="TextRange"/> of the AST node.</param>
		public IncompleteStatement(TextRange textRange) : base(textRange) {}

		/////////////////////////////////////////////////////////////////////////////////////////////////////
		// PUBLIC PROCEDURES
		/////////////////////////////////////////////////////////////////////////////////////////////////////

		/// <summary>
		/// Gets the <see cref="LuatNodeType"/> that identifies the type of node.
		/// </summary>
		/// <value>The <see cref="LuatNodeType"/> that identifies the type of node.</value>
		public override LuatNodeType NodeType { 
			get {
				return LuatNodeType.IncompleteStatement;
			}
		}

		/// <summary>
		/// Gets or sets the partial statement.
		/// </summary>
		/// <value>The partial statement.</value>
		public IAstNode Body {
			get {
				return this.GetChildNode(IncompleteStatement.BodyContextID) as IAstNode;
			}
			set {
				this.ChildNodes.Replace(value, IncompleteStatement.BodyContextID);
			}
		}

	}

	/// <summary>
	/// Represents a conditional block.
	/// </summary>
    partial class ConditionalBlock : Statement
    {

		/// <summary>
		/// Gets the context ID for the conditional.
		/// </summary>
        public const byte ConditionalContextID = Sce.Sled.SyntaxEditor.Intellisense.Lua.Parser.AST.Statement.StatementContextIDBase;

		/// <summary>
		/// Gets the context ID for the body.
		/// </summary>
        public const byte BlockContextID = Sce.Sled.SyntaxEditor.Intellisense.Lua.Parser.AST.Statement.StatementContextIDBase + 1;

		/// <summary>
		/// Gets the minimum context ID that should be used in your code for AST nodes inheriting this class.
		/// </summary>
		/// <remarks>
		/// Base all your context ID constants off of this value.
		/// </remarks>
        protected const byte ConditionalBlockContextIDBase = Sce.Sled.SyntaxEditor.Intellisense.Lua.Parser.AST.Statement.StatementContextIDBase + 2;

		/////////////////////////////////////////////////////////////////////////////////////////////////////
		// OBJECT
		/////////////////////////////////////////////////////////////////////////////////////////////////////

		/// <summary>
		/// Initializes a new instance of the <c>ConditionalBlock</c> class. 
		/// </summary>
		public ConditionalBlock() {}

		/// <summary>
		/// Initializes a new instance of the <c>ConditionalBlock</c> class. 
		/// </summary>
		/// <param name="textRange">The <see cref="TextRange"/> of the AST node.</param>
		public ConditionalBlock(TextRange textRange) : base(textRange) {}

		/////////////////////////////////////////////////////////////////////////////////////////////////////
		// PUBLIC PROCEDURES
		/////////////////////////////////////////////////////////////////////////////////////////////////////

		/// <summary>
		/// Gets the <see cref="LuatNodeType"/> that identifies the type of node.
		/// </summary>
		/// <value>The <see cref="LuatNodeType"/> that identifies the type of node.</value>
		public override LuatNodeType NodeType { 
			get {
				return LuatNodeType.ConditionalBlock;
			}
		}

		/// <summary>
		/// Gets or sets the conditional.
		/// </summary>
		/// <value>The conditional.</value>
		public Expression Conditional {
			get {
				return this.GetChildNode(ConditionalBlock.ConditionalContextID) as Expression;
			}
			set {
				this.ChildNodes.Replace(value, ConditionalBlock.ConditionalContextID);
			}
		}

		/// <summary>
		/// Gets or sets the body.
		/// </summary>
		/// <value>The body.</value>
		public BlockStatement Block {
			get {
				return this.GetChildNode(ConditionalBlock.BlockContextID) as BlockStatement;
			}
			set {
				this.ChildNodes.Replace(value, ConditionalBlock.BlockContextID);
			}
		}

	}

	/// <summary>
	/// Represents a while statement.
	/// </summary>
    partial class WhileStatement : ConditionalBlock
    {

		/// <summary>
		/// Gets the minimum context ID that should be used in your code for AST nodes inheriting this class.
		/// </summary>
		/// <remarks>
		/// Base all your context ID constants off of this value.
		/// </remarks>
        protected const byte WhileStatementContextIDBase = Sce.Sled.SyntaxEditor.Intellisense.Lua.Parser.AST.ConditionalBlock.ConditionalBlockContextIDBase;

		/////////////////////////////////////////////////////////////////////////////////////////////////////
		// OBJECT
		/////////////////////////////////////////////////////////////////////////////////////////////////////

		/// <summary>
		/// Initializes a new instance of the <c>WhileStatement</c> class. 
		/// </summary>
		public WhileStatement() {}

		/// <summary>
		/// Initializes a new instance of the <c>WhileStatement</c> class. 
		/// </summary>
		/// <param name="textRange">The <see cref="TextRange"/> of the AST node.</param>
		public WhileStatement(TextRange textRange) : base(textRange) {}

		/////////////////////////////////////////////////////////////////////////////////////////////////////
		// PUBLIC PROCEDURES
		/////////////////////////////////////////////////////////////////////////////////////////////////////

		/// <summary>
		/// Gets the <see cref="LuatNodeType"/> that identifies the type of node.
		/// </summary>
		/// <value>The <see cref="LuatNodeType"/> that identifies the type of node.</value>
		public override LuatNodeType NodeType { 
			get {
				return LuatNodeType.WhileStatement;
			}
		}

		public override string DisplayText {
			get {
				StringBuilder text = new StringBuilder();
				
				text.Append( "while " );
				text.Append( this.Conditional.DisplayText );
				text.Append( " then\n" );
				text.Append( this.Block.DisplayText );
				text.Append( "\nend" );
				
				return text.ToString();
			}
		}

	}

	/// <summary>
	/// Represents the base class for a statement.
	/// </summary>
    abstract partial class Statement : LuatAstNodeBase
    {

		/// <summary>
		/// Gets the minimum context ID that should be used in your code for AST nodes inheriting this class.
		/// </summary>
		/// <remarks>
		/// Base all your context ID constants off of this value.
		/// </remarks>
        protected const byte StatementContextIDBase = Sce.Sled.SyntaxEditor.Intellisense.Lua.Parser.AST.LuatAstNodeBase.LuatAstNodeBaseContextIDBase;

		/////////////////////////////////////////////////////////////////////////////////////////////////////
		// OBJECT
		/////////////////////////////////////////////////////////////////////////////////////////////////////

		/// <summary>
		/// Initializes a new instance of the <c>Statement</c> class. 
		/// </summary>
		public Statement() {}

		/// <summary>
		/// Initializes a new instance of the <c>Statement</c> class. 
		/// </summary>
		/// <param name="textRange">The <see cref="TextRange"/> of the AST node.</param>
		public Statement(TextRange textRange) : base(textRange) {}

		/////////////////////////////////////////////////////////////////////////////////////////////////////
		// PUBLIC PROCEDURES
		/////////////////////////////////////////////////////////////////////////////////////////////////////

	}

	/// <summary>
	/// Represents a parenthesized expression.
	/// </summary>
    partial class ParenthesizedExpression : Expression
    {

		/// <summary>
		/// Gets the context ID for the expression contained by the parenthesis.
		/// </summary>
        public const byte ExpressionContextID = Sce.Sled.SyntaxEditor.Intellisense.Lua.Parser.AST.Expression.ExpressionContextIDBase;

		/// <summary>
		/// Gets the minimum context ID that should be used in your code for AST nodes inheriting this class.
		/// </summary>
		/// <remarks>
		/// Base all your context ID constants off of this value.
		/// </remarks>
        protected const byte ParenthesizedExpressionContextIDBase = Sce.Sled.SyntaxEditor.Intellisense.Lua.Parser.AST.Expression.ExpressionContextIDBase + 1;

		/////////////////////////////////////////////////////////////////////////////////////////////////////
		// OBJECT
		/////////////////////////////////////////////////////////////////////////////////////////////////////

		/// <summary>
		/// Initializes a new instance of the <c>ParenthesizedExpression</c> class.
		/// </summary>
		/// <param name="expression">The <see cref="Expression"/> affected by the checked modifier.</param>
		/// <param name="textRange">The <see cref="TextRange"/> of the AST node.</param>
		public ParenthesizedExpression(Expression expression, TextRange textRange) : this(textRange) {
			// Initialize parameters
			this.Expression = expression;
		}

		/// <summary>
		/// Initializes a new instance of the <c>ParenthesizedExpression</c> class. 
		/// </summary>
		public ParenthesizedExpression() {}

		/// <summary>
		/// Initializes a new instance of the <c>ParenthesizedExpression</c> class. 
		/// </summary>
		/// <param name="textRange">The <see cref="TextRange"/> of the AST node.</param>
		public ParenthesizedExpression(TextRange textRange) : base(textRange) {}

		/////////////////////////////////////////////////////////////////////////////////////////////////////
		// PUBLIC PROCEDURES
		/////////////////////////////////////////////////////////////////////////////////////////////////////

		/// <summary>
		/// Gets the <see cref="LuatNodeType"/> that identifies the type of node.
		/// </summary>
		/// <value>The <see cref="LuatNodeType"/> that identifies the type of node.</value>
		public override LuatNodeType NodeType { 
			get {
				return LuatNodeType.ParenthesizedExpression;
			}
		}

		/// <summary>
		/// Gets or sets the expression contained by the parenthesis.
		/// </summary>
		/// <value>The expression contained by the parenthesis.</value>
		public Expression Expression {
			get {
				return this.GetChildNode(ParenthesizedExpression.ExpressionContextID) as Expression;
			}
			set {
				this.ChildNodes.Replace(value, ParenthesizedExpression.ExpressionContextID);
			}
		}

		/// <summary>
		/// Gets text representing the node that can be used for display, such as in a document outline.
		/// </summary>
		/// <value>Text representing the node that can be used for display, such as in a document outline.</value>
		public override string DisplayText {
			get {
				return "( Parenthesized Expression )";
			}
		}

	}

	/// <summary>
	/// Represents indexer.
	/// </summary>
    partial class IndexExpression : Expression
    {

		private IToken	indexToken;

		/// <summary>
		/// Gets the context ID for the left-hand-side of the index.
		/// </summary>
        public const byte LHSContextID = Sce.Sled.SyntaxEditor.Intellisense.Lua.Parser.AST.Expression.ExpressionContextIDBase;

		/// <summary>
		/// Gets the context ID for the right-hand-side of the index.
		/// </summary>
        public const byte RHSContextID = Sce.Sled.SyntaxEditor.Intellisense.Lua.Parser.AST.Expression.ExpressionContextIDBase + 1;

		/// <summary>
		/// Gets the minimum context ID that should be used in your code for AST nodes inheriting this class.
		/// </summary>
		/// <remarks>
		/// Base all your context ID constants off of this value.
		/// </remarks>
        protected const byte IndexExpressionContextIDBase = Sce.Sled.SyntaxEditor.Intellisense.Lua.Parser.AST.Expression.ExpressionContextIDBase + 2;

		/////////////////////////////////////////////////////////////////////////////////////////////////////
		// OBJECT
		/////////////////////////////////////////////////////////////////////////////////////////////////////

		/// <summary>
		/// Initializes a new instance of the <c>IndexExpression</c> class.
		/// </summary>
		/// <param name="lhs">The LHS expression.</param>
		/// <param name="rhs">The RHS index.</param>
		/// <param name="textRange">The <see cref="TextRange"/> of the AST node.</param>
		public IndexExpression(Expression lhs, IToken indexToken, LuatAstNodeBase rhs) : this(new TextRange( lhs.StartOffset, rhs.EndOffset ) ) {
			// Initialize parameters
			this.LHS        = lhs;
			this.IndexToken = indexToken;
			this.RHS        = rhs;
		}
		
		public IndexExpression(Expression lhs, IToken indexToken, LuatAstNodeBase rhs, TextRange textRange) : this( textRange ) {
			// Initialize parameters
			this.LHS        = lhs;
			this.IndexToken = indexToken;
			this.RHS        = rhs;
		}

		/// <summary>
		/// Initializes a new instance of the <c>IndexExpression</c> class. 
		/// </summary>
		public IndexExpression() {}

		/// <summary>
		/// Initializes a new instance of the <c>IndexExpression</c> class. 
		/// </summary>
		/// <param name="textRange">The <see cref="TextRange"/> of the AST node.</param>
		public IndexExpression(TextRange textRange) : base(textRange) {}

		/////////////////////////////////////////////////////////////////////////////////////////////////////
		// PUBLIC PROCEDURES
		/////////////////////////////////////////////////////////////////////////////////////////////////////

		/// <summary>
		/// Gets the <see cref="LuatNodeType"/> that identifies the type of node.
		/// </summary>
		/// <value>The <see cref="LuatNodeType"/> that identifies the type of node.</value>
		public override LuatNodeType NodeType { 
			get {
				return LuatNodeType.IndexExpression;
			}
		}

		/// <summary>
		/// Gets or sets the left-hand-side of the index.
		/// </summary>
		/// <value>The left-hand-side of the index.</value>
		public Expression LHS {
			get {
				return this.GetChildNode(IndexExpression.LHSContextID) as Expression;
			}
			set {
				this.ChildNodes.Replace(value, IndexExpression.LHSContextID);
			}
		}

		/// <summary>
		/// Gets or sets the token used for indexing.
		/// </summary>
		/// <value>The token used for indexing.</value>
		public IToken IndexToken {
			get {
				return indexToken;
			}
			set {
				indexToken = value;
			}
		}

		/// <summary>
		/// Gets or sets the right-hand-side of the index.
		/// </summary>
		/// <value>The right-hand-side of the index.</value>
		public LuatAstNodeBase RHS {
			get {
				return this.GetChildNode(IndexExpression.RHSContextID) as LuatAstNodeBase;
			}
			set {
				this.ChildNodes.Replace(value, IndexExpression.RHSContextID);
			}
		}

		/// <summary>
		/// Gets text representing the node that can be used for display, such as in a document outline.
		/// </summary>
		/// <value>Text representing the node that can be used for display, such as in a document outline.</value>
		public override string DisplayText {
			get {
				StringBuilder text = new StringBuilder();
				
				text.Append( ( null != LHS ) ? LHS.DisplayText : "?" );
				
				text.Append( "." );
				
				if ( null != RHS )
					{
					text.Append( RHS.DisplayText );
				}
				
				return text.ToString();
			}
		}

	}

	/// <summary>
	/// Represents an if statement.
	/// </summary>
    partial class IfStatement : ConditionalBlock
    {

		/// <summary>
		/// Gets the context ID for the ElseIfs.
		/// </summary>
        public const byte ElseIfContextID = ConditionalBlockContextIDBase;

		/// <summary>
		/// Gets the context ID for the else body.
		/// </summary>
        public const byte ElseContextID = ConditionalBlockContextIDBase + 1;

		/// <summary>
		/// Gets the minimum context ID that should be used in your code for AST nodes inheriting this class.
		/// </summary>
		/// <remarks>
		/// Base all your context ID constants off of this value.
		/// </remarks>
        protected const byte IfStatementContextIDBase = ConditionalBlockContextIDBase + 2;

		/////////////////////////////////////////////////////////////////////////////////////////////////////
		// OBJECT
		/////////////////////////////////////////////////////////////////////////////////////////////////////

		/// <summary>
		/// Initializes a new instance of the <c>IfStatement</c> class. 
		/// </summary>
		public IfStatement() {}

		/// <summary>
		/// Initializes a new instance of the <c>IfStatement</c> class. 
		/// </summary>
		/// <param name="textRange">The <see cref="TextRange"/> of the AST node.</param>
		public IfStatement(TextRange textRange) : base(textRange) {}

		/////////////////////////////////////////////////////////////////////////////////////////////////////
		// PUBLIC PROCEDURES
		/////////////////////////////////////////////////////////////////////////////////////////////////////

		/// <summary>
		/// Gets the <see cref="LuatNodeType"/> that identifies the type of node.
		/// </summary>
		/// <value>The <see cref="LuatNodeType"/> that identifies the type of node.</value>
		public override LuatNodeType NodeType { 
			get {
				return LuatNodeType.IfStatement;
			}
		}

		/// <summary>
		/// Gets the ElseIfs.
		/// </summary>
		/// <value>The collection of statements.</value>
		public IAstNodeList ElseIfs {
			get {
				return new AstNodeListWrapper(this, IfStatement.ElseIfContextID);
			}
		}

		/// <summary>
		/// Gets or sets the else body.
		/// </summary>
		/// <value>The else body.</value>
		public BlockStatement Else {
			get {
				return this.GetChildNode(IfStatement.ElseContextID) as BlockStatement;
			}
			set {
				this.ChildNodes.Replace(value, IfStatement.ElseContextID);
			}
		}

	}

	/// <summary>
	/// Represents the base class for a literal expression.
	/// </summary>
    abstract partial class LiteralExpression : Expression
    {

		/// <summary>
		/// Gets the minimum context ID that should be used in your code for AST nodes inheriting this class.
		/// </summary>
		/// <remarks>
		/// Base all your context ID constants off of this value.
		/// </remarks>
		protected const byte LiteralExpressionContextIDBase = ExpressionContextIDBase;

		/////////////////////////////////////////////////////////////////////////////////////////////////////
		// OBJECT
		/////////////////////////////////////////////////////////////////////////////////////////////////////

		/// <summary>
		/// Initializes a new instance of the <c>LiteralExpression</c> class. 
		/// </summary>
		public LiteralExpression() {}

		/// <summary>
		/// Initializes a new instance of the <c>LiteralExpression</c> class. 
		/// </summary>
		/// <param name="textRange">The <see cref="TextRange"/> of the AST node.</param>
		public LiteralExpression(TextRange textRange) : base(textRange) {}

		/////////////////////////////////////////////////////////////////////////////////////////////////////
		// PUBLIC PROCEDURES
		/////////////////////////////////////////////////////////////////////////////////////////////////////

	}

	/// <summary>
	/// Represents the value of nil.
	/// </summary>
    partial class NilExpression : LiteralExpression
    {

		/// <summary>
		/// Gets the minimum context ID that should be used in your code for AST nodes inheriting this class.
		/// </summary>
		/// <remarks>
		/// Base all your context ID constants off of this value.
		/// </remarks>
		protected const byte NilExpressionContextIDBase = LiteralExpressionContextIDBase;

		/////////////////////////////////////////////////////////////////////////////////////////////////////
		// OBJECT
		/////////////////////////////////////////////////////////////////////////////////////////////////////

		/// <summary>
		/// Initializes a new instance of the <c>NilExpression</c> class. 
		/// </summary>
		public NilExpression() {}

		/// <summary>
		/// Initializes a new instance of the <c>NilExpression</c> class. 
		/// </summary>
		/// <param name="textRange">The <see cref="TextRange"/> of the AST node.</param>
		public NilExpression(TextRange textRange) : base(textRange) {}

		/////////////////////////////////////////////////////////////////////////////////////////////////////
		// PUBLIC PROCEDURES
		/////////////////////////////////////////////////////////////////////////////////////////////////////

		/// <summary>
		/// Gets the <see cref="LuatNodeType"/> that identifies the type of node.
		/// </summary>
		/// <value>The <see cref="LuatNodeType"/> that identifies the type of node.</value>
		public override LuatNodeType NodeType { 
			get {
				return LuatNodeType.NilExpression;
			}
		}

		public override string DisplayText {
			get {
				return "nil";
			}
		}

	}

	/// <summary>
	/// Represents a block of statements.
	/// </summary>
    partial class BlockStatement : Statement
    {

		private IToken	firstUnconsumedToken;

		/// <summary>
		/// Gets the context ID for the collection of statements.
		/// </summary>
		public const byte StatementContextID = StatementContextIDBase;

		/// <summary>
		/// Gets the minimum context ID that should be used in your code for AST nodes inheriting this class.
		/// </summary>
		/// <remarks>
		/// Base all your context ID constants off of this value.
		/// </remarks>
		protected const byte BlockStatementContextIDBase = StatementContextIDBase + 1;

		/////////////////////////////////////////////////////////////////////////////////////////////////////
		// OBJECT
		/////////////////////////////////////////////////////////////////////////////////////////////////////

		/// <summary>
		/// Initializes a new instance of the <c>BlockStatement</c> class. 
		/// </summary>
		public BlockStatement() {}

		/// <summary>
		/// Initializes a new instance of the <c>BlockStatement</c> class. 
		/// </summary>
		/// <param name="textRange">The <see cref="TextRange"/> of the AST node.</param>
		public BlockStatement(TextRange textRange) : base(textRange) {}

		/////////////////////////////////////////////////////////////////////////////////////////////////////
		// PUBLIC PROCEDURES
		/////////////////////////////////////////////////////////////////////////////////////////////////////

		/// <summary>
		/// Gets the image index that is applicable for displaying this node in a user interface control.
		/// </summary>
		/// <value>The image index that is applicable for displaying this node in a user interface control.</value>
		public override int ImageIndex {
			get {
				return (System.Int32)ActiproSoftware.Products.SyntaxEditor.IconResource.Namespace;
			}
		}

		/// <summary>
		/// Gets the <see cref="LuatNodeType"/> that identifies the type of node.
		/// </summary>
		/// <value>The <see cref="LuatNodeType"/> that identifies the type of node.</value>
		public override LuatNodeType NodeType { 
			get {
				return LuatNodeType.BlockStatement;
			}
		}

		/// <summary>
		/// Gets the collection of statements.
		/// </summary>
		/// <value>The collection of statements.</value>
		public IAstNodeList Statements {
			get {
				return new AstNodeListWrapper(this, BlockStatement.StatementContextID);
			}
		}

		/// <summary>
		/// Gets or sets the last consumed token.
		/// </summary>
		/// <value>The last consumed token.</value>
		public IToken FirstUnconsumedToken {
			get {
				return firstUnconsumedToken;
			}
			set {
				firstUnconsumedToken = value;
			}
		}

		/// <summary>
		/// Gets text representing the node that can be used for display, such as in a document outline.
		/// </summary>
		/// <value>Text representing the node that can be used for display, such as in a document outline.</value>
		public override string DisplayText {
			get {
				StringBuilder text = new StringBuilder();
				
				int statementCount = this.Statements.Count;
				for ( int i = 0; i < statementCount; ++i )
					{
					text.Append( this.Statements[i].DisplayText );
					if ( i != statementCount - 1 )
						{
						text.Append( "\n" );
					}
				}
				
				return text.ToString();
			}
		}

	}

	/// <summary>
	/// Represents an incomplete statement.
	/// </summary>
    partial class IncompleteExpression : Expression
    {

		/// <summary>
		/// Gets the context ID for the partial statement.
		/// </summary>
		public const byte BodyContextID = ExpressionContextIDBase;

		/// <summary>
		/// Gets the minimum context ID that should be used in your code for AST nodes inheriting this class.
		/// </summary>
		/// <remarks>
		/// Base all your context ID constants off of this value.
		/// </remarks>
		protected const byte IncompleteExpressionContextIDBase = ExpressionContextIDBase + 1;

		/////////////////////////////////////////////////////////////////////////////////////////////////////
		// OBJECT
		/////////////////////////////////////////////////////////////////////////////////////////////////////

		public IncompleteExpression( IAstNode body, TextRange textTange ) : this( textTange )
			{
			if ( null != body )
				{
				body.StartOffset = textTange.StartOffset;
				body.EndOffset   = textTange.EndOffset;
			}
			this.Body = body;
		}

		/// <summary>
		/// Initializes a new instance of the <c>IncompleteExpression</c> class. 
		/// </summary>
		public IncompleteExpression() {}

		/// <summary>
		/// Initializes a new instance of the <c>IncompleteExpression</c> class. 
		/// </summary>
		/// <param name="textRange">The <see cref="TextRange"/> of the AST node.</param>
		public IncompleteExpression(TextRange textRange) : base(textRange) {}

		/////////////////////////////////////////////////////////////////////////////////////////////////////
		// PUBLIC PROCEDURES
		/////////////////////////////////////////////////////////////////////////////////////////////////////

		/// <summary>
		/// Gets the <see cref="LuatNodeType"/> that identifies the type of node.
		/// </summary>
		/// <value>The <see cref="LuatNodeType"/> that identifies the type of node.</value>
		public override LuatNodeType NodeType { 
			get {
				return LuatNodeType.IncompleteExpression;
			}
		}

		/// <summary>
		/// Gets or sets the partial statement.
		/// </summary>
		/// <value>The partial statement.</value>
		public IAstNode Body {
			get {
				return this.GetChildNode(IncompleteExpression.BodyContextID) as IAstNode;
			}
			set {
				this.ChildNodes.Replace(value, IncompleteExpression.BodyContextID);
			}
		}

	}

	/// <summary>
	/// Represents a repeat statement.
	/// </summary>
    partial class RepeatStatement : ConditionalBlock
    {

		/// <summary>
		/// Gets the minimum context ID that should be used in your code for AST nodes inheriting this class.
		/// </summary>
		/// <remarks>
		/// Base all your context ID constants off of this value.
		/// </remarks>
		protected const byte RepeatStatementContextIDBase = ConditionalBlockContextIDBase;

		/////////////////////////////////////////////////////////////////////////////////////////////////////
		// OBJECT
		/////////////////////////////////////////////////////////////////////////////////////////////////////

		/// <summary>
		/// Initializes a new instance of the <c>RepeatStatement</c> class. 
		/// </summary>
		public RepeatStatement() {}

		/// <summary>
		/// Initializes a new instance of the <c>RepeatStatement</c> class. 
		/// </summary>
		/// <param name="textRange">The <see cref="TextRange"/> of the AST node.</param>
		public RepeatStatement(TextRange textRange) : base(textRange) {}

		/////////////////////////////////////////////////////////////////////////////////////////////////////
		// PUBLIC PROCEDURES
		/////////////////////////////////////////////////////////////////////////////////////////////////////

		/// <summary>
		/// Gets the <see cref="LuatNodeType"/> that identifies the type of node.
		/// </summary>
		/// <value>The <see cref="LuatNodeType"/> that identifies the type of node.</value>
		public override LuatNodeType NodeType { 
			get {
				return LuatNodeType.RepeatStatement;
			}
		}

		public override string DisplayText {
			get {
				StringBuilder text = new StringBuilder();
				
				text.Append( "repeat " );
				text.Append( this.Block.DisplayText );
				text.Append( "\nwhile " );
				text.Append( this.Conditional.DisplayText );
				
				return text.ToString();
			}
		}

	}

	/// <summary>
	/// Represents a do statement.
	/// </summary>
    partial class DoStatement : Statement
    {

		/// <summary>
		/// Gets the context ID for the do body.
		/// </summary>
		public const byte BodyContextID = StatementContextIDBase;

		/// <summary>
		/// Gets the minimum context ID that should be used in your code for AST nodes inheriting this class.
		/// </summary>
		/// <remarks>
		/// Base all your context ID constants off of this value.
		/// </remarks>
		protected const byte DoStatementContextIDBase = StatementContextIDBase + 1;

		/////////////////////////////////////////////////////////////////////////////////////////////////////
		// OBJECT
		/////////////////////////////////////////////////////////////////////////////////////////////////////

		/// <summary>
		/// Initializes a new instance of the <c>DoStatement</c> class. 
		/// </summary>
		public DoStatement() {}

		/// <summary>
		/// Initializes a new instance of the <c>DoStatement</c> class. 
		/// </summary>
		/// <param name="textRange">The <see cref="TextRange"/> of the AST node.</param>
		public DoStatement(TextRange textRange) : base(textRange) {}

		/////////////////////////////////////////////////////////////////////////////////////////////////////
		// PUBLIC PROCEDURES
		/////////////////////////////////////////////////////////////////////////////////////////////////////

		/// <summary>
		/// Gets the <see cref="LuatNodeType"/> that identifies the type of node.
		/// </summary>
		/// <value>The <see cref="LuatNodeType"/> that identifies the type of node.</value>
		public override LuatNodeType NodeType { 
			get {
				return LuatNodeType.DoStatement;
			}
		}

		/// <summary>
		/// Gets or sets the do body.
		/// </summary>
		/// <value>The do body.</value>
		public BlockStatement Body {
			get {
				return this.GetChildNode(DoStatement.BodyContextID) as BlockStatement;
			}
			set {
				this.ChildNodes.Replace(value, DoStatement.BodyContextID);
			}
		}

		public override string DisplayText {
			get {
				StringBuilder text = new StringBuilder();
				
				text.Append( "do " );
				text.Append( this.Body.DisplayText );
				text.Append( "\nend" );
				
				return text.ToString();
			}
		}

	}

	/// <summary>
	/// Represents a table constructor expression.
	/// </summary>
    partial class TableConstructor : Expression
    {

		/// <summary>
		/// Gets the context ID for the initial table values.
		/// </summary>
		public const byte FieldContextID = ExpressionContextIDBase;

		/// <summary>
		/// Gets the minimum context ID that should be used in your code for AST nodes inheriting this class.
		/// </summary>
		/// <remarks>
		/// Base all your context ID constants off of this value.
		/// </remarks>
		protected const byte TableConstructorContextIDBase = ExpressionContextIDBase + 1;

		/////////////////////////////////////////////////////////////////////////////////////////////////////
		// OBJECT
		/////////////////////////////////////////////////////////////////////////////////////////////////////

		/// <summary>
		/// Initializes a new instance of the <c>TableConstructor</c> class. 
		/// </summary>
		public TableConstructor() {}

		/// <summary>
		/// Initializes a new instance of the <c>TableConstructor</c> class. 
		/// </summary>
		/// <param name="textRange">The <see cref="TextRange"/> of the AST node.</param>
		public TableConstructor(TextRange textRange) : base(textRange) {}

		/////////////////////////////////////////////////////////////////////////////////////////////////////
		// PUBLIC PROCEDURES
		/////////////////////////////////////////////////////////////////////////////////////////////////////

		/// <summary>
		/// Gets the <see cref="LuatNodeType"/> that identifies the type of node.
		/// </summary>
		/// <value>The <see cref="LuatNodeType"/> that identifies the type of node.</value>
		public override LuatNodeType NodeType { 
			get {
				return LuatNodeType.TableConstructor;
			}
		}

		/// <summary>
		/// Gets the initial table values.
		/// </summary>
		/// <value>The collection of statements.</value>
		public IAstNodeList Fields {
			get {
				return new AstNodeListWrapper(this, TableConstructor.FieldContextID);
			}
		}

		/// <summary>
		/// Gets text representing the node that can be used for display, such as in a document outline.
		/// </summary>
		/// <value>Text representing the node that can be used for display, such as in a document outline.</value>
		public override string DisplayText {
			get {
				return "Table constructor";
			}
		}

	}

	/// <summary>
	/// Represents a number expression.
	/// </summary>
    partial class NumberExpression : LiteralExpression
    {

		private System.Double	number;

		/// <summary>
		/// Gets the minimum context ID that should be used in your code for AST nodes inheriting this class.
		/// </summary>
		/// <remarks>
		/// Base all your context ID constants off of this value.
		/// </remarks>
		protected const byte NumberExpressionContextIDBase = LiteralExpressionContextIDBase;

		/////////////////////////////////////////////////////////////////////////////////////////////////////
		// OBJECT
		/////////////////////////////////////////////////////////////////////////////////////////////////////

		public NumberExpression(double number, TextRange textRange) : this(textRange) {
			// Initialize parameters
			this.Number = number;
		}

		/// <summary>
		/// Initializes a new instance of the <c>NumberExpression</c> class. 
		/// </summary>
		public NumberExpression() {}

		/// <summary>
		/// Initializes a new instance of the <c>NumberExpression</c> class. 
		/// </summary>
		/// <param name="textRange">The <see cref="TextRange"/> of the AST node.</param>
		public NumberExpression(TextRange textRange) : base(textRange) {}

		/////////////////////////////////////////////////////////////////////////////////////////////////////
		// PUBLIC PROCEDURES
		/////////////////////////////////////////////////////////////////////////////////////////////////////

		/// <summary>
		/// Gets the <see cref="LuatNodeType"/> that identifies the type of node.
		/// </summary>
		/// <value>The <see cref="LuatNodeType"/> that identifies the type of node.</value>
		public override LuatNodeType NodeType { 
			get {
				return LuatNodeType.NumberExpression;
			}
		}

		/// <summary>
		/// Gets or sets the number.
		/// </summary>
		/// <value>The number.</value>
		public System.Double Number {
			get {
				return number;
			}
			set {
				number = value;
			}
		}

		public override string DisplayText {
			get {
				return this.Number.ToString();
			}
		}

	}

	/// <summary>
	/// Represents a unary expression.
	/// </summary>
    partial class UnaryExpression : Expression
    {

		private OperatorType	operatorType;

		/// <summary>
		/// Gets the context ID for the expression affected by the unary operator.
		/// </summary>
		public const byte ExpressionContextID = ExpressionContextIDBase;

		/// <summary>
		/// Gets the minimum context ID that should be used in your code for AST nodes inheriting this class.
		/// </summary>
		/// <remarks>
		/// Base all your context ID constants off of this value.
		/// </remarks>
		protected const byte UnaryExpressionContextIDBase = ExpressionContextIDBase + 1;

		/////////////////////////////////////////////////////////////////////////////////////////////////////
		// OBJECT
		/////////////////////////////////////////////////////////////////////////////////////////////////////

		public UnaryExpression(OperatorType operatorType, Expression expression, TextRange textRange) : this( textRange )
			{
			// Initialize parameters
			this.operatorType	= operatorType;
			this.Expression		= expression;
		}

		/// <summary>
		/// Initializes a new instance of the <c>UnaryExpression</c> class. 
		/// </summary>
		public UnaryExpression() {}

		/// <summary>
		/// Initializes a new instance of the <c>UnaryExpression</c> class. 
		/// </summary>
		/// <param name="textRange">The <see cref="TextRange"/> of the AST node.</param>
		public UnaryExpression(TextRange textRange) : base(textRange) {}

		/////////////////////////////////////////////////////////////////////////////////////////////////////
		// PUBLIC PROCEDURES
		/////////////////////////////////////////////////////////////////////////////////////////////////////

		/// <summary>
		/// Gets the image index that is applicable for displaying this node in a user interface control.
		/// </summary>
		/// <value>The image index that is applicable for displaying this node in a user interface control.</value>
		public override int ImageIndex {
			get {
				return (System.Int32)ActiproSoftware.Products.SyntaxEditor.IconResource.Operator;
			}
		}

		/// <summary>
		/// Gets the <see cref="LuatNodeType"/> that identifies the type of node.
		/// </summary>
		/// <value>The <see cref="LuatNodeType"/> that identifies the type of node.</value>
		public override LuatNodeType NodeType { 
			get {
				return LuatNodeType.UnaryExpression;
			}
		}

		/// <summary>
		/// Gets or sets the expression affected by the unary operator.
		/// </summary>
		/// <value>The expression affected by the unary operator.</value>
		public Expression Expression {
			get {
				return this.GetChildNode(UnaryExpression.ExpressionContextID) as Expression;
			}
			set {
				this.ChildNodes.Replace(value, UnaryExpression.ExpressionContextID);
			}
		}

		/// <summary>
		/// Gets or sets an operator type indicating the unary operator type.
		/// </summary>
		/// <value>An operator type indicating the unary operator type.</value>
		public OperatorType OperatorType {
			get {
				return operatorType;
			}
			set {
				operatorType = value;
			}
		}

		public override string DisplayText {
			get {
				return "Unary Expression " + this.OperatorType;
			}
		}

	}

	/// <summary>
	/// Represents a string expression.
	/// </summary>
    partial class StringExpression : LiteralExpression
    {

		private System.String	@string;

		/// <summary>
		/// Gets the minimum context ID that should be used in your code for AST nodes inheriting this class.
		/// </summary>
		/// <remarks>
		/// Base all your context ID constants off of this value.
		/// </remarks>
		protected const byte StringExpressionContextIDBase = LiteralExpressionContextIDBase;

		/////////////////////////////////////////////////////////////////////////////////////////////////////
		// OBJECT
		/////////////////////////////////////////////////////////////////////////////////////////////////////

		/// <summary>
		/// Initializes a new instance of the <c>StringExpression</c> class.
		/// </summary>
		/// <param name="string">The string.</param>
		/// <param name="textRange">The <see cref="TextRange"/> of the AST node.</param>
		public StringExpression(string str, TextRange textRange) : this(textRange) {
			// Initialize parameters
			this.String = str;
		}

		/// <summary>
		/// Initializes a new instance of the <c>StringExpression</c> class. 
		/// </summary>
		public StringExpression() {}

		/// <summary>
		/// Initializes a new instance of the <c>StringExpression</c> class. 
		/// </summary>
		/// <param name="textRange">The <see cref="TextRange"/> of the AST node.</param>
		public StringExpression(TextRange textRange) : base(textRange) {}

		/////////////////////////////////////////////////////////////////////////////////////////////////////
		// PUBLIC PROCEDURES
		/////////////////////////////////////////////////////////////////////////////////////////////////////

		/// <summary>
		/// Gets the <see cref="LuatNodeType"/> that identifies the type of node.
		/// </summary>
		/// <value>The <see cref="LuatNodeType"/> that identifies the type of node.</value>
		public override LuatNodeType NodeType { 
			get {
				return LuatNodeType.StringExpression;
			}
		}

		/// <summary>
		/// Gets or sets the string.
		/// </summary>
		/// <value>The string.</value>
		public System.String String {
			get {
				return @string;
			}
			set {
				@string = value;
			}
		}

		/// <summary>
		/// Gets text representing the node that can be used for display, such as in a document outline.
		/// </summary>
		/// <value>Text representing the node that can be used for display, such as in a document outline.</value>
		public override string DisplayText {
			get {
				return "\"" + this.String + "\"";
			}
		}

	}

	/// <summary>
	/// Represents a return statement.
	/// </summary>
    partial class BreakStatement : Statement
    {

		/// <summary>
		/// Gets the minimum context ID that should be used in your code for AST nodes inheriting this class.
		/// </summary>
		/// <remarks>
		/// Base all your context ID constants off of this value.
		/// </remarks>
		protected const byte BreakStatementContextIDBase = StatementContextIDBase;

		/////////////////////////////////////////////////////////////////////////////////////////////////////
		// OBJECT
		/////////////////////////////////////////////////////////////////////////////////////////////////////

		/// <summary>
		/// Initializes a new instance of the <c>BreakStatement</c> class. 
		/// </summary>
		public BreakStatement() {}

		/// <summary>
		/// Initializes a new instance of the <c>BreakStatement</c> class. 
		/// </summary>
		/// <param name="textRange">The <see cref="TextRange"/> of the AST node.</param>
		public BreakStatement(TextRange textRange) : base(textRange) {}

		/////////////////////////////////////////////////////////////////////////////////////////////////////
		// PUBLIC PROCEDURES
		/////////////////////////////////////////////////////////////////////////////////////////////////////

		/// <summary>
		/// Gets the <see cref="LuatNodeType"/> that identifies the type of node.
		/// </summary>
		/// <value>The <see cref="LuatNodeType"/> that identifies the type of node.</value>
		public override LuatNodeType NodeType { 
			get {
				return LuatNodeType.BreakStatement;
			}
		}

	}

	/// <summary>
	/// Represents a binary expression.
	/// </summary>
    partial class BinaryExpression : Expression
    {

		private OperatorType	operatorType;

		/// <summary>
		/// Gets the context ID for the left expression affected by the binary operator.
		/// </summary>
		public const byte LeftExpressionContextID = ExpressionContextIDBase;

		/// <summary>
		/// Gets the context ID for the right expression affected by the binary operator.
		/// </summary>
		public const byte RightExpressionContextID = ExpressionContextIDBase + 1;

		/// <summary>
		/// Gets the minimum context ID that should be used in your code for AST nodes inheriting this class.
		/// </summary>
		/// <remarks>
		/// Base all your context ID constants off of this value.
		/// </remarks>
		protected const byte BinaryExpressionContextIDBase = ExpressionContextIDBase + 2;

		/////////////////////////////////////////////////////////////////////////////////////////////////////
		// OBJECT
		/////////////////////////////////////////////////////////////////////////////////////////////////////

		public BinaryExpression(OperatorType operatorType, Expression lhs, Expression rhs) : this( new TextRange( lhs.StartOffset, rhs.EndOffset ) )
			{
			// Initialize parameters
			this.operatorType		= operatorType;
			this.LeftExpression		= lhs;
			this.RightExpression	= rhs;
		}

		/// <summary>
		/// Initializes a new instance of the <c>BinaryExpression</c> class. 
		/// </summary>
		public BinaryExpression() {}

		/// <summary>
		/// Initializes a new instance of the <c>BinaryExpression</c> class. 
		/// </summary>
		/// <param name="textRange">The <see cref="TextRange"/> of the AST node.</param>
		public BinaryExpression(TextRange textRange) : base(textRange) {}

		/////////////////////////////////////////////////////////////////////////////////////////////////////
		// PUBLIC PROCEDURES
		/////////////////////////////////////////////////////////////////////////////////////////////////////

		/// <summary>
		/// Gets the image index that is applicable for displaying this node in a user interface control.
		/// </summary>
		/// <value>The image index that is applicable for displaying this node in a user interface control.</value>
		public override int ImageIndex {
			get {
				return (System.Int32)ActiproSoftware.Products.SyntaxEditor.IconResource.Operator;
			}
		}

		/// <summary>
		/// Gets the <see cref="LuatNodeType"/> that identifies the type of node.
		/// </summary>
		/// <value>The <see cref="LuatNodeType"/> that identifies the type of node.</value>
		public override LuatNodeType NodeType { 
			get {
				return LuatNodeType.BinaryExpression;
			}
		}

		/// <summary>
		/// Resolves the expression.  Since the type of the LHS of a binary expression 
		/// must match the type of the RHS, resolve the RHS and return that type.
		/// </summary>
		/// <param name="script"></param>
		/// <returns></returns>
		public override LuatValue Resolve(LuatScript script)
		{
			LuatValue value = base.Resolve(script);
			if (null != value)
			{
				return value;
			}

			if (null != RightExpression)
			{
				value = RightExpression.Resolve(script);
				if (null != value)
				{
					this.ResolvedValues[script] = value;
				}
			}
			return value;
		}

		/// <summary>
		/// Gets or sets the left expression affected by the binary operator.
		/// </summary>
		/// <value>The left expression affected by the binary operator.</value>
		public Expression LeftExpression {
			get {
				return this.GetChildNode(BinaryExpression.LeftExpressionContextID) as Expression;
			}
			set {
				this.ChildNodes.Replace(value, BinaryExpression.LeftExpressionContextID);
			}
		}

		/// <summary>
		/// Gets or sets the right expression affected by the binary operator.
		/// </summary>
		/// <value>The right expression affected by the binary operator.</value>
		public Expression RightExpression {
			get {
				return this.GetChildNode(BinaryExpression.RightExpressionContextID) as Expression;
			}
			set {
				this.ChildNodes.Replace(value, BinaryExpression.RightExpressionContextID);
			}
		}

		/// <summary>
		/// Gets or sets an operator type indicating the binary operator type.
		/// </summary>
		/// <value>An operator type indicating the binary operator type.</value>
		public OperatorType OperatorType {
			get {
				return operatorType;
			}
			set {
				operatorType = value;
			}
		}

		public override string DisplayText {
			get {
				return "Binary Expression " + this.OperatorType;
			}
		}

	}

	/// <summary>
	/// Represents a parenthesised list of arguments.
	/// </summary>
    partial class ArgumentList : LuatAstNodeBase
    {

		private TextRange	listTextRange;
		private bool	isClosed;

		/// <summary>
		/// Gets the context ID for the list of arguments.
		/// </summary>
		public const byte ArgumentContextID = LuatAstNodeBaseContextIDBase;

		/// <summary>
		/// Gets the minimum context ID that should be used in your code for AST nodes inheriting this class.
		/// </summary>
		/// <remarks>
		/// Base all your context ID constants off of this value.
		/// </remarks>
		protected const byte ArgumentListContextIDBase = LuatAstNodeBaseContextIDBase + 1;

		/////////////////////////////////////////////////////////////////////////////////////////////////////
		// OBJECT
		/////////////////////////////////////////////////////////////////////////////////////////////////////

		/// <summary>
		/// Initializes a new instance of the <c>ArgumentList</c> class. 
		/// </summary>
		public ArgumentList() {}

		/// <summary>
		/// Initializes a new instance of the <c>ArgumentList</c> class. 
		/// </summary>
		/// <param name="textRange">The <see cref="TextRange"/> of the AST node.</param>
		public ArgumentList(TextRange textRange) : base(textRange) {}

		/////////////////////////////////////////////////////////////////////////////////////////////////////
		// PUBLIC PROCEDURES
		/////////////////////////////////////////////////////////////////////////////////////////////////////

		/// <summary>
		/// Gets the <see cref="LuatNodeType"/> that identifies the type of node.
		/// </summary>
		/// <value>The <see cref="LuatNodeType"/> that identifies the type of node.</value>
		public override LuatNodeType NodeType { 
			get {
				return LuatNodeType.ArgumentList;
			}
		}

		/// <summary>
		/// Gets the list of arguments.
		/// </summary>
		/// <value>The collection of statements.</value>
		public IAstNodeList Arguments {
			get {
				return new AstNodeListWrapper(this, ArgumentList.ArgumentContextID);
			}
		}

		/// <summary>
		/// Gets or sets the text range of the list of arguments.
		/// </summary>
		/// <value>The text range of the list of arguments.</value>
		public TextRange ListTextRange {
			get {
				return listTextRange;
			}
			set {
				listTextRange = value;
			}
		}

		/// <summary>
		/// Gets or sets is this argument list terminated with a closing bracket?
		/// </summary>
		/// <value>Is this argument list terminated with a closing bracket?</value>
		public bool IsClosed {
			get {
				return isClosed;
			}
			set {
				isClosed = value;
			}
		}

		public override string DisplayText {
			get {
				StringBuilder sb = new StringBuilder();
				
				sb.Append( "( " );
				sb.Append( Arguments.ToArray().ToCommaSeperatedList( a => a.DisplayText ) );
				sb.Append( " )" );
				
				return sb.ToString();
			}
		}
		
		public bool InsideBrackets( int offset )
			{
		if( offset <= this.StartOffset ) { return false; }
	if( false  == IsClosed )         { return true;  }
if( offset >= this.EndOffset )   { return false; }
return true;
}

	}

	/// <summary>
	/// Represents a Luat language compilation unit.
	/// </summary>
	partial class CompilationUnit : LuatAstNodeBase, ICompilationUnit, ISemanticParseData {

		private string	sourceKey;
		private string	source;

		private bool						hasLanguageTransitions;
		private ArrayList					syntaxErrors;

		/// <summary>
		/// Gets the context ID for the body.
		/// </summary>
		public const byte BlockContextID = LuatAstNodeBaseContextIDBase;

		/// <summary>
		/// Gets the minimum context ID that should be used in your code for AST nodes inheriting this class.
		/// </summary>
		/// <remarks>
		/// Base all your context ID constants off of this value.
		/// </remarks>
		protected const byte CompilationUnitContextIDBase = LuatAstNodeBaseContextIDBase + 1;

		/////////////////////////////////////////////////////////////////////////////////////////////////////
		// OBJECT
		/////////////////////////////////////////////////////////////////////////////////////////////////////

		/// <summary>
		/// Initializes a new instance of the <c>CompilationUnit</c> class. 
		/// </summary>
		public CompilationUnit() {}

		/// <summary>
		/// Initializes a new instance of the <c>CompilationUnit</c> class. 
		/// </summary>
		/// <param name="textRange">The <see cref="TextRange"/> of the AST node.</param>
		public CompilationUnit(TextRange textRange) : base(textRange) {}

		/////////////////////////////////////////////////////////////////////////////////////////////////////
		// PUBLIC PROCEDURES
		/////////////////////////////////////////////////////////////////////////////////////////////////////

		/// <summary>
		/// Gets the <see cref="LuatNodeType"/> that identifies the type of node.
		/// </summary>
		/// <value>The <see cref="LuatNodeType"/> that identifies the type of node.</value>
		public override LuatNodeType NodeType { 
			get {
				return LuatNodeType.CompilationUnit;
			}
		}

		/// <summary>
		/// Gets or sets the body.
		/// </summary>
		/// <value>The body.</value>
		public BlockStatement Block {
			get {
				return this.GetChildNode(CompilationUnit.BlockContextID) as BlockStatement;
			}
			set {
				this.ChildNodes.Replace(value, CompilationUnit.BlockContextID);
			}
		}

		/// <summary>
		/// Gets or sets path to the lua file
		/// </summary>
		/// <value>Path to the lua file</value>
		public string SourceKey {
			get {
				return sourceKey;
			}
			set {
				sourceKey = value;
			}
		}

		/// <summary>
		/// Gets or sets the lua source
		/// </summary>
		/// <value>The lua source</value>
		public string Source {
			get {
				return source;
			}
			set {
				source = value;
			}
		}

		/// <summary>
		/// Returns whether an <see cref="CollapsibleNodeOutliningParser"/> should visit the child nodes of the specified <see cref="IAstNode"/>
		/// to look for collapsible nodes.
		/// </summary>
		/// <param name="node">The <see cref="IAstNode"/> to examine.</param>
		/// <returns>
		/// <c>true</c> if the child nodes should be visited; otherwise, <c>false</c>.
		/// </returns>
		bool ICompilationUnit.ShouldVisitChildNodesForOutlining(IAstNode node) {
			return true;
		}
		
		/// <summary>
		/// Adds any extra <see cref="CollapsibleNodeOutliningParserData"/> nodes to the <see cref="CollapsibleNodeOutliningParser"/>,
		/// such as for comments that should be marked as collapsible.
		/// </summary>
		/// <param name="outliningParser">The <see cref="CollapsibleNodeOutliningParser"/> to update.</param>
		void ICompilationUnit.UpdateOutliningParser(CollapsibleNodeOutliningParser outliningParser) {
		}
		
		/// <summary>
		/// Gets whether the compilation unit contains errors.
		/// </summary>
		/// <value>
		/// <c>true</c> if the compilation unit contains errors.
		/// </value>
		public bool HasErrors {
			get {
				return ((syntaxErrors != null) && (syntaxErrors.Count > 0));
			}
		}
		
		/// <summary>
		/// Gets or sets whether the compilation unit contains any language transitions.
		/// </summary>
		/// <value>
		/// <c>true</c> if the compilation unit contains any language transitions; otherwise, <c>false</c>.
		/// </value>
		public bool HasLanguageTransitions {
			get {
				return hasLanguageTransitions;
			}
			set {
				hasLanguageTransitions = value;
			}
		}
		
		/// <summary>
		/// Gets whether the AST node is a language root node.
		/// </summary>
		/// <value>
		/// <c>true</c> if the AST node is a language root node; otherwise, <c>false</c>.
		/// </value>
		/// <remarks>
		/// When in a scenario where AST node trees from multiple languages have been merged together,
		/// it is useful to identify where child language AST node trees begin within their parents.
		/// </remarks>
		public override bool IsLanguageRoot {
			get {
				return true;
			}
		}
		
		/// <summary>
		/// Gets the collection of syntax errors that were found in the compilation unit.
		/// </summary>
		/// <value>The collection of syntax errors that were found in the compilation unit.</value>
		public IList SyntaxErrors {
			get {
				if (syntaxErrors == null)
					syntaxErrors = new ArrayList();
				
				return syntaxErrors;
			}
		}

	}

	/// <summary>
	/// Represents a variable.
	/// </summary>
    partial class VariableExpression : Expression
    {

		/// <summary>
		/// Gets the context ID for the name of the variable.
		/// </summary>
		public const byte NameContextID = ExpressionContextIDBase;

		/// <summary>
		/// Gets the minimum context ID that should be used in your code for AST nodes inheriting this class.
		/// </summary>
		/// <remarks>
		/// Base all your context ID constants off of this value.
		/// </remarks>
		protected const byte VariableExpressionContextIDBase = ExpressionContextIDBase + 1;

		/////////////////////////////////////////////////////////////////////////////////////////////////////
		// OBJECT
		/////////////////////////////////////////////////////////////////////////////////////////////////////

		/// <summary>
		/// Initializes a new instance of the <c>VariableExpression</c> class.
		/// </summary>
		/// <param name="name">The name of the variable.</param>
		/// <param name="textRange">The <see cref="TextRange"/> of the AST node.</param>
		public VariableExpression(Identifier name) : this(name.TextRange) {
			// Initialize parameters
			this.Name = name;
		}

		/// <summary>
		/// Initializes a new instance of the <c>VariableExpression</c> class. 
		/// </summary>
		public VariableExpression() {}

		/// <summary>
		/// Initializes a new instance of the <c>VariableExpression</c> class. 
		/// </summary>
		/// <param name="textRange">The <see cref="TextRange"/> of the AST node.</param>
		public VariableExpression(TextRange textRange) : base(textRange) {}

		/////////////////////////////////////////////////////////////////////////////////////////////////////
		// PUBLIC PROCEDURES
		/////////////////////////////////////////////////////////////////////////////////////////////////////

		/// <summary>
		/// Gets the <see cref="LuatNodeType"/> that identifies the type of node.
		/// </summary>
		/// <value>The <see cref="LuatNodeType"/> that identifies the type of node.</value>
		public override LuatNodeType NodeType { 
			get {
				return LuatNodeType.VariableExpression;
			}
		}

		/// <summary>
		/// Gets or sets the name of the variable.
		/// </summary>
		/// <value>The name of the variable.</value>
		public Identifier Name {
			get {
				return this.GetChildNode(VariableExpression.NameContextID) as Identifier;
			}
			set {
				this.ChildNodes.Replace(value, VariableExpression.NameContextID);
			}
		}

		/// <summary>
		/// Gets text representing the node that can be used for display, such as in a document outline.
		/// </summary>
		/// <value>Text representing the node that can be used for display, such as in a document outline.</value>
		public override string DisplayText {
			get {
				return this.Name.Text;
			}
		}

	}

	/// <summary>
	/// Represents a function declaration.
	/// </summary>
    partial class Function : Expression, ICollapsibleNode
    {

		private bool	expectsSelf;

		/// <summary>
		/// Gets the context ID for the block statement.
		/// </summary>
		public const byte BlockContextID = ExpressionContextIDBase;

		/// <summary>
		/// Gets the context ID for the collection of parameters.
		/// </summary>
		public const byte ParameterContextID = ExpressionContextIDBase + 1;

		/// <summary>
		/// Gets the minimum context ID that should be used in your code for AST nodes inheriting this class.
		/// </summary>
		/// <remarks>
		/// Base all your context ID constants off of this value.
		/// </remarks>
		protected const byte FunctionContextIDBase = ExpressionContextIDBase + 2;

		/////////////////////////////////////////////////////////////////////////////////////////////////////
		// OBJECT
		/////////////////////////////////////////////////////////////////////////////////////////////////////

		/// <summary>
		/// Initializes a new instance of the <c>Function</c> class. 
		/// </summary>
		public Function() {}

		/// <summary>
		/// Initializes a new instance of the <c>Function</c> class. 
		/// </summary>
		/// <param name="textRange">The <see cref="TextRange"/> of the AST node.</param>
		public Function(TextRange textRange) : base(textRange) {}

		/////////////////////////////////////////////////////////////////////////////////////////////////////
		// PUBLIC PROCEDURES
		/////////////////////////////////////////////////////////////////////////////////////////////////////

		/// <summary>
		/// Gets the image index that is applicable for displaying this node in a user interface control.
		/// </summary>
		/// <value>The image index that is applicable for displaying this node in a user interface control.</value>
		public override int ImageIndex {
			get {
				return (System.Int32)ActiproSoftware.Products.SyntaxEditor.IconResource.PublicMethod;
			}
		}

		/// <summary>
		/// Gets the <see cref="LuatNodeType"/> that identifies the type of node.
		/// </summary>
		/// <value>The <see cref="LuatNodeType"/> that identifies the type of node.</value>
		public override LuatNodeType NodeType { 
			get {
				return LuatNodeType.Function;
			}
		}

		/// <summary>
		/// Gets or sets the block statement.
		/// </summary>
		/// <value>The block statement.</value>
		public BlockStatement Block {
			get {
				return this.GetChildNode(Function.BlockContextID) as BlockStatement;
			}
			set {
				this.ChildNodes.Replace(value, Function.BlockContextID);
			}
		}

		/// <summary>
		/// Gets the collection of parameters.
		/// </summary>
		/// <value>The collection of statements.</value>
		public IAstNodeList Parameters {
			get {
				return new AstNodeListWrapper(this, Function.ParameterContextID);
			}
		}

		/// <summary>
		/// Gets or sets does function expect the caller to use ':' instead of '.'?
		/// </summary>
		/// <value>Does function expect the caller to use ':' instead of '.'?</value>
		public bool ExpectsSelf {
			get {
				return expectsSelf;
			}
			set {
				expectsSelf = value;
			}
		}

		public int CollapsibleStartOffset = -1;
		public int CollapsibleEndOffset = -1;
		
		/// <summary>
		/// Gets whether the node is collapsible.
		/// </summary>
		/// <value>
		/// <c>true</c> if the node is collapsible; otherwise, <c>false</c>.
		/// </value>
		bool ICollapsibleNode.IsCollapsible {
			get {
				return (CollapsibleStartOffset != -1) &&
				(CollapsibleEndOffset   != -1);
			}
		}
		
		/// <summary>
		/// Gets the offset at which the outlining node starts.
		/// </summary>
		/// <value>The offset at which the outlining node starts.</value>
		int ICollapsibleNode.StartOffset {
			get {
				return CollapsibleStartOffset;
			}
		}
		
		/// <summary>
		/// Gets the offset at which the outlining node ends.
		/// </summary>
		/// <value>The offset at which the outlining node ends.</value>
		int ICollapsibleNode.EndOffset {
			get {
				return CollapsibleEndOffset;
			}
		}
		
		/// <summary>
		/// Gets whether the outlining indicator should be visible for the node.
		/// </summary>
		/// <value>
		/// <c>true</c> if the outlining indicator should be visible for the node; otherwise, <c>false</c>.
		/// </value>
		bool IOutliningNodeParseData.IndicatorVisible {
			get {
				return true;
			}
		}
		
		/// <summary>
		/// Gets whether the outlining node is for a language transition.
		/// </summary>
		/// <value>
		/// <c>true</c> if the outlining node is for a language transition; otherwise, <c>false</c>.
		/// </value>
		bool IOutliningNodeParseData.IsLanguageTransition {
			get {
				return false;
			}
		}
		
		/// <summary>
		/// Gets the character offset at which to navigate when the editor's caret should jump to the text representation of the AST node.
		/// </summary>
		/// <value>The character offset at which to navigate when the editor's caret should jump to the text representation of the AST node.</value>
		public override int NavigationOffset {
			get {
				return base.NavigationOffset;
			}
		}

	}

	/// <summary>
	/// Represents a field.
	/// </summary>
    partial class Field : LuatAstNodeBase
    {

		/// <summary>
		/// Gets the context ID for the key.
		/// </summary>
		public const byte KeyContextID = LuatAstNodeBaseContextIDBase;

		/// <summary>
		/// Gets the context ID for the value.
		/// </summary>
		public const byte ValueContextID = LuatAstNodeBaseContextIDBase + 1;

		/// <summary>
		/// Gets the minimum context ID that should be used in your code for AST nodes inheriting this class.
		/// </summary>
		/// <remarks>
		/// Base all your context ID constants off of this value.
		/// </remarks>
		protected const byte FieldContextIDBase = LuatAstNodeBaseContextIDBase + 2;

		/////////////////////////////////////////////////////////////////////////////////////////////////////
		// OBJECT
		/////////////////////////////////////////////////////////////////////////////////////////////////////

		public Field(IAstNode key, Expression value, TextRange textRange) : this(textRange) {
			this.Key   = key;
			this.Value = value;
		}

		/// <summary>
		/// Initializes a new instance of the <c>Field</c> class. 
		/// </summary>
		public Field() {}

		/// <summary>
		/// Initializes a new instance of the <c>Field</c> class. 
		/// </summary>
		/// <param name="textRange">The <see cref="TextRange"/> of the AST node.</param>
		public Field(TextRange textRange) : base(textRange) {}

		/////////////////////////////////////////////////////////////////////////////////////////////////////
		// PUBLIC PROCEDURES
		/////////////////////////////////////////////////////////////////////////////////////////////////////

		/// <summary>
		/// Gets the <see cref="LuatNodeType"/> that identifies the type of node.
		/// </summary>
		/// <value>The <see cref="LuatNodeType"/> that identifies the type of node.</value>
		public override LuatNodeType NodeType { 
			get {
				return LuatNodeType.Field;
			}
		}

		/// <summary>
		/// Gets or sets the key.
		/// </summary>
		/// <value>The key.</value>
		public new IAstNode Key {
			get {
				return this.GetChildNode(Field.KeyContextID) as IAstNode;
			}
			set {
				this.ChildNodes.Replace(value, Field.KeyContextID);
			}
		}

		/// <summary>
		/// Gets or sets the value.
		/// </summary>
		/// <value>The value.</value>
		public Expression Value {
			get {
				return this.GetChildNode(Field.ValueContextID) as Expression;
			}
			set {
				this.ChildNodes.Replace(value, Field.ValueContextID);
			}
		}

	}

	/// <summary>
	/// Represents an assignment statement.
	/// </summary>
    partial class AssignmentStatement : Statement
    {

		private bool	isLocal;

		/// <summary>
		/// Gets the context ID for the variables.
		/// </summary>
		public const byte VariableContextID = StatementContextIDBase;

		/// <summary>
		/// Gets the context ID for the values.
		/// </summary>
		public const byte ValueContextID = StatementContextIDBase + 1;

		/// <summary>
		/// Gets the minimum context ID that should be used in your code for AST nodes inheriting this class.
		/// </summary>
		/// <remarks>
		/// Base all your context ID constants off of this value.
		/// </remarks>
		protected const byte AssignmentStatementContextIDBase = StatementContextIDBase + 2;

		/////////////////////////////////////////////////////////////////////////////////////////////////////
		// OBJECT
		/////////////////////////////////////////////////////////////////////////////////////////////////////

		/// <summary>
		/// Initializes a new instance of the <c>AssignmentStatement</c> class. 
		/// </summary>
		public AssignmentStatement() {}

		/// <summary>
		/// Initializes a new instance of the <c>AssignmentStatement</c> class. 
		/// </summary>
		/// <param name="textRange">The <see cref="TextRange"/> of the AST node.</param>
		public AssignmentStatement(TextRange textRange) : base(textRange) {}

		/////////////////////////////////////////////////////////////////////////////////////////////////////
		// PUBLIC PROCEDURES
		/////////////////////////////////////////////////////////////////////////////////////////////////////

		/// <summary>
		/// Gets the <see cref="LuatNodeType"/> that identifies the type of node.
		/// </summary>
		/// <value>The <see cref="LuatNodeType"/> that identifies the type of node.</value>
		public override LuatNodeType NodeType { 
			get {
				return LuatNodeType.AssignmentStatement;
			}
		}

		/// <summary>
		/// Gets the variables.
		/// </summary>
		/// <value>The collection of statements.</value>
		public IAstNodeList Variables {
			get {
				return new AstNodeListWrapper(this, AssignmentStatement.VariableContextID);
			}
		}

		/// <summary>
		/// Gets the values.
		/// </summary>
		/// <value>The collection of statements.</value>
		public IAstNodeList Values {
			get {
				return new AstNodeListWrapper(this, AssignmentStatement.ValueContextID);
			}
		}

		/// <summary>
		/// Gets or sets is the assignment a local declaration?
		/// </summary>
		/// <value>Is the assignment a local declaration?</value>
		public bool IsLocal {
			get {
				return isLocal;
			}
			set {
				isLocal = value;
			}
		}

		/// <summary>
		/// Gets text representing the node that can be used for display, such as in a document outline.
		/// </summary>
		/// <value>Text representing the node that can be used for display, such as in a document outline.</value>
		public override string DisplayText {
			get {
				StringBuilder text = new StringBuilder();
				
				int variableCount = this.Variables.Count;
				for ( int i = 0; i < variableCount; ++i )
					{
				if ( i > 0 ) { text.Append( ", " ); }
				text.Append( this.Variables[i].DisplayText );
				text.Append( " = " );
				text.Append( (i < this.Values.Count) ? this.Values[i].DisplayText : "nil" );
			}
			
			return text.ToString();
		}
	}

	}

	/// <summary>
	/// Represents an identifier.
	/// </summary>
    partial class Identifier : LuatAstNodeBase
    {

		private System.String	text;

		/// <summary>
		/// Gets the minimum context ID that should be used in your code for AST nodes inheriting this class.
		/// </summary>
		/// <remarks>
		/// Base all your context ID constants off of this value.
		/// </remarks>
		protected const byte IdentifierContextIDBase = LuatAstNodeBaseContextIDBase;

		/////////////////////////////////////////////////////////////////////////////////////////////////////
		// OBJECT
		/////////////////////////////////////////////////////////////////////////////////////////////////////

		/// <summary>
		/// Initializes a new instance of the <c>Identifier</c> class.
		/// </summary>
		/// <param name="text">The text of the qualified identifier.</param>
		/// <param name="textRange">The <see cref="TextRange"/> of the AST node.</param>
		public Identifier(string text, TextRange textRange) : this(textRange) {
			// Initialize parameters
			this.text = text;
		}

		/// <summary>
		/// Initializes a new instance of the <c>Identifier</c> class. 
		/// </summary>
		public Identifier() {}

		/// <summary>
		/// Initializes a new instance of the <c>Identifier</c> class. 
		/// </summary>
		/// <param name="textRange">The <see cref="TextRange"/> of the AST node.</param>
		public Identifier(TextRange textRange) : base(textRange) {}

		/////////////////////////////////////////////////////////////////////////////////////////////////////
		// PUBLIC PROCEDURES
		/////////////////////////////////////////////////////////////////////////////////////////////////////

		/// <summary>
		/// Gets the <see cref="LuatNodeType"/> that identifies the type of node.
		/// </summary>
		/// <value>The <see cref="LuatNodeType"/> that identifies the type of node.</value>
		public override LuatNodeType NodeType { 
			get {
				return LuatNodeType.Identifier;
			}
		}

		/// <summary>
		/// Gets or sets the text of the qualified identifier.
		/// </summary>
		/// <value>The text of the qualified identifier.</value>
		public System.String Text {
			get {
				return text;
			}
			set {
				text = value;
			}
		}

		/// <summary>
		/// Gets text representing the node that can be used for display, such as in a document outline.
		/// </summary>
		/// <value>Text representing the node that can be used for display, such as in a document outline.</value>
		public override string DisplayText {
			get {
				return text;
			}
		}

	}

	/// <summary>
	/// Represents the base class for an expression.
	/// </summary>
    abstract partial class Expression : LuatAstNodeBase
    {

		/// <summary>
		/// Gets the minimum context ID that should be used in your code for AST nodes inheriting this class.
		/// </summary>
		/// <remarks>
		/// Base all your context ID constants off of this value.
		/// </remarks>
		protected const byte ExpressionContextIDBase = LuatAstNodeBaseContextIDBase;

		/////////////////////////////////////////////////////////////////////////////////////////////////////
		// OBJECT
		/////////////////////////////////////////////////////////////////////////////////////////////////////

		/// <summary>
		/// Initializes a new instance of the <c>Expression</c> class. 
		/// </summary>
		public Expression() {}

		/// <summary>
		/// Initializes a new instance of the <c>Expression</c> class. 
		/// </summary>
		/// <param name="textRange">The <see cref="TextRange"/> of the AST node.</param>
		public Expression(TextRange textRange) : base(textRange) {}

		/////////////////////////////////////////////////////////////////////////////////////////////////////
		// PUBLIC PROCEDURES
		/////////////////////////////////////////////////////////////////////////////////////////////////////

	}

	/// <summary>
	/// Represents an statement that is formed from an expression.
	/// </summary>
    partial class ExpressionStatement : Statement
    {

		/// <summary>
		/// Gets the context ID for the expression.
		/// </summary>
		public const byte ExpContextID = StatementContextIDBase;

		/// <summary>
		/// Gets the minimum context ID that should be used in your code for AST nodes inheriting this class.
		/// </summary>
		/// <remarks>
		/// Base all your context ID constants off of this value.
		/// </remarks>
		protected const byte ExpressionStatementContextIDBase = StatementContextIDBase + 1;

		/////////////////////////////////////////////////////////////////////////////////////////////////////
		// OBJECT
		/////////////////////////////////////////////////////////////////////////////////////////////////////

		/// <summary>
		/// Initializes a new instance of the <c>ExpressionStatement</c> class.
		/// </summary>
		/// <param name="owner">indicating the binary operator type.</param>
		/// <param name="name">The name of the function.</param>
		/// <param name="parameters">Parameters to the function.</param>
		public ExpressionStatement(Expression exp) : this(new TextRange( exp.StartOffset, exp.EndOffset ) )
			{
			// Initialize parameters
			this.Exp = exp;
		}

		/// <summary>
		/// Initializes a new instance of the <c>ExpressionStatement</c> class. 
		/// </summary>
		public ExpressionStatement() {}

		/// <summary>
		/// Initializes a new instance of the <c>ExpressionStatement</c> class. 
		/// </summary>
		/// <param name="textRange">The <see cref="TextRange"/> of the AST node.</param>
		public ExpressionStatement(TextRange textRange) : base(textRange) {}

		/////////////////////////////////////////////////////////////////////////////////////////////////////
		// PUBLIC PROCEDURES
		/////////////////////////////////////////////////////////////////////////////////////////////////////

		/// <summary>
		/// Gets the <see cref="LuatNodeType"/> that identifies the type of node.
		/// </summary>
		/// <value>The <see cref="LuatNodeType"/> that identifies the type of node.</value>
		public override LuatNodeType NodeType { 
			get {
				return LuatNodeType.ExpressionStatement;
			}
		}

		/// <summary>
		/// Gets or sets the expression.
		/// </summary>
		/// <value>The expression.</value>
		public Expression Exp {
			get {
				return this.GetChildNode(ExpressionStatement.ExpContextID) as Expression;
			}
			set {
				this.ChildNodes.Replace(value, ExpressionStatement.ExpContextID);
			}
		}

		/// <summary>
		/// Gets text representing the node that can be used for display, such as in a document outline.
		/// </summary>
		/// <value>Text representing the node that can be used for display, such as in a document outline.</value>
		public override string DisplayText {
			get {
				return Exp.ToString();
			}
		}

	}

	/// <summary>
	/// Represents an assignment statement.
	/// </summary>
    partial class ForStatement : Statement
    {

		/// <summary>
		/// Gets the context ID for the iterator.
		/// </summary>
		public const byte IteratorContextID = StatementContextIDBase;

		/// <summary>
		/// Gets the context ID for the iterator initial value.
		/// </summary>
		public const byte StartContextID = StatementContextIDBase + 1;

		/// <summary>
		/// Gets the context ID for the iterator test value.
		/// </summary>
		public const byte EndContextID = StatementContextIDBase + 2;

		/// <summary>
		/// Gets the context ID for the iterator test value.
		/// </summary>
		public const byte StepContextID = StatementContextIDBase + 3;

		/// <summary>
		/// Gets the context ID for the loop body.
		/// </summary>
		public const byte BodyContextID = StatementContextIDBase + 4;

		/// <summary>
		/// Gets the minimum context ID that should be used in your code for AST nodes inheriting this class.
		/// </summary>
		/// <remarks>
		/// Base all your context ID constants off of this value.
		/// </remarks>
		protected const byte ForStatementContextIDBase = StatementContextIDBase + 5;

		/////////////////////////////////////////////////////////////////////////////////////////////////////
		// OBJECT
		/////////////////////////////////////////////////////////////////////////////////////////////////////

		/// <summary>
		/// Initializes a new instance of the <c>ForStatement</c> class. 
		/// </summary>
		public ForStatement() {}

		/// <summary>
		/// Initializes a new instance of the <c>ForStatement</c> class. 
		/// </summary>
		/// <param name="textRange">The <see cref="TextRange"/> of the AST node.</param>
		public ForStatement(TextRange textRange) : base(textRange) {}

		/////////////////////////////////////////////////////////////////////////////////////////////////////
		// PUBLIC PROCEDURES
		/////////////////////////////////////////////////////////////////////////////////////////////////////

		/// <summary>
		/// Gets the <see cref="LuatNodeType"/> that identifies the type of node.
		/// </summary>
		/// <value>The <see cref="LuatNodeType"/> that identifies the type of node.</value>
		public override LuatNodeType NodeType { 
			get {
				return LuatNodeType.ForStatement;
			}
		}

		/// <summary>
		/// Gets or sets the iterator.
		/// </summary>
		/// <value>The iterator.</value>
		public VariableExpression Iterator {
			get {
				return this.GetChildNode(ForStatement.IteratorContextID) as VariableExpression;
			}
			set {
				this.ChildNodes.Replace(value, ForStatement.IteratorContextID);
			}
		}

		/// <summary>
		/// Gets or sets the iterator initial value.
		/// </summary>
		/// <value>The iterator initial value.</value>
		public Expression Start {
			get {
				return this.GetChildNode(ForStatement.StartContextID) as Expression;
			}
			set {
				this.ChildNodes.Replace(value, ForStatement.StartContextID);
			}
		}

		/// <summary>
		/// Gets or sets the iterator test value.
		/// </summary>
		/// <value>The iterator test value.</value>
		public Expression End {
			get {
				return this.GetChildNode(ForStatement.EndContextID) as Expression;
			}
			set {
				this.ChildNodes.Replace(value, ForStatement.EndContextID);
			}
		}

		/// <summary>
		/// Gets or sets the iterator test value.
		/// </summary>
		/// <value>The iterator test value.</value>
		public Expression Step {
			get {
				return this.GetChildNode(ForStatement.StepContextID) as Expression;
			}
			set {
				this.ChildNodes.Replace(value, ForStatement.StepContextID);
			}
		}

		/// <summary>
		/// Gets or sets the loop body.
		/// </summary>
		/// <value>The loop body.</value>
		public BlockStatement Body {
			get {
				return this.GetChildNode(ForStatement.BodyContextID) as BlockStatement;
			}
			set {
				this.ChildNodes.Replace(value, ForStatement.BodyContextID);
			}
		}

		public override string DisplayText {
			get {
				StringBuilder sb = new StringBuilder();
				sb.Append( "for " );
				sb.Append( Iterator.DisplayText );
				sb.Append( " = " );
				sb.Append( Start.DisplayText );
				sb.Append( ", " );
				sb.Append( End.DisplayText );
				
				if ( null != Step )
					{
					sb.Append( ", " );
					sb.Append( Step.DisplayText );
				}
				
				sb.Append( " do\n" );
				sb.Append( this.Body.DisplayText );
				sb.Append( "end" );
				
				return sb.ToString();
			}
		}

	}

	/// <summary>
	/// Represents an assignment statement.
	/// </summary>
    partial class ForInStatement : Statement
    {

		/// <summary>
		/// Gets the context ID for the iterators.
		/// </summary>
		public const byte IteratorContextID = StatementContextIDBase;

		/// <summary>
		/// Gets the context ID for the tables.
		/// </summary>
		public const byte TableContextID = StatementContextIDBase + 1;

		/// <summary>
		/// Gets the context ID for the loop body.
		/// </summary>
		public const byte BodyContextID = StatementContextIDBase + 2;

		/// <summary>
		/// Gets the minimum context ID that should be used in your code for AST nodes inheriting this class.
		/// </summary>
		/// <remarks>
		/// Base all your context ID constants off of this value.
		/// </remarks>
		protected const byte ForInStatementContextIDBase = StatementContextIDBase + 3;

		/////////////////////////////////////////////////////////////////////////////////////////////////////
		// OBJECT
		/////////////////////////////////////////////////////////////////////////////////////////////////////

		/// <summary>
		/// Initializes a new instance of the <c>ForInStatement</c> class. 
		/// </summary>
		public ForInStatement() {}

		/// <summary>
		/// Initializes a new instance of the <c>ForInStatement</c> class. 
		/// </summary>
		/// <param name="textRange">The <see cref="TextRange"/> of the AST node.</param>
		public ForInStatement(TextRange textRange) : base(textRange) {}

		/////////////////////////////////////////////////////////////////////////////////////////////////////
		// PUBLIC PROCEDURES
		/////////////////////////////////////////////////////////////////////////////////////////////////////

		/// <summary>
		/// Gets the <see cref="LuatNodeType"/> that identifies the type of node.
		/// </summary>
		/// <value>The <see cref="LuatNodeType"/> that identifies the type of node.</value>
		public override LuatNodeType NodeType { 
			get {
				return LuatNodeType.ForInStatement;
			}
		}

		/// <summary>
		/// Gets the iterators.
		/// </summary>
		/// <value>The collection of statements.</value>
		public IAstNodeList Iterators {
			get {
				return new AstNodeListWrapper(this, ForInStatement.IteratorContextID);
			}
		}

		/// <summary>
		/// Gets the tables.
		/// </summary>
		/// <value>The collection of statements.</value>
		public IAstNodeList Tables {
			get {
				return new AstNodeListWrapper(this, ForInStatement.TableContextID);
			}
		}

		/// <summary>
		/// Gets or sets the loop body.
		/// </summary>
		/// <value>The loop body.</value>
		public BlockStatement Body {
			get {
				return this.GetChildNode(ForInStatement.BodyContextID) as BlockStatement;
			}
			set {
				this.ChildNodes.Replace(value, ForInStatement.BodyContextID);
			}
		}

		public override string DisplayText {
			get {
				StringBuilder text = new StringBuilder();
				text.Append( "for " );
				
				int iteratorCount = this.Iterators.Count;
				for ( int i = 0; i < iteratorCount; ++i )
					{
				if ( i > 0 ) { text.Append( ", " ); }
				text.Append( this.Iterators[i].DisplayText );
			}
			
			text.Append( " in " );
			
			int tableCount = this.Tables.Count;
			for ( int i = 0; i < tableCount; ++i )
				{
			if ( i > 0 ) { text.Append( ", " ); }
			text.Append( this.Tables[i].DisplayText );
		}
		
		text.Append( " do\n" );
		text.Append( this.Body.DisplayText );
		text.Append( "end" );
		
		return text.ToString();
	}
}

	}

	/// <summary>
	/// Represents a return statement.
	/// </summary>
	partial class ReturnStatement : Statement {

		/// <summary>
		/// Gets the context ID for the values to be returned.
		/// </summary>
		public const byte ValueContextID = StatementContextIDBase;

		/// <summary>
		/// Gets the minimum context ID that should be used in your code for AST nodes inheriting this class.
		/// </summary>
		/// <remarks>
		/// Base all your context ID constants off of this value.
		/// </remarks>
		protected const byte ReturnStatementContextIDBase = StatementContextIDBase + 1;

		/////////////////////////////////////////////////////////////////////////////////////////////////////
		// OBJECT
		/////////////////////////////////////////////////////////////////////////////////////////////////////

		/// <summary>
		/// Initializes a new instance of the <c>ReturnStatement</c> class. 
		/// </summary>
		public ReturnStatement() {}

		/// <summary>
		/// Initializes a new instance of the <c>ReturnStatement</c> class. 
		/// </summary>
		/// <param name="textRange">The <see cref="TextRange"/> of the AST node.</param>
		public ReturnStatement(TextRange textRange) : base(textRange) {}

		/////////////////////////////////////////////////////////////////////////////////////////////////////
		// PUBLIC PROCEDURES
		/////////////////////////////////////////////////////////////////////////////////////////////////////

		/// <summary>
		/// Gets the <see cref="LuatNodeType"/> that identifies the type of node.
		/// </summary>
		/// <value>The <see cref="LuatNodeType"/> that identifies the type of node.</value>
		public override LuatNodeType NodeType { 
			get {
				return LuatNodeType.ReturnStatement;
			}
		}

		/// <summary>
		/// Gets the values to be returned.
		/// </summary>
		/// <value>The collection of statements.</value>
		public IAstNodeList Values {
			get {
				return new AstNodeListWrapper(this, ReturnStatement.ValueContextID);
			}
		}

		/// <summary>
		/// Gets text representing the node that can be used for display, such as in a document outline.
		/// </summary>
		/// <value>Text representing the node that can be used for display, such as in a document outline.</value>
		public override string DisplayText {
			get {
				StringBuilder text = new StringBuilder();
				text.Append( "Return" );
				
				IAstNodeList parameters = this.Values;
				for (int index = 0; index < Values.Count; index++) {
					text.Append( (index > 0) ? ", " : " " );
					text.Append(((Identifier)Values[index]).Text);
				}
				
				return text.ToString();
			}
		}

	}
}

#endregion