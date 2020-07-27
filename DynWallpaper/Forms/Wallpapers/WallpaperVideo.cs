namespace Maxstupo.DynWallpaper.Forms.Wallpapers {

    using System;
    using System.Windows.Forms;
    using HeyRed.Mime;
    using LibVLCSharp.Shared;
    using LibVLCSharp.WinForms;

    public sealed class WallpaperVideo : WallpaperBase {

        private readonly LibVLC libVLC;
        private readonly MediaPlayer player;

        private readonly VideoView videoView;

        public override float Volume { get => player.Volume / 100f; set => player.Volume = (int) (value * 100); }

        public override float Position { get => player.Position; set => player.Position = value; }

        public override bool IsPlaying => player.IsPlaying;

        public WallpaperVideo() {
            libVLC = new LibVLC();
            player = new MediaPlayer(libVLC);

            HandleCreated += delegate { player.Hwnd = Handle; };

            videoView = new VideoView {
                Dock = DockStyle.Fill,
                MediaPlayer = player
            };

            player.PositionChanged += Player_PositionChanged;
            player.Paused += Player_PlayStateChanged;
            player.Playing += Player_PlayStateChanged;

            Controls.Add(videoView);

            FormClosing += WallpaperVideo_FormClosing;
        }

        private void Player_PlayStateChanged(object sender, EventArgs e) {
            OnPlayingChanged();
        }

        private void Player_PositionChanged(object sender, MediaPlayerPositionChangedEventArgs e) {
            OnPositionChanged(e.Position);
        }

        private void WallpaperVideo_FormClosing(object sender, FormClosingEventArgs e) {
            player.Stop();
        }

        public override bool ApplyWallpaper() {
            if (!MimeTypesMap.GetMimeType(Filepath).StartsWith("video", StringComparison.InvariantCultureIgnoreCase))
                return false;

            player.Play(new Media(libVLC, Filepath, FromType.FromPath));

            return true;
        }

        public override void Toggle() {
            player.Pause();
        }

        protected override void Dispose(bool disposing) {
            if(disposing) 
                player.PositionChanged -= Player_PositionChanged;

            base.Dispose(disposing);

            if (disposing) {
              
                libVLC.Dispose();
                player.Dispose();
            }

        }

    }

}