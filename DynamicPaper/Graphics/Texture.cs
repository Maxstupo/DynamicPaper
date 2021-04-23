namespace Maxstupo.DynamicPaper.Graphics {

    using System;
    using System.Drawing;
    using System.Drawing.Imaging;
    using System.IO;
    using Maxstupo.DynamicPaper.Utility;
    using OpenTK.Graphics.OpenGL4;

    public sealed class Texture : IBindable {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        public int Id { get; }

        private TextureWrapMode textureWrapS;
        public TextureWrapMode WrapS {
            get => textureWrapS;
            set {
                Bind();
                textureWrapS = value;
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int) value);
                Unbind();
            }
        }

        private TextureWrapMode textureWrapT;
        public TextureWrapMode WrapT {
            get => textureWrapT;
            set {
                Bind();
                textureWrapT = value;
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int) value);
                Unbind();
            }
        }

        private TextureMagFilter textureMagFilter;
        public TextureMagFilter MagFilter {
            get => textureMagFilter;
            set {
                Bind();
                textureMagFilter = value;
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int) value);
                Unbind();
            }
        }

        private TextureMinFilter textureMinFilter;
        public TextureMinFilter MinFilter {
            get => textureMinFilter;
            set {
                Bind();
                textureMinFilter = value;
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int) value);
                Unbind();
            }
        }


        public bool IsDisposed { get; private set; }

        public Texture(byte[] fileData) {
            Id = GL.GenTexture();
            Logger.Trace("Created texture: {0}", Id);

            Bind();

            Logger.Trace("Load image data...");
            using (MemoryStream ms = new MemoryStream(fileData)) {
                using (Bitmap image = new Bitmap(ms)) {
                    BitmapData data = image.LockBits(new Rectangle(0, 0, image.Width, image.Height), ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

                    GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, image.Width, image.Height, 0, OpenTK.Graphics.OpenGL4.PixelFormat.Bgra, PixelType.UnsignedByte, data.Scan0);
                }
            }

            MinFilter = TextureMinFilter.Linear;
            MagFilter = TextureMagFilter.Linear;

            WrapS = TextureWrapMode.Repeat;
            WrapT = TextureWrapMode.Repeat;

            GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);

            Unbind();
        }

        public Texture(int width, int height) {
            Id = GL.GenTexture();
            Logger.Trace("Created texture: {0}", Id);

            Bind();

            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgb, width, height, 0, OpenTK.Graphics.OpenGL4.PixelFormat.Rgb, PixelType.UnsignedByte, IntPtr.Zero);

            MinFilter = TextureMinFilter.Linear;
            MagFilter = TextureMagFilter.Linear;

            WrapS = TextureWrapMode.Repeat;
            WrapT = TextureWrapMode.Repeat;

            Unbind();
        }

        public static Texture FromFile(string filepath) {
            Logger.Trace("Loading texture from {0}", filepath);
            using (Stream stream = File.OpenRead(filepath))
                return new Texture(stream.ReadAllBytes());
        }

        public void Bind() {
            Bind(TextureUnit.Texture0);
        }

        public void Bind(int unit) {
            Bind(TextureUnit.Texture0 + unit);
        }

        public void Bind(TextureUnit unit = TextureUnit.Texture0) {
            if (IsDisposed)
                throw new ObjectDisposedException(nameof(Texture));

            GL.ActiveTexture(unit);
            GL.BindTexture(TextureTarget.Texture2D, Id);
        }


        public void Unbind() {
            GL.BindTexture(TextureTarget.Texture2D, 0);
        }

        public void Dispose() {
            if (IsDisposed)
                return;
            Logger.Trace("Disposing...");

            Unbind();
            GL.DeleteTexture(Id);

            IsDisposed = true;
        }

    }

}