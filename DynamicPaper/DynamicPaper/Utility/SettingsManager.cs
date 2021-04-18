namespace Maxstupo.DynamicPaper.Utility {

    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Text;
    using System.Windows.Forms;
    using Newtonsoft.Json;

    public interface ISettings { void RestoreDefaults(); }

    public sealed class SettingsManager<T> where T : ISettings {

        private static readonly Encoding Encoding = Encoding.UTF8;

        public string AppDirectory { get; }

        public string Filename { get; }

        public string Filepath => Path.Combine(AppDirectory, Filename);


        public T Settings { get; private set; }

        public bool Formatted { get; set; } = true;


        private string settingsSnapshot;


        public SettingsManager(string filename, Func<T> settingsFactory) {
            this.Filename = filename;

            string rootDirectory = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            this.AppDirectory = Path.Combine(rootDirectory, Application.ProductName);

            Directory.CreateDirectory(AppDirectory);

            Debug.WriteLine($"App Directory: '{AppDirectory}'");

            Settings = settingsFactory();
            RestoreDefaults();
        }


        public SettingsManager<T> RestoreDefaults() {
            Settings.RestoreDefaults();
            return this;
        }

        public SettingsManager<T> Save() {
            string json = JsonConvert.SerializeObject(Settings, Formatted ? Formatting.Indented : Formatting.None);

            File.WriteAllText(Filepath, json, Encoding);

            settingsSnapshot = null;

            return this;
        }

        public bool Load(bool saveIfNotExists) {
            if (!File.Exists(Filepath)) {
                if (saveIfNotExists) Save();
                return false;
            }

            string json = File.ReadAllText(Filepath, Encoding);

            Settings = JsonConvert.DeserializeObject<T>(json);

            return true;
        }

        /// <summary>
        /// Reverts Settings to the last marked point. Warning: This replaces the object. Databinding will break after calling this method.
        /// </summary>
        public SettingsManager<T> Revert() {
            if (settingsSnapshot != null)
                Settings = JsonConvert.DeserializeObject<T>(settingsSnapshot);
            settingsSnapshot = null;
            return this;
        }

        public SettingsManager<T> Mark() {
            settingsSnapshot = JsonConvert.SerializeObject(Settings);
            return this;
        }

    }

}