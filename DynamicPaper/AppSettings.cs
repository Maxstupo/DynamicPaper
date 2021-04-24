namespace Maxstupo.DynamicPaper {
    using System;
    using System.Collections.Generic;
    using Maxstupo.DynamicPaper.Utility;
    using Maxstupo.DynamicPaper.Wallpaper;
    using Newtonsoft.Json;

    public class AppSettings : ISettings {

        public void RestoreDefaults() {
            CheckForUpdates = false;

            StartWithWindows = false;
            StartMinimized = false;

            CloseToTray = false;
            MinimizeToTray = true;

            RestorePlaying = false;
            RestorePlaylists = false;
            ShowTimeLeft = true;
        }

        [JsonProperty("check_for_updates")] public bool CheckForUpdates { get; set; }

        [JsonProperty("start_with_windows")] public bool StartWithWindows { get; set; }

        [JsonProperty("start_minimized")] public bool StartMinimized { get; set; }


        [JsonProperty("close_to_tray")] public bool CloseToTray { get; set; }

        [JsonProperty("minimize_to_tray")] public bool MinimizeToTray { get; set; }


        [JsonProperty("restore_playing")] public bool RestorePlaying { get; set; }

        [JsonProperty("restore_playlists")] public bool RestorePlaylists { get; set; }

        [JsonProperty("show_time_remaining")]
        public bool ShowTimeLeft { get; set; }

        [JsonProperty("default_media_duration")]
        public TimeSpan DefaultMediaDuration { get; set; } = TimeSpan.FromSeconds(5);

        [JsonProperty("monitor_playlists")]
        public Dictionary<int, Playlist> Playlists { get; } = new Dictionary<int, Playlist>();


    }

}