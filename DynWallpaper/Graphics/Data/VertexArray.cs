namespace Maxstupo.DynWallpaper.Graphics.Data {

    using System;
    using System.Collections.Generic;
    using OpenTK.Graphics.OpenGL4;

    public sealed class VertexArray : IBindable {

        public int Id { get; }

        public bool IsDisposed { get; private set; }

        public VertexArray() {
            Id = GL.GenVertexArray();
        }

        // Associates the VBO (and optional EBO) with this VAO.
        public void Link(VertexBuffer vbo, ElementBuffer ebo = null) {
            Bind();
            vbo.Bind();
            ebo?.Bind();

            UpdateVAO(vbo.Layout);

            Unbind();
            vbo.Unbind();
            ebo?.Unbind();
        }

        private void UpdateVAO(VertexBufferLayout vbl) {
            IReadOnlyList<VertexBufferItem> items = vbl.Items;

            int offset = 0;

            for (int index = 0; index < items.Count; index++) {
                VertexBufferItem item = items[index];

                GL.EnableVertexAttribArray(index);
                GL.VertexAttribPointer(index, item.Count, item.Type, item.Normalized, vbl.Stride, offset);

                offset += item.Count * VertexBufferLayout.GetTypeSize(item.Type);
            }

        }

        public void Bind() {
            if (IsDisposed)
                throw new ObjectDisposedException(nameof(VertexArray));

            GL.BindVertexArray(Id);
        }

        public void Unbind() {
            GL.BindVertexArray(0);
        }

        public void Dispose() {
            if (IsDisposed)
                return;

            Unbind();
            GL.DeleteVertexArray(Id);
            IsDisposed = true;
        }

    }

}