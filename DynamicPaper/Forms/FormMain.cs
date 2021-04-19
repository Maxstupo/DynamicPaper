namespace Maxstupo.DynamicPaper.Forms {

    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Drawing;
    using System.IO;
    using System.Linq;
    using System.Windows.Forms;
    using HeyRed.Mime;
    using LibVLCSharp.Shared;
    using Maxstupo.DynamicPaper.Utility;
    using Maxstupo.DynamicPaper.Utility.Windows;
    using Maxstupo.DynamicPaper.Wallpaper;
    using Maxstupo.DynamicPaper.Wallpaper.Players;
    using Microsoft.WindowsAPICodePack.Dialogs;

    public partial class FormMain : TrayAwareForm {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        private static readonly FileFilterBuilder FileFilterBuilder = new FileFilterBuilder()
           .Add("Image Files", "png", "jpg", "jpeg", "bmp", "gif")
           .Add("Video Files", "mkv", "mp4", "mov", "avi", "wmv", "gif", "webm")
           .Add("DynamicPaper Playlist Files", "dpp")
           .AddGroup("Supported Files", 0) // Will concat all previously added filters into a new filter. 
           .Add("All Files", "*");

        public static readonly string FileFilter = FileFilterBuilder.ToString();

        private ScreenInfo[] monitors;
        private ScreenInfo lastSelectedMonitor;

        private PlaylistItem SelectedPlaylistItem => lbxPlaylist.SelectedItem as PlaylistItem;

        private AppSettings Settings => settingsManager.Settings;
        private readonly SettingsManager<AppSettings> settingsManager = new SettingsManager<AppSettings>("settings.json", () => new AppSettings());


        private IPlaylistPlayer CurrentPlayer { get; set; }
        private readonly Dictionary<ScreenInfo, IPlaylistPlayer> players = new Dictionary<ScreenInfo, IPlaylistPlayer>();


        private bool willDrag = false;
        private bool hasDragged = false;

        // TODO: Add pipe server for program mutex.
        public FormMain() {
            if (!DesignMode) {
                Logger.Debug("Initializing LibVLC...");
                Core.Initialize();
            }

            Logger.Debug("Initializing windows wallpaper...");
            if (!WindowsWallpaper.Init()) {
                MessageBox.Show(this, "Failed to initalize windows wallpaper! Failed to acquire desktop window pointer!", Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
                Application.Exit();
            }

            MimeTypesMap.AddOrUpdate("playlist/json", "dpp");

            MediaPlayerStore.Instance.RegisterPlayer<VlcMediaPlayer>("video/mkv", "video/mp4", "video/mov", "video/avi", "video/wmv", "video/gif", "video/webm");
         

            settingsManager.OnSettingsChanged += (sender, settings) => {
                CloseToTray = settings.CloseToTray;
                MinimizeToTray = settings.MinimizeToTray;
            };
            settingsManager.Load(true);

            if (Settings.StartMinimized)
                WindowState = FormWindowState.Minimized;

            InitializeComponent();
            showToolStripMenuItem.Click += trayShowForm_Click; // Connect the show button in the tray with the required method.
        }

        private void FormMain_Load(object sender, EventArgs e) {
            RefreshMonitorList();
            RefreshEnabled();
            UpdateStatusBar(0);

            if (Settings.StartMinimized && Settings.MinimizeToTray)
                Visible = false;

        }

        private void FormMain_FormClosing(object sender, FormClosingEventArgs e) {
            if (e.Cancel)
                return;

            Logger.Debug("Disposing players...");
            foreach (IPlaylistPlayer player in players.Values)
                player.Dispose();

            Logger.Debug("Disposing libVLC...");
            VlcMediaPlayer.LibVLC.Dispose();

            WindowsWallpaper.ResetDesktopBackground();
        }

        private IPlaylistPlayer GetPlayer(ScreenInfo info) {
            if (players.TryGetValue(info, out IPlaylistPlayer player))
                return player;

            player = new PlaylistPlayer(info.Screen);
            players[info] = player;
            return player;
        }

        #region Refresh/Update Methods

        public void RefreshMonitorList() {
            monitors = ScreenInfo.AllScreens;

            cbxMonitor.DataSource = null;
            cbxMonitor.DisplayMember = nameof(ScreenInfo.DisplayName);
            cbxMonitor.DataSource = monitors;

            CurrentScreenSelectionChanged();
        }

        private void RefreshEnabled() {
            Logger.Trace("Refresh control enabled states...");

            bool isAttached = CurrentPlayer?.IsAttached ?? false;

            btnPlayPause.Enabled = isAttached || (!isAttached && (lbxPlaylist.SelectedItems.Count == 1 || lbxPlaylist.Items.Count == 1));
            btnStop.Enabled = isAttached;

            timelineSlider.Enabled = isAttached;
            volumeSlider.Enabled = isAttached;

            btnPlayPause.Image = (CurrentPlayer?.IsPlaying ?? false) ? Properties.Resources.pause_button : Properties.Resources.play_button;

        }

        private void RefreshPlaylist() {
            if (CurrentPlayer == null)
                return;

            Logger.Trace("Refresh playlist items...");

            lbxPlaylist.Items.Clear();
            foreach (PlaylistItem item in CurrentPlayer.Playlist)
                lbxPlaylist.Items.Add(item);
        }

        private void UpdateStatusBar(float newTime) {
            if (CurrentPlayer == null)
                return;
            TimeSpan current = TimeSpan.FromMilliseconds(CurrentPlayer.Duration.TotalMilliseconds * newTime);
            tsslTime.Text = Settings.ShowTimeLeft ? $"-{CurrentPlayer.Duration - current:hh':'mm':'ss} / {CurrentPlayer.Duration:hh':'mm':'ss}" : $"{current:hh':'mm':'ss} / {CurrentPlayer.Duration:hh':'mm':'ss}";

            tsslCurrentTrack.Text = CurrentPlayer.PlayingMedia?.Filepath ?? string.Empty;
        }


        #endregion

        #region Show Dialog Methods

        public void ShowSettingsDialog(bool useParent = true) {

            using (FormSettings dialog = new FormSettings(settingsManager)) {

                if (!useParent)
                    dialog.StartPosition = FormStartPosition.CenterScreen;

                dialog.ShowDialog(useParent ? this : null);
            }

        }

        public void ShowOpenFileDialog() {
            using (OpenFileDialog dialog = new OpenFileDialog()) {
                dialog.Title = "Select file(s)...";
                dialog.Multiselect = true;
                dialog.Filter = FileFilter;

                if (dialog.ShowDialog(this) == DialogResult.OK) {

                    foreach (string filepath in dialog.FileNames)
                        AddPlaylistItem(filepath, false);


                    if (CurrentPlayer != null)
                        CurrentPlayer.Playlist.NotifyChanged();

                }
            }
        }

        public void ShowOpenFolderDialog() {

            using (CommonOpenFileDialog dialog = new CommonOpenFileDialog()) {
                dialog.Title = "Select folder(s)...";
                dialog.Multiselect = true;
                dialog.IsFolderPicker = true;

                if (dialog.ShowDialog(Handle) == CommonFileDialogResult.Ok) {

                    HashSet<string> validExtensions = FileFilterBuilder.Extensions;

                    foreach (string path in dialog.FileNames) {
                        // TopDirectoryOnly for safety
                        foreach (string filepath in Directory.EnumerateFiles(path, "*.*", SearchOption.TopDirectoryOnly).Where(x => validExtensions.Contains(Path.GetExtension(x))))
                            AddPlaylistItem(filepath, false);
                    }

                    if (CurrentPlayer != null)
                        CurrentPlayer.Playlist.NotifyChanged();

                }
            }

        }

        #endregion

        public void AddPlaylistItem(string filepath, bool notifyChange = true) {
            if (CurrentPlayer == null)
                return;

            if (!File.Exists(filepath)) {
                Logger.Warn("Playlist item doesn't exist {0}, skip adding...", filepath);
                return;
            }

            string mimeType = MimeTypesMap.GetMimeType(filepath);

            if (mimeType.StartsWith("playlist/")) {
                // TODO: Add support for opening playlist files.
                return;
            }

            Logger.Debug("Adding playlist item {0}", filepath);

            PlaylistItem item = new PlaylistItem(filepath, mimeType);
            CurrentPlayer.Playlist.Add(item, notifyChange);
        }

        private void CurrentScreenSelectionChanged() {
            if (CurrentPlayer != null) { // Remove hooks for previous player.
                CurrentPlayer.OnChanged -= CurrentPlayer_OnChange;
                CurrentPlayer.OnPositionChanged -= CurrentPlayer_OnPositionChanged;
            }

            CurrentPlayer = GetPlayer((ScreenInfo) cbxMonitor.SelectedItem);

            if (CurrentPlayer != null) { // Add hooks for current player.
                CurrentPlayer.OnChanged += CurrentPlayer_OnChange;
                CurrentPlayer.OnPositionChanged += CurrentPlayer_OnPositionChanged;

                btnLoop.Value = CurrentPlayer.LoopMode;
                btnShuffle.Value = CurrentPlayer.ShuffleMode;
            }
        }
   
        #region Events

        private void CurrentPlayer_OnPositionChanged(object sender, float time) {
            // Logger.Trace("CurrentPlayer_OnPositionChanged({0})",time);

            Action action = () => {
                timelineSlider.DisableNotify = true;
                timelineSlider.Time = time;
                timelineSlider.DisableNotify = false;
                UpdateStatusBar(time);
            };

            if (InvokeRequired) {
                timelineSlider.Invoke(action);
            } else {
                action();
            }
        }

        private void CurrentPlayer_OnChange(object sender, EventArgs e) {
            // Logger.Trace("CurrentPlayer_OnChange()" );

            Action action = () => {
                RefreshPlaylist();
                RefreshEnabled();
            };

            if (InvokeRequired) {
                Invoke(action);
            } else {
                action();
            }
        }

        private void FormMain_OnTrayEntered(object sender, EventArgs e) {
            notifyIcon?.ShowBalloonTip(5000, Application.ProductName, "Now running in the background. Use tray icon to reveal.", ToolTipIcon.Info);
        }

        private void traySettingsToolStripMenuItem_Click(object sender, EventArgs e) {
            ShowSettingsDialog(false);
        }

        private void cbxMonitor_SelectionChangeCommitted(object sender, EventArgs e) {
            if (lastSelectedMonitor != cbxMonitor.SelectedItem) {
                lastSelectedMonitor = (ScreenInfo) cbxMonitor.SelectedItem;
                CurrentScreenSelectionChanged();
            }
        }

        #region Menu Strip 

        private void openFileToolStripMenuItem_Click(object sender, EventArgs e) {
            ShowOpenFileDialog();
        }

        private void openFolderToolStripMenuItem_Click(object sender, EventArgs e) {
            ShowOpenFolderDialog();
        }

        private void settingsToolStripMenuItem_Click(object sender, EventArgs e) {
            ShowSettingsDialog(false);
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e) {
            Application.Exit();
        }

        #endregion

        #region Player Control Events

        private void btnPlayPause_Click(object sender, EventArgs e) {
            if (CurrentPlayer == null)
                return;

            PlaylistItem item = SelectedPlaylistItem;


            if (!CurrentPlayer.IsAttached && item != null) { // No media loaded.
                CurrentPlayer.Play(item);

            } else if (!CurrentPlayer.IsEnded) {
                CurrentPlayer.Pause();

            } else {
                CurrentPlayer.Play();

            }

        }

        private void btnStop_Click(object sender, EventArgs e) {
            if (CurrentPlayer == null)
                return;

            CurrentPlayer.Stop();
        }

        private void btnLoop_Click(object sender, EventArgs e) {
            if (CurrentPlayer == null)
                return;
            CurrentPlayer.LoopMode = btnLoop.Value;
        }

        private void btnShuffle_Click(object sender, EventArgs e) {
            if (CurrentPlayer == null)
                return;
            CurrentPlayer.ShuffleMode = btnShuffle.Value;
        }

        #endregion

        #region Playlist Drag & Drop

        private void lbxPlaylist_DragDrop(object sender, DragEventArgs e) {
            if (e.Data.GetDataPresent(DataFormats.FileDrop)) {

                string[] files = (string[]) e.Data.GetData(DataFormats.FileDrop);
                foreach (string file in files)
                    AddPlaylistItem(file, false);

                if (CurrentPlayer != null)
                    CurrentPlayer.Playlist.NotifyChanged();

            } else if (e.Data.GetDataPresent(typeof(List<PlaylistItem>))) {
                Point point = lbxPlaylist.PointToClient(new Point(e.X, e.Y));

                int index = lbxPlaylist.IndexFromPoint(point);
                if (index < 0)
                    index = lbxPlaylist.Items.Count - 1;

                List<PlaylistItem> items = (List<PlaylistItem>) e.Data.GetData(typeof(List<PlaylistItem>));


                foreach (PlaylistItem item in items) {
                    CurrentPlayer.Playlist.Remove(item, false);
                    CurrentPlayer.Playlist.Insert(index, item, false);
                }

                CurrentPlayer.Playlist.NotifyChanged();

            }
        }

        private void lbxPlaylist_DragOver(object sender, DragEventArgs e) {
            if (e.Data.GetDataPresent(typeof(List<PlaylistItem>)))
                e.Effect = DragDropEffects.Move;
        }

        private void lbxPlaylist_DragEnter(object sender, DragEventArgs e) {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effect = DragDropEffects.Copy;
        }

        private void lbxPlaylist_MouseUp(object sender, MouseEventArgs e) {
            willDrag = false;
            hasDragged = false;
        }

        private void lbxPlaylist_MouseMove(object sender, MouseEventArgs e) {
            if (willDrag && !hasDragged) {
                lbxPlaylist.DoDragDrop(lbxPlaylist.SelectedItems.Cast<PlaylistItem>().ToList(), DragDropEffects.Move);
                hasDragged = true;
                willDrag = false;
            }
        }

        #endregion

        private void lbxPlaylist_MouseDown(object sender, MouseEventArgs e) {
            int index = lbxPlaylist.IndexFromPoint(e.Location);

            if (index == ListBox.NoMatches) {
                lbxPlaylist.ClearSelected();

            } else {
                if (lbxPlaylist.SelectedItems.Count <= 1 && e.Button == MouseButtons.Right) {
                    lbxPlaylist.ClearSelected();
                    lbxPlaylist.SetSelected(index, true);
                }

                if (e.Clicks == 1 && e.Button == MouseButtons.Left && lbxPlaylist.SelectedIndices.Contains(index)) {
                    willDrag = true;
                    hasDragged = false;
                }
            }

            if (e.Button == MouseButtons.Right) 
                cmsPlaylist.Show(lbxPlaylist.PointToScreen(e.Location));
        }
      
        private void lbxPlaylist_MouseDoubleClick(object sender, MouseEventArgs e) {
            if (lbxPlaylist.SelectedItem != null)
                playToolStripMenuItem.PerformClick();
        }


        #region Context Menu Strip for Playlist Events

        private void playToolStripMenuItem_Click(object sender, EventArgs e) {
            if (CurrentPlayer != null)
                CurrentPlayer.Play(lbxPlaylist.SelectedItem as PlaylistItem);
        }

        private void cmsPlaylist_Opening(object sender, System.ComponentModel.CancelEventArgs e) {
            bool hasItems = lbxPlaylist.SelectedItems.Count > 0;

            playToolStripMenuItem.Visible = hasItems;
            moveToToolStripMenuItem.Visible = hasItems;
            toolStripSeparator4.Visible = hasItems;
            removeToolStripMenuItem.Visible = hasItems;

            moveToToolStripMenuItem.DropDownItems.Clear();
            moveToToolStripMenuItem.DropDownItems.AddRange(monitors.Where(x => x != cbxMonitor.SelectedItem).Select(x => new ToolStripMenuItem(x.DisplayName) { Tag = x }).ToArray());

        }

        private void clearPlaylistToolStripMenuItem_Click(object sender, EventArgs e) {
            if (CurrentPlayer == null)
                return;

            CurrentPlayer.Playlist.Clear();
        }

        private void removeToolStripMenuItem_Click(object sender, EventArgs e) {
            if (CurrentPlayer == null)
                return;

            foreach (PlaylistItem item in lbxPlaylist.SelectedItems.Cast<PlaylistItem>().ToList())
                CurrentPlayer.Playlist.Remove(item);
        }

        private void moveToToolStripMenuItem_DropDownItemClicked(object sender, ToolStripItemClickedEventArgs e) {
            if (CurrentPlayer == null)
                return;

            IPlaylistPlayer targetPlayer = GetPlayer((ScreenInfo) e.ClickedItem.Tag);
            if (targetPlayer != null) {

                foreach (PlaylistItem item in lbxPlaylist.SelectedItems.Cast<PlaylistItem>()) {
                    if (!targetPlayer.Playlist.Contains(item)) {
                        targetPlayer.Playlist.Add(item);
                        CurrentPlayer.Playlist.Remove(item);
                    }
                }

                targetPlayer.Playlist.NotifyChanged();
                CurrentPlayer.Playlist.NotifyChanged();

            }

        }

        #endregion

        private void tsslTime_Click(object sender, EventArgs e) {
            Settings.ShowTimeLeft = !Settings.ShowTimeLeft;
            settingsManager.Save();
            UpdateStatusBar(CurrentPlayer?.Position ?? 0);
        }

        #endregion

    }

}