namespace Maxstupo.DynamicPaper.ShaderToy {

    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using Maxstupo.DynamicPaper.Graphics;
    using Maxstupo.DynamicPaper.Graphics.Data;
    using OpenTK.Graphics.OpenGL4;

    public class RenderPassBuilder {
        private readonly string name;
        private readonly string vertexShader;
        private readonly string fragmentShader;

        private string fragmentShaderHeader;
        private string fragmentShaderFooter;


        private readonly List<RenderInput> inputs = new List<RenderInput>();
        private readonly ISet<RenderOutput> outputs = new HashSet<RenderOutput>();

        private RenderPassBuilder(string name, string vertexCode, string fragmentCode) {
            this.name = name;
            this.vertexShader = vertexCode;
            this.fragmentShader = fragmentCode;
        }

        public static RenderPassBuilder FromFile(string name, string vertexCode, string filepath) {
            return new RenderPassBuilder(name, vertexCode, File.ReadAllText(filepath));
        }

        public static RenderPassBuilder FromString(string name, string vertexCode, string source) {
            return new RenderPassBuilder(name, vertexCode, source);
        }

        public RenderPassBuilder AddTextureInput(int channel, string filepath) {
            inputs.Add(new RenderInput(channel, filepath));
            return this;
        }

        public RenderPassBuilder AddTextureInput(int channel, string name, byte[] textureData) {
            inputs.Add(new RenderInput(channel, name, textureData));
            return this;
        }

        public RenderPassBuilder AddBufferInput(int channel, int bufferId) {
            inputs.Add(new RenderInput(channel, bufferId));
            return this;
        }

        public RenderPassBuilder AddBufferInput(int channel, RenderTarget renderTarget) {
            return AddBufferInput(channel, (int) renderTarget);
        }

        public RenderPassBuilder AddOutput(int bufferId) {
            outputs.Add(new RenderOutput(bufferId));
            return this;
        }

        public RenderPassBuilder AddOutput(RenderTarget renderTarget) {
            return AddOutput((int) renderTarget);
        }

        public RenderPass Create() {
            if (outputs.Count == 0)
                throw new InvalidOperationException("Render pass must have at least one output!");
            return new RenderPass(name, vertexShader, fragmentShader, inputs.ToArray(), outputs.ToArray());
        }

    }


    public sealed class RenderPass : IDisposable {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        public string FragmentShaderHeader { get; set; } =
            "#version 330 core\n" +
            "precision mediump float;\n" +
            "precision mediump int;\n" +
            "uniform vec3       iResolution;\n" +
            "uniform float      iGlobalTime;\n" +
            "uniform float      iTime;\n" +
            "uniform float      iChannelTime[4];\n" +
            "uniform vec4       iMouse;\n" +
            "uniform vec4       iDate;\n" +
            "uniform float      iSampleRate;\n" +
            "uniform vec3       iChannelResolution[4];\n" +
            "uniform int        iFrame;\n" +
            "uniform float      iTimeDelta;\n" +
            "uniform float      iFrameRate;\n" +
            "\n";

        public string FragmentShaderFooter { get; set; } =
            "\nvoid main() {\n" +
            "   mainImage(gl_FragColor.xyzw, gl_FragCoord.xy);\n" +
            "}";

        public static readonly string DefaultVertexShader =
            "#version 330 core\n" +
            "precision mediump float;\n" +
            "precision mediump int;\n" +
            "\n" +
            "layout (location = 0) in vec3 iPosition;\n" +
            "layout (location = 1) in vec2 iUV;\n" +
            "\n" +
            "void main() {\n" +
            "   gl_Position = vec4(iPosition, 1.0f);\n" +
            "}";


        private Shader shader;
        private readonly Dictionary<int, Texture> textures = new Dictionary<int, Texture>();

        private readonly string vertexCode;
        private readonly string fragmentCode;

        public string Name { get; }

        public RenderInput[] Inputs { get; }
        public RenderOutput[] Outputs { get; }

        public RenderPass(string name, string vertexCode, string fragmentCode, RenderInput[] inputs, RenderOutput[] outputs) {
            this.Name = name;
            this.vertexCode = vertexCode;
            this.fragmentCode = fragmentCode;
            this.Inputs = inputs;
            this.Outputs = outputs;
        }

        public void Init(IResourceProvider provider, string commonFragmentCode = null) {
            Logger.Debug("Initializing: {0}", Name);

            StringBuilder sb = new StringBuilder();

            sb.Append(FragmentShaderHeader);

            if (!string.IsNullOrEmpty(commonFragmentCode))
                sb.Append('\n').Append(commonFragmentCode).Append('\n');


            foreach (RenderInput renderInput in Inputs) {
                int channel = renderInput.Channel;

                sb.Append($"uniform sampler2D iChannel{channel};\n");

                if (renderInput.Type == InputType.Texture) {
                    Texture texture = renderInput.Data != null ? provider.LoadTexture(renderInput.Filepath, renderInput.Data) : provider.LoadTexture(renderInput.Filepath);
                    textures.Add(channel, texture);
                }
            }

            sb.Append(fragmentCode);
            sb.Append(FragmentShaderFooter);

            shader = new Shader(string.Empty, vertexCode, sb.ToString());
        }

        public void Render(VertexArray vao, int indiceCount, RenderData data, IResourceProvider provider) {

            foreach (KeyValuePair<int, Texture> pair in textures) // Bind textures 
                pair.Value.Bind(pair.Key);


            BindFrameBufferTextures(provider, true);
            {
                shader.Bind();
                {
                    SetUniformValues(data);

                    vao.Bind();
                    {
                        GL.DrawElements(PrimitiveType.Triangles, indiceCount, DrawElementsType.UnsignedInt, 0);
                    }
                    vao.Unbind();

                }
                shader.Unbind();
            }
            BindFrameBufferTextures(provider, false);


            foreach (Texture texture in textures.Values)
                texture.Unbind();

        }

        private void BindFrameBufferTextures(IResourceProvider provider, bool bind) {
            foreach (RenderInput renderInput in Inputs) {
                if (renderInput.Type != InputType.Buffer)
                    continue;

                FrameBuffer frameBuffer = provider.GetFrameBuffer(renderInput.BufferId);
                if (frameBuffer == null)
                    continue;

                if (bind) {
                    frameBuffer.Texture.Bind(renderInput.Channel);
                } else {
                    frameBuffer.Texture.Unbind();
                }

            }

        }

        public void SetUniformValues(RenderData input) {
            shader.SetUniform("iResolution", input.iResolution);
            shader.SetUniform("iTime", input.iTime);
            shader.SetUniform("iGlobalTime", input.iGlobalTime);
            shader.SetUniform("iMouse", input.iMouse);
            shader.SetUniform("iDate", input.iDate);
            shader.SetUniform("iSampleRate", input.iSampleRate);
            shader.SetUniform("iChannelResolution", input.iChannelResolution);
            shader.SetUniform("iChannelTime", input.iChannelTime);
            shader.SetUniform("iTimeDelta", input.iTimeDelta);
            shader.SetUniform("iFrame", input.iFrame);
            shader.SetUniform("iFrameRate", input.iFrameRate);

            shader.SetUniform("iChannel0", 0);
            shader.SetUniform("iChannel1", 1);
            shader.SetUniform("iChannel2", 2);
            shader.SetUniform("iChannel3", 3);
        }

        public void Dispose() {
            Logger.Trace("Disposing...");
            shader.Dispose();
        }

    }

}