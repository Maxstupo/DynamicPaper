namespace Maxstupo.DynWallpaper.Graphics.Data {

    using System;
    using System.Collections;
    using System.Collections.Generic;
    using OpenTK.Graphics.OpenGL4;

    public sealed class VertexBufferItem {

        public VertexAttribPointerType Type { get; }

        public int Count { get; }

        public bool Normalized { get; }

        public VertexBufferItem(VertexAttribPointerType type, int count, bool normalized) {
            this.Type = type;
            this.Count = count;
            this.Normalized = normalized;
        }

    }

    public sealed class VertexBufferLayout : IEnumerable<VertexBufferItem> {

        private readonly List<VertexBufferItem> items = new List<VertexBufferItem>();
        public IReadOnlyList<VertexBufferItem> Items => items.AsReadOnly();

        public int Stride { get; private set; }

        public VertexBufferLayout Clear() {
            items.Clear();
            Stride = 0;
            return this;
        }

        public VertexBufferLayout Add(VertexAttribPointerType type, int count, bool normalized = false) {
            if (count <= 0)
                throw new ArgumentOutOfRangeException(nameof(count), count, "Count must be greater than zero!");

            items.Add(new VertexBufferItem(type, count, normalized));
            Stride += count * GetTypeSize(type);

            return this;
        }

        public static int GetTypeSize(VertexAttribPointerType type) {
            switch (type) {
                case VertexAttribPointerType.Float: return 4;
                case VertexAttribPointerType.UnsignedInt: return 4;
                case VertexAttribPointerType.UnsignedByte: return 1;
            }
            throw new NotSupportedException($"Attribute pointer type {type} isn't supported!");
        }

        public IEnumerator<VertexBufferItem> GetEnumerator() {
            return items.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }

    }

}