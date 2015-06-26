/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

using ActiproSoftware.SyntaxEditor;

namespace Sce.Sled.SyntaxEditor.Intellisense.Lua
{
    class LuatWarning : SyntaxError
    {
        public LuatWarning(LuatScript script, WarningType type, TextRange textRange, string message)
            : base(textRange, message)
        {
            Script = script;
            Type = type;
        }

        public LuatScript Script { get; private set; }

        public WarningType Type { get; private set; }
    }

    class LuatWarningSpanIndicator : WaveLineSpanIndicator
    {
        public LuatWarningSpanIndicator(HighlightingStyle style, LuatWarning[] warnings)
            : base(style.ForeColor)
        {
            Warnings = warnings;
        }

        public LuatWarning[] Warnings { get; private set; }
    }
}