namespace Maxstupo.DynWallpaper.Forms.Wallpapers {

    using System.Windows.Forms;

    public sealed class WallpaperImage : WallpaperBase {
        private readonly PictureBox pb;

        public WallpaperImage() {

            pb = new PictureBox {
                Dock = DockStyle.Fill,
                SizeMode = PictureBoxSizeMode.Zoom
            };

            Controls.Add(pb);
        }

        public override void ApplyWallpaper(string filepath) {
            base.ApplyWallpaper(filepath);
            pb.ImageLocation = filepath;
        }

    }

}