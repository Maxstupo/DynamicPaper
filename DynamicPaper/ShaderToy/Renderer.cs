namespace Maxstupo.DynamicPaper.ShaderToy {

    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using Maxstupo.DynamicPaper.Graphics;
    using Maxstupo.DynamicPaper.Graphics.Data;
    using Maxstupo.DynamicPaper.Utility;
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

        private readonly Dictionary<int, RenderPass> renderPasses = new Dictionary<int, RenderPass>();
        private readonly Dictionary<int, FrameBuffer> frameBuffers = new Dictionary<int, FrameBuffer>();

        private readonly Dictionary<string, Texture> textures = new Dictionary<string, Texture>();

        public string SharedFragmentCode { get; set; }

        private int displayOutputBufferId;
        public int DisplayOutputBufferId {
            get => displayOutputBufferId;
            set {
                displayOutputBufferId = value;
                displayOutputRenderPassId = renderPasses.Where(x => x.Value.Outputs.Any(v => v.BufferId == value)).Select(x => x.Key).FirstOrDefault();
            }
        }

        private int displayOutputRenderPassId;

        public Renderer(int outputBufferId) {
            this.DisplayOutputBufferId = outputBufferId;
        }

        public Renderer(RenderTarget displayRenderTarget) : this((int) displayRenderTarget) { }

        public void Init() {
            Logger.Debug("Initializing...");

            VertexBufferLayout vbl = new VertexBufferLayout {
                { VertexAttribPointerType.Float, 3 },
                { VertexAttribPointerType.Float, 2 }
            };

            vbo = new VertexBuffer(vertices, BufferUsageHint.StaticDraw, vbl);
            ebo = new ElementBuffer(indices, BufferUsageHint.StaticDraw);

            vao = new VertexArray(vbo, ebo);

            InitPasses();
        }

        public void InitPasses() {
            Logger.Debug("Initializing passes...");

            foreach (RenderPass renderPass in renderPasses.Values) {

                renderPass.Init(this, SharedFragmentCode);

                foreach (RenderOutput output in renderPass.Outputs) {
                    if (output.BufferId == DisplayOutputBufferId) // We dont need a frame buffer for the actual "output"
                        continue;

                    if (!frameBuffers.ContainsKey(output.BufferId))
                        frameBuffers.Add(output.BufferId, new FrameBuffer(800, 450));
                }
            }
        }

        public void Clear() {
            DisposePasses();

        }

        public void Add(RenderPass renderPass) {
            renderPasses.Add(renderPass.Name.GetHashCode(), renderPass);
        }

        public FrameBuffer GetFrameBuffer(int bufferId) {
            return frameBuffers.TryGetValue(bufferId, out FrameBuffer buffer) ? buffer : null;
        }

        public Texture LoadTexture(string filepath) {
            if (textures.TryGetValue(filepath, out Texture texture))
                return texture;

            using (Stream stream = File.OpenRead(filepath))
                return LoadTexture(filepath, stream.ReadAllBytes());
        }

        public Texture LoadTexture(string name, byte[] data) {
            if (textures.TryGetValue(name, out Texture texture))
                return texture;

            Logger.Debug("Loading texture: {0}", name);

            texture = new Texture(data);
            textures.Add(name, texture);
            return texture;
        }

        public void Render(RenderData data) {
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            foreach (KeyValuePair<int, RenderPass> rp in renderPasses) {
                RenderPass renderPass = rp.Value;

                bool isOutputPass = rp.Key == displayOutputRenderPassId;


                if (!isOutputPass) {
                    foreach (RenderOutput output in renderPass.Outputs)
                        frameBuffers[output.BufferId].Bind();
                }

                renderPass.Render(vao, indices.Length, data, this);

                if (!isOutputPass) {
                    foreach (RenderOutput output in renderPass.Outputs)
                        frameBuffers[output.BufferId].Unbind();
                }

            }

        }

        public void RecreateBuffers(int newWidth, int newHeight) {
            Logger.Debug("Recreate buffers: {0}x{1}", newWidth, newHeight);

            foreach (KeyValuePair<int, FrameBuffer> pair in new Dictionary<int, FrameBuffer>(frameBuffers)) {
                pair.Value.Dispose();
                frameBuffers[pair.Key] = new FrameBuffer(newWidth, newHeight);
            }
        }


        public void DisposePasses() {
            Logger.Debug("Disposing passes...");

            foreach (RenderPass renderPass in renderPasses.Values)
                renderPass.Dispose();

            foreach (FrameBuffer frameBuffer in frameBuffers.Values)
                frameBuffer.Dispose();

            foreach (Texture texture in textures.Values)
                texture.Dispose();

            renderPasses.Clear();
            frameBuffers.Clear();
            textures.Clear();
        }

        public void Dispose() {
            Logger.Debug("Disposing...");

            DisposePasses();

            vao.Dispose();
            vbo.Dispose();
            ebo.Dispose();
        }


    }

}