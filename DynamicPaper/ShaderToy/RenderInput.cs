namespace Maxstupo.DynamicPaper.ShaderToy {

    using System;

    public enum InputType {
        Texture,
        Buffer
    }

    public sealed class RenderInput {

        public InputType Type { get; }

        public string Filepath { get; }
        public byte[] Data { get; }

        public int Channel { get; }

        public int BufferId { get; }

        public RenderInput(int channel, string filepath) {
            this.Channel = channel;
            this.Type = InputType.Texture;
            this.Filepath = filepath ?? throw new ArgumentNullException(nameof(filepath));
            this.Data = null;
        }

        public RenderInput(int channel, string filepath, byte[] textureData) {
            this.Channel = channel;
            this.Type = InputType.Texture;
            this.Filepath = filepath;
            this.Data = textureData ?? throw new ArgumentNullException(nameof(textureData));
        }

        public RenderInput(int channel, int bufferId) {
            this.Channel = channel;
            this.BufferId = bufferId;
            this.Type = InputType.Buffer;
            this.Filepath = null;
            this.Data = null;
        }

    }

}