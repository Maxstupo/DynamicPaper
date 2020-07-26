namespace Maxstupo.DynWallpaper.Forms {

    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Windows.Forms;
    using HeyRed.Mime;
    using LibVLCSharp.Shared;
    using Maxstupo.DynWallpaper.Forms.Wallpapers;
    using Maxstupo.DynWallpaper.Utility;
    using Microsoft.Win32;

    public partial class FormMain : Form {

        public Screen SelectedScreen => (cbxDisplay.SelectedItem as ScreenInfo)?.Screen;

        public WallpaperBase SelectedWallpaper => SelectedScreen == null ? null : (wallpapers.TryGetValue(SelectedScreen, out WallpaperBase wallpaper) ? wallpaper : null);

        private readonly Dictionary<Screen, WallpaperBase> wallpapers = new Dictionary<Screen, WallpaperBase>();

        public FormMain() {
            if (!DesignMode)
                Core.Initialize();

            InitializeComponent();

            Application.ApplicationExit += Application_ApplicationExit;

            SystemEvents.DisplaySettingsChanged += SystemEvents_DisplaySettingsChanged;


            cbxImageMode.DataSource = Enum.GetValues(typeof(PictureBoxSizeMode));
            cbxImageMode.SelectedItem = PictureBoxSizeMode.Zoom;

            volumeSlider.VolumeChanged += VolumeSlider_VolumeChanged;
            timeline.TimeChanged += Timeline_TimeChanged;

            if (!WallpaperSystem.Init()) {
                MessageBox.Show(this, "Failed to initialize wallpaper system!", "DynWallpaper", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Application.Exit();
            }

        }



        private void Timeline_TimeChanged(object sender, float newTime) {
            WallpaperBase wallpaper = SelectedWallpaper;
            if (wallpaper != null)
                wallpaper.Position = newTime;
        }

        private void VolumeSlider_VolumeChanged(object sender, float newVolume) {
            WallpaperBase wallpaper = SelectedWallpaper;
            if (wallpaper != null)
                wallpaper.Volume = newVolume;


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
            int oldIndex = cbxDisplay.Items.Count == 0 ? 0 : cbxDisplay.SelectedIndex;

            cbxDisplay.Items.Clear();
            cbxDisplay.DisplayMember = nameof(ScreenInfo.DisplayName);

            Screen[] screens = Screen.AllScreens;
            for (int i = 0; i < screens.Length; i++)
                cbxDisplay.Items.Add(new ScreenInfo(screens[i], i + 1));

            cbxDisplay.SelectedIndex = oldIndex;

            cbxDisplay_SelectionChangeCommitted(null, EventArgs.Empty);
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
            WallpaperBase wallpaper = SelectedWallpaper;

            if (wallpaper != null) {
                wallpapers.Remove(SelectedScreen);

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

            wallpaper.SizeMode = (PictureBoxSizeMode) cbxImageMode.SelectedItem;
            wallpaper.Looping = cbLooping.Checked;
            wallpaper.BackColor = btnBackgroundColor.BackColor;
            wallpaper.Volume = volumeSlider.Volume;

            wallpaper.PositionChanged += Wallpaper_PositionChanged;
            wallpaper.PlayingChanged += Wallpaper_PlayingChanged;

            WallpaperSystem.SetParent(wallpaper);

            return wallpaper;
        }

        private void Wallpaper_PlayingChanged(object sender, EventArgs e) {
            WallpaperBase wallpaper = SelectedWallpaper;

            if (sender.Equals(wallpaper)) {
                btnPause.Text = wallpaper.IsPlaying ? "Pause" : "Play";
            }
        }

        private void Wallpaper_PositionChanged(object sender, float newPosition) {
            WallpaperBase wallpaper = SelectedWallpaper;
            if (sender.Equals(wallpaper)) {
                timeline.DisableNotify = true;
                timeline.Time = newPosition;
                timeline.DisableNotify = false;


            }
        }

        private void cbxDisplay_SelectionChangeCommitted(object sender, EventArgs e) {
            WallpaperBase wallpaper = SelectedWallpaper;

            if (wallpaper != null) {
                btnRemove.Enabled = wallpapers.ContainsKey(SelectedScreen);

                txtFilepath.Text = wallpaper.Filepath;
                volumeSlider.Volume = wallpaper.Volume;
                btnBackgroundColor.BackColor = wallpaper.BackColor;
                cbxImageMode.SelectedItem = wallpaper.SizeMode;
                cbLooping.Checked = wallpaper.Looping;
                timeline.Time = wallpaper.Position;
            }
        }

        private void txtFilepath_TextChanged(object sender, EventArgs e) {
            btnSet.Enabled = File.Exists(txtFilepath.Text);

            string mimeType = MimeTypesMap.GetMimeType(txtFilepath.Text);
            tabControl.Enabled = true;
            if (mimeType.StartsWith("image", StringComparison.InvariantCultureIgnoreCase)) {
                tabControl.SelectedIndex = 1;

            } else if (mimeType.StartsWith("video", StringComparison.InvariantCultureIgnoreCase)) {
                tabControl.SelectedIndex = 0;

            } else {
                tabControl.Enabled = false;
            }


        }

        private void ApplyWallpaperOptions(object sender, EventArgs e) {
            WallpaperBase wallpaper = SelectedWallpaper;

            if (wallpaper != null) {
                wallpaper.SizeMode = (PictureBoxSizeMode) cbxImageMode.SelectedItem;
                wallpaper.BackColor = btnBackgroundColor.BackColor;
                wallpaper.Looping = cbLooping.Checked;
            }
        }

        private void btnBackgroundColor_Click(object sender, EventArgs e) {

            using (ColorDialog dialog = new ColorDialog()) {
                if (dialog.ShowDialog(this) == DialogResult.OK) {
                    btnBackgroundColor.BackColor = dialog.Color;

                    ApplyWallpaperOptions(sender, e);
                }
            }

        }

        private void exitToolStripMenuItem1_Click(object sender, EventArgs e) {
            Application.Exit();
        }

        private void btnPause_Click(object sender, EventArgs e) {
            WallpaperBase wallpaper = SelectedWallpaper;

            if (wallpaper != null)
                wallpaper.Toggle();
        }

        private void minimizeToTrayToolStripMenuItem_Click(object sender, EventArgs e) {
            minimizeToTrayToolStripMenuItem.Checked = !minimizeToTrayToolStripMenuItem.Checked;
        }

        private void FormMain_Resize(object sender, EventArgs e) {

        }

    }

}