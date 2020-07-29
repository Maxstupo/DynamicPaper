namespace Maxstupo.DynWallpaper.Forms {

    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Windows.Forms;
    using HeyRed.Mime;
    using LibVLCSharp.Shared;
    using Maxstupo.DynWallpaper.Forms.Wallpapers;
    using Maxstupo.DynWallpaper.Graphics.ShaderToy;
    using Maxstupo.DynWallpaper.Utility;
    using Microsoft.Win32;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    public partial class FormMain : Form {

        public Screen SelectedScreen => (cbxDisplay.SelectedItem as ScreenInfo)?.Screen;

        public WallpaperBase SelectedWallpaper => SelectedScreen == null ? null : (wallpapers.TryGetValue(SelectedScreen, out WallpaperBase wallpaper) ? wallpaper : null);

        private readonly Dictionary<Screen, WallpaperBase> wallpapers = new Dictionary<Screen, WallpaperBase>();

        private FormWindowState prevWindowState;
        private FormWindowState resumeWindowState;

        public string AppDirectory {
            get {
                string rootDirectory = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                return Path.Combine(rootDirectory, "DynWallpaper");
            }
        }

        public string ConfigFilepath => Path.Combine(AppDirectory, "config.json");

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

            Directory.CreateDirectory(AppDirectory);
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
            LoadSettings();
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

            SetWallpaper(screen, txtFilepath.Text);
            SaveSettings();
        }

        private WallpaperBase SetWallpaper(Screen screen, string filepath, bool silent = false) {

            string mimeType = MimeTypesMap.GetMimeType(filepath);

            if (!wallpapers.TryGetValue(screen, out WallpaperBase wallpaper)) {

                wallpaper = CreateWallpaper(screen, mimeType);

                wallpapers.Add(screen, wallpaper);
            }

            if (!wallpaper.ApplyWallpaper(filepath)) {

                // The current wallpaper doesn't support the specified file.

                WallpaperBase newWallpaper = CreateWallpaper(screen, mimeType);

                if (newWallpaper == null) {
                    if (!silent)
                        MessageBox.Show(this, "The specified file format is unsupported by DynWallpaper!", "DynWallpaper", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    return null;
                } else { // A wallpaper is supported for this file, so switch to it.
                    wallpaper.Close();
                    wallpapers[screen] = newWallpaper;
                    wallpaper = newWallpaper;
                    newWallpaper.ApplyWallpaper(filepath);
                }

            }

            cbxDisplay_SelectionChangeCommitted(null, EventArgs.Empty);

            return wallpaper;
        }


        private void btnRemove_Click(object sender, EventArgs e) {
            WallpaperBase wallpaper = SelectedWallpaper;

            if (wallpaper != null) {
                wallpapers.Remove(SelectedScreen);

                wallpaper.Close();

                WallpaperSystem.ResetDesktopBackground();

                cbxDisplay_SelectionChangeCommitted(sender, e);

                SaveSettings();
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

        private void exitToolStripMenuItem_Click(object sender, EventArgs e) {
            Application.Exit();
        }

        private void btnPause_Click(object sender, EventArgs e) {
            WallpaperBase wallpaper = SelectedWallpaper;

            if (wallpaper != null)
                wallpaper.Toggle();
        }

        private void minimizeToTrayToolStripMenuItem_Click(object sender, EventArgs e) {

            minimizeToTrayToolStripMenuItem.Checked = !minimizeToTrayToolStripMenuItem.Checked;
            minimizeToTrayToolStripMenuItem1.Checked = minimizeToTrayToolStripMenuItem.Checked;

            if (!minimizeToTrayToolStripMenuItem.Checked) {
                Visible = true;
                WindowState = resumeWindowState;
            }

            SaveSettings();
        }

        private void notifyIcon_MouseDoubleClick(object sender, MouseEventArgs e) {
            if (e.Button != MouseButtons.Left)
                return;

            if (WindowState == FormWindowState.Minimized && !Visible) {
                Visible = true;
                WindowState = resumeWindowState;
            }
        }

        private void showToolStripMenuItem_Click(object sender, EventArgs e) {
            notifyIcon_MouseDoubleClick(sender, new MouseEventArgs(MouseButtons.Left, 0, 0, 0, 0));
        }

        private void FormMain_Resize(object sender, EventArgs e) {

            if (minimizeToTrayToolStripMenuItem.Checked && prevWindowState != WindowState && WindowState == FormWindowState.Minimized) {
                resumeWindowState = prevWindowState;
                notifyIcon.ShowBalloonTip(0, "DynWallpaper", "Now running in the background. Use tray icon to reveal.", ToolTipIcon.Info);
                Visible = false;
            }

            prevWindowState = WindowState;

        }

        private void SaveSettings() {
            StringBuilder sb = new StringBuilder();

            using (StringWriter sw = new StringWriter(sb)) {
                using (JsonTextWriter jw = new JsonTextWriter(sw)) {
                    jw.Formatting = Formatting.Indented;
                    jw.WriteStartObject();
                    {
                        jw.WritePropertyName("minimize_to_tray");
                        jw.WriteValue(minimizeToTrayToolStripMenuItem.Checked);

                        jw.WritePropertyName("wallpapers");
                        jw.WriteStartArray();
                        {
                            foreach (KeyValuePair<Screen, WallpaperBase> pair in wallpapers) {
                                jw.WriteStartObject();
                                {
                                    jw.WritePropertyName("display");
                                    jw.WriteValue(pair.Key.DeviceName);

                                    jw.WritePropertyName("filepath");
                                    jw.WriteValue(pair.Value.Filepath);


                                    jw.WritePropertyName("volume");
                                    jw.WriteValue(pair.Value.Volume);

                                    jw.WritePropertyName("looping");
                                    jw.WriteValue(pair.Value.Looping);


                                    jw.WritePropertyName("background");
                                    jw.WriteValue($"{pair.Value.BackColor.R}, {pair.Value.BackColor.G}, {pair.Value.BackColor.B}");

                                    jw.WritePropertyName("mode");
                                    jw.WriteValue(pair.Value.SizeMode);

                                }
                                jw.WriteEndObject();
                            }
                        }
                        jw.WriteEndArray();
                    }
                    jw.WriteEndObject();
                }
            }

            File.WriteAllText(ConfigFilepath, sb.ToString());
        }

        private void LoadSettings() {
            if (!File.Exists(ConfigFilepath))
                return;

            foreach (WallpaperBase wallpaper in wallpapers.Values)
                wallpaper.Close();
            wallpapers.Clear();

            WallpaperSystem.ResetDesktopBackground();
            cbxDisplay_SelectionChangeCommitted(null, EventArgs.Empty);


            JObject json = JObject.Parse(File.ReadAllText(ConfigFilepath));

            minimizeToTrayToolStripMenuItem.Checked = json["minimize_to_tray"].Value<bool>();

            foreach (JObject wallpaperItem in json["wallpapers"]) {
                string displayDeviceName = wallpaperItem["display"].Value<string>();

                Screen screen = Screen.AllScreens.FirstOrDefault(x => x.DeviceName == displayDeviceName);
                if (screen == null)
                    continue;

                string filepath = wallpaperItem["filepath"].Value<string>();

                WallpaperBase wallpaper = SetWallpaper(screen, filepath, true);
                if (wallpaper != null) {
                    wallpaper.Volume = wallpaperItem["volume"].Value<float>();
                    wallpaper.Looping = wallpaperItem["looping"].Value<bool>();
                    int modeId = wallpaperItem["mode"].Value<int>();
                    wallpaper.SizeMode = (PictureBoxSizeMode) modeId;
                    string[] rgb = wallpaperItem["background"].Value<string>().Split(',');
                    if (rgb.Length == 3 && int.TryParse(rgb[0], out int r) && int.TryParse(rgb[1], out int g) && int.TryParse(rgb[2], out int b))
                        wallpaper.BackColor = Color.FromArgb(r, g, b);

                }
            }
        }



        // TEMP: ------------------------------------------------------------------------------------
        WallpaperBase stWallpaper;
        private void btnShaderToyTest_Click(object sender, EventArgs e) {

            if (stWallpaper != null) {
                stWallpaper.Close();
                stWallpaper.Dispose();
                stWallpaper = null;
                WallpaperSystem.ResetDesktopBackground();
                return;
            }

            ShaderToyRenderer renderer = new ShaderToyRenderer {
                // SharedFragmentCode = File.ReadAllText("Res/Shaders/tree/common.fs")
            };

            //renderer.Add(Pass.BufferA, RenderPass.FromFile("Res/Shaders/tree/buffer_a.fs",
            //    new RenderInput(2, "Res/Textures/bw_64_noise.png"))
            //);

            //renderer.Add(Pass.BufferB, RenderPass.FromFile("Res/Shaders/tree/buffer_b.fs",
            //    new RenderInput(0, Pass.BufferB),
            //    new RenderInput(1, Pass.BufferA),
            //    new RenderInput(2, "Res/Textures/bw_64_noise.png"),
            //    new RenderInput(3, "Res/Textures/black.png"))
            //);

            //renderer.Add(Pass.Image, RenderPass.FromFile("Res/Shaders/tree/image.fs",
            //    new RenderInput(0, Pass.BufferB),
            //    new RenderInput(1, Pass.BufferA))
            //);

            renderer.Add(Pass.Image, RenderPass.FromFile("Res/Shaders/supernova_remnant.shadertoy.fs",
               new RenderInput(0, "Res/Textures/rgba_256_noise.png"),
               new RenderInput(1, "Res/Textures/black.png"),
               new RenderInput(2, "Res/Textures/bw_64_noise.png"))
           );



            stWallpaper = new WallpaperOpenGL(renderer);

            bool background = !cbShaderToyTestWindowed.Checked && SelectedScreen != null && !wallpapers.ContainsKey(SelectedScreen);

            if (!background) {
                stWallpaper.FormBorderStyle = FormBorderStyle.Sizable;
                stWallpaper.Size = new Size(800, 450);
                stWallpaper.ShowDialog(this);
                stWallpaper.Dispose();
                stWallpaper = null;
            } else {
                stWallpaper.Show();
                stWallpaper.Bounds = WallpaperSystem.GetScreenBounds(SelectedScreen, SystemInformation.VirtualScreen);
                WallpaperSystem.SetParent(stWallpaper);
            }

        }
        //-------------------------------------------------------------------------------------------
    }

}