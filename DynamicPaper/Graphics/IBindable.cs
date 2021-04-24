namespace Maxstupo.DynamicPaper.Graphics {

    using System;

    public interface IBindable : IDisposable {

        int Id { get; }

        void Bind();

        void Unbind();

    }

}