using System;
using System.Collections.Generic;
using System.ComponentModel.Composition.Hosting;
using System.IO;

namespace NoCap.GUI.WPF.Runtime {
    class ExtensionManager {
        private readonly CompositionContainer compositionContainer;
        private readonly FileSystemWatcher fileSystemWatcher;

        private readonly AggregateCatalog aggregateCatalog;
        private readonly ICollection<DirectoryCatalog> directoryCatalogs = new List<DirectoryCatalog>();

        public CompositionContainer CompositionContainer {
            get {
                return this.compositionContainer;
            }
        }

        public ExtensionManager(DirectoryInfo rootDirectory) {
            this.aggregateCatalog = new AggregateCatalog(new AssemblyCatalog(typeof(ExtensionManager).Assembly));

            this.compositionContainer = new CompositionContainer(this.aggregateCatalog);

            this.fileSystemWatcher = new FileSystemWatcher(".");
            this.fileSystemWatcher.Changed += CheckExtension;
            this.fileSystemWatcher.Created += CheckExtension;
            this.fileSystemWatcher.Deleted += CheckExtension;
            this.fileSystemWatcher.Renamed += CheckExtension;
            this.fileSystemWatcher.Created += CheckNewExtension;

            AddDirectoryCatalog(rootDirectory.FullName);

            foreach (var subDirectory in rootDirectory.EnumerateDirectories("*", SearchOption.AllDirectories)) {
                AddDirectoryCatalog(subDirectory.FullName);
            }
        }

        private void CheckNewExtension(object sender, FileSystemEventArgs e) {
            if (File.GetAttributes(e.FullPath).HasFlag(FileAttributes.Directory)) {
                AddDirectoryCatalog(e.FullPath);
            }
        }

        private void AddDirectoryCatalog(string fullPath) {
            using (new ExtensionCompositionScope(fullPath)) {
                var catalog = new DirectoryCatalog(fullPath);
                this.directoryCatalogs.Add(catalog);
                this.aggregateCatalog.Catalogs.Add(catalog);
            }
        }

        private void CheckExtension(object sender, FileSystemEventArgs e) {
            // TODO Only check affected extensions
            foreach (var catalog in this.directoryCatalogs) {
                using (new ExtensionCompositionScope(catalog.FullPath)) {
                    catalog.Refresh();
                }
            }
        }
    }

    sealed class ExtensionCompositionScope : IDisposable {
        private bool disposed = false;

        public ExtensionCompositionScope(string fullPath) {
            // FIXME Any non-deprecated way to deal with this?
            AppDomain.CurrentDomain.AppendPrivatePath(fullPath);
        }

        public void Dispose() {
            if (disposed) {
                throw new ObjectDisposedException("ExtensionCompositionScope");
            }

            AppDomain.CurrentDomain.ClearPrivatePath();

            disposed = true;
        }
    }
}
