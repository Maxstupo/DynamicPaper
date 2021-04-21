namespace Maxstupo.DynamicPaper.Forms {

    using System;
    using System.Windows.Forms;

    public partial class DialogSetDuration : Form {

        public TimeSpan Duration {
            get {
                float t = float.Parse(txtDuration.Text);
                return TimeSpan.FromSeconds(t);
            }
        }

        public DialogSetDuration(TimeSpan duration) {
            InitializeComponent();

            txtDuration.Text = ((int) duration.TotalSeconds).ToString();
        }

        private void btnOkay_Click(object sender, EventArgs e) {
            DialogResult = DialogResult.OK;
        }

        private void DialogSetDuration_Load(object sender, EventArgs e) {

        }

        private void btnNoDuration_Click(object sender, EventArgs e) {
            txtDuration.Text = "-1";
            btnOkay.PerformClick();
        }

        private void btnDefaultDuration_Click(object sender, EventArgs e) {
            txtDuration.Text = "0";
            btnOkay.PerformClick();
        }

    }

}