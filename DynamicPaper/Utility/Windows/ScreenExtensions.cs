namespace Maxstupo.DynamicPaper.Utility.Windows {

    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using System.Runtime.InteropServices;
    using System.Windows.Forms;

    // https://stackoverflow.com/a/28257839
    public static class ScreenExtensions {
        private const int ERROR_SUCCESS = 0;

        private static string MonitorFriendlyName(LUID adapterId, uint targetId) {
            DISPLAYCONFIG_TARGET_DEVICE_NAME deviceName = new DISPLAYCONFIG_TARGET_DEVICE_NAME {
                header = {
                    size = (uint)Marshal.SizeOf(typeof (DISPLAYCONFIG_TARGET_DEVICE_NAME)),
                    adapterId = adapterId,
                    id = targetId,
                    type = DISPLAYCONFIG_DEVICE_INFO_TYPE.DISPLAYCONFIG_DEVICE_INFO_GET_TARGET_NAME
                }
            };
            int error = NativeMethods.DisplayConfigGetDeviceInfo(ref deviceName);
            if (error != ERROR_SUCCESS)
                throw new Win32Exception(error);
            return deviceName.monitorFriendlyDeviceName;
        }

        private static IEnumerable<string> GetAllMonitorsFriendlyNames() {
            int error = NativeMethods.GetDisplayConfigBufferSizes(QUERY_DEVICE_CONFIG_FLAGS.QDC_ONLY_ACTIVE_PATHS, out uint pathCount, out uint modeCount);
            if (error != ERROR_SUCCESS)
                throw new Win32Exception(error);

            DISPLAYCONFIG_PATH_INFO[] displayPaths = new DISPLAYCONFIG_PATH_INFO[pathCount];
            DISPLAYCONFIG_MODE_INFO[] displayModes = new DISPLAYCONFIG_MODE_INFO[modeCount];
            error = NativeMethods.QueryDisplayConfig(QUERY_DEVICE_CONFIG_FLAGS.QDC_ONLY_ACTIVE_PATHS, ref pathCount, displayPaths, ref modeCount, displayModes, IntPtr.Zero);
            if (error != ERROR_SUCCESS)
                throw new Win32Exception(error);

            for (int i = 0; i < modeCount; i++) {
                if (displayModes[i].infoType == DISPLAYCONFIG_MODE_INFO_TYPE.DISPLAYCONFIG_MODE_INFO_TYPE_TARGET)
                    yield return MonitorFriendlyName(displayModes[i].adapterId, displayModes[i].id);
            }
        }

        public static string DeviceFriendlyName(this Screen screen) {
            IEnumerable<string> allFriendlyNames = GetAllMonitorsFriendlyNames();

            for (int i = 0; i < Screen.AllScreens.Length; i++) {
                if (screen == Screen.AllScreens[i])
                    return allFriendlyNames.ElementAt(i);
            }
            return null;
        }

    }

}