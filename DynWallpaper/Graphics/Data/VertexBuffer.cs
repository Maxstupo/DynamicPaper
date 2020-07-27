namespace Maxstupo.DynWallpaper.Graphics.Data {

    using System;
    using OpenTK.Graphics.OpenGL4;

    public sealed class VertexBuffer : IBindable {

        public int Id { get; }

        public VertexBufferLayout Layout { get; }

        public bool IsDisposed { get; private set; }

        public VertexBuffer(uint[] data, BufferUsageHint usage, VertexBufferLayout layout) {
            Layout = layout ?? throw new ArgumentNullException(nameof(layout));

            Id = GL.GenBuffer();

            Bind();
            GL.BufferData(BufferTarget.ArrayBuffer, data.Length * sizeof(uint), data, usage);
            Unbind();
        }

        public VertexBuffer(float[] data, BufferUsageHint usage, VertexBufferLayout layout) {
            Layout = layout ?? throw new ArgumentNullException(nameof(layout));

            Id = GL.GenBuffer();

            Bind();
            GL.BufferData(BufferTarget.ArrayBuffer, data.Length * sizeof(float), data, usage);
            Unbind();
        }

        public void Bind() {
            if (IsDisposed)
                throw new ObjectDisposedException(nameof(VertexBuffer));

            GL.BindBuffer(BufferTarget.ArrayBuffer, Id);
        }

        public void Unbind() {
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
        }

        public void Dispose() {
            if (IsDisposed)
                return;

            Unbind();
            GL.DeleteBuffer(Id);
            IsDisposed = true;
        }

    }

}