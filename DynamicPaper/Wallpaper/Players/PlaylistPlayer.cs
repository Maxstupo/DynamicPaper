namespace Maxstupo.DynamicPaper.Wallpaper.Players {

    using System;
    using System.Diagnostics;
    using System.Windows.Forms;

    public enum LoopMode : int {
        None = 0,
        All = 1,
        Current = 2,
    }

    public enum ShuffleMode : int {
        None = 0,
        All = 1,
    }


    public class PlaylistPlayer : IPlaylistPlayer {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        private readonly Screen screen;

        public bool IsAttached => player?.IsAttached ?? false;
        public bool IsPlaying => player?.IsPlaying ?? false;
        public bool IsEnded => player?.IsEnded ?? false;

        public LoopMode LoopMode { get; set; }
        public ShuffleMode ShuffleMode { get; set; }

        public Playlist Playlist { get; } = new Playlist();

        public PlaylistItem PlayingMedia => player?.PlayingMedia;

        public TimeSpan Duration => player?.Duration ?? TimeSpan.Zero;
        public float Position => player?.Position ?? 0;


        public event EventHandler OnChanged;
        public event EventHandler<float> OnPositionChanged;

        private IAttachablePlayer player;


        public PlaylistPlayer(Screen screen) {
            this.screen = screen;
            Playlist.OnChange += Event_OnChange;
        }

        public void Play(PlaylistItem item = null) {

            if (item != null) {

                IAttachablePlayer newPlayer = MediaPlayerStore.Instance.CreatePlayer(item.MimeType, player);
                if (newPlayer == null) {
                    Logger.Warn($"Failed to find suitable player for {0}!", item.MimeType);
                    return;
                }

                if (player != null && player.GetType() != newPlayer.GetType()) {
                    Logger.Debug("Detaching previous player (as new player created)");

                    player.OnChanged -= Event_OnChange;
                    player.OnPositionChanged -= Player_OnPositionChanged;
                    player.Detach(false);

                }

                player = newPlayer;
                player.OnChanged += Event_OnChange;
                player.OnPositionChanged += Player_OnPositionChanged; ;
            }

            player.Attach(screen);
            player?.Play(item);
        }


        public void Pause() {
            player?.Pause();
        }

        public void Stop() {
            player?.Stop();

            foreach (PlaylistItem item in Playlist)
                item.IsPlaying = false;
        }

        public void Dispose() {
            if (player != null) {
                player.OnChanged -= Event_OnChange;
                player.OnPositionChanged -= Player_OnPositionChanged;
                player.Dispose();
            }
            player = null;
        }

        private void Event_OnChange(object sender, EventArgs e) {
            OnChanged?.Invoke(sender, e);
        }

        private void Player_OnPositionChanged(object sender, float position) {
            OnPositionChanged?.Invoke(sender, position);
        }

    }

}