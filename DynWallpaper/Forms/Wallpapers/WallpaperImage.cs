namespace Maxstupo.DynWallpaper.Forms.Wallpapers {

    using System;
    using System.Windows.Forms;
    using HeyRed.Mime;

    public sealed class WallpaperImage : WallpaperBase {

        private readonly PictureBox pb;

        public override PictureBoxSizeMode SizeMode { get => pb.SizeMode; set => pb.SizeMode = value; }

        public WallpaperImage() {

            pb = new PictureBox {
                Dock = DockStyle.Fill,
                SizeMode = PictureBoxSizeMode.Zoom
            };

            Controls.Add(pb);
        }


        public override bool ApplyWallpaper() {
            if (!MimeTypesMap.GetMimeType(Filepath).StartsWith("image", StringComparison.InvariantCultureIgnoreCase))
                return false;
            pb.ImageLocation = Filepath;
            return true;
        }


    }

}