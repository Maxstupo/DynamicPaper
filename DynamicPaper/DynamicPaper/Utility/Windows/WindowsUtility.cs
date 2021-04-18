namespace Maxstupo.DynamicPaper.Utility.Windows {

    using System;
    using System.Windows.Forms;
    using Microsoft.Win32;

    public static class WindowsUtility {

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
                Console.Error.WriteLine(e);
                return false;
            }
        }

    }

}