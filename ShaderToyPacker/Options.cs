namespace Maxstupo.ShaderToyPack {

    using CommandLine;

    public sealed class Options {

        [Value(0, HelpText = "The URL to a ShaderToy shader.", Required = true, MetaName = "url")]
        public string Url { get; set; }

        [Option("key", HelpText = "The ShaderToy API key to use when fetching the shader information.", Default = null)]
        public string ApiKey { get; set; }

        [Option('f', "filename", HelpText = "The filename override for the packed shadertoy output. (Defaults: ShaderToy Title)", Default = null)]
        public string Filename { get; set; }

        [Option('d', "dir", HelpText = "The output directory override for the packed shadertoy output. (Defaults: Current Directory)", Default = null)]
        public string Directory { get; set; }

        [Option('v', "vertex-shader", HelpText = "The filepath to a custom shared vertex shader that will be packed with the output.", Default = null)]
        public string VertexShaderFilepath { get; set; }

        [Option('y', "overwrite", HelpText = "Overwrite the output file if it exists.")]
        public bool OverwriteOutput { get; set; }

    }

}