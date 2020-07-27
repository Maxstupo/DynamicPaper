namespace Maxstupo.DynWallpaper.Graphics {

    using System;

    public interface IBindable : IDisposable {

        int Id { get; }

        void Bind();

        void Unbind();

    }

}