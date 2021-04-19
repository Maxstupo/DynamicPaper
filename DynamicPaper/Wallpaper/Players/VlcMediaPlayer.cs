namespace Maxstupo.DynamicPaper.Wallpaper.Players {

    using System;
    using System.Windows.Forms;
    using LibVLCSharp.Shared;
    using LibVLCSharp.WinForms;
    using Maxstupo.DynamicPaper.Utility.Windows;

    public class VlcMediaPlayer : IAttachablePlayer {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        public static readonly LibVLC LibVLC = new LibVLC();

        public string[] SupportedMimeTypes { get; set; }

        private bool isAttached;
        public bool IsAttached { get => isAttached; private set { isAttached = value; NotifyChange(); } }
        public bool IsPlaying => IsAttached && vlcPlayer.IsPlaying;
        public bool IsEnded => IsAttached && vlcPlayer.State == VLCState.Ended;

        public PlaylistItem PlayingMedia { get; private set; }

        public TimeSpan Duration => IsAttached ? TimeSpan.FromMilliseconds(vlcPlayer.Length) : TimeSpan.Zero;
        public float Position => IsAttached ? vlcPlayer.Position : 0;

        private MediaPlayer vlcPlayer;
        private Control view;

        public event EventHandler OnChanged;
        public event EventHandler<float> OnPositionChanged;

        public void Attach(Screen screen) {
            if (IsAttached)
                return;

            vlcPlayer = new MediaPlayer(LibVLC) { EnableHardwareDecoding = true };
            vlcPlayer.EndReached += VlcPlayer_EndReached;
            vlcPlayer.Paused += VlcPlayer_Change;
            vlcPlayer.Playing += VlcPlayer_Change;
            vlcPlayer.PositionChanged += VlcPlayer_PositionChanged;

            view = new VideoView { MediaPlayer = vlcPlayer };

            view.Show();
            view.Bounds = WindowsWallpaper.GetScreenBounds(screen);
            WindowsWallpaper.SetParent(view);

            IsAttached = true;
        }

        private void VlcPlayer_PositionChanged(object sender, MediaPlayerPositionChangedEventArgs e) {
            OnPositionChanged?.Invoke(this, e.Position);
        }

        private void VlcPlayer_Change(object sender, EventArgs e) {
            NotifyChange();
        }

        private void VlcPlayer_EndReached(object sender, EventArgs e) {
            NotifyChange();//TODO: Add playlist looping and shuffling.
        }

        public void Detach(bool resetBackground) {
            if (!IsAttached)
                return;

            view?.Hide();
            view?.Dispose();
            view = null;

            vlcPlayer?.Dispose();
            vlcPlayer = null;

            Logger.Trace("Disposing vlc player...");

            if (resetBackground)
                WindowsWallpaper.ResetDesktopBackground();

            IsAttached = false;
        }

        public void Play(PlaylistItem item = null) {
            Media media = item != PlayingMedia && item != null ? new Media(LibVLC, item.Filepath, FromType.FromPath) : vlcPlayer.Media;

            if (vlcPlayer.Play(media)) {
                if (PlayingMedia != null && item != null)
                    PlayingMedia.IsPlaying = false;

                if (item != null)
                    PlayingMedia = item;


                if (item != null)
                    item.IsPlaying = true;
            }
        }

        public void Pause() {
            if (IsAttached)
                vlcPlayer.Pause();
        }

        public void Stop() {
            Detach(true);
        }

        public void Dispose() {
            Detach(false);
        }

        private void NotifyChange() {
            OnChanged?.Invoke(this, EventArgs.Empty);
        }

    }

}