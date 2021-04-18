namespace Maxstupo.DynamicPaper.Forms {

    using System;
    using System.Data;
    using System.Diagnostics;
    using System.Linq;
    using System.Windows.Forms;
    using LibVLCSharp.Shared;
    using Maxstupo.DynamicPaper.Utility;
    using Maxstupo.DynamicPaper.Utility.Windows;

    public partial class FormMain : TrayAwareForm {

        public static readonly string FileFilter = new FileFilterBuilder()
           .Add("Image Files", "png", "jpg", "jpeg", "bmp", "gif")
           .Add("Video Files", "mkv", "mp4", "mov", "avi", "wmv", "gif", "webm")
           .Add("DynamicPaper Playlist Files", "dpp")
           .AddGroup("Supported Files", 0) // Will concat all previously added filters into a new filter. 
           .Add("All Files", "*")
           .ToString();

        private LibVLC libVLC;


        private ScreenInfo[] monitors;

        private readonly SettingsManager<AppSettings> settingsManager = new SettingsManager<AppSettings>("settings.json", () => new AppSettings());
        private AppSettings Settings => settingsManager.Settings;

        public FormMain() {
            if (!DesignMode)
                Core.Initialize();
            libVLC = new LibVLC();

            if (!WindowsWallpaper.Init()) {
                MessageBox.Show(this, "Failed to initalize windows wallpaper! Failed to acquire desktop window pointer!", Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
                Application.Exit();
            }

            Application.ApplicationExit += Application_ApplicationExit;


            settingsManager.OnSettingsChanged += (sender, settings) => {
                CloseToTray = settings.CloseToTray;
                MinimizeToTray = settings.MinimizeToTray;
            };
            settingsManager.Load(true);

            if (Settings.StartMinimized)
                WindowState = FormWindowState.Minimized;

            InitializeComponent();
            showToolStripMenuItem.Click += trayShowForm_Click; // Connect the show button in the tray with the required method.
        }

        private void FormMain_Load(object sender, EventArgs e) {
            RefreshMonitorList();

            if (Settings.StartMinimized && Settings.MinimizeToTray)
                Visible = false;
        }

        private void Application_ApplicationExit(object sender, EventArgs e) {
            libVLC.Dispose();
            libVLC = null;
            Debug.WriteLine("Disposing libVLC");
            WindowsWallpaper.ResetDesktopBackground();
        }

        public void RefreshMonitorList() {
            monitors = ScreenInfo.AllScreens;

            cbxMonitor.DataSource = null;
            cbxMonitor.DisplayMember = nameof(ScreenInfo.DisplayName);
            cbxMonitor.DataSource = monitors;
        }

        public void ShowSettingsDialog(bool useParent = true) {

            using (FormSettings dialog = new FormSettings(settingsManager)) {

                if (!useParent)
                    dialog.StartPosition = FormStartPosition.CenterScreen;

                dialog.ShowDialog(useParent ? this : null);
            }

        }


        #region Events

        private void FormMain_OnTrayEntered(object sender, EventArgs e) {
            notifyIcon?.ShowBalloonTip(5000, Application.ProductName, "Now running in the background. Use tray icon to reveal.", ToolTipIcon.Info);
        }

        private void settingsToolStripMenuItem_Click(object sender, EventArgs e) {
            ShowSettingsDialog(false);
        }

        private void traySettingsToolStripMenuItem_Click(object sender, EventArgs e) {
            ShowSettingsDialog(false);
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e) {
            Application.Exit();
        }

        #endregion


    }

}