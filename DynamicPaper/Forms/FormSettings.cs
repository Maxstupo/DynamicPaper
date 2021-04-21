namespace Maxstupo.DynamicPaper.Forms {
    using System;
    using System.Windows.Forms;
    using Maxstupo.DynamicPaper.Utility;

    public partial class FormSettings : Form {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        private readonly SettingsManager<AppSettings> settingsManager;
        private readonly BindingSource bindingSource;

        public FormSettings(SettingsManager<AppSettings> settingsManager) {
            InitializeComponent();
            this.settingsManager = settingsManager;
            this.settingsManager.Mark();

            bindingSource = new BindingSource(settingsManager, nameof(SettingsManager<AppSettings>.Settings));


            cbStartWithWindows.DataBindings.Add(nameof(CheckBox.Checked), bindingSource, nameof(AppSettings.StartWithWindows));
            cbStartMinimized.DataBindings.Add(nameof(CheckBox.Checked), bindingSource, nameof(AppSettings.StartMinimized));

            cbCloseToTray.DataBindings.Add(nameof(CheckBox.Checked), bindingSource, nameof(AppSettings.CloseToTray));
            cbMinimizeToTray.DataBindings.Add(nameof(CheckBox.Checked), bindingSource, nameof(AppSettings.MinimizeToTray));

            cbRestorePlaying.DataBindings.Add(nameof(CheckBox.Checked), bindingSource, nameof(AppSettings.RestorePlaying));
            cbRestorePlaylists.DataBindings.Add(nameof(CheckBox.Checked), bindingSource, nameof(AppSettings.RestorePlaylists));
          
            cbRestorePlaylists_CheckedChanged(null, EventArgs.Empty);
        }


        private void linkRestoreDefaults_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e) {
            settingsManager.RestoreDefaults();
            bindingSource.ResetBindings(false);
        }

        private void btnOkay_Click(object sender, System.EventArgs e) {
            settingsManager.Save();
            DialogResult = DialogResult.OK;
        }

        private void OnDispose() {
            settingsManager.Revert();
        }

        private void cbRestorePlaylists_CheckedChanged(object sender, EventArgs e) {
            cbRestorePlaying.Enabled = cbRestorePlaylists.Checked;
        }

    }

}