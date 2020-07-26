namespace Maxstupo.DynWallpaper.Controls {

    using System;
    using System.Windows.Forms;

    // https://github.com/Maxstupo/ydl-ui/blob/master/YDL-UI/Controls/TablessTabControl.cs
    public class TablessTabControl : TabControl {

        protected override void WndProc(ref Message m) {
            // Hide tabs by trapping the TCM_ADJUSTRECT message
            if (m.Msg == 0x1328 && !DesignMode)
                m.Result = (IntPtr) 1;
            else
                base.WndProc(ref m);
        }

    }

}