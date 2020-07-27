namespace Maxstupo.DynWallpaper.Forms.Wallpapers {
    using System;
    using System.Drawing;
    using System.Windows.Forms;
    using OpenTK;
    using OpenTK.Graphics.OpenGL4;

    public sealed class WallpaperOpenGL : WallpaperBase {

        private readonly GLControl glControl;

        public WallpaperOpenGL() {
            Load += WallpaperOpenGL_Load;

            glControl = new GLControl {
                Dock = DockStyle.Fill,
                VSync = false
            };
            glControl.Resize += glControl_Resize;
            glControl.Paint += (s, e) => Render();

            Controls.Add(glControl);
        }

        private void WallpaperOpenGL_Load(object sender, EventArgs e) {
            GL.ClearColor(Color.MidnightBlue);

            glControl_Resize(null, EventArgs.Empty); // Ensure the Viewport is set up correctly
        }

        private void Render() {
            // TODO: Render OpenGL here.

            glControl.SwapBuffers();
        }

        private void glControl_Resize(object sender, EventArgs e) {
            if (glControl.ClientSize.Height == 0)
                glControl.ClientSize = new Size(glControl.ClientSize.Width, 1);

            //   float ratio = Width / (float) Height;

            GL.Viewport(0, 0, glControl.ClientSize.Width, glControl.ClientSize.Height);
        }

        public override bool ApplyWallpaper() {

            return false;
        }

    }

}