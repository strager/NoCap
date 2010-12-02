using System;
using System.Collections.Generic;
using System.ComponentModel.Composition.Hosting;
using System.IO;
using System.Reflection;

namespace NoCap.GUI.WPF.Runtime {
    // TODO Rewrite
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
            AppDomain.CurrentDomain.AssemblyResolve += LoadAssemblyDependency;

            this.aggregateCatalog = new AggregateCatalog(
                new AssemblyCatalog(typeof(ExtensionManager).Assembly)
            );

            this.compositionContainer = new CompositionContainer(this.aggregateCatalog);

            this.fileSystemWatcher = new FileSystemWatcher(".");
            this.fileSystemWatcher.Changed += CheckFileSystem;
            this.fileSystemWatcher.Created += CheckFileSystem;
            this.fileSystemWatcher.Deleted += CheckFileSystem;
            this.fileSystemWatcher.Renamed += CheckFileSystem;
            this.fileSystemWatcher.Created += CheckNewExtension;

            AddDirectoryCatalog(rootDirectory.FullName);

            foreach (var subDirectory in rootDirectory.EnumerateDirectories("*", SearchOption.AllDirectories)) {
                AddDirectoryCatalog(subDirectory.FullName);
            }
        }

        private bool tryLoadingAssemblies = true;

        private Assembly LoadAssemblyDependency(object sender, ResolveEventArgs e) {
            // This method is needed because some extensions have their
            // dependencies in their subfolder.  The runtime does not look
            // in those subfolders by default, so we have to do it
            // ourselves.  I hope you don't mind...
            //
            // But hey; FIXME please; this seems like a poor solution.
            // It breaks when there are two copies of an assembly which have
            // different versions or something like that.  I'm sure you can
            // think of other ways to break this system.  Just please...
            // Fix this if you can.

            if (!this.tryLoadingAssemblies) {
                return null;
            }

            this.tryLoadingAssemblies = false;

            try {
                foreach (var catalog in this.directoryCatalogs) {
                    Assembly assembly;

                    if (TryLoadAssemblyFromPath(catalog.FullPath, e.Name, out assembly)) {
                        return assembly;
                    }
                }
            } finally {
                this.tryLoadingAssemblies = true;
            }

            return null;
        }

        private static bool TryLoadAssemblyFromPath(string path, string assemblyName, out Assembly assembly) {
            // FIXME This seems like a poor solution.
            // TODO Check assembly versions.

            string assemblyBaseName = assemblyName.Split(',')[0];

            foreach (var file in Directory.EnumerateFiles(path, assemblyBaseName + "*")) {
                var fileName = Path.Combine(path, file);

                Assembly currentAssembly;

                if (TryLoadAssemblyFrom(fileName, out currentAssembly)) {
                    assembly = currentAssembly;

                    return true;
                }
            }

            assembly = null;

            return false;
        }

        private static bool TryLoadAssemblyFrom(string fileName, out Assembly assembly) {
            try {
                assembly = Assembly.LoadFrom(fileName);

                return true;
            } catch (IOException) {
            } catch (BadImageFormatException) {
            }

            assembly = null;

            return false;
        }

        private void CheckNewExtension(object sender, FileSystemEventArgs e) {
            if (File.GetAttributes(e.FullPath).HasFlag(FileAttributes.Directory)) {
                AddDirectoryCatalog(e.FullPath);
            }
        }

        private void AddDirectoryCatalog(string fullPath) {
            var catalog = new DirectoryCatalog(fullPath);
            this.directoryCatalogs.Add(catalog);
            this.aggregateCatalog.Catalogs.Add(catalog);
        }

        private void CheckFileSystem(object sender, FileSystemEventArgs e) {
            foreach (var catalog in this.directoryCatalogs) {
                catalog.Refresh();
            }
        }
    }
}
