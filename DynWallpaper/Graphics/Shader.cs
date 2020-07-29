namespace Maxstupo.DynWallpaper.Graphics {

    using System;
    using System.Collections.Generic;
    using OpenTK;
    using OpenTK.Graphics.OpenGL4;

    public sealed class Shader : IBindable {

        public string Name { get; }

        public int Id { get; }

        public bool IsDisposed { get; private set; }

        private readonly Dictionary<string, int> cachedUniformLocations = new Dictionary<string, int>();

        private readonly string fragmentSource;

        public Shader(string name, string vertexSource, string fragmentSource) {
            this.Name = name;
            this.fragmentSource = fragmentSource;

            /* Compile our shaders. */
            int vertexShaderId = CompileShader(ShaderType.VertexShader, vertexSource);
            int fragmentShaderId = CompileShader(ShaderType.FragmentShader, fragmentSource);

            /* Create our shader program & link our shaders. */
            Id = GL.CreateProgram();
            GL.AttachShader(Id, vertexShaderId);
            GL.AttachShader(Id, fragmentShaderId);
            GL.LinkProgram(Id);

            /* Check linking status. */
            GL.GetProgram(Id, GetProgramParameterName.LinkStatus, out int value);
            if (value == 0) {
                string infoLog = GL.GetProgramInfoLog(Id);
                Console.WriteLine($"SHADER {name} - LINKING_FAILED\n{infoLog}");
            }

            /* Detach and delete our shaders, as we no longer need them. */
            GL.DetachShader(Id, vertexShaderId);
            GL.DetachShader(Id, fragmentShaderId);
            GL.DeleteShader(vertexShaderId);
            GL.DeleteShader(fragmentShaderId);
        }

        private int CompileShader(ShaderType type, string source) {

            int id = GL.CreateShader(type);

            /* Upload and compile our shader. */
            GL.ShaderSource(id, source);
            GL.CompileShader(id);

            /* Check compilation status. */
            GL.GetShader(id, ShaderParameter.CompileStatus, out int value);
            if (value == 0) {
                string infoLog = GL.GetShaderInfoLog(id);
                Console.WriteLine($"{type} - COMPILATION_FAILED {Name}\n{infoLog}");
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

        public int GetUniformLocation(string name) {
            if (cachedUniformLocations.TryGetValue(name, out int location))  // Return cached location, if exists.
                return location;

            // Get location and update cache.
            location = GL.GetUniformLocation(Id, name);

#if DEBUG
            if (location == -1)
                Console.WriteLine($"[Shader#{Id}] Uniform '{name}' doesn't exist!");
            else
                Console.WriteLine($"[Shader#{Id}] Cached uniform location: '{name}' -> {location}");
#endif

            cachedUniformLocations.Add(name, location);
            return location;
        }

        public void Dispose() {
            if (IsDisposed)
                return;

            Unbind();
            GL.DeleteProgram(Id);
            IsDisposed = true;
        }

    }

}