namespace Maxstupo.DynWallpaper.Forms.Wallpapers {

    using System;
    using System.Windows.Forms;
    using HeyRed.Mime;

    public sealed class WallpaperImage : WallpaperBase {
        private readonly PictureBox pb;

        public WallpaperImage() {

            pb = new PictureBox {
                Dock = DockStyle.Fill,
                SizeMode = PictureBoxSizeMode.Zoom
            };

            Controls.Add(pb);
        }

        public override bool ApplyWallpaper(string filepath) {
            if (!MimeTypesMap.GetMimeType(filepath).StartsWith("image", StringComparison.InvariantCultureIgnoreCase))
                return false;

            pb.ImageLocation = filepath;

            return true;
        }


    }

}