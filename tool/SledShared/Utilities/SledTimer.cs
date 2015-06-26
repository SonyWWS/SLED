/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

using System;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace Sce.Sled.Shared.Utilities
{
    /// <summary>
    /// SLED Hi Performance Timer Class
    /// </summary>
    public class SledHiPerfTimer
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public SledHiPerfTimer()
        {
            try
            {
                if (!QueryPerformanceFrequency(out m_freq))
                    throw new Win32Exception("QueryPerformanceFrequency");
            }
            catch (Exception)
            {
                m_freq = 0;
            }
        }

        /// <summary>
        /// Start the timer
        /// </summary>
        public void Start()
        {
            try
            {
                if (!QueryPerformanceCounter(out m_start))
                    throw new Win32Exception("QueryPerformanceCounter");
            }
            catch (Exception)
            {
                m_start = 0;
            }
        }

        /// <summary>
        /// Reset the timer
        /// </summary>
        public void Reset()
        {
            Start();
        }

        /// <summary>
        /// Get the timer duration
        /// </summary>
        /// <returns>Elapsed time</returns>
        public double Elapsed
        {
            get
            {
                try
                {
                    long end;
                    if (!QueryPerformanceCounter(out end))
                        throw new Win32Exception("QueryPerformanceCounter");

                    return ((double)end - (double)m_start) / (double)m_freq;
                }
                catch (Exception)
                {
                    return 0.0;
                }
            }
        }

        private long m_start;

        private readonly long m_freq;

        [DllImport("KERNEL32")]
        private static extern bool QueryPerformanceCounter(out long lpPerformanceCount);

        [DllImport("Kernel32.dll")]
        private static extern bool QueryPerformanceFrequency(out long lpFrequency);

        /// <summary>
        /// Outline wrapper Class
        /// </summary>
        /// <remarks>Convenience to wrap timer in using statement
        /// and have elapsed time displayed at the end</remarks>
        public class OutlineWrapper : IDisposable
        {
            /// <summary>
            /// Constructor
            /// </summary>
            public OutlineWrapper()
                : this(string.Empty)
            {
            }

            /// <summary>
            /// Constructor with parameters
            /// </summary>
            /// <param name="format">Optional text to display</param>
            /// <param name="args">Optional parameters</param>
            public OutlineWrapper(string format, params object[] args)
                : this(SledMessageType.Info, format, args)
            {
            }

            /// <summary>
            /// Constructor with parameters
            /// </summary>
            /// <param name="messageType">Message type</param>
            /// <param name="format">Optional text to display</param>
            /// <param name="args">Optional parameters</param>
            public OutlineWrapper(SledMessageType messageType, string format, params object[] args)
            {
                m_timer = new SledHiPerfTimer();
                m_timer.Start();

                m_message = string.Format(format, args);
                m_messageType = messageType;
            }

            /// <summary>
            /// Perform application-defined tasks associated with freeing, releasing, or
            /// resetting unmanaged resources</summary>
            public void Dispose()
            {
                SledOutDevice.OutLine(m_messageType, "{0} ({1} seconds)", m_message, m_timer.Elapsed);
            }
            
            private readonly string m_message;
            private readonly SledHiPerfTimer m_timer;
            private readonly SledMessageType m_messageType;
        }
    }
}