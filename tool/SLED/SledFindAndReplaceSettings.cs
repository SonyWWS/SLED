/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

using System;
using System.Collections.Generic;
using System.Windows.Forms;

using Sce.Sled.Shared.Services;

namespace Sce.Sled
{
    /// <summary>
    /// ISledFindAndReplaceSubForm Interface
    /// </summary>
    interface ISledFindAndReplaceSubForm
    {
        /// <summary>
        /// Gets the underlying UserControl
        /// </summary>
        Control Control
        {
            get;
        }

        /// <summary>
        /// Sets the initial text shown in the find or replace form
        /// </summary>
        string InitialText { set; }

        /// <summary>
        /// Event fired when a subform is doing a find or replace event
        /// </summary>
        event EventHandler<SledFindAndReplaceEventArgs> FindAndReplaceEvent;
    }

    /// <summary>
    /// Enumeration describing the places to look when finding or replacing
    /// </summary>
    enum SledFindAndReplaceLookIn
    {
        Invalid,
        CurrentDocument,
        AllOpenDocuments,
        CurrentProject,
        EntireSolution,
        Custom,
    }

    /// <summary>
    /// Enumeration describing the possible search patterns to use when finding or replacing
    /// </summary>
    enum SledFindAndReplaceSearchType
    {
        Normal,
        RegularExpressions,
        WildCards,
    }

    /// <summary>
    /// Which find and replace window to use
    /// </summary>
    enum SledFindAndReplaceResultsWindow
    {
        None,
        One,
        Two,
    }

    /// <summary>
    /// Result of a find or replace action
    /// </summary>
    enum SledFindAndReplaceResult
    {
        Invalid,
        NothingToSearch,
        NothingFound,
        Success,
    }

    /// <summary>
    /// Abstract class for find and replace event arguments to derive from
    /// </summary>
    abstract class SledFindAndReplaceEventArgs : EventArgs, ICloneable
    {
        #region ICloneable Interface

        public abstract object Clone();

        #endregion

        #region SledFindAndReplaceEventArgs Specific Stuff

        protected SledFindAndReplaceEventArgs(SledFindAndReplaceModes mode)
        {
            m_mode = mode;
        }

        public SledFindAndReplaceModes Mode
        {
            get { return m_mode; }
        }

        public SledFindAndReplaceResult Result
        {
            get { return m_result; }
            set { m_result = value; }
        }

        private readonly SledFindAndReplaceModes m_mode;
        private SledFindAndReplaceResult m_result = SledFindAndReplaceResult.Invalid;

        #endregion

        #region Quick Find Specific Event Args Class

        public class QuickFind : SledFindAndReplaceEventArgs
        {
            public QuickFind(string szFindWhat, SledFindAndReplaceLookIn lookIn, bool bMatchCase, bool bMatchWholeWord, bool bSearchUp, SledFindAndReplaceSearchType searchType)
                : base(SledFindAndReplaceModes.QuickFind)
            {
                FindWhat = szFindWhat;
                LookIn = lookIn;
                MatchCase = bMatchCase;
                MatchWholeWord = bMatchWholeWord;
                SearchUp = bSearchUp;
                SearchType = searchType;
            }

            public string FindWhat { get; private set; }

            public SledFindAndReplaceLookIn LookIn { get; private set; }

            public bool MatchCase { get; private set; }

            public bool MatchWholeWord { get; private set; }

            public bool SearchUp { get; private set; }

            public SledFindAndReplaceSearchType SearchType { get; private set; }

            #region ICloneable Override

            public override object Clone()
            {
                var obj = new QuickFind(
                    FindWhat,
                    LookIn,
                    MatchCase,
                    MatchWholeWord,
                    SearchUp,
                    SearchType);

                return obj;
            }

            #endregion
        }

        #endregion

        #region Find in Files Specific Event Args Class

