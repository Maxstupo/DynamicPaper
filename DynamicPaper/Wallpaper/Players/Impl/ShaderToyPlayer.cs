namespace Maxstupo.DynamicPaper.Wallpaper.Players.Impl {

    using System.Drawing;
    using OpenTK.Graphics.OpenGL4;

    public sealed class ShaderToyPlayer : OpenGLPlayer {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        protected override void Init() {
            GL.ClearColor(Color.Red);

        }
        float pol = 1;
        float r = 0;
        protected override void Update(float deltaTime) {
            r += deltaTime * 0.25f * pol;
            if (r >= 1) {
                pol *= -1;
                r = 1;
            }else if (r <= 0) {
                pol *= -1;
                r = 0;
            }

            //Console.WriteLine(deltaTime);
        }


        protected override void Render() {

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);


            GL.ClearColor(r, 0, 0, 1);

            //   System.Console.WriteLine(Time);

        }

        protected override void OnResized(int width, int height, float ratio) {
            GL.Viewport(0, 0, width, height);
        }

    }

}