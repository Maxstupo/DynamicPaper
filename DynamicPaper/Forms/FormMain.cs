﻿namespace Maxstupo.DynamicPaper.Forms {

    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Diagnostics;
    using System.Drawing;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Windows.Forms;
    using HeyRed.Mime;
    using Maxstupo.DynamicPaper.Utility;
    using Maxstupo.DynamicPaper.Utility.Windows;
    using Maxstupo.DynamicPaper.Wallpaper;
    using Maxstupo.DynamicPaper.Wallpaper.Players;
    using Maxstupo.DynamicPaper.Wallpaper.Players.Impl;
    using Microsoft.WindowsAPICodePack.Dialogs;
    using Newtonsoft.Json;

    public partial class FormMain : TrayAwareForm {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        private const string SupportUrl = @"https://www.github.com/Maxstupo/DynamicPaper/wiki";

        private static readonly FileFilterBuilder FileFilterBuilder = new FileFilterBuilder()
           .Add("Image Files", "png", "jpg", "jpeg", "bmp", "gif", "tiff", "svg")
           .Add("Video Files", "mkv", "mp4", "mov", "avi", "wmv", "gif", "webm")
           .Add("DynamicPaper Playlist Files", "dpp")
           .Add("DynamicPaper Shadertoy Files", "dpst", "shadertoy")
           .AddGroup("Supported Files", 0) // Will concat all previously added filters into a new filter. 
           .Add("All Files", "*");

        private static readonly string PlaylistFileFilter = new FileFilterBuilder()
           .Add("DynamicPaper Playlist Files", "dpp")
           .Add("All Files", "*").ToString();

        public static readonly string FileFilter = FileFilterBuilder.ToString();

        private ScreenInfo[] monitors;
        private ScreenInfo lastSelectedMonitor;

        private IPlaylistItem SelectedPlaylistItem => lbxPlaylist.SelectedItem as IPlaylistItem;

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
                Vlc.Initialize();
            }

            Logger.Debug("Initializing windows wallpaper...");
            if (!WindowsWallpaper.Init()) {
                MessageBox.Show(this, "Failed to initalize windows wallpaper! Failed to acquire desktop window pointer!", Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
                Application.Exit();
            }

            MimeTypesMap.AddOrUpdate("playlist/json", "dpp");
            MimeTypesMap.AddOrUpdate("shadertoy/dpst", "dpst");
            MimeTypesMap.AddOrUpdate("shadertoy/shadertoy", "shadertoy");

            MediaPlayerStore.Instance.RegisterPlayer<VlcMediaPlayer>("video/x-matroska", "video/mp4", "video/mov", "video/avi", "video/wmv", "video/gif", "video/webm");
            MediaPlayerStore.Instance.RegisterPlayer<PictureBoxPlayer>("image/jpeg", "image/png", "image/jpg", "image/bmp", "image/tiff", "image/svg");
            MediaPlayerStore.Instance.RegisterPlayer<ShaderToyPlayer>("shadertoy/dpst", "shadertoy/shadertoy");

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
            UpdateTrackInformation();

            if (Settings.RestorePlaylists)
                RestorePlaylists();

            if (Settings.RestorePlaying && Settings.RestorePlaylists)
                RestorePlaying();


            if (Settings.StartMinimized && Settings.MinimizeToTray)
                Visible = false;

        }

        private void SaveCurrentPlaylistToFile(string filepath) {
            string json = JsonConvert.SerializeObject(CurrentPlayer.Playlist, Formatting.Indented);
            File.WriteAllText(filepath, json, Encoding.UTF8);
        }

        private void SavePlaylists() {
            Logger.Debug("Saving playlists...");


            Settings.Playlists.Clear();
            foreach (ScreenInfo monitor in monitors) {
                IPlaylistPlayer player = GetPlayer(monitor);

                if (player.Playlist.Count == 0)
                    continue;

                Settings.Playlists.Add(monitor.Index, player.Playlist);
            }

            settingsManager.Save();
        }

        private void RestorePlaylists() {
            Logger.Debug("Restoring playlists...");

            foreach (ScreenInfo monitor in monitors) {
                IPlaylistPlayer player = GetPlayer(monitor);
                if (Settings.Playlists.TryGetValue(monitor.Index, out Playlist playlist)) {
                    player.Playlist.Clear(false);
                    player.Playlist.AddRange(playlist.Items, true);

                    player.Playlist.CurrentIndex = playlist.CurrentIndex;
                }
            }
        }

        private void RestorePlaying() {
            Logger.Debug("Restoring currently playing...");

            foreach (ScreenInfo monitor in monitors) {
                IPlaylistPlayer player = GetPlayer(monitor);

                if (player.Playlist.Count > 0)
                    player.Play(player.Playlist.CurrentItem);
            }
        }

        private void FormMain_FormClosing(object sender, FormClosingEventArgs e) {
            if (e.Cancel)
                return;

            SavePlaylists();

            Logger.Debug("Disposing players...");
            foreach (IPlaylistPlayer player in players.Values) {
                try {
                    player.Dispose();
                } catch (Exception ee) {
                    Logger.Error(ee);
                }
            }

            Logger.Debug("Disposing libVLC...");
            Vlc.LibVLC.Dispose();

            //        WindowsWallpaper.ResetDesktopBackground();
        }

        private IPlaylistPlayer GetPlayer(ScreenInfo info) {
            if (info == null)
                return null;
            if (players.TryGetValue(info, out IPlaylistPlayer player))
                return player;

            player = new MultiMediaPlayer(info.Screen, Settings);
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

            bool hasMedia = CurrentPlayer != null && CurrentPlayer.Media != null;

            btnPlayPause.Enabled = hasMedia || (!hasMedia && lbxPlaylist.Items.Count > 0);
            btnStop.Enabled = hasMedia;

            timelineSlider.Enabled = hasMedia;
            if (!timelineSlider.Enabled)
                timelineSlider.Time = 0;
            volumeSlider.Enabled = hasMedia;

            btnLoop.Enabled = btnShuffle.Enabled = openFileToolStripMenuItem.Enabled = openFolderToolStripMenuItem.Enabled = CurrentPlayer != null;

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
            TimeSpan duration = CurrentPlayer?.Duration ?? TimeSpan.Zero;

            TimeSpan current = TimeSpan.FromMilliseconds(duration.TotalMilliseconds * newTime);
            tsslTime.Text = Settings.ShowTimeLeft ? $"-{duration - current:hh':'mm':'ss} / {duration:hh':'mm':'ss}" : $"{current:hh':'mm':'ss} / {duration:hh':'mm':'ss}";
        }

        private void UpdateTrackInformation() {
            tsslCurrentTrack.Text = CurrentPlayer?.Media?.Filepath ?? string.Empty;
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
            if (!File.Exists(filepath)) {
                Logger.Warn("Playlist item doesn't exist {0}, skip adding...", filepath);
                return;
            }

            string mimeType = MimeTypesMap.GetMimeType(filepath);

            if (mimeType.StartsWith("playlist/")) {
                // TODO: Add support for opening playlist files.
                throw new NotImplementedException();
             //   return;
            }

            Logger.Debug("Adding playlist item {0}", filepath);

            PlaylistItem item = new PlaylistItem(filepath);
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

            RefreshPlaylist();
            RefreshEnabled();
        }

        #region Events

        private void CurrentPlayer_OnPositionChanged(object sender, float time) {
            timelineSlider.DisableNotify = true;
            timelineSlider.Time = time;
            timelineSlider.DisableNotify = false;

            UpdateStatusBar(time);
        }

        private void CurrentPlayer_OnChange(object sender, EventArgs e) {
            RefreshPlaylist();
            RefreshEnabled();
            UpdateTrackInformation();

            volumeSlider.DisableNotify = true;
            volumeSlider.Volume = (CurrentPlayer.Volume / 100f);
            volumeSlider.DisableNotify = false;
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

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e) {
            using (AboutBox dialog = new AboutBox())
                dialog.ShowDialog(this);
        }

        private void supportToolStripMenuItem_Click(object sender, EventArgs e) {
            Process.Start(SupportUrl);
        }

        #endregion

        #region Player Control Events

        private void btnPlayPause_Click(object sender, EventArgs e) {

            IPlaylistItem item = SelectedPlaylistItem;

            if (!CurrentPlayer.IsPlaying && item == null && lbxPlaylist.Items.Count > 0)
                item = CurrentPlayer.Playlist.Items[0];



            if (CurrentPlayer.Media == null && item != null) { // No media loaded.
                CurrentPlayer.Play(item);

            } else if (!CurrentPlayer.IsEnded) {
                CurrentPlayer.Pause();

            } else {
                CurrentPlayer.Play();

            }

        }

        private void btnStop_Click(object sender, EventArgs e) {
            CurrentPlayer.Stop();
        }

        private void btnLoop_Click(object sender, EventArgs e) {
            CurrentPlayer.LoopMode = btnLoop.Value;
        }

        private void btnShuffle_Click(object sender, EventArgs e) {
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

            if (e.Button == MouseButtons.Right && CurrentPlayer != null)
                cmsPlaylist.Show(lbxPlaylist.PointToScreen(e.Location));
        }

        private void lbxPlaylist_MouseDoubleClick(object sender, MouseEventArgs e) {
            if (lbxPlaylist.SelectedItem != null)
                playToolStripMenuItem.PerformClick();
        }


        #region Context Menu Strip for Playlist Events

        private void playToolStripMenuItem_Click(object sender, EventArgs e) {
            if (CurrentPlayer != null)
                CurrentPlayer.Play(lbxPlaylist.SelectedItem as IPlaylistItem);
        }

        private void cmsPlaylist_Opening(object sender, System.ComponentModel.CancelEventArgs e) {
            bool hasItems = lbxPlaylist.SelectedItems.Count > 0;

            bool hasNativeDuration = lbxPlaylist.SelectedItems.Cast<IPlaylistItem>().Any(itm => itm.HasNativeDuration);

            setDurationToolStripMenuItem.Visible = toolStripSeparator5.Visible = !hasNativeDuration && hasItems;

            playToolStripMenuItem.Visible = hasItems;
            moveToToolStripMenuItem.Visible = hasItems;
            toolStripSeparator4.Visible = hasItems;
            removeToolStripMenuItem.Visible = hasItems;



            moveToToolStripMenuItem.DropDownItems.Clear();
            moveToToolStripMenuItem.DropDownItems.AddRange(monitors.Where(x => x != cbxMonitor.SelectedItem).Select(x => new ToolStripMenuItem(x.DisplayName) { Tag = x }).ToArray());
        }

        private void clearPlaylistToolStripMenuItem_Click(object sender, EventArgs e) {
            CurrentPlayer.Playlist.Clear();
        }

        private void removeToolStripMenuItem_Click(object sender, EventArgs e) {
            foreach (PlaylistItem item in lbxPlaylist.SelectedItems.Cast<PlaylistItem>().ToList())
                CurrentPlayer.Playlist.Remove(item);
        }

        private void moveToToolStripMenuItem_DropDownItemClicked(object sender, ToolStripItemClickedEventArgs e) {
            IPlaylistPlayer targetPlayer = GetPlayer((ScreenInfo) e.ClickedItem.Tag);
            if (targetPlayer != null) {

                foreach (PlaylistItem item in lbxPlaylist.SelectedItems.Cast<PlaylistItem>()) {
                    if (!targetPlayer.Playlist.Contains(item)) {
                        targetPlayer.Playlist.Add(item, false);
                        CurrentPlayer.Playlist.Remove(item, false);
                    }
                }

                targetPlayer.Playlist.NotifyChanged();
                CurrentPlayer.Playlist.NotifyChanged();

            }

        }

        #endregion

        private void tsslTime_Click(object sender, EventArgs e) {
            Settings.ShowTimeLeft = !Settings.ShowTimeLeft;
            UpdateStatusBar(CurrentPlayer?.Position ?? 0);
        }

        private void savePlaylistToFileToolStripMenuItem_Click(object sender, EventArgs e) {
            using (SaveFileDialog dialog = new SaveFileDialog()) {
                dialog.Title = "Save playlist as...";
                dialog.Filter = PlaylistFileFilter;

                if (dialog.ShowDialog(this) == DialogResult.OK) {
                    SaveCurrentPlaylistToFile(dialog.FileName);
                }
            }
        }

        private void timelineSlider_TimeChanged(object sender, float e) {
            CurrentPlayer.Position = e;
            UpdateStatusBar(e);
        }

        private void volumeSlider_VolumeChanged(object sender, float e) {
            CurrentPlayer.Volume = (int) (e * 100f);
        }

        private void setDurationToolStripMenuItem_Click(object sender, EventArgs e) {
            TimeSpan duration = TimeSpan.Zero;

            if (lbxPlaylist.SelectedItems.Count == 1) {
                IPlaylistItem item = lbxPlaylist.SelectedItems[0] as IPlaylistItem;
                duration = item.CustomDuration;
            }

            using (DialogSetDuration dialog = new DialogSetDuration(duration)) {
                if (dialog.ShowDialog(this) == DialogResult.OK) {
                    duration = dialog.Duration;

                    foreach (IPlaylistItem item in lbxPlaylist.SelectedItems.Cast<IPlaylistItem>())
                        item.CustomDuration = duration;

                }
            }
        }

        private void FormMain_KeyDown(object sender, KeyEventArgs e) {

            switch (e.KeyCode) {
                case Keys.Space:
                    btnPlayPause.PerformClick();
                    e.Handled = true;
                    break;
                case Keys.Escape:
                    btnStop.PerformClick();
                    e.Handled = true;
                    break;
                case Keys.Left:
                    CurrentPlayer?.PlayPrevious();
                    e.Handled = true;
                    break;
                case Keys.Right:
                    CurrentPlayer?.PlayNext();
                    e.Handled = true;
                    break;
                default:
                    break;
            }

            int keyCode = e.KeyValue;
            if (keyCode >= (int) Keys.D0 && keyCode <= (int) Keys.D9) {
                int index = 8 - ((int) Keys.D9 - keyCode);
                if (index == -1) // Keys.D0
                    index = 9;


                if (index < cbxMonitor.Items.Count) {

                }
                e.Handled = true;
            }

        }

        #endregion

    }

}