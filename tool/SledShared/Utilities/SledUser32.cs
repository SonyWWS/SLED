/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

using System;
using System.Runtime.InteropServices;

namespace Sce.Sled.Shared.Utilities
{
    /// <summary>
    /// User32.dll wrapper
    /// </summary>
    public static class SledUser32
    {
        /// <summary>
        /// Windows SendMessage() function wrapper. 
        /// For more details, see http://msdn.microsoft.com/en-us/library/windows/desktop/ms644950%28v=vs.85%29.aspx.
        /// </summary>
        /// <param name="hWnd">Handle to window whose window procedure receives message</param>
        /// <param name="uMsg">Message to be sent</param>
        /// <param name="wParam">Additional message-specific information</param>
        /// <param name="lParam">Additional message-specific information</param>
        /// <returns>Result of message processing depending on message sent</returns>
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr SendMessage(IntPtr hWnd, UInt32 uMsg, IntPtr wParam, IntPtr lParam);

        /// <summary>
        /// Return a handle to foreground window
        /// </summary>
        /// <returns>Handle to foreground window</returns>
        [DllImport("user32.dll")]
        public static extern IntPtr GetForegroundWindow();

        /// <summary>
        /// Make a window flash in the tray
        /// <remarks>For more information, see http://msdn.microsoft.com/en-us/library/windows/desktop/ms679346%28v=vs.85%29.aspx. </remarks>
        /// </summary>
        /// <param name="handle">Window handle</param>
        /// <returns>Window's state before the call to the FlashWindow()</returns>
        public static bool FlashWindow(IntPtr handle)
        {
            return FlashWindowHelpers.Flash(handle, 3);
        }

        private static class FlashWindowHelpers
        {
            [DllImport("user32.dll")]
            [return: MarshalAs(UnmanagedType.Bool)]
            private static extern bool FlashWindowEx(ref FLASHWINFO pwfi);

            [StructLayout(LayoutKind.Sequential)]
            private struct FLASHWINFO
            {
                public uint cbSize;
                public IntPtr hwnd;
                public uint dwFlags;
                public uint uCount;
                public uint dwTimeout;
            }

            public static bool Flash(IntPtr handle, uint count)
            {
                if (Environment.OSVersion.Version.Major < 5)
                    return false;

                const uint FLASHW_ALL = 3;

                var fi = new FLASHWINFO();
                fi.cbSize = Convert.ToUInt32(Marshal.SizeOf(fi));
                fi.hwnd = handle;
                fi.dwFlags = FLASHW_ALL;
                fi.uCount = count;
                fi.dwTimeout = 0;

                return FlashWindowEx(ref fi);
            }
        }
    }
}