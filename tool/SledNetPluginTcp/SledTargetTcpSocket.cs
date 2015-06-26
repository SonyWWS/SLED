/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

using System;
using System.Net.Sockets;
using System.Threading;

using Sce.Sled.Shared.Services;

namespace Sce.Sled.Net.Tcp
{
    /// <summary>
    /// High level client TCP socket</summary>
    class TargetTcpSocket
    {
        #region Events
        /// <summary>
        /// Event that is raised when the requested connection is established</summary>
        public event ConnectHandler Connected;

        /// <summary>
        /// Event that is raised when the connection is terminated</summary>
        public event ConnectHandler Disconnected;

        /// <summary>
        /// Event that is raised when data is ready to be read</summary>
        public event DataReadyHandler DataReady;

        /// <summary>
        /// Event that is raised when there is an unhandled exception</summary>
        public event ExceptonHandler UnHandledException;

        /// <summary>
        /// Callback when a connection is made</summary>
        /// <param name="sender">Sender</param>
        /// <param name="target">Target</param>
        public delegate void ConnectHandler(object sender, ISledTarget target);

        /// <summary>
        /// Callback when data is ready</summary>
        /// <param name="sender">Sender</param>
        /// <param name="buffer">Data</param>
        public delegate void DataReadyHandler(object sender, byte[] buffer);

        /// <summary>
        /// Callback when there is an unhandled exception</summary>
        /// <param name="sender">Sender</param>
        /// <param name="ex">Exception</param>
        public delegate void ExceptonHandler(object sender, Exception ex);

        #endregion

        /// <summary>
        /// Creates client tcp socket.</summary>
        public TargetTcpSocket()
            : this(5000)
        {
        }

        /// <summary>
        /// Creates client tcp socket</summary>
        /// <param name="maximumMessageSize">the maximum size of TCP/IP message payloads, in bytes</param>
        /// <remarks>Large blocks of data passed to Send() will be broken down into multiple separate
        /// messages of 'maximumMessageSize'.</remarks>
        public TargetTcpSocket(int maximumMessageSize)
        {
            m_cctx = SynchronizationContext.Current;
            if (m_cctx == null)
            {
                throw new Exception("The instance of this class can only be created on a thread"
                    + "that has WindowsFormsSynchronizationContext, ie GUI thread");
            }

            m_theSocket = null;
            m_curTarget = null;
            m_recieveClb = ReceiveClb;
            m_connectClb = ConnectClb;
            m_connectionInProgress = false;
            MessageSize = maximumMessageSize;
        }

        /// <summary>
        /// The maximum number of bytes of data to be sent at once on the TCP socket.</summary>
        public int MessageSize
        {
            get { return m_messageSize; }
            set
            {
                if (value < 1)
                    throw new ArgumentException("Invalid arg");
                lock (m_syncSocket)
                {
                    m_messageSize = value;
                }
            }
        }

        /// <summary>
        /// Gets a value indicating if the socket is connected.</summary>
        public bool IsConnected
        {
            get
            {
                if (m_theSocket == null)
                    return false;

                lock (m_syncSocket)
                {
                    // Must have lock or m_theSocket can be set to null in between condition statement
                    //  and getting the 'Connected' property.
// ReSharper disable ConditionIsAlwaysTrueOrFalse
                    return m_theSocket != null && m_theSocket.Connected;
// ReSharper restore ConditionIsAlwaysTrueOrFalse
                }
            }
        }

        /// <summary>
        /// Connects using IPEndPoint</summary>
        /// <param name="target">Target</param>
        public void Connect(ISledTarget target)
        {
            lock (m_syncSocket)
            {
                if (m_connectionInProgress)
                    return;
            }

            if (IsConnected)
                return;

            lock (m_syncSocket)
            {
                if (m_theSocket != null)
                {
                    m_theSocket.Close();
                    m_theSocket = null;
                    m_curTarget = null;
                }
            }

            // try to connect;
            try
            {
                lock (m_syncSocket)
                {
                    var ipaddr = target.EndPoint.Address;
                    m_theSocket = new Socket(ipaddr.AddressFamily, SocketType.Stream, ProtocolType.Tcp)
                    {
                        Blocking = true,
                        SendTimeout = 5000
                    };

                    //m_theSocket.ExclusiveAddressUse = true;
                    m_theSocket.BeginConnect(ipaddr, target.EndPoint.Port, m_connectClb, target);
                    m_connectionInProgress = true;
                }

            }
            catch (Exception ex)
            {
                lock (m_syncSocket)
                {
                    if (m_theSocket != null)
                    {
                        m_theSocket.Close();
                        m_theSocket = null;
                        m_curTarget = null;
                    }
                }
                OnUnHandledException(ex);
            }
        }

        /// <summary>
        /// Sends the whole array of bytes to the connected server, breaking it up into multiple
        /// messages if .</summary>
        /// <param name="data">the data to send</param>
        public void Send(byte[] data)
        {
            Send(data, data.Length);
        }

