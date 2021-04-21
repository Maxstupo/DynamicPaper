namespace Maxstupo.DynamicPaper.Wallpaper.Players.Impl {
    using System;
    using System.Threading;
    using System.Windows.Forms;
    using LibVLCSharp.Shared;
    using LibVLCSharp.WinForms;

    public static class Vlc {
        public static LibVLC LibVLC { get; private set; }

        public static void Initialize() {
            Core.Initialize();
            LibVLC = new LibVLC();
        }
    }

    public sealed class VlcMediaPlayer : AttachablePlayer<VideoView> {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        public override float Position { get => vlcPlayer.Position; set => vlcPlayer.Position = value; }
        public override TimeSpan Duration { get => TimeSpan.FromMilliseconds(vlcPlayer.Length); protected set => throw new NotSupportedException(); }
        public override TimeSpan? CustomDuration { get; set; }

        public override int Volume { get => vlcPlayer.Volume; set => vlcPlayer.Volume = value; }

        public override bool IsPlaying { get => vlcPlayer.IsPlaying; protected set => throw new NotSupportedException(); }
        public override bool IsEnded { get => vlcPlayer.State == VLCState.Ended; protected set => throw new NotSupportedException(); }

        private MediaPlayer vlcPlayer; // The actual VLC media player from LibVLCSharp.


        public override VideoView CreateView(Screen screen) {
            vlcPlayer = new MediaPlayer(Vlc.LibVLC) {
                EnableHardwareDecoding = true
            };
            vlcPlayer.Playing += VlcPlayer_StateChanged;
            vlcPlayer.Paused += VlcPlayer_StateChanged;
            vlcPlayer.Stopped += VlcPlayer_StateChanged;
            vlcPlayer.EndReached += VlcPlayer_EndReached;
            vlcPlayer.PositionChanged += VlcPlayer_PositionChanged;

            return new VideoView {
                MediaPlayer = vlcPlayer
            };
        }

        private void VlcPlayer_PositionChanged(object sender, MediaPlayerPositionChangedEventArgs e) {
            NotifyOnPositionChanged(e.Position); // MediaPlayer events are invoked from a different thread. 
        }

        private void VlcPlayer_EndReached(object sender, EventArgs e) {
            NotifyOnChanged(); // MediaPlayer events are invoked from a different thread. 
        }

        private void VlcPlayer_StateChanged(object sender, EventArgs e) {
            Logger.Debug("VlcPlayer_StateChanged: {0}", IsPlaying);
            NotifyOnChanged(); // MediaPlayer events are invoked from a different thread. 
        }

        protected override void PlayMedia(IMediaItem item = null) {
            Media vlcMedia = item != null ? new Media(Vlc.LibVLC, item.Filepath, FromType.FromPath) : vlcPlayer.Media;

            if (vlcPlayer.Play(vlcMedia)) {
                if (item != null)
                    Media = item;
            }
        }

        protected override void PauseMedia() {
            vlcPlayer.Pause();
        }

        protected override void StopMedia() {
            Media = null;
            vlcPlayer.Stop();
        }

        protected override void Dispose(bool disposing) {
            if (disposing) {
                Logger.Trace("Disposing media player...");

                vlcPlayer.Dispose();
                vlcPlayer = null;

            }
            base.Dispose(disposing);
        }

    }
}
