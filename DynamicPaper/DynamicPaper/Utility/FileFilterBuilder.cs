namespace Maxstupo.DynamicPaper.Utility {

    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    public sealed class FileFilterItem {

        public string Description { get; }

        public string[] Extensions { get; }

        private readonly string pattern;

        public FileFilterItem(string description, string[] extensions) {
            if (string.IsNullOrEmpty(description))
                throw new ArgumentException("Description must be a non-null string!", nameof(description));
            if (extensions == null || extensions.Length == 0)
                throw new ArgumentException("Extensions must be a non-null array with one or more items!", nameof(extensions));

            this.Description = description;
            this.Extensions = extensions;
            this.pattern = string.Join(";", extensions.Select(x => Path.ChangeExtension("*", x)));
        }

        public override string ToString() {
            return $"{Description} ({pattern})|{pattern}";
        }

    }

    public sealed class FileFilterBuilder {
        private readonly List<FileFilterItem> items = new List<FileFilterItem>();

        public FileFilterBuilder Add(string description, params string[] extensions) {
            items.Add(new FileFilterItem(description, extensions));
            return this;
        }

        public FileFilterBuilder AddGroup(string description, int insertIndex = -1) {
            string[] extensions = items.SelectMany(x => x.Extensions).ToArray();

            int index = insertIndex < 0 ? items.Count : insertIndex;

            items.Insert(index, new FileFilterItem(description, extensions));
            return this;
        }

        public override string ToString() {
            return string.Join("|", items.Select(x => x.ToString()));
        }

        public static implicit operator string(FileFilterBuilder builder) {
            return builder.ToString();
        }

    }

}