        public class FindInFiles : SledFindAndReplaceEventArgs
        {
            public FindInFiles(string szFindWhat, SledFindAndReplaceLookIn lookIn, string[] lookInFolders, bool bIncludeSubFolders, string[] fileExts, bool bMatchCase, bool bMatchWholeWord, SledFindAndReplaceSearchType searchType, bool bUseResults1Window)
                : base(SledFindAndReplaceModes.FindInFiles)
            {
                m_szFindWhat = szFindWhat;
                m_lookIn = lookIn;
                m_lookInFolders = lookInFolders;
                m_bIncludeSubFolders = bIncludeSubFolders;
                m_fileExts = fileExts;
                m_bMatchCase = bMatchCase;
                m_bMatchWholeWord = bMatchWholeWord;
                m_searchType = searchType;
                m_bUseResults1Window = bUseResults1Window;
            }

            public string FindWhat
            {
                get { return m_szFindWhat; }
            }

            public SledFindAndReplaceLookIn LookIn
            {
                get { return m_lookIn; }
            }

            public string[] LookInFolders
            {
                get { return m_lookInFolders; }
            }

            public bool IncludeSubFolders
            {
                get { return m_bIncludeSubFolders; }
            }

            public string[] FileExts
            {
                get { return m_fileExts; }
            }

            public bool MatchCase
            {
                get { return m_bMatchCase; }
            }

            public bool MatchWholeWord
            {
                get { return m_bMatchWholeWord; }
            }

            public SledFindAndReplaceSearchType SearchType
            {
                get { return m_searchType; }
            }

            public bool UseResults1Window
            {
                get { return m_bUseResults1Window; }
            }

            private readonly string m_szFindWhat;
            private readonly SledFindAndReplaceLookIn m_lookIn;
            private readonly string[] m_lookInFolders;
            private readonly bool m_bIncludeSubFolders;
            private readonly string[] m_fileExts;
            private readonly bool m_bMatchCase;
            private readonly bool m_bMatchWholeWord;
            private readonly SledFindAndReplaceSearchType m_searchType;
            private readonly bool m_bUseResults1Window;

            #region ICloneable Override

            public override object Clone()
            {
                var obj = new FindInFiles(
                    m_szFindWhat,
                    m_lookIn,
                    m_lookInFolders,
                    m_bIncludeSubFolders,
                    m_fileExts,
                    m_bMatchCase,
                    m_bMatchWholeWord,
                    m_searchType,
                    m_bUseResults1Window);

                return obj;
            }

            #endregion
        }

        #endregion

        #region Quick Replace Specific Event Args Class

        public class QuickReplace : SledFindAndReplaceEventArgs
        {
            public QuickReplace(string szFindWhat, string szReplaceWith, SledFindAndReplaceLookIn lookIn, bool bMatchCase, bool bMatchWholeWord, bool bSearchUp, SledFindAndReplaceSearchType searchType)
                : base(SledFindAndReplaceModes.QuickReplace)
            {
                m_szFindWhat = szFindWhat;
                m_szReplaceWith = szReplaceWith;
                m_lookIn = lookIn;
                m_bMatchCase = bMatchCase;
                m_bMatchWholeWord = bMatchWholeWord;
                m_bSearchUp = bSearchUp;
                m_searchType = searchType;
            }

            public string FindWhat
            {
                get { return m_szFindWhat; }
            }

            public string ReplaceWith
            {
                get { return m_szReplaceWith; }
            }

            public SledFindAndReplaceLookIn LookIn
            {
                get { return m_lookIn; }
            }

            public bool MatchCase
            {
                get { return m_bMatchCase; }
            }

            public bool MatchWholeWord
            {
                get { return m_bMatchWholeWord; }
            }

            public bool SearchUp
            {
                get { return m_bSearchUp; }
            }

            public SledFindAndReplaceSearchType SearchType
            {
                get { return m_searchType; }
            }

            private readonly string m_szFindWhat;
            private readonly string m_szReplaceWith;
            private readonly SledFindAndReplaceLookIn m_lookIn;
            private readonly bool m_bMatchCase;
            private readonly bool m_bMatchWholeWord;
            private readonly bool m_bSearchUp;
            private readonly SledFindAndReplaceSearchType m_searchType;

            #region ICloneable Override

