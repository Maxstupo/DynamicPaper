namespace Maxstupo.DynamicPaper.Wallpaper {
    using System;
    using System.Collections;
    using System.Collections.Generic;

    public sealed class Playlist : IEnumerable<PlaylistItem> {

        private readonly List<PlaylistItem> items = new List<PlaylistItem>();
        public IReadOnlyList<PlaylistItem> Items => items.AsReadOnly();

        public int CurrentIndex { get; set; } = 0;

        public event EventHandler OnChange;

        public void Clear() {
            items.Clear();
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