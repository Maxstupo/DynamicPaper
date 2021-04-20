namespace Maxstupo.DynamicPaper.Wallpaper {

    using System;
    using System.Collections.Generic;
    using System.IO;
    using HeyRed.Mime;
    using Maxstupo.DynamicPaper.Controls;
    using Newtonsoft.Json;

    public sealed class PlaylistItem : IEquatable<PlaylistItem> {

        [JsonProperty]
        public string Filepath { get; private set; }

        public int Volume { get; set; } = 100;

        [JsonIgnore]
        [ListBoxItemDisplayText]
        public string Name => Path.GetFileName(Filepath);

        [JsonIgnore]
        [ListBoxItemHighlighting]
        public bool IsPlaying { get; set; } = false;

        [JsonIgnore]
        public string MimeType => MimeTypesMap.GetMimeType(Filepath);

        [JsonIgnore]
        public bool FromPlaylist { get; set; } = true;

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