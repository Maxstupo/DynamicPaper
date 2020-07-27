namespace Maxstupo.DynWallpaper.Forms.Wallpapers {
    using System;
    using System.ComponentModel;
    using System.Drawing;
    using System.Threading;
    using System.Windows.Forms;
    using Maxstupo.DynWallpaper.Graphics;
    using OpenTK;

    public sealed class WallpaperOpenGL : WallpaperBase {

        private readonly GLControl glControl;

        private readonly IRenderer renderer;

        public WallpaperOpenGL(IRenderer renderer) {
            this.renderer = renderer;

            Load += WallpaperOpenGL_Load;

            glControl = new GLControl {
                Dock = DockStyle.Fill,
                VSync = false
            };
            glControl.Resize += glControl_Resize;
            glControl.Paint += delegate { Render(); };

            Controls.Add(glControl);
        }

        protected override void OnClosing(CancelEventArgs e) {
            Application.Idle -= Application_Idle;
            base.OnClosing(e);
        }

        private void Application_Idle(object sender, EventArgs e) {
            while (glControl.IsIdle)
                Render();
        }

        private void WallpaperOpenGL_Load(object sender, EventArgs e) {
            renderer.Init();

            glControl_Resize(null, EventArgs.Empty); // Ensure the Viewport is set up correctly
            Application.Idle += Application_Idle;
        }

        private void Render() {
            renderer.Render(1f);
            glControl.SwapBuffers();
        }

        private void glControl_Resize(object sender, EventArgs e) {
            if (glControl.ClientSize.Height == 0)
                glControl.ClientSize = new Size(glControl.ClientSize.Width, 1);

            float ratio = (float) Width / Height;
            renderer.OnResized(glControl.ClientSize.Width, glControl.ClientSize.Height, ratio);
        }

        public override bool ApplyWallpaper() {

            return false;
        }

        protected override void Dispose(bool disposing) {
            base.Dispose(disposing);
            if (disposing)
                renderer.Dispose();

        }

    }

}