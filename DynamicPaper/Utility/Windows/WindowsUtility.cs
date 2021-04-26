namespace Maxstupo.DynamicPaper.Utility.Windows {

    using System;
    using System.Windows.Forms;
    using Microsoft.Win32;

    public static class WindowsUtility {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        public static bool StartApplicationWithWindows(string applicationId, bool enabled) {
            try {
                RegistryKey key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true);

                if (enabled) {
                    key.SetValue(applicationId, $"\"{Application.ExecutablePath}\" -resume");
                } else {
                    key.DeleteValue(applicationId, false);
                }
                return true;
            } catch (Exception e) {
                Logger.Error("Failed setting the start with windows registry key 'HKEY_CURRENT_USER\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run\\{0}'", applicationId, e);
                return false;
            }
        }

    }

}