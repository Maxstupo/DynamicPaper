namespace Maxstupo.DynWallpaper.Graphics {

    using System;
    using OpenTK.Graphics.OpenGL4;

    public sealed class FrameBuffer : IBindable {

        public int Id { get; }

        public bool IsDisposed { get; private set; }

        private readonly int rbo;

        public Texture Texture { get; }

        public FrameBuffer(int width, int height) {
            Id = GL.GenFramebuffer();
            Texture = new Texture(width, height);

            Bind();
            Texture.Bind();

            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2D, Texture.Id, 0);

            rbo = GL.GenRenderbuffer();
            GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, rbo);
            GL.RenderbufferStorage(RenderbufferTarget.Renderbuffer, RenderbufferStorage.Depth24Stencil8, width, height);
            GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, 0);

            GL.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthStencilAttachment, RenderbufferTarget.Renderbuffer, rbo);

            if (GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer) != FramebufferErrorCode.FramebufferComplete)
                throw new Exception();

            Unbind();
            Texture.Unbind();
        }

        public void Bind() {
            if (IsDisposed)
                throw new ObjectDisposedException(nameof(FrameBuffer));
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, Id);
        }

        public void Unbind() {
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        }

        public void Dispose() {
            if (IsDisposed)
                return;
            Unbind();

            Texture.Dispose();
            GL.DeleteRenderbuffer(rbo);
            GL.DeleteFramebuffer(Id);
            IsDisposed = true;
        }

    }

}