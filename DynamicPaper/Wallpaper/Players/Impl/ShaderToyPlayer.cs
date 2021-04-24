namespace Maxstupo.DynamicPaper.Wallpaper.Players.Impl {
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.IO;
    using System.IO.Compression;
    using Maxstupo.DynamicPaper.Graphics;
    using Maxstupo.DynamicPaper.Graphics.Data;
    using Maxstupo.DynamicPaper.ShaderToy;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using System.Linq;
    using OpenTK.Graphics.OpenGL4;
    using Maxstupo.DynamicPaper.Utility;

    public sealed class ShaderToyPlayer : OpenGLPlayer {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        private Renderer renderer;
        private readonly RenderData data = new RenderData();

        // The file to load on next render.
        private string pendingFileToLoad;

        protected override void Init() {
            GL.ClearColor(Color.Black);

            renderer = new Renderer(RenderTarget.Display);
            renderer.Init();
        }


        protected override void Update(float deltaTime) {

            data.iFrame++;
            data.iTime = Time / 1000f;
            data.iGlobalTime = data.iTime;
            data.iFrameRate = Fps;

            DateTime time = DateTime.Now;
            data.iDate = new OpenTK.Vector4(time.Year, time.Month, time.Day, (float) time.TimeOfDay.TotalSeconds);

        }

        private void LoadShaderToyFile(string filepath) {
            int displayOutputBufferId = 1;

            renderer.Clear();

            using (ZipArchive zip = ZipFile.OpenRead(filepath)) {
                renderer.SharedFragmentCode = zip.GetEntry("common.glsl")?.ReadToEnd();

                string vertexShader = zip.GetEntry("shared.vs.glsl").ReadToEnd();


                foreach (ZipArchiveEntry entry in zip.Entries) {

                    if (entry.Name.StartsWith("rp") && entry.Name.EndsWith(".json")) {

                        JObject jRenderPass = JObject.Parse(entry.ReadToEnd());

                        if (jRenderPass["type"].Value<string>() == "image")
                            displayOutputBufferId = jRenderPass["outputs"].Values<int>().First();

                        ZipArchiveEntry shaderEntry = zip.GetEntry(jRenderPass["code"].Value<string>());
                        string shaderCode = shaderEntry.ReadToEnd();

                        RenderPassBuilder rpb = RenderPassBuilder.FromString(jRenderPass["name"].Value<string>(), vertexShader, shaderCode);

                        foreach (JObject obj in jRenderPass["inputs"]) {
                            string type = obj["type"].Value<string>();

                            if (type == "buffer") {
                                rpb.AddBufferInput(obj["channel"].Value<int>(), obj["id"].Value<int>());

                            } else if (type == "texture") {
                                string src = obj["src"].Value<string>();

                                ZipArchiveEntry e = zip.Entries.FirstOrDefault(x => src.EndsWith(x.Name));
                                rpb.AddTextureInput(obj["channel"].Value<int>(), e.Name, e.ReadAllBytes());
                            }
                        }

                        foreach (int bufferId in jRenderPass["outputs"].Values<int>())
                            rpb.AddOutput(bufferId);




                        RenderPass rp = rpb.Create();
                        renderer.Add(rp);
                    }
                }

            }

            renderer.DisplayOutputBufferId = displayOutputBufferId;
            renderer.InitPasses();
        }


        protected override void Render() {
            if (pendingFileToLoad != null) {
                LoadShaderToyFile(pendingFileToLoad);
                pendingFileToLoad = null;
            }
            renderer?.Render(data);
        }

        protected override void PlayMedia(IMediaItem item = null) {
            if (Media != item) {
                Logger.Error("Media Changed!");
                if (item != null)
                    pendingFileToLoad = item.Filepath; // Can't call LoadShaderToyFile directly as this method is invoked by a thread that doesn't have the OpenGL context.
            }

            base.PlayMedia(item);
        }

        protected override void OnResized(int width, int height, float ratio) {

            GL.Viewport(0, 0, width, height);
            renderer.RecreateBuffers(width, height);

            data.iResolution = new OpenTK.Vector3(width, height, 0);

        }

        protected override void Dispose(bool disposing) {
            base.Dispose(disposing);
            if (disposing) {
                renderer.Dispose();
                renderer = null;
            }
        }

    }

}