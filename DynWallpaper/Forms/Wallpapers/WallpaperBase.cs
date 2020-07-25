namespace Maxstupo.DynWallpaper.Forms.Wallpapers {

    using System.Drawing;
    using System.Windows.Forms;

    public abstract class WallpaperBase : Form {
    
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

        public virtual void ApplyWallpaper(string filepath) { }
    
    }
    
}