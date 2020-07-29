namespace Maxstupo.DynWallpaper.Graphics.ShaderToy {

    using System;

    public interface IResourceProvider : IDisposable {
        FrameBuffer GetBuffer(Pass pass);
        Texture LoadTexture(string value);
    }

}