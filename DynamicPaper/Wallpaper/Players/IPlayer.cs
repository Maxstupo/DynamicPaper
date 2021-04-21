namespace Maxstupo.DynamicPaper.Wallpaper.Players {

    using System;
    using System.Windows.Forms;

    public interface IMediaItem {

        string Filepath { get; }

        string MimeType { get; }

        int PreferredVolume { get; set; }

        bool IsLoaded { get; set; }

    }

    public interface IPlaylistItem : IMediaItem {

        int PlaylistIndex { get; set; }

    }


    public interface IPlayer : IDisposable {

        /// <summary>The current time position of the media between 0.0 and 1.0</summary>
        float Position { get; set; }

        /// <summary>The duration of the media.</summary>
        TimeSpan Duration { get; }

        /// <summary>The custom duration of the media. Will override the normal duration if not null.</summary>
        TimeSpan? CustomDuration { get; set; }

        int Volume { get; set; }

        /// <summary>True if the media is playing (eg. video is moving)</summary>
        bool IsPlaying { get; }

        /// <summary>True if the media has finished playing, and is still loaded.</summary>
        bool IsEnded { get; }

        /// <summary>The media item that is loaded/playing.</summary>
        IMediaItem Media { get; }

        /// <summary>Invoked when the state of this player changes.  Implementation makes no promises regarding what thread this event handler is invoked on.</summary>
        event EventHandler OnChanged;

        /// <summary>Invoked when the position of this player changes. Implementation makes no promises regarding what thread this event handler is invoked on.</summary>
        event EventHandler<float> OnPositionChanged;

        /// <summary>Play the specified media item, or if null is passed, restart the current loaded media.</summary>
        void Play(IMediaItem item = null);

        /// <summary>Toggle media playback. (Pause toggle)</summary>
        void Pause();

        /// <summary>Stop playing the media and unload it.</summary>
        void Stop();

    }


    public enum LoopMode : int {
        None = 0,
        All = 1,
        Current = 2,
    }

    public enum ShuffleMode : int {
        None = 0,
        All = 1,
    }

    public interface IPlaylistPlayer : IPlayer {

        Playlist Playlist { get; }

        LoopMode LoopMode { get; set; }

        ShuffleMode ShuffleMode { get; set; }



        void Play(IPlaylistItem item = null);

    }

    // TODO: Delete interface IAttachablePlayer and replace with abstract AttachablePlayer class
    public interface IAttachablePlayer : IPlayer {

        /// <summary>Mime types supported by this player. DO NOT SET ... managed by MediaPlayerStore when registering a attachable player.</summary>
        string[] SupportedMimeTypes { get; set; }

        /// <summary>True if this player is "attached" to a Screen, and the drawing surface is ready to be used.</summary>
        bool IsAttached { get; }

        /// <summary>Attaches the view onto the desktop background window, and sets the screen bounds of the view.</summary>
        void Attach(Screen screen);

        /// <summary>Detaches the view from the desktop background window.</summary>
        void Detach();

    }

}