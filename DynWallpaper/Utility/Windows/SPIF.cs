namespace Maxstupo.DynWallpaper.Utility.Windows {

    using System;

    // Data structure and information from https://www.pinvoke.net/
    [Flags]
    public enum SPIF : uint {

        None = 0x00,

        /// <summary>Writes the new system-wide parameter setting to the user profile.</summary>
        UPDATEINIFILE = 0x01,

        /// <summary>Broadcasts the WM_SETTINGCHANGE message after updating the user profile.</summary>
        SENDCHANGE = 0x02,

        /// <summary>Same as SPIF.SENDCHANGE.</summary>
        SENDWININICHANGE = 0x02

    }

}