/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

namespace Sce.Sled.SyntaxEditor
{
    public static class TextEditorFactory
    {
        public static ISyntaxEditorControl CreateSyntaxHighlightingEditor()
        {
            return new SyntaxEditorControl();
        }

        public static ISyntaxEditorFindReplaceOptions CreateSyntaxEditorFindReplaceOptions()
        {
            return new SyntaxEditorFindReplaceOptions();
        }
    }

    public static class LuaTextEditorFactory
    {
        /// <summary>
        /// Create or get the exisitng Lua Intellisense broker
        /// </summary>
        /// <returns></returns>
        public static Intellisense.Lua.ILuaIntellisenseBroker CreateOrGetBroker()
        {
            return Intellisense.Lua.LuaIntellisenseBroker.Create();
        }

        /// <summary>
        /// Return the Lua Intellisense navigator (might be null if the broker hasn't been created)
        /// </summary>
        /// <returns></returns>
        public static Intellisense.Lua.ILuaIntellisenseNavigator Get()
        {
            return Intellisense.Lua.LuaIntellisenseBroker.Get();
        }
    }
}