            public override object Clone()
            {
                var obj = new QuickReplace(
                    m_szFindWhat,
                    m_szReplaceWith,
                    m_lookIn,
                    m_bMatchCase,
                    m_bMatchWholeWord,
                    m_bSearchUp,
                    m_searchType);

                return obj;
            }

            #endregion
        }

        #endregion

        #region Replace in Files Specific Event Args Class

        public class ReplaceInFiles : SledFindAndReplaceEventArgs
        {
            public ReplaceInFiles(string szFindWhat, string szReplaceWith, SledFindAndReplaceLookIn lookIn, string[] lookInFolders, bool bIncludeSubFolders, string[] fileExts, bool bMatchCase, bool bMatchWholeWord, SledFindAndReplaceSearchType searchType, SledFindAndReplaceResultsWindow resultsWindow, bool bKeepModifiedDocsOpen)
                : base(SledFindAndReplaceModes.ReplaceInFiles)
            {
                m_szFindWhat = szFindWhat;
                m_szReplaceWith = szReplaceWith;
                m_lookIn = lookIn;
                m_lookInFolders = lookInFolders;
                m_bIncludeSubFolders = bIncludeSubFolders;
                m_fileExts = fileExts;
                m_bMatchCase = bMatchCase;
                m_bMatchWholeWord = bMatchWholeWord;
                m_searchType = searchType;
                m_resultsWindow = resultsWindow;
                m_bKeepModifiedDocsOpen = bKeepModifiedDocsOpen;
            }

            public string FindWhat
            {
                get { return m_szFindWhat; }
            }

            public string ReplaceWith
            {
                get { return m_szReplaceWith; }
            }

            public SledFindAndReplaceLookIn LookIn
            {
                get { return m_lookIn; }
            }

            public string[] LookInFolders
            {
                get { return m_lookInFolders; }
            }

            public bool IncludeSubFolders
            {
                get { return m_bIncludeSubFolders; }
            }

            public string[] FileExts
            {
                get { return m_fileExts; }
            }

            public bool MatchCase
            {
                get { return m_bMatchCase; }
            }

            public bool MatchWholeWord
            {
                get { return m_bMatchWholeWord; }
            }

            public SledFindAndReplaceSearchType SearchType
            {
                get { return m_searchType; }
            }

            public SledFindAndReplaceResultsWindow ResultsWindow
            {
                get { return m_resultsWindow; }
            }

            public bool KeepModifiedDocsOpen
            {
                get { return m_bKeepModifiedDocsOpen; }
            }

            private readonly string m_szFindWhat;
            private readonly string m_szReplaceWith;
            private readonly SledFindAndReplaceLookIn m_lookIn;
            private readonly string[] m_lookInFolders;
            private readonly bool m_bIncludeSubFolders;
            private readonly string[] m_fileExts;
            private readonly bool m_bMatchCase;
            private readonly bool m_bMatchWholeWord;
            private readonly SledFindAndReplaceSearchType m_searchType;
            private readonly SledFindAndReplaceResultsWindow m_resultsWindow;
            private readonly bool m_bKeepModifiedDocsOpen;

            #region ICloneable Override

            public override object Clone()
            {
                var obj = new ReplaceInFiles(
                    m_szFindWhat,
                    m_szReplaceWith,
                    m_lookIn,
                    m_lookInFolders,
                    m_bIncludeSubFolders,
                    m_fileExts,
                    m_bMatchCase,
                    m_bMatchWholeWord,
                    m_searchType,
                    m_resultsWindow,
                    m_bKeepModifiedDocsOpen);

                return obj;
            }

            #endregion
        }

        #endregion
    }

    /// <summary>
    /// Static class to hold settings for the find & replace forms that need to be persisted
    /// </summary>
    static class SledFindAndReplaceSettings
    {
        public class WordList
        {
            public WordList(int capacity)
            {
                m_cap = capacity;
                m_lstWhat = new List<string>();
            }

            public string[] Items
            {
                get { return m_lstWhat.ToArray(); }
            }

