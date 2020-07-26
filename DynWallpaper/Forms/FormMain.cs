namespace Maxstupo.DynWallpaper.Forms {

    using System;
    using System.Collections.Generic;
    using System.Windows.Forms;
    using HeyRed.Mime;
    using LibVLCSharp.Shared;
    using Maxstupo.DynWallpaper.Forms.Wallpapers;
    using Maxstupo.DynWallpaper.Utility;
    using Microsoft.Win32;

    public partial class FormMain : Form {

        public Screen SelectedScreen => (cbxDisplay.SelectedItem as ScreenInfo)?.Screen;

        private readonly Dictionary<Screen, WallpaperBase> wallpapers = new Dictionary<Screen, WallpaperBase>();

        public FormMain() {
            if (!DesignMode)
                Core.Initialize();

            InitializeComponent();

            Application.ApplicationExit += Application_ApplicationExit;
            SystemEvents.DisplaySettingsChanged += SystemEvents_DisplaySettingsChanged;


            cbxImageMode.DataSource = Enum.GetValues(typeof(PictureBoxSizeMode));
            cbxImageMode.SelectedItem = PictureBoxSizeMode.Zoom;


            if (!WallpaperSystem.Init()) {
                MessageBox.Show(this, "Failed to initialize wallpaper system!", "DynWallpaper", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Application.Exit();
            }

        }

        private void FormMain_Load(object sender, EventArgs e) {
            RefreshDisplayList();
        }

        private void Application_ApplicationExit(object sender, EventArgs e) {
            SystemEvents.DisplaySettingsChanged -= SystemEvents_DisplaySettingsChanged;

            foreach (WallpaperBase wallpaper in wallpapers.Values)
                wallpaper.Close();

            WallpaperSystem.ResetDesktopBackground();
        }

        private void SystemEvents_DisplaySettingsChanged(object sender, EventArgs e) {
            RefreshDisplayList();
        }

        private void RefreshDisplayList() {
            cbxDisplay.Items.Clear();
            cbxDisplay.DisplayMember = nameof(ScreenInfo.DisplayName);

            Screen[] screens = Screen.AllScreens;
            for (int i = 0; i < screens.Length; i++)
                cbxDisplay.Items.Add(new ScreenInfo(screens[i], i + 1));

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

            string mimeType = MimeTypesMap.GetMimeType(txtFilepath.Text);

            if (!wallpapers.TryGetValue(screen, out WallpaperBase wallpaper)) {

                wallpaper = CreateWallpaper(screen, mimeType);

                wallpapers.Add(screen, wallpaper);
            }

            if (!wallpaper.ApplyWallpaper(txtFilepath.Text)) {

                // The current wallpaper doesn't support the specified file.

                WallpaperBase newWallpaper = CreateWallpaper(screen, mimeType);

                if (newWallpaper == null) {
                    MessageBox.Show(this, "The specified file format is unsupported by DynWallpaper!", "DynWallpaper", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    return;
                } else { // A wallpaper is supported for this file, so switch to it.
                    wallpaper.Close();
                    wallpapers[screen] = newWallpaper;
                    newWallpaper.ApplyWallpaper(txtFilepath.Text);
                }

            }

            cbxDisplay_SelectionChangeCommitted(sender, e);
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

        private WallpaperBase CreateWallpaper(Screen screen, string mimeType) {
            WallpaperBase wallpaper;

            if (mimeType.StartsWith("image", StringComparison.InvariantCultureIgnoreCase)) {
                wallpaper = new WallpaperImage();

            } else if (mimeType.StartsWith("video", StringComparison.InvariantCultureIgnoreCase)) {
                wallpaper = new WallpaperVideo();

            } else {
                return null;
            }

            wallpaper.Show();
            wallpaper.Bounds = WallpaperSystem.GetScreenBounds(screen, SystemInformation.VirtualScreen);

            WallpaperSystem.SetParent(wallpaper);

            return wallpaper;
        }

        private void cbxDisplay_SelectionChangeCommitted(object sender, EventArgs e) {
            Screen screen = SelectedScreen;

            if (screen == null)
                return;

            btnRemove.Enabled = wallpapers.ContainsKey(screen);
        }

        private void txtFilepath_TextChanged(object sender, EventArgs e) {

        }

        /// <summary>
        /// Click event that will display a color picker dialog and change the background color of the control that invoked it.
        /// </summary>
        private void pickColor_Click(object sender, EventArgs e) {
            if (sender is Control c) {
                using (ColorDialog dialog = new ColorDialog()) {
                    if (dialog.ShowDialog(this) == DialogResult.OK)
                        c.BackColor = dialog.Color;
                }
            }
        }
    }

}