namespace Maxstupo.DynamicPaper.ShaderToy {

    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using Maxstupo.DynamicPaper.Graphics;
    using Maxstupo.DynamicPaper.Graphics.Data;
    using OpenTK.Graphics.OpenGL4;

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

        public string VertexShader { get; set; } =
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

        private readonly string fragmentCode;
        private readonly RenderInput[] inputs;

        public RenderPass(string fragmentCode, params RenderInput[] inputs) {
            this.fragmentCode = fragmentCode;
            this.inputs = inputs;
        }

        public static RenderPass FromFile(string filepath, params RenderInput[] inputs) {
            return new RenderPass(File.ReadAllText(filepath), inputs);
        }

        public void Init(IResourceProvider provider, string commonFragmentCode = null) {
            Logger.Debug("Initializing...");

            StringBuilder sb = new StringBuilder();

            sb.Append(FragmentShaderHeader);

            if (!string.IsNullOrEmpty(commonFragmentCode))
                sb.Append(commonFragmentCode);

            foreach (RenderInput renderInput in inputs) {
                int channel = renderInput.Channel;

                sb.Append($"uniform sampler2D iChannel{channel};\n");

                if (renderInput.Type == InputType.Texture) {
                    Texture texture = provider.LoadTexture(renderInput.Value);
                    textures.Add(channel, texture);
                }
            }

            sb.Append(fragmentCode);
            sb.Append(FragmentShaderFooter);

            shader = new Shader(string.Empty, VertexShader, sb.ToString());
        }

        public void Render(VertexArray vao, int indiceCount, ref RenderData data, IResourceProvider provider) {
            if (shader == null) return;

            foreach (KeyValuePair<int, Texture> pair in textures) // Bind textures 
                pair.Value.Bind(pair.Key);

            foreach (RenderInput renderInput in inputs) { // Bind the framebuffer textures.
                if (renderInput.Type != InputType.Buffer)
                    continue;

                FrameBuffer fb = provider.GetBuffer(renderInput.Pass);
                if (fb != null)
                    fb.Texture.Bind(renderInput.Channel);
            }


            shader.Bind();
            {
                SetUniformValues(ref data);

                vao.Bind();
                {
                    GL.DrawElements(PrimitiveType.Triangles, indiceCount, DrawElementsType.UnsignedInt, 0);
                }
                vao.Unbind();

            }
            shader.Unbind();


            foreach (Texture texture in textures.Values)
                texture.Unbind();

        }

        public void SetUniformValues(ref RenderData input) {
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