namespace Maxstupo.DynamicPaper.Wallpaper.Players {

    using System;
    using System.Threading;
    using System.Windows.Forms;
    using Maxstupo.DynamicPaper.Utility;
    using OpenTK;
    using OpenTK.Graphics.OpenGL4;

    public abstract class OpenGLPlayer : AttachablePlayer<GLControl> {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        private const float TargetFps = 60;
        private const int NotifyPositionChangedInterval = 250;


        public override float Position {
            get => Duration == TimeSpan.Zero ? 0 : (int) Time / (float) Duration.TotalMilliseconds;
            set { if (Duration != TimeSpan.Zero) Time = (int) (value * (float) Duration.TotalMilliseconds); }
        }

        public override TimeSpan Duration {
            get => Media != null ? (Media.CustomDuration.TotalSeconds > 0 ? Media.CustomDuration : Media.CustomDuration.TotalSeconds == 0 ? DefaultDuration : TimeSpan.Zero) : TimeSpan.Zero;
            protected set => throw new NotSupportedException();
        }

        public override int Volume { get => 0; set { } }


        public override bool IsPlaying { get; protected set; }
        public override bool IsEnded { get => Position >= 1f && Duration != TimeSpan.Zero; protected set => throw new NotSupportedException(); }


        protected int Fps { get; private set; }
        protected float Time { get; private set; }

        private GLControl glControl;
        private Thread thread;

        private bool glControlResized = false;

        public override GLControl CreateView(Screen screen) {
            glControl = new GLControl {
                VSync = false
            };

            glControl.MakeCurrent();
            {
                Init();
            }
            glControl.Context.MakeCurrent(null);

            glControl.Resize += GlControl_Resize;

            thread = new Thread(Run);
            thread.Start();

            return glControl;
        }


        private void Run() {
            glControl.MakeCurrent();
            glControlResized = true;


            float optimalTime = 1000 / TargetFps;
            long lastLoopTime = TimeUtils.CurrentTimeMilliseconds;
            long lastFpsTime = 0;
            long lastNotifyPositionChanged = 0;
            int fps = 0;

            while (IsAttached) {
                long elapsedTime = TimeUtils.CurrentTimeMilliseconds - lastLoopTime;
                lastLoopTime = TimeUtils.CurrentTimeMilliseconds;

                float delta = elapsedTime / 1000f;

                // update the frame counter
                lastFpsTime += elapsedTime;
                lastNotifyPositionChanged += elapsedTime;
                fps++;


                if (lastNotifyPositionChanged >= NotifyPositionChangedInterval && IsPlaying) {
                    NotifyOnPositionChanged(Position);

                    if (IsEnded) {
                        IsPlaying = false;
                        NotifyOnChanged();
                    }

                    lastNotifyPositionChanged = 0;
                }

                if (lastFpsTime >= 1000) {
                    Fps = fps;
                    lastFpsTime = 0;
                    fps = 0;
                    Logger.Trace("FPS: {0}", Fps);
                }



                if (glControlResized) {
                    glControlResized = false;

                    float ratio = (float) glControl.Width / glControl.Height;

                    Logger.Trace("Resize: {0}x{1} ({2})", glControl.Width, glControl.Height, ratio);

                    OnResized(glControl.Width, glControl.Height, ratio);
                }

                if (IsPlaying) {
                    Time += elapsedTime;
                    Update(delta);

                    Render();
                    glControl.SwapBuffers();
                }

                int sleep = (int) (lastLoopTime - TimeUtils.CurrentTimeMilliseconds + optimalTime);
                if (sleep > 0)
                    Thread.Sleep(sleep);
            }

            Logger.Trace("Render thread exited");
        }

        public override void Reset() {
            Time = 0;
        }

        protected abstract void Init();

        protected abstract void Render();
        protected abstract void Update(float deltaTime);

        protected abstract void OnResized(int width, int height, float ratio);

        protected override void PlayMedia(IMediaItem item = null) {
            IsPlaying = true;

            if (item != null) {
                Media = item;
            } else if (Media != null) {
                Time = 0;
            }

            NotifyOnChanged();
        }

        protected override void PauseMedia() {
            IsPlaying = !IsPlaying;
            NotifyOnChanged();
        }

        protected override void StopMedia() {
            IsPlaying = false;
            Media = null;
            NotifyOnChanged();
        }

        private void GlControl_Resize(object sender, EventArgs e) {
            if (glControl != null)
                glControlResized = true;
        }

        protected override void Dispose(bool disposing) {
            base.Dispose(disposing);

            if (disposing) {
                if (thread != null) {
                    thread.Join();
                    thread = null;
                }
                glControl.Dispose();
                glControl = null;
            }

        }

    }

}