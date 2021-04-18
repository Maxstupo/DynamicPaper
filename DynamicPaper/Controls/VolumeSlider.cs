namespace Maxstupo.DynamicPaper.Controls {

    using System;
    using System.Drawing;
    using System.Windows.Forms;

    public partial class VolumeSlider : UserControl {

        private readonly object _lock = new object();

        public float MinDb { get; set; } = -48;

        private float volume = 1.0f;
        public float Volume {
            get => volume;
            set {
                float newVolume = Math.Max(Math.Min(value, 1f), 0f);

                if (volume != newVolume) {
                    lock (_lock) {
                        volume = newVolume;
                        if (!DisableNotify)
                            VolumeChanged?.Invoke(this, volume);
                    }
                    Invalidate();
                }

            }
        }

        private bool disableNotify = false;
        public bool DisableNotify {
            get => disableNotify;
            set { lock (_lock) disableNotify = value; }
        }


        public event EventHandler<float> VolumeChanged;


        public VolumeSlider() {
            this.AutoScaleMode = AutoScaleMode.Font;
            this.Size = new Size(96, 16);
            DoubleBuffered = true;
            Resize += VolumeSlider_Resize;
        }

        private void VolumeSlider_Resize(object sender, EventArgs e) {
            Refresh();
        }

        protected override void OnMouseMove(MouseEventArgs e) {
            base.OnMouseMove(e);

            if (e.Button == MouseButtons.Left)
                UpdateVolume(e.X);
        }

        protected override void OnMouseDown(MouseEventArgs e) {
            base.OnMouseDown(e);

            if (e.Button == MouseButtons.Left)
                UpdateVolume(e.X);
        }

        private void UpdateVolume(int mouseX) {
            if (mouseX < 0) {
                Volume = 0;
            } else {
                float dbVolume = (1.0f - (float) mouseX / Width) * MinDb;
                Volume = (float) Math.Pow(10.0f, dbVolume / 20.0f);
            }
        }

        protected override void OnPaint(PaintEventArgs e) {
            Graphics g = e.Graphics;

            g.DrawRectangle(Pens.Black, 0, 0, Width - 1, Height - 1);

            float db = 20.0f * (float) Math.Log10(volume);

            float percent = 1.0f - (db / MinDb);
            float width = (int) ((Width - 2) * percent);

            using (Brush brush = new SolidBrush(Enabled ? ((SolidBrush) Brushes.LightGreen).Color : Color.LightGray))
                g.FillRectangle(brush, 1, 1, width, Height - 2);

            string dbValue = string.Format("{0:F2} dB", db);

            using (StringFormat format = new StringFormat {
                LineAlignment = StringAlignment.Center,
                Alignment = StringAlignment.Center
            }) {

                g.DrawString(dbValue, Font, SystemBrushes.ControlText, ClientRectangle, format);

            }

        }

    }

}