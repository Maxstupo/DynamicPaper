namespace Maxstupo.DynamicPaper.Forms {
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Windows.Forms;

    public class TrayAwareForm : Form {

        protected FormWindowState PrevWindowState { get; private set; }
        protected FormWindowState ResumeWindowState { get; private set; }

        public bool CloseToTray { get; set; }
        public bool MinimizeToTray { get; set; }

        public bool ShowOnDoubleClick { get; set; } = true;

        public NotifyIcon TrayIcon { get; set; }

        public event EventHandler OnTrayEntered;
        public event EventHandler OnFormHidden;
        public event EventHandler OnFormShown;


        public TrayAwareForm() {
            FormClosing += TrayAwareForm_FormClosing;
            Resize += TrayAwareForm_Resize;

            Load += TrayAwareForm_Load;
        }


        private void ToTray() {
            ResumeWindowState = PrevWindowState;
            Visible = false;
            OnTrayEntered?.Invoke(this, EventArgs.Empty);
        }

        private void TrayAwareForm_Load(object sender, EventArgs e) {
            if (TrayIcon != null)
                TrayIcon.MouseDoubleClick += notifyIcon_MouseDoubleClick;
        }

        private void notifyIcon_MouseDoubleClick(object sender, MouseEventArgs e) {
            if (!ShowOnDoubleClick || e.Button != MouseButtons.Left)
                return;

            Visible = true;
            WindowState = ResumeWindowState;
            BringToFront();
        }

        public void trayShowForm_Click(object sender, EventArgs e) {
            notifyIcon_MouseDoubleClick(sender, new MouseEventArgs(MouseButtons.Left, 0, 0, 0, 0));
        }


        private void TrayAwareForm_FormClosing(object sender, FormClosingEventArgs e) {
            if (e.CloseReason == CloseReason.UserClosing && CloseToTray) {
                e.Cancel = true;
                PrevWindowState = WindowState;
                ToTray();
            }
        }

        private void TrayAwareForm_Resize(object sender, EventArgs e) {
            Debug.WriteLine($"{PrevWindowState} -> {WindowState}");

            if (MinimizeToTray && PrevWindowState != WindowState && WindowState == FormWindowState.Minimized)
                ToTray();

            if (PrevWindowState != WindowState) {
                if (WindowState == FormWindowState.Minimized) {
                    OnFormHidden?.Invoke(this, EventArgs.Empty);
                } else if (PrevWindowState == FormWindowState.Minimized) {
                    OnFormShown?.Invoke(this, EventArgs.Empty);
                }
            }


            PrevWindowState = WindowState;
        }

    }
}
