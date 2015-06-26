/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

using System;
using System.IO;
using System.Reflection;

using ActiproSoftware.SyntaxEditor;
using ActiproSoftware.SyntaxEditor.Addons.Dynamic;

namespace Sce.Sled.SyntaxEditor
{
    /// <summary>
    /// Custom dynamic outlining syntax language to add code folding/outlining to Lua documents
    /// </summary>
    class LuaSyntaxLanguage : DynamicOutliningSyntaxLanguage
    {
        public LuaSyntaxLanguage(Assembly assembly, string resourceName)
            : base(assembly, resourceName, 0)
        {
        }

        public LuaSyntaxLanguage(Stream stream)
        {
            LoadStream(stream);
        }

        public override void GetTokenOutliningAction(TokenStream tokenStream, ref string outliningKey, ref OutliningNodeAction tokenAction)
        {
            // Get the token
            var token = tokenStream.Peek();
            if (token == null)
                return;

            if (string.IsNullOrEmpty(token.Key))
                return;

            // See if the token starts or ends an outlining node
            switch (token.Key)
            {
                //case "OpenCurlyBraceToken":
                //    outliningKey = "CodeBlock";
                //    tokenAction = OutliningNodeAction.Start;
                //    break;

                //case "CloseCurlyBraceToken":
                //    outliningKey = "CodeBlock";
                //    tokenAction = OutliningNodeAction.End;
                //    break;

                case ReservedWordToken:
                {
                    var tokenString =
                        token.AutoCaseCorrectText;

                    if (string.IsNullOrEmpty(tokenString))
                        return;

                    switch (tokenString)
                    {
                        case "do":
                        //case "while": // while's also contain "do"
                        //case "for":   // for's also contain "do"
                        case "if":
                        case "repeat":
                        case "function":
                            outliningKey = "CodeBlock";
                            tokenAction = OutliningNodeAction.Start;
                            break;

                        case "until":
                        case "end":
                            outliningKey = "CodeBlock";
                            tokenAction = OutliningNodeAction.End;
                            break;
                    }
                }
                break;

                case MultiLineCommentStartToken:
                    outliningKey = "MultiLineComment";
                    tokenAction = OutliningNodeAction.Start;
                    break;

                case MultiLineCommentEndToken:
                    outliningKey = "MultiLineComment";
                    tokenAction = OutliningNodeAction.End;
                    break;
            }
        }

        private void LoadStream(Stream stream)
        {
            try
            {
                using (var reader = new StreamReader(stream))
                {
                    var definitionXml = reader.ReadToEnd();

                    // Technically this property isn't supposed
                    // to be available to us
                    DefinitionXml = definitionXml;
                }
            }
            catch (Exception ex)
            {
                Atf.Outputs.WriteLine(
                    Atf.OutputMessageType.Error,
                    "Exception loading stream: {0}",
                    ex.Message);
            }
        }

        private const string ReservedWordToken = "ReservedWordToken";
        private const string MultiLineCommentStartToken = "MultiLineCommentStartToken";
        private const string MultiLineCommentEndToken = "MultiLineCommentEndToken";
    }
}