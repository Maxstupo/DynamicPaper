namespace Maxstupo.DynamicPaper {

    using Maxstupo.DynamicPaper.Utility;
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
        }

        [JsonProperty("check_for_updates")] public bool CheckForUpdates { get; set; }

        [JsonProperty("start_with_windows")] public bool StartWithWindows { get; set; }

        [JsonProperty("start_minimized")] public bool StartMinimized { get; set; }


        [JsonProperty("close_to_tray")] public bool CloseToTray { get; set; }

        [JsonProperty("minimize_to_tray")] public bool MinimizeToTray { get; set; }


        [JsonProperty("restore_playing")] public bool RestorePlaying { get; set; }

        [JsonProperty("restore_playlists")] public bool RestorePlaylists { get; set; }

    }

}