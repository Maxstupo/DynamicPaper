namespace Maxstupo.DynamicPaper.Wallpaper.Players {

    using System;
    using System.Windows.Forms;
    using Maxstupo.DynamicPaper.Utility.Windows;

    public abstract class AttachablePlayer<T> : IAttachablePlayer where T : Control {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        public string[] SupportedMimeTypes { get; set; }

        public abstract float Position { get; set; }
        public abstract TimeSpan Duration { get; protected set; }
        public TimeSpan DefaultDuration { get; set; }

        public abstract int Volume { get; set; }
        public abstract bool IsPlaying { get; protected set; }
        public abstract bool IsEnded { get; protected set; }


        private IMediaItem media;
        public virtual IMediaItem Media {
            get => media;
            protected set {
                if (media != null && value != media)
                    media.IsLoaded = false;
                media = value;
                if (media != null)
                    media.IsLoaded = true;
            }
        }

        public bool IsAttached { get; private set; }


        public event EventHandler OnChanged;
        public event EventHandler<float> OnPositionChanged;

        protected T View { get; private set; }



        // The original window parent pointer of the view. Restored on Detach()
        private IntPtr originalParent = IntPtr.Zero;

        public abstract T CreateView(Screen screen);


        public void Attach(Screen screen) {
            if (IsAttached)
                return;
            Logger.Trace("Attaching to screen...");

            IsAttached = true;

            if (View == null) {
                Logger.Debug("Creating view...");

                View = CreateView(screen);
                originalParent = WindowsWallpaper.GetParent(View);
            }

            View.Show();
            View.Bounds = WindowsWallpaper.GetScreenBounds(screen);

            WindowsWallpaper.SetParent(View);
        }

        public void Detach() {
            if (!IsAttached)
                return;

            IsAttached = false;

            Logger.Trace("Detaching from screen...");

            NativeMethods.SetParent(View.Handle, originalParent);
            View?.Hide();

            WindowsWallpaper.ResetDesktopBackground();
        }

        public void Play(IMediaItem item = null) {
            if (!IsAttached)
                throw new InvalidOperationException("The player must be Attached() before this method can be called!");
            PlayMedia(item);
        }

        public void Pause() {
            if (!IsAttached)
                throw new InvalidOperationException("The player must be Attached() before this method can be called!");
            PauseMedia();
        }

        public void Stop() {
            if (!IsAttached)
                throw new InvalidOperationException("The player must be Attached() before this method can be called!");
            StopMedia();
        }

        protected abstract void PlayMedia(IMediaItem item = null);
        protected abstract void PauseMedia();
        protected abstract void StopMedia();

        public virtual void Reset() { }

        protected void NotifyOnChanged() {
            OnChanged?.Invoke(this, EventArgs.Empty);
        }

        protected void NotifyOnPositionChanged(float position) {
            OnPositionChanged?.Invoke(this, position);
        }

        protected virtual void Dispose(bool disposing) {
            if (disposing) {
                // managed resources

                Detach();

                Logger.Trace("Disposing view...");
                View?.Dispose();
                View = null;

            }
            // unmanged resources
        }

        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }


        // uncomment if using unmanaged resources in this class.
        // ~AttachablePlayer() {
        //     Dispose(false);
        // }

    }

}