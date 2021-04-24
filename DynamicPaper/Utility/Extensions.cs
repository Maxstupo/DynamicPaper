namespace Maxstupo.DynamicPaper.Utility {

    using System.IO;
    using System.IO.Compression;

    public static class Extensions {

        public static string ReadToEnd(this Stream src) {
            using (StreamReader r = new StreamReader(src))
                return r.ReadToEnd();
        }

        public static byte[] ReadAllBytes(this Stream src) {
            using (MemoryStream ms = new MemoryStream()) {
                src.CopyTo(ms);
                return ms.ToArray();
            }
        }

        public static string ReadToEnd(this ZipArchiveEntry entry) {
            using (Stream stream = entry.Open())
                return ReadToEnd(stream);
        }

        public static byte[] ReadAllBytes(this ZipArchiveEntry entry) {
            using (Stream stream = entry.Open())
                return ReadAllBytes(stream);
        }

    }

}