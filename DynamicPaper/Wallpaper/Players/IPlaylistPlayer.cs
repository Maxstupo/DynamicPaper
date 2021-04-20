namespace Maxstupo.DynamicPaper.Wallpaper.Players {

    using System;
    using System.Windows.Forms;

    public interface IPlayer : IDisposable {

        float Position { get; set; }
        TimeSpan Duration { get; }
        int Volume { get; set; }

        bool IsAttached { get; }
        bool IsPlaying { get; }
        bool IsEnded { get; }

        PlaylistItem PlayingMedia { get; }

        event EventHandler OnChanged;
        event EventHandler<float> OnPositionChanged;

        void Play(PlaylistItem item = null);

        void Pause();

        void Stop();

    }

    public interface IPlaylistPlayer : IPlayer {


        Playlist Playlist { get; }

        LoopMode LoopMode { get; set; }

        ShuffleMode ShuffleMode { get; set; }

    }

    public interface IAttachablePlayer : IPlayer {

        string[] SupportedMimeTypes { get; set; }

        void Attach(Screen screen);

        void Detach(bool resetBackground);

    }

}