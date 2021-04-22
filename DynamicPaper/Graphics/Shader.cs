namespace Maxstupo.DynamicPaper.Graphics {

    using System;
    using System.Collections.Generic;
    using OpenTK;
    using OpenTK.Graphics.OpenGL4;

    public sealed class Shader : IBindable {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        public string Name { get; }

        public int Id { get; }

        public bool IsDisposed { get; private set; }

        private readonly Dictionary<string, int> cachedUniformLocations = new Dictionary<string, int>();

        public Shader(string name, string vertexSource, string fragmentSource) {
            this.Name = name;

            /* Compile our shaders. */
            int vertexShaderId = CompileShader(ShaderType.VertexShader, vertexSource);
            int fragmentShaderId = CompileShader(ShaderType.FragmentShader, fragmentSource);

        

            /* Create our shader program & link our shaders. */
            Id = GL.CreateProgram();
            Logger.Trace("Created shader program: {0}", Id);

            GL.AttachShader(Id, vertexShaderId);
            GL.AttachShader(Id, fragmentShaderId);
            GL.LinkProgram(Id);

            /* Check linking status. */
            GL.GetProgram(Id, GetProgramParameterName.LinkStatus, out int value);
            if (value == 0) {
                string infoLog = GL.GetProgramInfoLog(Id);
                Logger.Error("SHADER {0} - LINKING_FAILED\n{1}", name, infoLog);
            }

            /* Detach and delete our shaders, as we no longer need them. */
            GL.DetachShader(Id, vertexShaderId);
            GL.DetachShader(Id, fragmentShaderId);
            GL.DeleteShader(vertexShaderId);
            GL.DeleteShader(fragmentShaderId);
        }

        private int CompileShader(ShaderType type, string source) {
            Logger.Trace("Compiling {0} shader...", type);

            int id = GL.CreateShader(type);

            /* Upload and compile our shader. */
            GL.ShaderSource(id, source);
            GL.CompileShader(id);

            /* Check compilation status. */
            GL.GetShader(id, ShaderParameter.CompileStatus, out int value);
            if (value == 0) {
                string infoLog = GL.GetShaderInfoLog(id);
                Logger.Error("{0} - COMPILATION_FAILED {1}\n{2}", type, Name, infoLog);
            }

            return id;
        }

        public void Bind() {
            if (IsDisposed)
                throw new ObjectDisposedException(nameof(Shader));

            GL.UseProgram(Id);
        }

        public void Unbind() {
            GL.UseProgram(0);
        }

        public void SetUniform(string name, int value) {
            Bind();
            int location = GetUniformLocation(name);
            GL.Uniform1(location, value);
        }

        public void SetUniform(string name, float value) {
            Bind();
            int location = GetUniformLocation(name);
            GL.Uniform1(location, value);
        }

        public void SetUniform(string name, bool value) {
            Bind();
            int location = GetUniformLocation(name);
            GL.Uniform1(location, value ? 1 : 0);
        }

        public void SetUniform(string name, Vector3 value) {
            Bind();
            int location = GetUniformLocation(name);
            GL.Uniform3(location, ref value);
        }

        public void SetUniform(string name, Vector4 value) {
            Bind();
            int location = GetUniformLocation(name);
            GL.Uniform4(location, ref value);
        }

        public void SetUniform(string name, float[] array) {
            Bind();
            int location = GetUniformLocation(name);
            GL.Uniform1(location, array.Length, array);
        }

        public void SetUniform(string name, Vector3[] array) {
            Bind();
            int location = GetUniformLocation(name);


            float[] arr = new float[array.Length * 3];
            for (int i = 0; i < array.Length; i += 3) {
                arr[i] = array[i].X;
                arr[i + 1] = array[i].Y;
                arr[i + 2] = array[i].Z;
            }

            GL.Uniform3(location, array.Length, arr);
        }

        public int GetUniformLocation(string uniformName) {
            if (cachedUniformLocations.TryGetValue(uniformName, out int location))  // Return cached location, if exists.
                return location;

            // Get location and update cache.
            location = GL.GetUniformLocation(Id, uniformName);

            if (location == -1)
                Logger.Warn("[Shader #{0} - {1}] Uniform '{2}' doesn't exist!", Id, Name, uniformName);
            else
                Logger.Trace("[Shader #{0} - {1}] Cached uniform location: '{2}' -> {3}", Id, Name, uniformName, location);

            cachedUniformLocations.Add(uniformName, location);
            return location;
        }

        public void Dispose() {
            if (IsDisposed)
                return;
            Logger.Trace("Disposing...");

            Unbind();
            GL.DeleteProgram(Id);

            IsDisposed = true;
        }

    }

}