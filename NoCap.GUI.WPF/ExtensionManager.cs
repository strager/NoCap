using System.ComponentModel.Composition.Hosting;
using System.IO;

namespace NoCap.GUI.WPF {
    class ExtensionManager {
        private readonly CompositionContainer compositionContainer;
        private readonly DirectoryCatalog directoryCatalog;
        private readonly FileSystemWatcher fileSystemWatcher;

        public CompositionContainer CompositionContainer {
            get {
                return this.compositionContainer;
            }
        }

        public ExtensionManager() {
            this.compositionContainer = new CompositionContainer(
                new AggregateCatalog(
                    (this.directoryCatalog = new DirectoryCatalog(".")),
                    new AssemblyCatalog(typeof(ExtensionManager).Assembly)
                )
            );

            this.fileSystemWatcher = new FileSystemWatcher(".");
            this.fileSystemWatcher.Changed += CheckFileSystem;
            this.fileSystemWatcher.Created += CheckFileSystem;
            this.fileSystemWatcher.Deleted += CheckFileSystem;
            this.fileSystemWatcher.Renamed += CheckFileSystem;
        }

        private void CheckFileSystem(object sender, FileSystemEventArgs e) {
            this.directoryCatalog.Refresh();
        }
    }
}
