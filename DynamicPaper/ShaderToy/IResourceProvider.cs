namespace Maxstupo.DynamicPaper.ShaderToy {

    using System;
    using Maxstupo.DynamicPaper.Graphics;

    public interface IResourceProvider : IDisposable {

        FrameBuffer GetBuffer(Pass pass);

        Texture LoadTexture(string value);

    }

}