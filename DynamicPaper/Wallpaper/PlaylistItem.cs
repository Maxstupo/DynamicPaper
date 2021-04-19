namespace Maxstupo.DynamicPaper.Wallpaper {

    using System.IO;
    using Maxstupo.DynamicPaper.Controls;
    using Newtonsoft.Json;

    public sealed class PlaylistItem {

        public string Filepath { get; }

        public int Volume { get; set; } = 100;

        [JsonIgnore]
        [ListBoxItemDisplayText]
        public string Name => Path.GetFileName(Filepath);

        [JsonIgnore]
        [ListBoxItemHighlighting]
        public bool IsPlaying { get; set; } = false;

        [JsonIgnore]
        public string MimeType { get; }

        public PlaylistItem(string filepath) {
            this.Filepath = filepath;
        }

        public PlaylistItem(string filepath, string mimeType) : this(filepath) {
            this.MimeType = mimeType;
        }
        public override string ToString() {
            return Name;
        }
    }

}