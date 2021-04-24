namespace Maxstupo.DynamicPaper.Graphics.Data {

    using System;
    using OpenTK.Graphics.OpenGL4;

    public sealed class VertexBuffer : IBindable {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        public int Id { get; }

        public VertexBufferLayout Layout { get; }

        public bool IsDisposed { get; private set; }

        public VertexBuffer(uint[] data, BufferUsageHint usage, VertexBufferLayout layout) {
            Layout = layout ?? throw new ArgumentNullException(nameof(layout));

            Id = GL.GenBuffer();
            Logger.Trace("Generated buffer: {0}", Id);

            Bind();
            GL.BufferData(BufferTarget.ArrayBuffer, data.Length * sizeof(uint), data, usage);
            Logger.Trace("Set buffer data: {1} bytes ({0})", usage, data.Length * sizeof(uint));
            Unbind();
        }

        public VertexBuffer(float[] data, BufferUsageHint usage, VertexBufferLayout layout) {
            Layout = layout ?? throw new ArgumentNullException(nameof(layout));

            Id = GL.GenBuffer();
            Logger.Trace("Generated buffer: {0}", Id);

            Bind();
            GL.BufferData(BufferTarget.ArrayBuffer, data.Length * sizeof(float), data, usage);
            Logger.Trace("Set buffer data: {1} bytes ({0})", usage, data.Length * sizeof(float));
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
            Logger.Trace("Disposing...");

            Unbind();
            GL.DeleteBuffer(Id);

            IsDisposed = true;
        }

    }


}