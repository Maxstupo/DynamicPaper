namespace Maxstupo.DynWallpaper.Graphics.ShaderToy {

    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using Maxstupo.DynWallpaper.Graphics.Data;
    using OpenTK;
    using OpenTK.Graphics.OpenGL4;

    public sealed class ShaderToyRenderer : IRenderer, IResourceProvider {

        private readonly float[] vertices = {
             // Position    , Texture coordinates
            -1f,  1.0f, 0.0f, 0.0f, 1.0f, // top left
             1f,  1.0f, 0.0f, 1.0f, 1.0f, // top right
             1f, -1.0f, 0.0f, 1.0f, 0.0f, // bottom right
            -1f, -1.0f, 0.0f, 0.0f, 0.0f, // bottom left
        };

        private readonly uint[] indices = {
            0, 1, 2,
            0, 2, 3
        };

        private readonly ShaderToyInput input = new ShaderToyInput();

        private VertexBuffer vbo;
        private VertexArray vao;
        private ElementBuffer ebo;


        private DateTime startTime;

        private readonly Dictionary<Pass, RenderPass> renderPasses = new Dictionary<Pass, RenderPass>();
        private readonly Dictionary<Pass, FrameBuffer> buffers = new Dictionary<Pass, FrameBuffer>();

        private readonly Dictionary<string, Texture> textures = new Dictionary<string, Texture>();

        private RenderPass imagePass;

        public string SharedFragmentCode { get; set; }


        public ShaderToyRenderer() {
        }


        public void Init() {
            GL.ClearColor(Color.DarkGray);

            VertexBufferLayout vbl = new VertexBufferLayout {
                { VertexAttribPointerType.Float, 3 },
                { VertexAttribPointerType.Float, 2 }
            };

            vbo = new VertexBuffer(vertices, BufferUsageHint.StaticDraw, vbl);
            ebo = new ElementBuffer(indices, BufferUsageHint.StaticDraw);

            vao = new VertexArray(vbo, ebo);

            startTime = DateTime.Now;

            foreach (KeyValuePair<Pass, RenderPass> pair in renderPasses) {
                Pass pass = pair.Key;
                RenderPass renderPass = pair.Value;

                renderPass.Init(this, SharedFragmentCode);
                buffers.Add(pass, new FrameBuffer(800, 450));
            }

            imagePass.Init(this, SharedFragmentCode);

        }

        public void Add(Pass pass, RenderPass renderPass) {
            if (pass == Pass.Image) {
                if (imagePass != null)
                    throw new ArgumentException("Only one Image pass is allowed.");
                imagePass = renderPass;
            } else {
                renderPasses.Add(pass, renderPass);
            }
        }

        public FrameBuffer GetBuffer(Pass pass) {
            return buffers.TryGetValue(pass, out FrameBuffer buffer) ? buffer : null;
        }

        public Texture LoadTexture(string filepath) {
            if (textures.TryGetValue(filepath, out Texture texture))
                return texture;
            texture = new Texture(filepath);
            textures.Add(filepath, texture);
            return texture;
        }

        public void Render(float deltaTime) {
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);


            UpdateUniformValues(deltaTime);

            foreach (KeyValuePair<Pass, RenderPass> pair in renderPasses) {

                buffers[pair.Key].Bind();

                pair.Value.Render(vao, indices.Length, input, this);

                buffers[pair.Key].Unbind();

            }

            imagePass.Render(vao, indices.Length, input, this);

        }


        public void OnResized(int width, int height, float ratio) {
            GL.Viewport(0, 0, width, height);
            input.iResolution = new Vector3(width, height, 0);

            // Recreate the frame buffers for the new window size.
            foreach (KeyValuePair<Pass, FrameBuffer> pair in new Dictionary<Pass, FrameBuffer>(buffers)) {
                pair.Value.Dispose();

                buffers[pair.Key] = new FrameBuffer(width, height);
            }
        }

        private void UpdateUniformValues(float deltaTime) {
            input.iTime = (float) (DateTime.Now - startTime).TotalSeconds;
            input.iGlobalTime = input.iTime;

            DateTime time = DateTime.Now;
            input.iDate = new Vector4(time.Year, time.Month, time.Day, (float) time.TimeOfDay.TotalSeconds);

            input.iFrame++;
        }




        public void Dispose() {

            foreach (RenderPass renderPass in renderPasses.Values)
                renderPass.Dispose();
            imagePass.Dispose();

            foreach (FrameBuffer fb in buffers.Values)
                fb.Dispose();

            vao.Dispose();
            vbo.Dispose();
            ebo.Dispose();

            foreach (Texture texture in textures.Values)
                texture.Dispose();
        }

    }

}