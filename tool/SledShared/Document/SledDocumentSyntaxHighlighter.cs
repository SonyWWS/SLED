/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

using System;
using System.IO;
using System.Reflection;

using Sce.Sled.SyntaxEditor;

namespace Sce.Sled.Shared.Document
{
    /// <summary>
    /// SledDocument syntax highlighter class</summary>
    public abstract class SledDocumentSyntaxHighlighter
    {
        /// <summary>
        /// Get ActiproSoftware SyntaxEditor language/stream to use for
        /// syntax highlighting. If a language, then use one found in
        /// Sce.Atf.Controls.TextEditing.Languages enum. If a stream,
        /// it must be a valid ActiproSoftware SyntaxEditor stream.</summary>
        public abstract object Highlighter { get; }

        /// <summary>
        /// Set the ISyntaxEditorControl to use highlighting from the SledDocumentSyntaxHighlighter</summary>
        /// <param name="highlighter">SledDocumentSyntaxHighlighter to use</param>
        /// <param name="syntaxEditor">ISyntaxEditorControl</param>
        public static void FeedHighlighterToSyntaxEditor(SledDocumentSyntaxHighlighter highlighter, ISyntaxEditorControl syntaxEditor)
        {
            if (syntaxEditor == null)
                return;

            var bSuccess = false;

            try
            {
                if (highlighter == null)
                    return;

                if (highlighter.Highlighter == null)
                    return;

                var hl = highlighter.Highlighter;

                if (hl is Languages)
                {
                    syntaxEditor.SetLanguage((Languages)hl);
                    bSuccess = true;
                }
                else if (hl is Stream)
                {
                    syntaxEditor.SetLanguage((Stream)hl);
                    bSuccess = true;
                }
                else if (hl is LanguageStreamPair)
                {
                    var pair = (LanguageStreamPair)hl;

                    syntaxEditor.SetLanguage(pair.BaseLanguage, pair.LanguageDefinition);

                    bSuccess = true;
                }
            }
            catch (Exception)
            {
                bSuccess = false;
            }
            finally
            {
                if (!bSuccess)
                    syntaxEditor.SetLanguage(Languages.Text);
            }
        }

        /// <summary>
        /// Class pairing a language with its definition stream</summary>
        public class LanguageStreamPair
        {
            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="baseLanguage">Base language the language definition stream is based on</param>
            /// <param name="languageDefinition">Language definition stream</param>
            public LanguageStreamPair(Languages baseLanguage, Stream languageDefinition)
            {
                BaseLanguage = baseLanguage;
                LanguageDefinition = languageDefinition;
            }

            /// <summary>
            /// Gets base language the language definition stream is based on
            /// </summary>
            public Languages BaseLanguage { get; private set; }
            
            /// <summary>
            /// Gets language definition stream
            /// </summary>
            public Stream LanguageDefinition { get; private set; }
        }
    }

    /// <summary>
    /// SledDocument language syntax highlighter class
    /// </summary>
    public sealed class SledDocumentLanguageSyntaxHighlighter : SledDocumentSyntaxHighlighter
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="language">Language</param>
        public SledDocumentLanguageSyntaxHighlighter(Languages language)
        {
            m_language = language;
        }

        /// <summary>
        /// Get ActiproSoftware SyntaxEditor language/stream to use for
        /// syntax highlighting. If a language, then use one found in
        /// Sce.Atf.Controls.TextEditing.Languages enum. If a stream,
        /// it must be a valid ActiproSoftware SyntaxEditor stream.
        /// </summary>
        public override object Highlighter
        {
            get { return m_language; }
        }

        private readonly Languages m_language;
    }

    /// <summary>
    /// SledDocument assembly stream syntax highlighter class
    /// </summary>
    public sealed class SledDocumentAssemblyStreamSyntaxHighlighter : SledDocumentSyntaxHighlighter
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="assemblyType">Assembly type</param>
        /// <param name="pathInAssembly">Path in assembly</param>
        public SledDocumentAssemblyStreamSyntaxHighlighter(Type assemblyType, string pathInAssembly)
        {
            m_type = assemblyType;
            m_path = pathInAssembly;
        }

        /// <summary>
        /// Get ActiproSoftware SyntaxEditor language/stream to use for
        /// syntax highlighting. If a language, then use one found in
        /// Sce.Atf.Controls.TextEditing.Languages enum. If a stream,
        /// it must be a valid ActiproSoftware SyntaxEditor stream.
        /// </summary>
        public override object Highlighter
        {
            get
            {
                try
                {
                    var assembly = Assembly.GetAssembly(m_type);
                    var stream = assembly.GetManifestResourceStream(m_path);
                    return stream;
                }
                catch (Exception)
                {
                    return Languages.Text;
                }
            }
        }

        private readonly Type m_type;
        private readonly string m_path;
    }

    /// <summary>
    /// SledDocument file stream syntax highlighter class
    /// </summary>
    public sealed class SledDocumentFileStreamSyntaxHighlighter : SledDocumentSyntaxHighlighter
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="absFilePath">Absolute file path</param>
        public SledDocumentFileStreamSyntaxHighlighter(string absFilePath)
        {
            m_path = absFilePath;
        }

        /// <summary>
        /// Get ActiproSoftware SyntaxEditor language/stream to use for
        /// syntax highlighting. If a language, then use one found in
        /// Sce.Atf.Controls.TextEditing.Languages enum. If a stream,
        /// it must be a valid ActiproSoftware SyntaxEditor stream.
        /// </summary>
        public override object Highlighter
        {
            get
            {
                try
                {
                    Stream stream = File.OpenRead(m_path);
                    return stream;
                }
                catch (Exception)
                {
                    return Languages.Text;
                }
            }
        }

        private readonly string m_path;
    }
}