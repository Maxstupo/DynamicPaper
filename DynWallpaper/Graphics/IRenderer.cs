namespace Maxstupo.DynWallpaper.Graphics {

    public interface IRenderer {

        void Init();

        void Render(float deltaTime);

        void OnResized(int width, int height, float ratio);

        void Dispose();

    }

}