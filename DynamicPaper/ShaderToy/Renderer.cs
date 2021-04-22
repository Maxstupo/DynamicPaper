namespace Maxstupo.DynamicPaper.ShaderToy {

    using System;
    using System.Collections.Generic;
    using Maxstupo.DynamicPaper.Graphics;
    using Maxstupo.DynamicPaper.Graphics.Data;
    using OpenTK.Graphics.OpenGL4;

    public sealed class Renderer : IResourceProvider {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        private readonly float[] vertices = {
             // Position    , Texture coordinates
            -1f,  1.0f, 0.0f, 0.0f, 1.0f, // top left
             1f,  1.0f, 0.0f, 1.0f, 1.0f, // top right
             1f, -1.0f, 0.0f, 1.0f, 0.0f, // bottom right
            -1f, -1.0f, 0.0f, 0.0f, 0.0f, // bottom left
        };


        private readonly uint[] indices = {
            0, 1, 2, // 1
            0, 2, 3  // 2
        };

        private VertexBuffer vbo;
        private VertexArray vao;
        private ElementBuffer ebo;

        private readonly Dictionary<Pass, RenderPass> renderPasses = new Dictionary<Pass, RenderPass>();
        private readonly Dictionary<Pass, FrameBuffer> buffers = new Dictionary<Pass, FrameBuffer>();

        private readonly Dictionary<string, Texture> textures = new Dictionary<string, Texture>();

        private RenderPass imagePass;

        public string SharedFragmentCode { get; set; }

        public Renderer() {

        }

        public void Init() {
            Logger.Debug("Initializing...");

            VertexBufferLayout vbl = new VertexBufferLayout {
                { VertexAttribPointerType.Float, 3 },
                { VertexAttribPointerType.Float, 2 }
            };

            vbo = new VertexBuffer(vertices, BufferUsageHint.StaticDraw, vbl);
            ebo = new ElementBuffer(indices, BufferUsageHint.StaticDraw);

            vao = new VertexArray(vbo, ebo);


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
                Logger.Debug("Setting image pass...");
            } else {
                Logger.Debug("Setting render pass {0}", pass);
                renderPasses.Add(pass, renderPass);
            }
        }

        public FrameBuffer GetBuffer(Pass pass) {
            return buffers.TryGetValue(pass, out FrameBuffer buffer) ? buffer : null;
        }

        public Texture LoadTexture(string filepath) {
            if (textures.TryGetValue(filepath, out Texture texture))
                return texture;
            Logger.Debug("Loading texture: {0}", filepath);

            texture = new Texture(filepath);
            textures.Add(filepath, texture);
            return texture;
        }


        public void Render(float deltaTime, ref RenderData data) {

            foreach (KeyValuePair<Pass, RenderPass> pair in renderPasses) {

                buffers[pair.Key].Bind();
                {
                    pair.Value.Render(vao, indices.Length, ref data, this);
                }
                buffers[pair.Key].Unbind();

            }

            imagePass.Render(vao, indices.Length, ref data, this);
        }

        public void RecreateBuffers(int newWidth, int newHeight) {
            Logger.Debug("Recreate buffers: {0}x{1}", newWidth, newHeight);

            foreach (KeyValuePair<Pass, FrameBuffer> pair in new Dictionary<Pass, FrameBuffer>(buffers)) {
                pair.Value.Dispose();

                buffers[pair.Key] = new FrameBuffer(newWidth, newHeight);
            }
        }




        public void Dispose() {
            Logger.Debug("Disposing...");

            foreach (RenderPass renderPass in renderPasses.Values)
                renderPass.Dispose();

            imagePass.Dispose();

            foreach (FrameBuffer frameBuffer in buffers.Values)
                frameBuffer.Dispose();

            vao.Dispose();
            vbo.Dispose();
            ebo.Dispose();

            foreach (Texture texture in textures.Values)
                texture.Dispose();

        }

    }

}