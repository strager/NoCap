using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using NoCap.Library.Util;

namespace NoCap.Update {
    public class PatchingEnvironment {
        private static readonly Microsoft.VisualBasic.Devices.Computer Computer = new Microsoft.VisualBasic.Devices.Computer();

        private const string DefaultVersion = "0.0.0";
        private const string VersionFileName = "version";
        private static readonly Encoding VersionFileEncoding = Encoding.UTF8;

        private readonly object fileSystemSync = new object();

        private readonly string applicationRoot;

        public string ApplicationRoot {
            get { return this.applicationRoot; }
        }

        public bool IsModified {
            get;
            private set;
        }

        public string Version {
            get;
            private set;
        }

        private PatchingEnvironment(string applicationRoot) {
            this.applicationRoot = applicationRoot;
        }

        public static PatchingEnvironment Create(PatchingEnvironment baseEnvironment) {
            string newApplicationRoot = FileSystem.GetTempDirectory();

            Computer.FileSystem.CopyDirectory(baseEnvironment.ApplicationRoot, newApplicationRoot);

            return FromExisting(newApplicationRoot);
        }

        public static PatchingEnvironment FromExisting(string path) {
            var patchingEnvironment = new PatchingEnvironment(path);
            patchingEnvironment.Version = patchingEnvironment.ReadVersion() ?? DefaultVersion;

            return patchingEnvironment;
        }

        public static PatchingEnvironment GetCurrent() {
            return FromExisting(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location));
        }

        public void ApplyPatch(Patch patch) {
            lock (fileSystemSync) {
                if (Version != patch.FromVersion) {
                    throw new ArgumentException(string.Format("Cannot upgrade from version '{0}' using patch for version '{1}'", Version, patch.FromVersion));
                }

                IsModified = true;

                foreach (var instruction in patch.Instructions) {
                    instruction.Apply(patch.PatchDataRoot, ApplicationRoot);
                }

                WriteVersion(patch.ToVersion);

                Version = patch.ToVersion;
            }
        }

        private string ReadVersion() {
            var versionFileName = Path.Combine(ApplicationRoot, VersionFileName);

            if (!File.Exists(versionFileName)) {
                return null;
            }

            return File.ReadAllLines(versionFileName, VersionFileEncoding).FirstOrDefault();
        }

        private void WriteVersion(string version) {
            File.WriteAllText(Path.Combine(ApplicationRoot, VersionFileName), version, VersionFileEncoding);
        }

        private static void MoveDirectory(string source, string destination) {
            try {
                Directory.Move(source, destination);
            } catch (IOException) {
                Computer.FileSystem.CopyDirectory(source, destination);
                Directory.Delete(source, true);
            }
        }

        public void ReplaceWith(PatchingEnvironment other) {
            // TODO Sanity checks, etc.

            string source = other.ApplicationRoot;
            string destination = ApplicationRoot;

            // Rename the destination,
            // move source to (old) destination,
            // delete renamed destination

            string tempRoot = Path.Combine(Path.GetDirectoryName(destination), string.Format("NoCap-{0}", Path.GetRandomFileName()));

            bool success;

            success = false;

            try {
                MoveDirectory(destination, tempRoot);

                success = true;
            } finally {
                if (!success) {
                    if (Directory.Exists(tempRoot)) {
                        MoveDirectory(tempRoot, destination);
                    }
                }
            }

            success = false;

            try {
                MoveDirectory(source, destination);

                success = true;
            } finally {
                if (!success) {
                    if (Directory.Exists(destination)) {
                        Directory.Delete(destination, true);
                    }

                    MoveDirectory(tempRoot, destination);
                }
            }

            Directory.Delete(tempRoot, true);
        }
    }
}
