namespace Maxstupo.DynamicPaper.Wallpaper.Players.Impl {

    using System;
    using System.Threading;
    using System.Windows.Forms;

    public sealed class MultiMediaPlayer : IPlaylistPlayer {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        private readonly Random random = new Random();

        private readonly Screen screen;
        private readonly AppSettings settings;

        public float Position {
            get => internalPlayer?.Position ?? 0;
            set {
                if (internalPlayer != null)
                    internalPlayer.Position = value;
            }
        }
        public TimeSpan Duration => internalPlayer?.Duration ?? TimeSpan.Zero;
        public TimeSpan DefaultDuration { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }

        public int Volume {
            get => internalPlayer?.Volume ?? 0;
            set {
                if (internalPlayer != null)
                    internalPlayer.Volume = value;
            }
        }

        public bool IsPlaying => internalPlayer?.IsPlaying ?? false;
        public bool IsEnded => internalPlayer?.IsEnded ?? false;

        public IMediaItem Media => internalPlayer?.Media;

        public Playlist Playlist { get; } = new Playlist();
        public LoopMode LoopMode { get; set; }
        public ShuffleMode ShuffleMode { get; set; }

        public event EventHandler OnChanged;
        public event EventHandler<float> OnPositionChanged;

        private IAttachablePlayer internalPlayer;

        private readonly SynchronizationContext context;

        public MultiMediaPlayer(Screen screen, AppSettings settings) {
            this.screen = screen;
            this.settings = settings;
            Playlist.OnChange += Playlist_OnChange;

            context = SynchronizationContext.Current;
        }

        public void Play(IPlaylistItem item = null) {
            if (item != null)
                Playlist.CurrentIndex = item.PlaylistIndex;

            Play(item as IMediaItem);
        }

        public void Play(IMediaItem item = null) {
            if (item != null) {
                IAttachablePlayer newPlayer = MediaPlayerStore.Instance.CreatePlayer(item.MimeType, out bool isRecycled, internalPlayer);

                if (newPlayer == null) {
                    Logger.Warn($"Failed to find suitable player for '{0}'", item.MimeType);
                    return;
                }

                if (internalPlayer != null && !isRecycled) {
                    Logger.Debug("Disposing previous internal player...");

                    internalPlayer.OnChanged -= InternalPlayer_OnChanged;
                    internalPlayer.OnPositionChanged -= InternalPlayer_OnPositionChanged;
                    internalPlayer.Stop();
                    internalPlayer.Dispose();
                }

                internalPlayer = newPlayer;

                if (!isRecycled) {
                    Logger.Trace("Connecting event listeners...");

                    internalPlayer.OnChanged += InternalPlayer_OnChanged;
                    internalPlayer.OnPositionChanged += InternalPlayer_OnPositionChanged;

                } else if (item != internalPlayer.Media) {
                    Logger.Debug("Reseting player...");
                    internalPlayer.Reset();
                }

            }

            if (internalPlayer != null) {
                internalPlayer.DefaultDuration = settings.DefaultMediaDuration;

                internalPlayer.Attach(screen);
                internalPlayer.Play(item);
            }

        }



        public void Pause() {
            internalPlayer?.Pause();
        }

        public void Stop() {
            internalPlayer?.Stop();

            // Dispose instead of detach to perserve memory that internal players may hog.
            Dispose();
        }

        public void Dispose() {
            Logger.Trace("Disposing...");

            if (internalPlayer != null) {
                internalPlayer.OnChanged -= InternalPlayer_OnChanged;
                internalPlayer.Dispose();
            }
            internalPlayer = null;
        }

        private void InternalPlayer_OnChanged(object sender, EventArgs e) {

            object[] state = new object[] { // TODO: Find a better way to capture state.
                LoopMode,
                ShuffleMode,
                IsEnded,
                Playlist
            };

            context.Post(obj => {
                object[] objects = (object[]) obj;
                LoopMode loopmode = (LoopMode) objects[0];
                ShuffleMode shufflemode = (ShuffleMode) objects[1];
                bool isEnded = (bool) objects[2];
                Playlist playlist = (Playlist) objects[3];


                if (isEnded) {

                    int index = playlist.CurrentIndex;

                    if (loopmode == LoopMode.Current) {
                        Play((IMediaItem) null);
                    } else {
                        IPlaylistItem item = playlist.Items[index];

                        if (shufflemode == ShuffleMode.All) {
                            index = random.Next(0, playlist.Count);
                        } else {
                            index++;
                        }

                        bool isPlaylistEnd = index >= playlist.Count;
                        if (loopmode == LoopMode.All && shufflemode == ShuffleMode.None && isPlaylistEnd)
                            index = 0;

                        if (!isPlaylistEnd || LoopMode == LoopMode.All) {
                            item = Playlist.Items[index];
                            Play(item);
                        }
                    }

                }

                OnChanged?.Invoke(null, EventArgs.Empty);
            }, state);
        }

        public void PlayNext() {
            int index = Playlist.CurrentIndex;
            if (index < Playlist.Count - 1)
                Play(Playlist.Items[index + 1]);
        }

        public void PlayPrevious() {
            int index = Playlist.CurrentIndex;
            if (index > 0)
                Play(Playlist.Items[index - 1]);
        }

        private void InternalPlayer_OnPositionChanged(object sender, float pos) {
            context.Post(position => OnPositionChanged?.Invoke(null, (float) position), pos);
        }

        private void Playlist_OnChange(object sender, EventArgs e) {
            context.Post(_ => OnChanged?.Invoke(null, EventArgs.Empty), null);
        }

    }

}