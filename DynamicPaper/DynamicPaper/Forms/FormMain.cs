namespace Maxstupo.DynamicPaper.Forms {

    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Data;
    using System.Drawing;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
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

        public FormMain() {
            InitializeComponent();
        }

        private void FormMain_Load(object sender, EventArgs e) {
            RefreshMonitorList();
        }


        public void RefreshMonitorList() {
            int index = 0;
            monitors = Screen.AllScreens.Select(x => new ScreenInfo(x, index++)).ToArray();

            cbxMonitor.DataSource = null;
            cbxMonitor.DisplayMember = nameof(ScreenInfo.DisplayName);
            cbxMonitor.DataSource = monitors;
        }

        public void ShowSettingsDialog(bool useParent = true) {
            using (FormSettings dialog = new FormSettings()) {
                
                if (!useParent)
                    dialog.StartPosition = FormStartPosition.CenterScreen;

                if (dialog.ShowDialog(useParent ? this : null) == DialogResult.OK) {

                }

            }
        }


        #region Events

        private void settingsToolStripMenuItem_Click(object sender, EventArgs e) {
            ShowSettingsDialog(false);
        }

        #endregion

    }

}