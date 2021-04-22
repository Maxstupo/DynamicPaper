namespace Maxstupo.DynamicPaper.ShaderToy {
    
    using System;

    public enum InputType {
        Texture,
        Buffer
    }

    public sealed class RenderInput {

        public InputType Type { get; }

        public string Value { get; }

        public int Channel { get; }

        public Pass Pass { get; }

        public RenderInput(int channel, string value) {
            this.Channel = channel;
            this.Type = InputType.Texture;
            this.Value = value ?? throw new ArgumentNullException(nameof(value));
        }

        public RenderInput(int channel, Pass pass) {
            this.Channel = channel;
            this.Pass = pass;
            this.Type = InputType.Buffer;
        }

    }

}