        /// <summary>
        /// Sends the data to the connected server, breaking it up into multiple messages if
        /// necessary.</summary>
        /// <param name="data">data to send</param>
        /// <param name="size">number of bytes to send, starting with data[0]</param>
        public void Send(byte[] data, int size)
        {
            try
            {
                lock (m_syncSocket)
                {
                    if (m_theSocket == null || !m_theSocket.Connected)
                        throw new Exception("The socket is not connected. Please use Connect method to establish a connection");

                    int i;
                    for (i = 0; i <= size - m_messageSize; i += m_messageSize)
                        m_theSocket.Send(data, i, m_messageSize, SocketFlags.None);

                    var remain = size - i;
                    if (remain > 0)
                        m_theSocket.Send(data, i, remain, SocketFlags.None);
                }
            }
            catch (Exception ex)
            {
                lock (m_syncSocket)
                {
                    if (m_theSocket != null && !m_theSocket.Connected)
                    {
                        m_theSocket.Close();
                        m_theSocket = null;
                        OnDisconnected();
                        m_curTarget = null;
                    }
                }
                OnUnHandledException(ex);
            }
        }

        /// <summary>
        /// Disconnects from the server.</summary>
        public void Disconnect()
        {
            if (IsConnected)
            {
                lock (m_syncSocket)
                {
                    m_theSocket.Shutdown(SocketShutdown.Both);
                    m_theSocket.Close();
                    m_theSocket = null;
                    OnDisconnected();
                    m_curTarget = null;
                }
            }
        }

        /// <summary>
        /// Start asynchronous receive</summary>
        public void StartReceive()
        {
            try
            {
                var buf = new byte[m_messageSize];

                if (IsConnected)
                {
                    lock (m_syncSocket)
                    {
                        m_theSocket.BeginReceive(buf, 0, m_messageSize, SocketFlags.None, m_recieveClb, buf);
                    }
                }
            }
            catch (Exception ex)
            {
                lock (m_syncSocket)
                {
                    if (m_theSocket != null && !m_theSocket.Connected)
                    {
                        m_theSocket.Close();
                        m_theSocket = null;
                        OnDisconnected();
                        m_curTarget = null;
                    }
                }
                OnUnHandledException(ex);
            }
        }


        #region Private Methods


        /// <summary>
        /// Receive call back</summary>
        /// <param name="ar">passed from beginreceive</param>
        private void ReceiveClb(IAsyncResult ar)
        {
            try
            {
                var data = ar.AsyncState as byte[];
                var nBytes = 0;
                if (IsConnected)
                {
                    lock (m_syncSocket)
                    {
                        nBytes = m_theSocket.EndReceive(ar);
                    }
                }
                if (nBytes > 0)
                {
                    var tmpBuf = new byte[nBytes];
                    Buffer.BlockCopy(data, 0, tmpBuf, 0, nBytes);
                    var dr = DataReady;
                    if (dr != null)
                    {
                        // raise data ready event on the gui thread.
                        m_cctx.Post(delegate
                        {
                            dr(this, tmpBuf);
                        }, null);
                    }
                }
                StartReceive();
            }
            catch (Exception ex)
            {
                var disconnected = false;
                lock (m_syncSocket)
                {
                    if (m_theSocket != null && !m_theSocket.Connected)
                    {
                        m_theSocket.Close();
                        m_theSocket = null;
                        disconnected = true;
                    }
                }
                if (disconnected)
                {
                    OnDisconnected();
                    m_curTarget = null;
                }
                OnUnHandledException(ex);
            }
        }

        private void ConnectClb(IAsyncResult ar)
        {
            try
            {
                lock (m_syncSocket)
                {
                    m_theSocket.EndConnect(ar);
                    m_connectionInProgress = false;
                }
                OnConnect((ISledTarget)ar.AsyncState);
                StartReceive();
            }
            catch (Exception ex)
            {
                lock (m_syncSocket)
                {
                    m_connectionInProgress = false;
                    if (m_theSocket != null)
                    {
                        m_theSocket.Close();
                        m_theSocket = null;
                        m_curTarget = null;
                    }
                }
                OnUnHandledException(ex);
            }

        }
        /// <summary>
        /// raise unhandle exception event on the subscriber's thread.
        /// if there is no subscriber then display message box.</summary>
        /// <param name="ex"></param>
        private void OnUnHandledException(Exception ex)
        {
            var handler = UnHandledException;
            if (handler == null)
                return;
            m_cctx.Send(delegate
            {
                handler(this, ex);
            }, null);

        }

        private void OnDisconnected()
        {
            var handler = Disconnected;
            lock (m_syncSocket)
            {
                m_connectionInProgress = false;
            }
            if (handler != null)
            {
                m_cctx.Send(delegate
                {
                    handler(this, m_curTarget);
                }, null);
            }
        }

        private void OnConnect(ISledTarget trg)
        {
            var handler = Connected;
            m_curTarget = trg;

            if (handler != null)
            {
                m_cctx.Send(delegate
                {
                    handler(this, trg);
                }, null);
            }
        }

        #endregion

        private volatile Socket m_theSocket;  // the only socket object.
        private volatile ISledTarget m_curTarget;   // the ip:port of the server for the current conneciton.
        private readonly AsyncCallback m_recieveClb;   // recieve callback.
        private readonly AsyncCallback m_connectClb;   // recieve callback.        
        private readonly SynchronizationContext m_cctx;
        private volatile object m_syncSocket = new object(); // used to synchronization.
        private int m_messageSize;
        private volatile bool m_connectionInProgress;
    }
}