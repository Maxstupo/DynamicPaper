namespace Maxstupo.DynWallpaper.Controls {

    using System;
    using System.Drawing;
    using System.Windows.Forms;

    // Really should use a base slider class for timeline and volume sliders.
    public partial class TimelineSlider : UserControl {
        private readonly object _lock = new object();

        private float time = 1.0f;
        public float Time {
            get => time;
            set {
                float newTime = Math.Max(Math.Min(value, 1f), 0f);

                if (time != newTime) {
                    lock (_lock) {
                        time = newTime;
                        if (!DisableNotify)
                            TimeChanged?.Invoke(this, time);
                    }
                    Invalidate();
                }

            }
        }

        public event EventHandler<float> TimeChanged;

        private bool disableNotify = false;
        public bool DisableNotify { get => disableNotify; set { lock (_lock) disableNotify = value; } }


        public Color OutlineColor { get; set; } = Color.Black;
        public Color ForegroundColor { get; set; } = Color.LightGreen;

        public TimelineSlider() {
            InitializeComponent();
        }

        protected override void OnMouseMove(MouseEventArgs e) {
            base.OnMouseMove(e);

            if (e.Button == MouseButtons.Left)
                UpdateTime(e.X);
        }

        protected override void OnMouseDown(MouseEventArgs e) {
            base.OnMouseDown(e);

            if (e.Button == MouseButtons.Left)
                UpdateTime(e.X);
        }

        private void UpdateTime(int mouseX) {
            Time = (mouseX < 0) ? 0f : (float) mouseX / Width;
        }

        protected override void OnPaint(PaintEventArgs e) {
            Graphics g = e.Graphics;

            using (Pen pen = new Pen(OutlineColor))
                g.DrawRectangle(pen, 0, 0, Width - 1, Height - 1);

            float width = (int) ((Width - 2) * Time);

            using (Brush brush = new SolidBrush(ForegroundColor))
                g.FillRectangle(brush, 1, 1, width, Height - 2);
        }

    }

}