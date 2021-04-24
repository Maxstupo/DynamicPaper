namespace Maxstupo.DynamicPaper.ShaderToy {

    using System;
    using Maxstupo.DynamicPaper.Graphics;

    public interface IResourceProvider : IDisposable {

        FrameBuffer GetFrameBuffer(int bufferId);

        Texture LoadTexture(string value);
        Texture LoadTexture(string name, byte[] data);

    }

}