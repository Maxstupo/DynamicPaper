namespace Maxstupo.DynWallpaper.Forms {

    using System;
    using System.Drawing;
    using System.Windows.Forms;
    using Maxstupo.DynWallpaper.Forms.Wallpapers;
    using Maxstupo.DynWallpaper.Utility;

    public partial class FormMain : Form {

        private WallpaperBase wallpaper;

        public FormMain() {
            InitializeComponent();

            WallpaperSystem.Init();
        }

        private void btnBrowse_Click(object sender, EventArgs e) {
            using (OpenFileDialog dialog = new OpenFileDialog()) {
                dialog.Title = "Select wallpaper...";
                dialog.Filter = "Media Files|*.mp4;*.webm;*.mkv;*.avi;*.gif;*.png;*.jpeg;*.jpg;*.bmp;*.tiff|Video Files|*.mp4;*.webm;*.mkv;*.avi;*.gif|Image Files|*.png;*.jpeg;*.jpg;*.bmp;*.tiff|All Files|*.*";

                dialog.CheckFileExists = true;
                dialog.CheckPathExists = true;

                if (dialog.ShowDialog(this) == DialogResult.OK)
                    txtFilepath.Text = dialog.FileName;

            }
        }


        private void btnSet_Click(object sender, EventArgs e) {
            if (wallpaper != null) {
                wallpaper.ApplyWallpaper(txtFilepath.Text);
                return;
            }

            wallpaper = new WallpaperImage();
            wallpaper.Show();

            wallpaper.Bounds = new Rectangle(0, 0, 800, 600);

            wallpaper.ApplyWallpaper(txtFilepath.Text);

            WallpaperSystem.SetParent(wallpaper);
        }

        private void btnRemove_Click(object sender, EventArgs e) {
            wallpaper.Close();
            wallpaper.Dispose();
            wallpaper = null;

            WallpaperSystem.ResetDesktopBackground();
        }

    }

}