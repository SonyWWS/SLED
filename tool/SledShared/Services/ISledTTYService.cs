/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

using System;
using System.Drawing;

using Sce.Sled.Shared.Plugin;

namespace Sce.Sled.Shared.Services
{
    /// <summary>
    /// SLED TTY message class
    /// </summary>
    public class SledTtyMessage
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="messageType">SledMessageType</param>
        /// <param name="message">Message text</param>
        public SledTtyMessage(SledMessageType messageType, string message)
            : this(message)
        {
            m_messageType = messageType;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="message">Message text</param>
        /// <param name="textColor">Message text color</param>
        /// <param name="backColor">Message text background color</param>
        public SledTtyMessage(string message, Color textColor, Color backColor)
            : this(message)
        {
            m_textColor = textColor;
            m_backColor = backColor;
            m_usesCustomColors = true;
            m_messageType = SledMessageType.Info;
        }

        private SledTtyMessage(string message)
        {
            Time = DateTime.Now;
            Message = message;
        }

        /// <summary>
        /// Get time message received
        /// </summary>
        public DateTime Time { get; private set; }

        /// <summary>
        /// Get message text to display
        /// </summary>
        public string Message { get; private set; }

        /// <summary>
        /// Get message text color
        /// </summary>
        public Color TextColor
        {
            get { return m_usesCustomColors ? m_textColor : GetTextColor(this); }
        }

        /// <summary>
        /// Get message text background color
        /// </summary>
        public Color BackColor
        {
            get { return m_usesCustomColors ? m_backColor : GetBackColor(this); }
        }

        private static Color GetTextColor(SledTtyMessage message)
        {
            switch (message.m_messageType)
            {
                case SledMessageType.Warning:
                    return SledTtyMessageColorer.Instance.WarningTextColor;

                case SledMessageType.Error:
                    return SledTtyMessageColorer.Instance.ErrorTextColor;
            }

            return SledTtyMessageColorer.Instance.NormalTextColor;
        }

        private static Color GetBackColor(SledTtyMessage message)
        {
            switch (message.m_messageType)
            {
                case SledMessageType.Warning:
                    return SledTtyMessageColorer.Instance.WarningBackColor;

                case SledMessageType.Error:
                    return SledTtyMessageColorer.Instance.ErrorBackColor;
            }

            return SledTtyMessageColorer.Instance.NormalBackColor;
        }

        private readonly Color m_textColor;
        private readonly Color m_backColor;
        private readonly bool m_usesCustomColors;
        private readonly SledMessageType m_messageType;
    }

    /// <summary>
    /// Class to handle SkinService potential mass skinning of TTY messages
    /// </summary>
    public class SledTtyMessageColorer
    {
        private SledTtyMessageColorer()
        {
            NormalTextColor = Color.Black;
            WarningTextColor = Color.Orange;
            ErrorTextColor = Color.Red;

            NormalBackColor = SystemColors.ControlLightLight;
            WarningBackColor = SystemColors.ControlLightLight;
            ErrorBackColor = SystemColors.ControlLightLight;
        }

        /// <summary>
        /// Get or set the normal message text color
        /// </summary>
        public Color NormalTextColor { get; set; }

        /// <summary>
        /// Get or set the warning message text color
        /// </summary>
        public Color WarningTextColor { get; set; }

        /// <summary>
        /// Get or set the error message text color
        /// </summary>
        public Color ErrorTextColor { get; set; }

        /// <summary>
        /// Get or set the normal message background color
        /// </summary>
        public Color NormalBackColor { get; set; }

        /// <summary>
        /// Get or set the warning message background color
        /// </summary>
        public Color WarningBackColor { get; set; }

        /// <summary>
        /// Get or set the error message background color
        /// </summary>
        public Color ErrorBackColor { get; set; }

        /// <summary>
        /// Get the instance of the colorer
        /// </summary>
        public static SledTtyMessageColorer Instance
        {
            get { return s_instance ?? (s_instance = new SledTtyMessageColorer()); }
        }

        private static SledTtyMessageColorer s_instance;
    }

    /// <summary>
    /// SLED TTY service interface
    /// </summary>
    public interface ISledTtyService
    {
        /// <summary>
        /// Display message in the TTY window
        /// </summary>
        /// <param name="message">Message text</param>
        void Write(SledTtyMessage message);

        /// <summary>
        /// Get whether input is enabled
        /// </summary>
        bool InputEnabled
        {
            get;
        }

        /// <summary>
        /// Clear TTY window
        /// </summary>
        void Clear();

        /// <summary>
        /// Register a language plugin with the TTY service
        /// </summary>
        /// <param name="languagePlugin">Language plugin to register</param>
        void RegisterLanguage(ISledLanguagePlugin languagePlugin);
    }
}
