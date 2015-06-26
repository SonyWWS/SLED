/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

using System;
using System.Drawing;
using System.Net;
using System.Windows.Forms;
using System.Xml;

using Sce.Sled.Shared.Services;

namespace Sce.Sled.Shared.Plugin
{
    /// <summary>
    /// Delegate representing a connection to a target event
    /// </summary>
    /// <param name="sender">Object that triggered the event</param>
    /// <param name="target">ISledTarget of event</param>
    public delegate void ConnectionHandler(object sender, ISledTarget target);

    /// <summary>
    /// Delegate representing data received from the target
    /// </summary>
    /// <param name="sender">Object that triggered the event</param>
    /// <param name="buffer">Data received</param>
    public delegate void DataReadyHandler(object sender, byte[] buffer);

    /// <summary>
    /// Delegate representing an unhandled exception event
    /// </summary>
    /// <param name="sender">Object that triggered the event</param>
    /// <param name="ex">Unhandled Exception</param>
    public delegate void UnHandledExceptionHandler(object sender, Exception ex);

    /// <summary>
    /// Base network plugin interface for SLED
    /// <remarks>Derived from IDisposable, so a Dispose() method is needed as well that 
    /// gets called by SLED after disconnecting from a target or if an unhandled 
    /// exception event is triggered and received by SLED.</remarks>
    /// </summary>
    public interface ISledNetworkPlugin : IDisposable, ISledNetworkPluginPersistedSettings
    {
        /// <summary>
        /// Make a connection to a target 
        /// <remarks>Upon successful connection to a target, the
        /// ConnectedEvent event should be triggered, or if an error
        /// occurs, the UnHandledExceptionEvent should be triggered.</remarks>
        /// </summary>
        /// <param name="target">ISledTarget</param>
        void Connect(ISledTarget target);

        /// <summary>
        /// Get whether or not the plugin is connected to a target 
        /// <remarks>This property should not lock or rely on locking mechanisms, because it 
        /// is used frequently when the status of GUI elements is updated to reflect 
        /// the connection state.</remarks> 
        /// </summary>
        bool IsConnected
        {
            get;
        }

        /// <summary>
        /// Disconnect from the target 
        /// <remarks>This method should trigger the DisconnectedEvent after successful 
        /// disconnection or trigger the UnHandledExceptionEvent if an error occurs 
        /// while trying to disconnect.</remarks> 
        /// </summary>
        void Disconnect();

        /// <summary>
        /// Send data to the target
        /// </summary>
        /// <param name="buffer">Data to send</param>
        /// <returns>Length of data sent or -1 if failure</returns>
        int Send(byte[] buffer);

        /// <summary>
        /// Send data to the target
        /// <remarks>Allow specification of length of buffer</remarks>
        /// </summary>
        /// <param name="buffer">Data to send</param>
        /// <param name="length">Length of data to send</param>
        /// <returns>Length of data sent or -1 if failure</returns>
        int Send(byte[] buffer, int length);

        /// <summary>
        /// Event to trigger after connecting to a target
        /// </summary>
        event ConnectionHandler ConnectedEvent;

        /// <summary>
        /// Event to trigger after disconnecting from a target
        /// </summary>
        event ConnectionHandler DisconnectedEvent;

        /// <summary>
        /// Event to trigger when data has been received from the target
        /// </summary>
        event DataReadyHandler DataReadyEvent;

        /// <summary>
        /// Event to trigger when an unhandled exception has occurred
        /// </summary>
        event UnHandledExceptionHandler UnHandledExceptionEvent;

        /// <summary>
        /// Get name of plugin
        /// <remarks>Name can be any string</remarks>
        /// </summary>
        string Name
        {
            get;
        }

        /// <summary>
        /// Get protocol that plugin uses
        /// <remarks>This should be a simple string, such as "TCP", etc.</remarks>
        /// </summary>
        string Protocol
        {
            get;
        }

        /// <summary>
        /// Create a settings control based on a specific target
        /// </summary>
        /// <param name="target">Target to build settings from or null if no target created yet</param>
        /// <returns>Settings control</returns>
        SledNetworkPluginTargetFormSettings CreateSettingsControl(ISledTarget target);

        /// <summary>
        /// Create and set up a new target
        /// </summary>
        /// <param name="name">Name of the target</param>
        /// <param name="endPoint">IP address and port of the target</param>
        /// <param name="settings">Optional settings to pass in</param>
        /// <returns>New target or null if an error occurred</returns>
        ISledTarget CreateAndSetup(string name, IPEndPoint endPoint, params object[] settings);

        /// <summary>
        /// Get an array of any automatically generated targets
        /// <remarks>Some network plugins may be able to notify SLED of targets
        /// based on outside software.</remarks>
        /// </summary>
        ISledTarget[] ImportedTargets { get; }

        /// <summary>
        /// Get unique identifier for the network plugin
        /// </summary>
        Guid PluginGuid { get; }
    }

