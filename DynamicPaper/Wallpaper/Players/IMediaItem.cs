namespace Maxstupo.DynamicPaper.Wallpaper.Players {

    using System;
    using System.Drawing;

    public interface IMediaItem {

        string Filepath { get; }

        string MimeType { get; }

        int PreferredVolume { get; set; }

        /// <summary>True if this media item has a native duration (e.g. video or audio)</summary>
        bool HasNativeDuration { get; }

        /// <summary>True if this media has a duration.</summary>
        bool HasDuration { get; }

        /// <summary>The custom duration of the media. Will override the native duration if above zero.</summary>
        TimeSpan CustomDuration { get; set; }

        /// <summary>The color of the background when the media doesn't take up the full screen.</summary>
        Color BackColor { get; set; }

        bool IsLoaded { get; set; }

    }

    public interface IPlaylistItem : IMediaItem {

        int PlaylistIndex { get; set; }

    }

}