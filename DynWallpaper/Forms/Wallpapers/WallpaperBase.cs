namespace Maxstupo.DynWallpaper.Forms.Wallpapers {
    using System;
    using System.Drawing;
    using System.Windows.Forms;

    public abstract class WallpaperBase : Form {

        public string Filepath { get; private set; }

        // Volume of this wallpaper, depending on subclass might not be used.
        public virtual float Volume { get; set; }

        public virtual PictureBoxSizeMode SizeMode { get; set; }

        public virtual bool Looping { get; set; }

        public virtual float Position { get; set; }

        public virtual bool IsPlaying { get; }

        public event EventHandler<float> PositionChanged;
        public event EventHandler PlayingChanged;

        public WallpaperBase() {
            InitializeComponent();
        }

        private void InitializeComponent() {
            AutoScaleDimensions = new SizeF(6F, 13F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);

            FormBorderStyle = FormBorderStyle.None;

            MaximizeBox = false;
            MinimizeBox = false;

            Text = "Wallpaper";
        }

        protected void OnPositionChanged(float newPosition) {
            Invoke((Action) (() => PositionChanged?.Invoke(this, newPosition)));
        }
        protected void OnPlayingChanged() {
            Invoke((Action) (() => PlayingChanged?.Invoke(this, EventArgs.Empty)));
        }


        public bool ApplyWallpaper(string filepath) {
            Filepath = filepath;
            return ApplyWallpaper();
        }

        public abstract bool ApplyWallpaper();

        public virtual void Toggle() { }

    }

}