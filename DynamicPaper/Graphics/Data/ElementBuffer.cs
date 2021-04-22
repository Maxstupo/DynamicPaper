namespace Maxstupo.DynamicPaper.Graphics.Data {
    using System;
    using OpenTK.Graphics.OpenGL4;

    public sealed class ElementBuffer : IBindable {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        public int Id { get; }

        public bool IsDisposed { get; private set; }

        public ElementBuffer(uint[] data, BufferUsageHint usage) {
            Id = GL.GenBuffer();
            Logger.Trace("Generated buffer: {0}", Id);

            Bind();
            GL.BufferData(BufferTarget.ElementArrayBuffer, data.Length * sizeof(uint), data, usage);
            Unbind();
        }

        public void Bind() {
            if (IsDisposed)
                throw new ObjectDisposedException(nameof(ElementBuffer));

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, Id);
        }

        public void Unbind() {
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
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