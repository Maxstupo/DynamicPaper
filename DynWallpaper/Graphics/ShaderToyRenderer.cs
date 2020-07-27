namespace Maxstupo.DynWallpaper.Graphics {
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.IO;
    using System.Text;
    using Maxstupo.DynWallpaper.Graphics.Data;
    using OpenTK;
    using OpenTK.Graphics.OpenGL4;

    public sealed class ShaderToyInput {
        public Vector3 iResolution;                             // viewport resolution (in pixels)
        public float iTime;                                     // shader playback time (in seconds)
        public float iGlobalTime;
        public Vector4 iMouse;                                  // mouse pixel coords. xy: current (if MLB down), zw: click
        public Vector4 iDate;                                   // (year, month, day, time in seconds)
        public float iSampleRate;                               // sound sample rate (i.e., 44100)
        public Vector3[] iChannelResolution = new Vector3[4];   // channel resolution (in pixels)
        public float[] iChannelTime = new float[4];             // channel playback time (in seconds)

        public int iFrame;       // shader playback frame
        public float iTimeDelta; // render time (in seconds)
        public float iFrameRate;

        public ShaderToyInput() {
            iResolution = new Vector3(1.0f, 1.0f, 1.0f);
            iTime = 0.0f;
            iGlobalTime = 0.0f;
            iMouse = new Vector4(0.0f, 0.0f, 0.0f, 0.0f);
            iDate = new Vector4(0.0f, 0.0f, 0.0f, 0.0f);
            iSampleRate = 44100 * 1.0f;


            iFrame = 0;
            iTimeDelta = 0.0f;
            iFrameRate = 0.0f;
        }

    }


    public sealed class ShaderToyRenderer : IRenderer {

        private const string FragmentShaderHeader =
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

        private const string FragmentShaderFooter =
            "\nvoid main() {\n" +
            "   mainImage(gl_FragColor.xyzw, gl_FragCoord.xy);\n" +
            "}";

        private const string VertexShader =
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

        private VertexBuffer vbo;
        private VertexArray vao;
        private ElementBuffer ebo;

        private Shader shader;

        private readonly List<Texture> textures = new List<Texture>();


        private readonly string fragmentShaderFilepath;
        private readonly string[] textureFilepaths;

        private readonly ShaderToyInput input = new ShaderToyInput();

        private DateTime startTime;

        public ShaderToyRenderer(string fragmentShaderFilepath, params string[] textureFilepaths) {
            this.fragmentShaderFilepath = fragmentShaderFilepath;
            this.textureFilepaths = textureFilepaths;
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

            LoadTextures();
            LoadShader();

            startTime = DateTime.Now;

        }

        private void LoadTextures() {
            for (int i = 0; i < textureFilepaths.Length; i++) {
                Texture texture = new Texture(textureFilepaths[i]);
                texture.Bind(i);
                textures.Add(texture);
            }
        }

        private void LoadShader() {
            StringBuilder sb = new StringBuilder();

            sb.Append(FragmentShaderHeader);

            for (int i = 0; i < textureFilepaths.Length; i++)
                sb.Append($"uniform sampler2D iChannel{i};\n");

            string fragShaderContents = File.ReadAllText(fragmentShaderFilepath);
            sb.Append(fragShaderContents);

            sb.Append(FragmentShaderFooter);

            shader = new Shader(VertexShader, sb.ToString());
        }

        public void Render(float deltaTime) {
            GL.Clear(ClearBufferMask.ColorBufferBit);

            for (int i = 0; i < textures.Count; i++)
                textures[i].Bind(i);

            shader.Bind();

     
            UpdateUniformValues(deltaTime);
            SetUniformValues();

            vao.Bind();

            GL.DrawElements(PrimitiveType.Triangles, indices.Length, DrawElementsType.UnsignedInt, 0);

            vao.Unbind();

            shader.Unbind();

            foreach (Texture tex in textures)
                tex.Unbind();

        }


        public void OnResized(int width, int height, float ratio) {
            GL.Viewport(0, 0, width, height);
            input.iResolution = new Vector3(width, height, 0);
        }

        private void UpdateUniformValues(float deltaTime) {
            input.iTime = (float) (DateTime.Now - startTime).TotalSeconds;
            input.iGlobalTime = input.iTime;

            DateTime time = DateTime.Now;
            input.iDate = new Vector4(time.Year, time.Month, time.Day, (float) time.TimeOfDay.TotalSeconds);

            input.iFrame++;
        }

        private void SetUniformValues() {
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

            shader.Dispose();

            vao.Dispose();
            vbo.Dispose();
            ebo.Dispose();

            foreach (Texture texture in textures)
                texture.Dispose();
        }

    }

}