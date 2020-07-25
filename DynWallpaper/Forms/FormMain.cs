namespace Maxstupo.DynWallpaper.Forms {

    using System;
    using System.Collections.Generic;
    using System.Windows.Forms;
    using Maxstupo.DynWallpaper.Forms.Wallpapers;
    using Maxstupo.DynWallpaper.Utility;
    using Microsoft.Win32;

    public partial class FormMain : Form {

        public Screen SelectedScreen => cbxDisplay.SelectedItem as Screen;

        private readonly Dictionary<Screen, WallpaperBase> wallpapers = new Dictionary<Screen, WallpaperBase>();

        public FormMain() {
            InitializeComponent();

            if (!WallpaperSystem.Init()) {
                MessageBox.Show(this, "Failed to initialize wallpaper system!", "DynWallpaper", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Application.Exit();
            }
        }

        private void FormMain_Load(object sender, EventArgs e) {
            Application.ApplicationExit += Application_ApplicationExit;
            SystemEvents.DisplaySettingsChanged += SystemEvents_DisplaySettingsChanged;

            RefreshDisplayList();
        }

        private void Application_ApplicationExit(object sender, EventArgs e) {
            SystemEvents.DisplaySettingsChanged -= SystemEvents_DisplaySettingsChanged;

            WallpaperSystem.ResetDesktopBackground();
        }

        private void SystemEvents_DisplaySettingsChanged(object sender, EventArgs e) {
            RefreshDisplayList();
        }

        private void RefreshDisplayList() {
            cbxDisplay.DataSource = null;
            cbxDisplay.DataSource = Screen.AllScreens;

            cbxDisplay.DisplayMember = nameof(Screen.DeviceName);

            cbxDisplay.SelectedIndex = 0;
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
            Screen screen = SelectedScreen;

            if (screen == null)
                return;

            if (!wallpapers.TryGetValue(screen, out WallpaperBase wallpaper)) {
                wallpaper = new WallpaperImage();

                wallpapers.Add(screen, wallpaper);

                wallpaper.Show();
                wallpaper.Bounds = WallpaperSystem.GetScreenBounds(screen, SystemInformation.VirtualScreen);

                WallpaperSystem.SetParent(wallpaper);

                cbxDisplay_SelectionChangeCommitted(sender, e);
            }

            wallpaper.ApplyWallpaper(txtFilepath.Text);

        }

        private void btnRemove_Click(object sender, EventArgs e) {
            Screen screen = SelectedScreen;

            if (screen == null)
                return;

            if (wallpapers.TryGetValue(screen, out WallpaperBase wallpaper)) {
                wallpapers.Remove(screen);

                wallpaper.Close();

                WallpaperSystem.ResetDesktopBackground();

                cbxDisplay_SelectionChangeCommitted(sender, e);
            }
        }

        private void cbxDisplay_SelectionChangeCommitted(object sender, EventArgs e) {
            Screen screen = SelectedScreen;

            if (screen == null)
                return;

            btnRemove.Enabled = wallpapers.ContainsKey(screen);
        }


    }

}