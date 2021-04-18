namespace Maxstupo.DynamicPaper.Forms {

    using System;
    using System.Data;
    using System.Linq;
    using System.Windows.Forms;
    using Maxstupo.DynamicPaper.Utility;

    public partial class FormMain : Form {

        public static readonly string FileFilter = new FileFilterBuilder()
           .Add("Image Files", "png", "jpg", "jpeg", "bmp", "gif")
           .Add("Video Files", "mkv", "mp4", "mov", "avi", "wmv", "gif", "webm")
           .Add("DynamicPaper Playlist Files", "dpp")
           .AddGroup("Supported Files", 0) // Will concat all previously added filters into a new filter. 
           .Add("All Files", "*")
           .ToString();

        private ScreenInfo[] monitors;

        private readonly SettingsManager<AppSettings> settingsManager = new SettingsManager<AppSettings>("settings.json", () => new AppSettings());

        public FormMain() {
            InitializeComponent();
        }

        private void FormMain_Load(object sender, EventArgs e) {
            settingsManager.Load(true);

            RefreshMonitorList();

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

        private void settingsToolStripMenuItem_Click(object sender, EventArgs e) {
            ShowSettingsDialog(false);
        }

        #endregion

    }

}