using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;
using Ionic.Zip;

namespace NoCap.GUI.WPF {
    class Extension : IDisposable {
        public string Name { get; set; }
        public IEnumerable<string> Authors { get; set; }
        public string Namespace { get; set; }
        public IEnumerable<Assembly> Assemblies { get; set; }

        private string directory;

        private static string GetTempDirectory() {
            string directoryName = Path.GetRandomFileName();
            string directoryPath = Path.Combine(Path.GetTempPath(), directoryName);

            Directory.CreateDirectory(directoryPath);

            return directoryPath;
        }

        private static Assembly LoadAssemblyFromFile(string assemblyFileName) {
            string assemblyDirectory = Path.GetDirectoryName(assemblyFileName);

            // FIXME Is there a cleaner way to do this?

            AppDomain.CurrentDomain.AssemblyResolve += (sender, e) => {
                if (e.RequestingAssembly == null
                || !AreDirectoriesSame(Path.GetDirectoryName(e.RequestingAssembly.Location), assemblyDirectory)) {
                    return null;
                }

			    string assemblyFile = Path.Combine(assemblyDirectory, e.Name.Substring(0, e.Name.IndexOf(",")) + ".dll");

                return File.Exists(assemblyFile) ? Assembly.LoadFrom(assemblyFile) : null;
            };

            return Assembly.LoadFile(assemblyFileName);
        }

        private static bool AreDirectoriesSame(string directoryA, string directoryB) {
            return directoryA == directoryB;
        }

        private Extension() {
        }

        public static Extension Load(string extensionFileName) {
            string extensionDirectory = GetTempDirectory();

            using (var file = File.Open(extensionFileName, FileMode.Open, FileAccess.Read))
            using (var zipFile = ZipFile.Read(file)) {
                zipFile.ExtractAll(extensionDirectory, ExtractExistingFileAction.OverwriteSilently);
            }

            var config = new XmlDocument();
            config.Load(Path.Combine(extensionDirectory, "nocap.xml"));

            var assemblyFileNames = config.SelectNodes("//Assembly").OfType<XmlNode>().Select((node) => node.InnerText).ToArray();

            return new Extension {
                Assemblies = assemblyFileNames.Select(
                    (fileName) => LoadAssemblyFromFile(Path.Combine(extensionDirectory, fileName))
                ),
                directory = extensionDirectory,
            };
        }

        public void Dispose() {
            if (this.directory != null) {
                // TODO Delete directory
                // Currently, we can't delete the directory, because
                // there are references to files inside the directory.

                // Directory.Delete(this.directory, true);
            }
        }
    }
}
