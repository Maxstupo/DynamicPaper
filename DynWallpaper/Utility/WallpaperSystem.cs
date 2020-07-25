namespace Maxstupo.DynWallpaper.Utility {

    using System;
    using System.Windows.Forms;
    using Maxstupo.DynWallpaper.Utility.Windows;
    using Microsoft.Win32;

    public static class WallpaperSystem {

        private static IntPtr workerw;

        public static bool Init() {
            if (workerw != IntPtr.Zero)
                return false;

            // Fetch the Progman window
            IntPtr progman = NativeMethods.FindWindow("Progman", null);

            // Send 0x052C to Progman. This message directs Progman to spawn a 
            // WorkerW behind the desktop icons. If it is already there, nothing happens.
            NativeMethods.SendMessageTimeout(progman, 0x052C, IntPtr.Zero, IntPtr.Zero, SendMessageTimeoutFlags.SMTO_NORMAL, 1000, out IntPtr result);


            // We enumerate all Windows, until we find one, that has the SHELLDLL_DefView as a child. 
            // If we found that window, we take its next sibling and assign it to workerw.
            workerw = IntPtr.Zero;
            NativeMethods.EnumWindows((tophandle, topparamhandle) => {

                IntPtr p = NativeMethods.FindWindowEx(tophandle, IntPtr.Zero, "SHELLDLL_DefView", string.Empty);

                if (p != IntPtr.Zero) // Gets the WorkerW Window after the current one.
                    workerw = NativeMethods.FindWindowEx(IntPtr.Zero, tophandle, "WorkerW", string.Empty);
                
                return true;

            }, IntPtr.Zero);

            return workerw != IntPtr.Zero;
        }

        public static void SetParent(Form form) {
            if (workerw == IntPtr.Zero)
                throw new InvalidOperationException("WallpaperSystem.Init() must be called before setting form parent!");

            NativeMethods.SetParent(form.Handle, workerw);
        }

        /// <summary>
        /// Sets the desktop background image to the currently set one. Essentially refreshing/redrawing the desktop.
        /// </summary>
        public static void ResetDesktopBackground() {
            RegistryKey rkCurrentUser = Registry.CurrentUser;
            RegistryKey rkControlPanel = rkCurrentUser.OpenSubKey("Control Panel");
            RegistryKey rkDesktop = rkControlPanel.OpenSubKey("Desktop");

            string path = Convert.ToString(rkDesktop.GetValue("Wallpaper"));

            NativeMethods.SystemParametersInfo(SPI.SETDESKWALLPAPER, 0, path, SPIF.SENDCHANGE);
        }

    }

}