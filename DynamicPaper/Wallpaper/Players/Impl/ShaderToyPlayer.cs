namespace Maxstupo.DynamicPaper.Wallpaper.Players.Impl {
    using System;
    using System.Drawing;
    using System.IO;
    using Maxstupo.DynamicPaper.Graphics;
    using Maxstupo.DynamicPaper.Graphics.Data;
    using Maxstupo.DynamicPaper.ShaderToy;
    using OpenTK.Graphics.OpenGL4;

    public sealed class ShaderToyPlayer : OpenGLPlayer {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        private Renderer renderer;
        private RenderData data = new RenderData();


        protected override void Init() {
            GL.ClearColor(Color.Black);

            renderer = new Renderer {
                // SharedFragmentCode = File.ReadAllText("Res/Shaders/tree/common.fs")
            };
            //renderer.Add(Pass.Image, RenderPass.FromFile("Resources/Shaders/supernova_remnant.shadertoy.fs",
            //    new RenderInput(0, "Resources/Textures/rgba_256_noise.png"),
            //    new RenderInput(1, "Resources/Textures/black.png"),
            //    new RenderInput(2, "Resources/Textures/bw_64_noise.png")
            //)); 
        
            renderer.Add(Pass.Image, RenderPass.FromFile("Resources/Shaders/fire.fs"                
            ));
         
            //renderer.Add(Pass.Image, RenderPass.FromFile("Resources/Shaders/fractal.fs" ));

            renderer.Init();
        }


        protected override void Update(float deltaTime) {

            data.iFrame++;
            data.iTime = Time / 1000f;
            data.iGlobalTime = data.iTime;

            DateTime time = DateTime.Now;
            data.iDate = new OpenTK.Vector4(time.Year, time.Month, time.Day, (float) time.TimeOfDay.TotalSeconds);

        }


        protected override void Render() {
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            renderer?.Render(1, ref data);
        }

        protected override void OnResized(int width, int height, float ratio) {

            GL.Viewport(0, 0, width, height);
            renderer.RecreateBuffers(width, height);

            data.iResolution = new OpenTK.Vector3(width, height, 0);

        }

        protected override void Dispose(bool disposing) {
            if (disposing) {
                renderer.Dispose();
                renderer = null;
            }
            base.Dispose(disposing);
        }

    }

}