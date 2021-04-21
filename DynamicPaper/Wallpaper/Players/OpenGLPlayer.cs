namespace Maxstupo.DynamicPaper.Wallpaper.Players {

    using System;
    using System.Windows.Forms;
    using OpenTK;

    public abstract class OpenGLPlayer : AttachablePlayer<GLControl> {

        public override float Position { get; set; }
        public override TimeSpan Duration { get; protected set; }
        public override TimeSpan? CustomDuration { get; set; }
        public override int Volume { get; set; }
        public override bool IsPlaying { get; protected set; }
        public override bool IsEnded { get; protected set; }

        private GLControl glControl;

        public override GLControl CreateView(Screen screen) {
            glControl = new GLControl {
                VSync = false
            };
            glControl.Resize += GlControl_Resize;
            glControl.Paint += delegate { Render(); };

            Init();

            return glControl;
        }

        protected abstract void Init();
        protected abstract void Render();
        protected abstract void Update(TimeSpan deltaTime);

        protected abstract void OnResized(float width, float height, float ratio);

        protected override void PlayMedia(IMediaItem item = null) {

        }

        protected override void PauseMedia() {

        }

        protected override void StopMedia() {

        }

        private void GlControl_Resize(object sender, EventArgs e) {
            if (glControl != null) {
                float ratio = (float) glControl.ClientSize.Width / glControl.ClientSize.Height;
                OnResized(glControl.ClientSize.Width, glControl.ClientSize.Height, ratio);
            }
        }

    }

}