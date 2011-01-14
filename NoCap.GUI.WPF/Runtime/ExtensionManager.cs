using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition.Hosting;
using System.IO;
using System.Linq;
using log4net;

namespace NoCap.GUI.WPF.Runtime {
    class ExtensionManager : IDisposable {
        private static readonly ILog Log = LogManager.GetLogger(typeof(ExtensionManager));

        private static readonly string ExtensionFilter = "*.nocap";

        private readonly CompositionContainer commandCompositionContainer;
        private readonly FileSystemWatcher fileSystemWatcher;

        private readonly AggregateCatalog aggregateCatalog;

        private readonly IList<Extension> loadedExtensions = new List<Extension>();

        public CompositionContainer CommandCompositionContainer {
            get {
                return this.commandCompositionContainer;
            }
        }

        public IEnumerable<Extension> Extensions {
            get {
                return new ReadOnlyCollection<Extension>(this.loadedExtensions);
            }
        }

        public ExtensionManager(DirectoryInfo rootDirectory) {
            // FRAGILE; FIXME
            this.aggregateCatalog = new AggregateCatalog();
            this.aggregateCatalog.Catalogs.Add(new AssemblyCatalog(typeof(NoCap.Library.ICommand).Assembly));

            this.commandCompositionContainer = new CompositionContainer(this.aggregateCatalog);

            Log.Debug("Creating file system watcher");

            this.fileSystemWatcher = new FileSystemWatcher(rootDirectory.FullName, ExtensionFilter) {
                IncludeSubdirectories = false,
            };

            this.fileSystemWatcher.Changed += CheckExtension;
            this.fileSystemWatcher.Created += CheckExtension;
            this.fileSystemWatcher.Deleted += CheckExtension;
            this.fileSystemWatcher.Renamed += CheckExtension;
            this.fileSystemWatcher.Created += CheckNewExtension;

            Log.Debug("Loading extensions...");

            foreach (var file in rootDirectory.EnumerateFiles(ExtensionFilter, SearchOption.TopDirectoryOnly)) {
                LoadExtension(file.FullName);
            }
        }

        private void CheckNewExtension(object sender, FileSystemEventArgs e) {
            LoadExtension(e.FullPath);
        }

        private Extension LoadExtension(string extensionFileName) {
            Log.InfoFormat("Loading extension '{0}'", extensionFileName);

            var extension = Extension.ReadFromArchive(extensionFileName);
            extension.LoadAssemblies();

            var catalog = new AggregateCatalog(extension.Assemblies.Select((assembly) => new AssemblyCatalog(assembly)));

            this.aggregateCatalog.Catalogs.Add(catalog);

            this.loadedExtensions.Add(extension);

            Log.Debug("Extension loaded");

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
