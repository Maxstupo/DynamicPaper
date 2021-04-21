namespace Maxstupo.DynamicPaper.Wallpaper {

    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.IO;
    using HeyRed.Mime;
    using Maxstupo.DynamicPaper.Controls;
    using Maxstupo.DynamicPaper.Wallpaper.Players;
    using Newtonsoft.Json;

    public sealed class PlaylistItem : IPlaylistItem, IEquatable<PlaylistItem> {

        [JsonProperty]
        public string Filepath { get; private set; }

        public int PreferredVolume { get; set; } = 100;

        [JsonIgnore]
        public int PlaylistIndex { get; set; }

        [JsonIgnore]
        [ListBoxItemDisplayText]
        public string Name => Path.GetFileName(Filepath);

        [JsonIgnore]
        [ListBoxItemHighlighting]
        public bool IsLoaded { get; set; } = false;

        [JsonIgnore] public string MimeType => MimeTypesMap.GetMimeType(Filepath);

        [JsonIgnore] public bool HasNativeDuration => MimeType.StartsWith("video") || MimeType.StartsWith("audio");
        [JsonIgnore] public bool HasDuration => HasNativeDuration || CustomDuration.Ticks > 0;

        public TimeSpan CustomDuration { get; set; }

        public Color BackColor { get; set; } = Color.Black;

        public PlaylistItem() { }

        public PlaylistItem(string filepath) {
            this.Filepath = filepath;
        }

        public override bool Equals(object obj) {
            return Equals(obj as PlaylistItem);
        }

        public bool Equals(PlaylistItem other) {
            return other != null &&
                   this.Filepath == other.Filepath;
        }

        public override int GetHashCode() {
            return -1462308956 + EqualityComparer<string>.Default.GetHashCode(this.Filepath);
        }

        public static bool operator ==(PlaylistItem left, PlaylistItem right) {
            return EqualityComparer<PlaylistItem>.Default.Equals(left, right);
        }

        public static bool operator !=(PlaylistItem left, PlaylistItem right) {
            return !(left == right);
        }

    }

}