namespace Maxstupo.DynamicPaper.Wallpaper {

    using System;
    using System.Collections;
    using System.Collections.Generic;
    using Newtonsoft.Json;

    [JsonObject]
    public sealed class Playlist : IEnumerable<PlaylistItem> {

        [JsonProperty("items")]
        private readonly List<PlaylistItem> items = new List<PlaylistItem>();

        [JsonIgnore]
        public IReadOnlyList<PlaylistItem> Items => items.AsReadOnly();

        [JsonProperty("playing_index")]
        public int CurrentIndex { get; set; } = 0;

        [JsonIgnore]
        public int Count => items.Count;

        public event EventHandler OnChange;

        public void Clear(bool notify = true) {
            items.Clear();
            if (notify)
                NotifyChanged();
        }

        public void AddRange(IEnumerable<PlaylistItem> src, bool notify = true) {
            items.AddRange(src);

            if (notify)
                NotifyChanged();
        }

        public void Add(PlaylistItem item, bool notify = true) {
            if (item == null)
                return;
            items.Add(item);

            if (notify)
                NotifyChanged();
        }

        public bool Remove(PlaylistItem item, bool notify = true) {

            bool result = items.Remove(item);

            if (notify && result)
                NotifyChanged();

            return result;
        }

        public void Insert(int index, PlaylistItem item, bool notify = true) {
            if (item == null || index < 0)
                return;
            items.Insert(index, item);

            if (notify)
                NotifyChanged();
        }

        public void NotifyChanged() {
            OnChange?.Invoke(this, EventArgs.Empty);
        }

        public IEnumerator<PlaylistItem> GetEnumerator() {
            return items.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }


    }

}