            public void Add(string item)
            {
                if (string.IsNullOrEmpty(item))
                    return;

                var idx = m_lstWhat.IndexOf(item);
                if (idx != -1)
                {
                    // Item already in list so bump to top
                    m_lstWhat.RemoveAt(idx);
                }

                // Add to top
                m_lstWhat.Insert(0, item);

                // If now over capacity remove last item
                if (m_lstWhat.Count > m_cap)
                    m_lstWhat.RemoveAt(m_lstWhat.Count - 1);
            }

            public void Clear()
            {
                m_lstWhat.Clear();
            }

            private readonly int m_cap;
            private readonly List<string> m_lstWhat;
        }

        public static WordList GlobalFindWhat
        {
            get { return s_findWhat; }
        }

        public static WordList GlobalReplaceWith
        {
            get { return s_replaceWith; }
        }

        private static readonly WordList s_findWhat = new WordList(20);
        private static readonly WordList s_replaceWith = new WordList(20);

        #region Quick Find settings

        public class QuickFindSettings
        {
            public int LookInIndex { get; set; }

            public bool FindOptionsExpanded { get; set; }

            public bool MatchCaseChecked { get; set; }

            public bool MatchWholeWordChecked { get; set; }

            public bool SearchUpChecked { get; set; }

            public bool UseChecked { get; set; }

            public int UseIndex { get; set; }
        }

        public static QuickFindSettings QuickFind
        {
            get { return s_quickFind; }
        }

        private static readonly QuickFindSettings s_quickFind = new QuickFindSettings();

        #endregion

        #region Find in Files settings

        public class FindInFilesSettings
        {
            public int LookInIndex { get; set; }

            public bool IncludeSubFolders { get; set; }

            public bool FindOptionsExpanded { get; set; }

            public bool MatchCaseChecked { get; set; }

            public bool MatchWholeWordChecked { get; set; }

            public bool UseChecked { get; set; }

            public int UseIndex { get; set; }

            public bool ResultOptionsExpanded { get; set; }

            public bool Results1WindowChecked
            {
                get { return m_bResults1WindowChecked; }
                set { m_bResults1WindowChecked = value; }
            }

            public bool DisplayFileNamesOnlyChecked { get; set; }

            private bool m_bResults1WindowChecked = true;
        }

        public static FindInFilesSettings FindInFiles
        {
            get { return s_findInFiles; }
        }

        private static readonly FindInFilesSettings s_findInFiles = new FindInFilesSettings();

        #endregion

        #region Quick Replace settings

        public class QuickReplaceSettings
        {
            public int LookInIndex { get; set; }

            public bool FindOptionsExpanded { get; set; }

            public bool MatchCaseChecked { get; set; }

            public bool MatchWholeWordChecked { get; set; }

            public bool SearchUpChecked { get; set; }

            public bool UseChecked { get; set; }

            public int UseIndex { get; set; }
        }

        public static QuickReplaceSettings QuickReplace
        {
            get { return s_quickReplace; }
        }

        private static readonly QuickReplaceSettings s_quickReplace = new QuickReplaceSettings();

        #endregion

        #region Replace in Files settings

        public class ReplaceInFilesSettings
        {
            public int LookInIndex { get; set; }

            public bool IncludeSubFolders { get; set; }

            public bool FindOptionsExpanded { get; set; }

            public bool MatchCaseChecked { get; set; }

            public bool MatchWholeWordChecked { get; set; }

            public bool UseChecked { get; set; }

            public int UseIndex { get; set; }

            public bool ResultOptionsExpanded { get; set; }

            public bool Results1WindowChecked
            {
                get { return m_bResults1WindowChecked; }
                set { m_bResults1WindowChecked = value; }
            }

            public bool DisplayFileNamesOnlyChecked { get; set; }

            public bool KeepModifiedFilesOpen { get; set; }

            private bool m_bResults1WindowChecked = true;
        }

        public static ReplaceInFilesSettings ReplaceInFiles
        {
            get { return s_replaceInFiles; }
        }

        private static readonly ReplaceInFilesSettings s_replaceInFiles = new ReplaceInFilesSettings();

        #endregion
    }
}