    /// <summary>
    /// Interface for SLED network plugin persisted settings
    /// </summary>
    public interface ISledNetworkPluginPersistedSettings
    {
        /// <summary>
        /// Write a target to an XmlElement
        /// </summary>
        /// <param name="target">Target being saved</param>
        /// <param name="elem">XmlElement to write data to</param>
        /// <returns>True iff successful</returns>
        bool Save(ISledTarget target, XmlElement elem);

        /// <summary>
        /// Read a target from an XmlElement
        /// </summary>
        /// <param name="target">Target to fill in from the XmlElement</param>
        /// <param name="elem">XmlElement to read from</param>
        /// <returns>True iff successful</returns>
        bool Load(out ISledTarget target, XmlElement elem);
    }

    /// <summary>
    /// Interface for SLED network plugin target form customizer
    /// </summary>
    public interface ISledNetworkPluginTargetFormCustomizer
    {
        /// <summary>
        /// Get whether the protocol displays in the protocol combo box
        /// </summary>
        bool AllowProtocolOption { get; }
    }

    /// <summary>
    /// SLED network targets form render arguments class
    /// </summary>
    public class SledNetworkTargetsFormRenderArgs
    {
        /// <summary>
        /// Constructor with parameters
        /// </summary>
        /// <param name="gfx">GDI+ drawing surface</param>
        /// <param name="bounds">Bounding rectangle for drawing</param>
        /// <param name="font">Font</param>
        /// <param name="selected">Whether item selected</param>
        /// <param name="item">ISledTarget item being rendered</param>
        /// <param name="textColor">Text color</param>
        /// <param name="highlightTextColor">Highlight text color</param>
        /// <param name="highlightBackColor">Highlight background color</param>
        public SledNetworkTargetsFormRenderArgs(Graphics gfx, Rectangle bounds, Font font, bool selected, ISledTarget item, Color textColor, Color highlightTextColor, Color highlightBackColor)
        {
            Graphics = gfx;
            Bounds = bounds;
            Font = font;
            Selected = selected;
            Item = item;
            DrawDefault = false;
            TextColor = textColor;
            HighlightTextColor = highlightTextColor;
            HighlightBackColor = highlightBackColor;
        }

        /// <summary>
        /// Get Graphics device (GDI+ drawing surface)
        /// </summary>
        public Graphics Graphics { get; private set; }

        /// <summary>
        /// Get bounds
        /// </summary>
        public Rectangle Bounds { get; private set; }

        /// <summary>
        /// Get font
        /// </summary>
        public Font Font { get; private set; }

        /// <summary>
        /// Get whether the item is selected or not
        /// </summary>
        public bool Selected { get; private set; }

        /// <summary>
        /// Get the ISledTarget item being rendered
        /// </summary>
        public ISledTarget Item { get; private set; }

        /// <summary>
        /// Get or set whether normal drawing should take place
        /// </summary>
        public bool DrawDefault { get; set; }

        /// <summary>
        /// Get or set the text color
        /// </summary>
        public Color TextColor { get; set; }

        /// <summary>
        /// Get or set the highlight text color
        /// </summary>
        public Color HighlightTextColor { get; set; }

        /// <summary>
        /// Get or set the highlight background color
        /// </summary>
        public Color HighlightBackColor { get; set; }
    }

    /// <summary>
    /// SLED network targets form renderer interface
    /// </summary>
    public interface ISledNetworkTargetsFormRenderer
    {
        /// <summary>
        /// Draw the name of the target
        /// </summary>
        /// <param name="e">Render arguments</param>
        void DrawName(SledNetworkTargetsFormRenderArgs e);

        /// <summary>
        /// Draw the IP address
        /// </summary>
        /// <param name="e">Render arguments</param>
        void DrawHost(SledNetworkTargetsFormRenderArgs e);

        /// <summary>
        /// Draw the port
        /// </summary>
        /// <param name="e">Render arguments</param>
        void DrawPort(SledNetworkTargetsFormRenderArgs e);
    }

    /// <summary>
    /// A custom settings control that fits into the SledTargetForm form when adding a new
    /// or editing an existing target
    /// </summary>
    public abstract class SledNetworkPluginTargetFormSettings : UserControl
    {
        /// <summary>
        /// Determine whether the current settings are valid or not
        /// </summary>
        /// <param name="errorMsg">Error message to display if there is an error</param>
        /// <param name="errorControl">Control to focus on if there is an error</param>
        /// <returns>True iff settings contain errors</returns>
        public abstract bool ContainsErrors(out string errorMsg, out Control errorControl);

        /// <summary>
        /// Get a BLOB of data that contains the additional (with respect to ISledTarget) settings
        /// </summary>
        /// <returns>A BLOB of all additional settings</returns>
        public abstract object[] GetDataBlob();

        /// <summary>
        /// Get the protocol's default port number
        /// </summary>
        public abstract int DefaultPort { get; }
    }
}
