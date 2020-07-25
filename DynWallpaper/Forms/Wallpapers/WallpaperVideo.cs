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

        public WallpaperVideo() {
            libVLC = new LibVLC();
            player = new MediaPlayer(libVLC);

            HandleCreated += delegate { player.Hwnd = Handle; };

            videoView = new VideoView {
                Dock = DockStyle.Fill,
                MediaPlayer = player
            };

            Controls.Add(videoView);

            FormClosing += WallpaperVideo_FormClosing;
        }

        private void WallpaperVideo_FormClosing(object sender, FormClosingEventArgs e) {
            player.Stop();
        }

        public override bool ApplyWallpaper(string filepath) {
            if (!MimeTypesMap.GetMimeType(filepath).StartsWith("video", StringComparison.InvariantCultureIgnoreCase))
                return false;

            player.Play(new Media(libVLC, filepath, FromType.FromPath));

            return true;
        }

        protected override void Dispose(bool disposing) {
            base.Dispose(disposing);

            if (disposing) {
                player.Dispose();
                libVLC.Dispose();
            }
        }

    }

}