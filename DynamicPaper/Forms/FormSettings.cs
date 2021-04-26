namespace Maxstupo.DynamicPaper.Forms {
    using System;
    using System.Windows.Forms;
    using Maxstupo.DynamicPaper.Utility;
    using Maxstupo.DynamicPaper.Utility.Windows;

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

            Binding binding = nudDefaultMediaDuration.DataBindings.Add(nameof(NumericUpDown.Value), bindingSource, nameof(AppSettings.DefaultMediaDuration));
            binding.Format += (s, e) => { e.Value = (decimal) ((int) ((TimeSpan) e.Value).TotalSeconds); };
            binding.Parse += (s, e) => { e.Value = TimeSpan.FromSeconds(int.Parse(e.Value.ToString())); };

            cbRestorePlaylists_CheckedChanged(null, EventArgs.Empty);
        }


        private void linkRestoreDefaults_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e) {
            settingsManager.RestoreDefaults();
            bindingSource.ResetBindings(false);
        }

        private void btnOkay_Click(object sender, EventArgs e) {
            WindowsUtility.StartApplicationWithWindows(Application.ProductName, settingsManager.Settings.StartWithWindows);

            settingsManager.Save();
            DialogResult = DialogResult.OK;
        }

        private void OnDispose() {
            settingsManager.Revert();
        }

        private void cbRestorePlaylists_CheckedChanged(object sender, EventArgs e) {
            cbRestorePlaying.Enabled = cbRestorePlaylists.Checked;
        }

        private void FormSettings_Load(object sender, EventArgs e) {

        }
    }

}