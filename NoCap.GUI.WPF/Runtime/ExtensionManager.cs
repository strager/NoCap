using System;
using System.Collections.Generic;
using System.ComponentModel.Composition.Hosting;
using System.IO;
using System.Linq;

namespace NoCap.GUI.WPF.Runtime {
    class ExtensionManager : IDisposable {
        private static readonly string ExtensionFilter = "*.nocap";

        private readonly CompositionContainer compositionContainer;
        private readonly FileSystemWatcher fileSystemWatcher;

        private readonly AggregateCatalog aggregateCatalog;

        private readonly ICollection<Extension> loadedExtensions = new List<Extension>();

        public CompositionContainer CompositionContainer {
            get {
                return this.compositionContainer;
            }
        }

        public ExtensionManager(DirectoryInfo rootDirectory) {
            this.aggregateCatalog = new AggregateCatalog(new AssemblyCatalog(typeof(ExtensionManager).Assembly));

            this.compositionContainer = new CompositionContainer(this.aggregateCatalog);

            this.fileSystemWatcher = new FileSystemWatcher(rootDirectory.FullName, ExtensionFilter) {
                IncludeSubdirectories = false,
            };

            this.fileSystemWatcher.Changed += CheckExtension;
            this.fileSystemWatcher.Created += CheckExtension;
            this.fileSystemWatcher.Deleted += CheckExtension;
            this.fileSystemWatcher.Renamed += CheckExtension;
            this.fileSystemWatcher.Created += CheckNewExtension;

            foreach (var file in rootDirectory.EnumerateFiles(ExtensionFilter, SearchOption.TopDirectoryOnly)) {
                LoadExtension(file.FullName);
            }
        }

        private void CheckNewExtension(object sender, FileSystemEventArgs e) {
            LoadExtension(e.FullPath);
        }

        private Extension LoadExtension(string extensionFileName) {
            var extension = Extension.Load(extensionFileName);
            var catelog = new AggregateCatalog(extension.Assemblies.Select((assembly) => new AssemblyCatalog(assembly)));

            this.aggregateCatalog.Catalogs.Add(catelog);

            this.loadedExtensions.Add(extension);

            return extension;
        }

        private static void CheckExtension(object sender, FileSystemEventArgs e) {
            // TODO
        }

        public void Dispose() {
            foreach (var extension in this.loadedExtensions) {
                extension.Dispose();
            }
        }
    }
}
