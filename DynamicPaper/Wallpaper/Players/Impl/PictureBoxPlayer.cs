namespace Maxstupo.DynamicPaper.Wallpaper.Players.Impl {

    using System;
    using System.Windows.Forms;

    public sealed class PictureBoxPlayer : AttachablePlayer<PictureBox> {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        private const int UpdateRate = 250;

        public override float Position { get => time / (float) Duration.TotalMilliseconds; set => time = (int) (value * (float) Duration.TotalMilliseconds); }
        public override TimeSpan Duration {
            get => Media != null ? (Media.CustomDuration.TotalSeconds > 0 ? Media.CustomDuration : DefaultDuration) : TimeSpan.Zero;
            protected set => throw new NotSupportedException();
        }

        public override int Volume { get => 0; set { } }

        public override bool IsPlaying { get => playTimer.Enabled; protected set => throw new NotSupportedException(); }
        public override bool IsEnded { get => Position >= 1f; protected set => throw new NotSupportedException(); }

        private int time;
        private Timer playTimer;

        public override PictureBox CreateView(Screen screen) {
            PictureBox box = new PictureBox {
                SizeMode = PictureBoxSizeMode.Zoom
            };

            playTimer = new Timer { Interval = UpdateRate };
            playTimer.Tick += Timer_Tick;

            return box;
        }

        private void Timer_Tick(object sender, EventArgs e) {
            time += UpdateRate;

            if (IsEnded) {
                playTimer.Stop();
                NotifyOnChanged();
            }

            NotifyOnPositionChanged(Position);
        }

        protected override void PlayMedia(IMediaItem item = null) {
            if (item != null) {
                Media = item;
                View.BackColor = item.BackColor;
                View.ImageLocation = item.Filepath;
            } else if (Media != null) {
                Position = 0;
            }

            playTimer.Start();

            NotifyOnChanged();
        }

        protected override void PauseMedia() {
            playTimer.Enabled = !playTimer.Enabled;

            NotifyOnChanged();
        }


        protected override void StopMedia() {
            Media = null;
            playTimer.Stop();

            NotifyOnChanged();
        }

        public override void Reset() {
            time = 0;
        }

        protected override void Dispose(bool disposing) {
            if (disposing) {
                playTimer?.Dispose();
                playTimer = null;
            }
            base.Dispose(disposing);
        }

    }

}