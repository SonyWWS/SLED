/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;

using Sce.Atf;
using Sce.Atf.Applications;

namespace Sce.Sled.Shared
{
    /// <summary>
    /// Wrapper around Scea.Utilities.MessageType
    /// </summary>
    public enum SledMessageType
    {
        /// <summary>
        /// Informational message
        /// </summary>
        Info = 1,

        /// <summary>
        /// Warning message
        /// </summary>
        Warning = 2,

        /// <summary>
        /// Error message
        /// </summary>
        Error = 3,
    }

    /// <summary>
    /// SledOutDevice Class
    /// <remarks>Queues messages until they can be shown and then redirects them to OutputService</remarks>
    /// </summary>
    [Export(typeof(IInitializable))]
    [Export(typeof(SledOutDevice))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class SledOutDevice : IInitializable
    {
        /// <summary>
        /// Constructor with parameters</summary>
        /// <param name="mainForm">MainForm object</param>
        /// <param name="writer">IOutputWriter object for output messages</param>
        [ImportingConstructor]
        public SledOutDevice(MainForm mainForm, IOutputWriter writer)
        {
            mainForm.Shown += MainFormShown;

            m_writer = writer;

            s_instance = this;
            s_dictMsgTypeLookup.Add(SledMessageType.Info, OutputMessageType.Info);
            s_dictMsgTypeLookup.Add(SledMessageType.Warning, OutputMessageType.Warning);
            s_dictMsgTypeLookup.Add(SledMessageType.Error, OutputMessageType.Error);
        }

        #region IInitializable Interface

        /// <summary>
        /// Finish initializing component</summary>
        void IInitializable.Initialize()
        {
            // Keep this here
        }

        #endregion

        #region MainForm Events

        private void MainFormShown(object sender, EventArgs e)
        {
            s_bOutDeviceReady = true;

            // Show all queued messages
            foreach (var queuedMsg in s_lstMessages)
            {
                if (queuedMsg.OutOrOutLine)
                    Out(queuedMsg.MessageType, queuedMsg.Message);
                else
                    OutLine(queuedMsg.MessageType, queuedMsg.Message);
            }

            s_lstMessages.Clear();
        }

        #endregion

        /// <summary>
        /// Displays message to the user in a text control
        /// </summary>
        /// <param name="messageType">Message type, which modifies display of message</param>
        /// <param name="format">Text message to display</param>
        /// <param name="args">Optional arguments</param>
        public static void Out(SledMessageType messageType, string format, params object[] args)
        {
            try
            {
                var formatted = string.Format(format, args);

                if (s_bOutDeviceReady)
                    s_instance.m_writer.Write(s_dictMsgTypeLookup[messageType], formatted);
                else
                    s_lstMessages.Add(new QueuedMessage(true, messageType, formatted));
            }
            finally
            {
                s_bLastMessageBreak = false;
            }
        }

        /// <summary>
        /// Displays message to the user in a text control
        /// </summary>
        /// <param name="messageType">Message type, which modifies display of message</param>
        /// <param name="format">Text message to display</param>
        /// <param name="args">Optional arguments</param>
        public static void OutLine(SledMessageType messageType, string format, params object[] args)
        {
            try
            {
                var formatted = args.Length <= 0 ? format : string.Format(format, args);

                if (s_bOutDeviceReady)
                    s_instance.m_writer.Write(s_dictMsgTypeLookup[messageType], formatted + Environment.NewLine);
                else
                    s_lstMessages.Add(new QueuedMessage(false, messageType, formatted));
            }
            catch (Exception ex)
            {
                if ((s_instance != null) && (s_instance.m_writer != null))
                    s_instance.m_writer.Write(OutputMessageType.Error, "SledOutDevice Exception: {0}", ex.Message);
            }
            finally
            {
                s_bLastMessageBreak = false;
            }
        }

        /// <summary>
        /// Displays a break between groups of text
        /// </summary>
        public static void OutBreak()
        {
            if (s_bLastMessageBreak)
                return;

            try
            {
                const SledMessageType messageType = SledMessageType.Info;

                if (s_bOutDeviceReady)
                    s_instance.m_writer.Write(s_dictMsgTypeLookup[messageType], Break);
                else
                    s_lstMessages.Add(new QueuedMessage(false, messageType, Break));
            }
            finally
            {
                s_bLastMessageBreak = true;
            }
        }

        /// <summary>
        /// Write a list of items to the Output window with each item on a new line
        /// </summary>
        /// <typeparam name="T">Type of item to output</typeparam>
        /// <param name="messageType">Message type</param>
        /// <param name="prefix">String to prepend to each line</param>
        /// <param name="lstItems">List of items to write</param>
        public static void OutLineList<T>(SledMessageType messageType, string prefix, IEnumerable<T> lstItems) where T : class
        {
            if (lstItems == null)
                return;

            foreach (var item in lstItems)
            {
                var itemString =
                    item == null
                        ? string.Empty
                        : item.ToString();

                var message =
                    string.IsNullOrEmpty(prefix)
                        ? string.Format("{0}", itemString)
                        : string.Format("{0} {1}", prefix, itemString);

                OutLine(messageType, message);
            }
        }

        /// <summary>
        /// Clear text
        /// </summary>
        public static void Clear()
        {
            if (!s_bOutDeviceReady)
                return;

            s_instance.m_writer.Clear();
        }

        /// <summary>
        /// Displays message to the user in a text control but only when "DEBUG" is defined
        /// </summary>
        /// <param name="messageType">Message type, which modifies display of message</param>
        /// <param name="format">Text message to display</param>
        /// <param name="args">Optional arguments</param>
        [System.Diagnostics.Conditional("DEBUG")]
        public static void OutDebug(SledMessageType messageType, string format, params object[] args)
        {
            try
            {
                var formatted = string.Format(format, args);

                if (s_bOutDeviceReady)
                    s_instance.m_writer.Write(s_dictMsgTypeLookup[messageType], formatted);
                else
                    s_lstMessages.Add(new QueuedMessage(true, messageType, formatted));
            }
            finally
            {
                s_bLastMessageBreak = false;
            }
        }

        /// <summary>
        /// Displays message to the user in a text control but only when "DEBUG" is defined
        /// </summary>
        /// <param name="messageType">Message type, which modifies display of message</param>
        /// <param name="format">Text message to display</param>
        /// <param name="args">Optional arguments</param>
        [System.Diagnostics.Conditional("DEBUG")]
        public static void OutLineDebug(SledMessageType messageType, string format, params object[] args)
        {
            try
            {
                var formatted = args.Length <= 0 ? format : string.Format(format, args);

                if (s_bOutDeviceReady)
                    s_instance.m_writer.Write(s_dictMsgTypeLookup[messageType], formatted + Environment.NewLine);
                else
                    s_lstMessages.Add(new QueuedMessage(false, messageType, formatted));
            }
            catch (Exception ex)
            {
                if ((s_instance != null) && (s_instance.m_writer != null))
                    s_instance.m_writer.Write(OutputMessageType.Error, "SledOutDeviceDebug Exception: {0}", ex.Message);
            }
            finally
            {
                s_bLastMessageBreak = false;
            }
        }

        /// <summary>
        /// BreakBlock Class
        /// <remarks>Convenience to wrap a chunk of output with breaks</remarks>
        /// </summary>
        public class BreakBlock : IDisposable
        {
            /// <summary>
            /// Constructor
            /// </summary>
            public BreakBlock()
            {
                OutBreak();
            }

            /// <summary>
            /// Dispose
            /// </summary>
            public void Dispose()
            {
                OutBreak();
            }
        }

        private readonly IOutputWriter m_writer;

        private const string Break = "-";
        
        private static bool s_bOutDeviceReady;
        private static bool s_bLastMessageBreak;

        private static SledOutDevice s_instance;

        private static readonly List<QueuedMessage> s_lstMessages = 
            new List<QueuedMessage>();

        private static readonly Dictionary<SledMessageType, OutputMessageType> s_dictMsgTypeLookup =
            new Dictionary<SledMessageType, OutputMessageType>(new SledMessageTypeEqualityComparer());

        #region QueuedMessage Class

        private class QueuedMessage
        {
            public QueuedMessage(bool bOutOrOutLine, SledMessageType messageType, string message)
            {
                OutOrOutLine = bOutOrOutLine;
                MessageType = messageType;
                Message = message;
            }

            public bool OutOrOutLine { get; private set; }

            public SledMessageType MessageType { get; private set; }

            public string Message { get; private set; }
        }

        #endregion

        #region SledMessageTypeEqualityComparer Class

        private class SledMessageTypeEqualityComparer : IEqualityComparer<SledMessageType>
        {
            public bool Equals(SledMessageType item1, SledMessageType item2)
            {
                return item1 == item2;
            }

            public int GetHashCode(SledMessageType item)
            {
                return (int)item;
            }
        }

        #endregion
    }
}
