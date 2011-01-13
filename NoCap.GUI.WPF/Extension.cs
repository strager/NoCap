using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;
using Ionic.Zip;
using log4net;
using NoCap.Library.Util;

namespace NoCap.GUI.WPF {
    class Extension : IDisposable {
        private static readonly ILog Log = LogManager.GetLogger(typeof(Extension));

        public string Name { get; private set; }
        public IEnumerable<string> Authors { get; private set; }
        public string Namespace { get; private set; }
        public IEnumerable<Assembly> Assemblies { get; private set; }
        public IDictionary<string, string> Licenses { get; private set; }

        public string RootDirectory {
            get {
                return dataDirectoryPath;
            }
        }

        private readonly string dataDirectoryPath;

        private IEnumerable<string> assemblyFileNames;

        private Assembly ResolveAssembly(object sender, ResolveEventArgs e) {
            // FIXME Is there a cleaner way to do this?

            if (e.RequestingAssembly == null
                || !FileSystem.AreDirectoriesSame(Path.GetDirectoryName(e.RequestingAssembly.Location), this.dataDirectoryPath)) {
                return null;
            }

            string assemblyFile = Path.Combine(this.dataDirectoryPath, e.Name.Substring(0, e.Name.IndexOf(",")) + ".dll");

            return File.Exists(assemblyFile) ? Assembly.LoadFrom(assemblyFile) : null;
        }

        private Extension(string dataDirectoryPath) {
            AppDomain.CurrentDomain.AssemblyResolve += ResolveAssembly;

            this.dataDirectoryPath = dataDirectoryPath;
        }

        public static Extension ReadFromArchive(string archiveFileName) {
            string dataDirectoryPath = FileSystem.GetTempDirectory();

            using (var file = File.Open(archiveFileName, FileMode.Open, FileAccess.Read))
            using (var zipFile = ZipFile.Read(file)) {
                zipFile.ExtractAll(dataDirectoryPath, ExtractExistingFileAction.OverwriteSilently);
            }

            var extension = new Extension(dataDirectoryPath);
            extension.LoadConfiguration();

            return extension;
        }

        private void LoadConfiguration() {
            Log.DebugFormat("Loading extension '{0}' configuration", RootDirectory);

            var config = new XmlDocument();
            config.Load(Path.Combine(this.dataDirectoryPath, "nocap.xml"));

            var extension = config.DocumentElement;

            this.assemblyFileNames = extension.SelectNodes("Assembly").OfType<XmlNode>().Select((node) => node.InnerText).ToArray();

            Name      = extension.SelectNodes("Name"     ).OfType<XmlNode>().Select((node) => node.InnerText).FirstOrDefault();
            Namespace = extension.SelectNodes("Namespace").OfType<XmlNode>().Select((node) => node.InnerText).FirstOrDefault();

            Authors = new ReadOnlyCollection<string>(
                extension.SelectNodes("Author").OfType<XmlNode>().Select((node) => node.InnerText).ToArray()
            );

            Licenses = new Dictionary<string, string>();

            foreach (var node in extension.SelectNodes("License").OfType<XmlNode>()) {
                const string termAttributeName = "Term";
                string term = null;

                if (node.Attributes != null && node.Attributes[termAttributeName] != null) {
                    term = node.Attributes[termAttributeName].Value;
                }

                string licenseText = node.InnerText;

                Licenses[term ?? ""] = licenseText;
            }
        }

        public void LoadAssemblies() {
            var assemblies = new List<Assembly>();
            
            foreach (var assemblyFileName in this.assemblyFileNames) {
                Log.DebugFormat("Loading assembly '{0}'", assemblyFileName);

                var assembly = Assembly.LoadFile(Path.Combine(this.dataDirectoryPath, assemblyFileName));

                assemblies.Add(assembly);
            }

            Assemblies = new ReadOnlyCollection<Assembly>(assemblies);
        }

        public void Dispose() {
            Log.DebugFormat("Unloading extension '{0}'", RootDirectory);

            if (this.dataDirectoryPath != null) {
                FileSystem.DeleteLater(this.dataDirectoryPath);
            }
        }
    }
}
