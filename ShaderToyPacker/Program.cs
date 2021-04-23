namespace ShaderToyPack {

    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.IO.Compression;
    using System.Linq;
    using System.Text;
    using System.Text.RegularExpressions;
    using CommandLine;
    using Maxstupo.ShaderToyPack;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    public static class Program {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetLogger("Packer");

        // This key is the default shadertoy api key used when one isn't specified within the command-line arguments.
        private const string PublicApiKey = "BdrtMn";

        private const string ApiUrl = @"https://www.shadertoy.com/api/v1/shaders/{id}?key={apiKey}";
        private const string ApiAssetUrl = @"https://www.shadertoy.com{stub}";

        private static readonly Regex UrlRegex = new Regex(@"https?:\/\/www\.shadertoy\.com\/view\/(\w+)\/?", RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);


        public static readonly string DefaultVertexShader =
           "#version 330 core\n" +
           "precision mediump float;\n" +
           "precision mediump int;\n" +
           "\n" +
           "layout (location = 0) in vec3 iPosition;\n" +
           "layout (location = 1) in vec2 iUV;\n" +
           "\n" +
           "void main() {\n" +
           "   gl_Position = vec4(iPosition, 1.0f);\n" +
           "}";


        private static void PackUrl(string url, string apiKey, string outputFilename = null, string outputDirectory = null, string vertexShader = null, bool overwriteOutput = true) {
            Match match = UrlRegex.Match(url);
            if (match.Success) {
                string id = match.Groups[1].Value.Trim();
                Pack(id, apiKey, outputFilename, outputDirectory, vertexShader, overwriteOutput);
            } else {
                Logger.Error("Failed to extract ID from URL '{0}'", url);
            }
        }

        private static void Pack(string id, string apiKey, string outputFilename = null, string outputDirectory = null, string vertexShader = null, bool overrideOutput = true) {
            string apiUrl = ApiUrl.Replace("{id}", id).Replace("{apiKey}", apiKey);
            string json = Http.Get(apiUrl);

            JToken jRoot = JToken.Parse(json);
            JToken root = jRoot["Shader"];


            string baseDirectory = null;
            bool deleteOnDispose = true;
#if DEBUG
            baseDirectory = ".";
            deleteOnDispose = false;
#endif

            using (TempDirectory tempDirectory = Util.CreateTempDirectory(baseDirectory, deleteOnDispose)) {
                DirectoryInfo assetsFolder = Directory.CreateDirectory(Path.Combine(tempDirectory, "assets"));

#if DEBUG
                // Write full JSON response.
                Logger.Info("Writing full JSON response...");
                File.WriteAllText(Path.Combine(tempDirectory, "response.json"), jRoot.ToString(Formatting.Indented));
#endif

                // Write info file.
                Logger.Info("Writing info file...");
                File.WriteAllText(Path.Combine(tempDirectory, "info.json"), root["info"].ToString(Formatting.Indented));

                // Write default vertex shader.
                Logger.Info("Writing shared vertex shader...");
                File.WriteAllText(Path.Combine(tempDirectory, $"shared.vs.glsl"), vertexShader, Encoding.UTF8);

                // Write render passes.
                Logger.Info("Writing render passes...");
                int index = 0;
                foreach (JToken renderPass in root["renderpass"]) {
                    string name = renderPass["name"].Value<string>();
                    string description = renderPass["description"].Value<string>();
                    string type = renderPass["type"].Value<string>();

                    // Write GLSL shader file(s)...
                    string renderPassName = string.Empty;
                    if (type == "common") {
                        index--;
                        Logger.Debug("  - Writing common shader - #{0} {1} ({2})...", index, name, type);
                        renderPassName = "common";
                    } else {
                        Logger.Debug("  - Writing render pass shader - #{0} {1} ({2})...", index, name, type);
                        renderPassName = Util.MakeValidFileName($"rp{index}-{name.Replace(" ", "_")}.fs").ToLower();
                    }
                    string glslFilename = $"{renderPassName}.glsl";

                    string code = renderPass["code"].Value<string>();
                    File.WriteAllText(Path.Combine(tempDirectory, glslFilename), code, Encoding.UTF8);

                    if (type != "common") {
                        Logger.Debug("     - Processing inputs...");

                        List<JObject> inputs = new List<JObject>();
                        foreach (JToken input in renderPass["inputs"]) {
                            int inputId = input["id"].Value<int>();
                            string inputSrc = input["src"].Value<string>();
                            string inputType = input["ctype"].Value<string>();
                            int inputChannel = input["channel"].Value<int>();

                            if (inputType == "buffer") {
                                inputs.Add(new JObject {
                                { "id", inputId },
                                { "type" , inputType },
                                { "channel", inputChannel },
                                { "sampler", new JObject {
                                    { "filter", input["sampler"]["filter"].Value<string>() },
                                    { "wrap", input["sampler"]["wrap"].Value<string>() }
                                } }
                            });
                            } else if (inputType == "texture") {

                                // TODO: Download texture asset...

                                string srcUrl = ApiAssetUrl.Replace("{stub}", inputSrc);
                                Logger.Info("         - Fetching texture asset {0}", srcUrl);

                                int lastSlashOfSrc = inputSrc.LastIndexOf('/');
                                string assetFilename = $"{inputSrc.Substring(lastSlashOfSrc + 1)}";
                                string assetFilepath = Path.Combine(assetsFolder.FullName, assetFilename);

                                Http.GetFile(srcUrl, assetFilepath);

                                inputs.Add(new JObject {
                                { "id", inputId },
                                { "type" , inputType },
                                { "src", $"assets/{assetFilename}"},
                                { "channel", inputChannel },
                                { "sampler", new JObject {
                                    { "filter", input["sampler"]["filter"].Value<string>() },
                                    { "wrap", input["sampler"]["wrap"].Value<string>() },
                                    { "vflip", input["sampler"]["vflip"].Value<bool>() }
                                } }
                            });
                            } else {
                                Logger.Warn("         - Unknown channel type '{0}' for render pass '{1}'", inputType, name);
                            }
                        }

                        Logger.Debug("     - Processing outputs...");
                        IEnumerable<int> outputs = renderPass["outputs"].Select(x => x["id"].Value<int>());


                        Logger.Debug("     - Writing configuration file...");
                        string configFilename = $"{renderPassName}.json";
                        File.WriteAllText(Path.Combine(tempDirectory, configFilename), JsonConvert.SerializeObject(new {

                            name,
                            description,
                            type,
                            code = glslFilename,
                            inputs,
                            outputs

                        }, Formatting.Indented), Encoding.UTF8);
                    }

                    index++;
                }

                // Zip directory..
                Logger.Info("Packing...");

                string packExtension = "dpst";
#if DEBUG
                packExtension = "zip";
#endif

                if (string.IsNullOrEmpty(outputFilename))
                    outputFilename = $"{Util.MakeValidFileName(root["info"]["name"].Value<string>())}.{packExtension}";
                else
                    outputFilename = $"{outputFilename}.{packExtension}";

                if (outputDirectory == null || !Directory.Exists(outputDirectory))
                    outputDirectory = Directory.GetCurrentDirectory();

                string zipFile = Path.Combine(outputDirectory, outputFilename);
                if (File.Exists(zipFile)) {
                    if (overrideOutput) {
                        Logger.Warn("Overwriting output pack file '{0}'", outputFilename);
                        File.Delete(zipFile);
                    } else {
                        Logger.Error("A pack file already exists with the output filename '{0}'", outputFilename);
                    }
                }

                Logger.Info("Packing to '{0}'", outputFilename);
                ZipFile.CreateFromDirectory(tempDirectory, zipFile, CompressionLevel.Optimal, false, Encoding.UTF8);

                Logger.Info("Done.");

            }

        }


        static int Main(string[] args) {

            return Parser.Default.ParseArguments<Options>(args).MapResult(
                options => Run(options),
                _ => -1
            );

        }

        private static int Run(Options options) {
            string vertexShader = DefaultVertexShader;

            if (options.VertexShaderFilepath != null && File.Exists(options.VertexShaderFilepath))
                vertexShader = File.ReadAllText(options.VertexShaderFilepath, Encoding.UTF8);

            PackUrl(options.Url, options.ApiKey ?? PublicApiKey, options.Filename, options.Directory, vertexShader, options.OverwriteOutput);

            //Console.ReadKey();
            return 0;
        }

    }

}