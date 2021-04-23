namespace Maxstupo.ShaderToyPack {
   
    using System;
    using System.IO;
    using System.Net;
    using System.Text.RegularExpressions;

    public static class Http {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        public static string Get(string url) {
            Logger.Info("GET {0}", url);

            HttpWebRequest request = (HttpWebRequest) WebRequest.Create(url);
            request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;

            using (HttpWebResponse response = (HttpWebResponse) request.GetResponse()) {
                using (Stream stream = response.GetResponseStream()) {
                    using (StreamReader reader = new StreamReader(stream))
                        return reader.ReadToEnd();
                }
            }

        }

        public static void GetFile(string url, string destFilepath) {
            Logger.Info("GET {0} -> {1}", url, destFilepath);

            HttpWebRequest request = (HttpWebRequest) WebRequest.Create(url);
            request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;

            using (HttpWebResponse response = (HttpWebResponse) request.GetResponse()) {
                using (FileStream fileStream = File.OpenWrite(destFilepath)) {
                    using (Stream stream = response.GetResponseStream())
                        stream.CopyTo(fileStream);
                }
            }

        }
    }


    public sealed class TempDirectory : IDisposable {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        private readonly bool deleteOnDispose;

        public DirectoryInfo DirectoryInfo { get; }

        public TempDirectory(string path, bool deleteOnDispose) {
            this.DirectoryInfo = new DirectoryInfo(path);
            this.deleteOnDispose = deleteOnDispose;
        }

        public TempDirectory(DirectoryInfo directoryInfo, bool deleteOnDispose) {
            this.DirectoryInfo = directoryInfo;
            this.deleteOnDispose = deleteOnDispose;
        }

        public void Dispose() {
            if (deleteOnDispose) {
                Logger.Debug("Deleting temp directory...");
                DirectoryInfo.Delete(true);
            }
        }



        public static implicit operator DirectoryInfo(TempDirectory tempDir) => tempDir.DirectoryInfo;
        public static implicit operator string(TempDirectory tempDir) => tempDir.DirectoryInfo.FullName;


    }

    public static class Util {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        public static TempDirectory CreateTempDirectory(string baseDirectory = null, bool deleteOnDispose = true) {
            string name = Path.GetRandomFileName();
            name = name.Substring(0, name.LastIndexOf('.'));

            string path = Path.Combine(baseDirectory != null ? Path.GetFullPath(baseDirectory) : Path.GetTempPath(), name);
            Logger.Info("Creating temp directory: {0}", path);
            return new TempDirectory(Directory.CreateDirectory(path), deleteOnDispose);
        }

        // https://stackoverflow.com/a/847251
        public static string MakeValidFileName(string name) {
            string invalidChars = Regex.Escape(new string(Path.GetInvalidFileNameChars()));
            string invalidRegStr = string.Format(@"([{0}]*\.+$)|([{0}]+)", invalidChars);

            return Regex.Replace(name, invalidRegStr, "_");
        }

        public static string GetRelativePath(string baseDirectory, string filepath) {
            Uri baseDir = new Uri(baseDirectory);
            Uri relativePath = baseDir.MakeRelativeUri(new Uri(filepath));
            return Uri.UnescapeDataString(relativePath.OriginalString);
        }

    }

}