namespace Maxstupo.DynamicPaper.Wallpaper.Players.Impl {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Windows.Forms;

    public sealed class PictureBoxPlayer : AttachablePlayer<PictureBox> {

        public override float Position { get; set; }
        public override TimeSpan Duration { get; protected set; }
        public override TimeSpan? CustomDuration { get; set; }

        public override int Volume { get; set; }

        public override bool IsPlaying { get; protected set; }
        public override bool IsEnded { get; protected set; }


        public override PictureBox CreateView(Screen screen) {
            return new PictureBox {
                SizeMode = PictureBoxSizeMode.Zoom
            };
        }

        protected override void PlayMedia(IMediaItem item = null) {
            IsPlaying = true;

            if (item != null)
                Media = item;

            View.ImageLocation = item.Filepath;

            NotifyOnChanged();
        }

        protected override void PauseMedia() {
            IsPlaying = !IsPlaying;

            NotifyOnChanged();
        }


        protected override void StopMedia() {
            IsPlaying = false;
            Media = null;

            NotifyOnChanged();
        }

    }

}