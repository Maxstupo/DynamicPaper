namespace Maxstupo.DynamicPaper.Utility {

    using System;
    using System.Windows.Forms;
    using Maxstupo.DynamicPaper.Utility.Windows;

    public sealed class ScreenInfo {


        public Screen Screen { get; }

        public string DisplayName { get; }
        public int Index { get; }

        public ScreenInfo(Screen screen, int index = -1) {
            this.Index = index;
            this.Screen = screen ?? throw new ArgumentNullException(nameof(screen));

            string name = $"{screen.DeviceFriendlyName()}{(screen.Primary ? " (Primary)" : string.Empty)}";
            DisplayName = (index < 0) ? name : $"{index}: {name}";
        }


    }

}