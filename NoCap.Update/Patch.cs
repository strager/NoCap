using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Xml;
using Ionic.Zip;
using NoCap.Library.Util;

namespace NoCap.Update {
    public class Patch : IDisposable {
        public IEnumerable<IPatchInstruction> Instructions {
            get;
            private set;
        }

        private readonly string patchDataRoot;

        internal string PatchDataRoot {
            get { return this.patchDataRoot; }
        }

        public string FromVersion {
            get;
            private set;
        }

        public string ToVersion {
            get;
            private set;
        }

        private Patch(string patchDataRoot) {
            this.patchDataRoot = patchDataRoot;
        }

        public static Patch LoadFromArchive(string fileName) {
            using (var file = File.Open(fileName, FileMode.Open, FileAccess.Read)) {
                return LoadFromArchive(file);
            }
        }

        public static Patch LoadFromArchive(byte[] data) {
            using (var zipFile = ZipFile.Read(data)) {
                return LoadFromArchive(zipFile);
            }
        }

        public static Patch LoadFromArchive(Stream stream) {
            using (var zipFile = ZipFile.Read(stream)) {
                return LoadFromArchive(zipFile);
            }
        }

        private static Patch LoadFromArchive(ZipFile zipFile) {
            string patchDataRoot = FileSystem.GetTempDirectory();

            zipFile.ExtractAll(patchDataRoot, ExtractExistingFileAction.Throw);

            return InitializeFrom(patchDataRoot);
        }

        private static Patch InitializeFrom(string patchDataRoot) {
            var config = new XmlDocument();
            config.Load(Path.Combine(patchDataRoot, "nocap.xml"));

            var patch = new Patch(patchDataRoot);
            ParsePatchConfig(patch, config);
            
            return patch;
        }

        private static void ParsePatchConfig(Patch patch, XmlDocument config) {
            var configRoot = config.DocumentElement;

            patch.Instructions = new ReadOnlyCollection<IPatchInstruction>(
                configRoot.SelectNodes("Instructions/*").OfType<XmlElement>().Select(ParseInstruction).ToArray()
            );

            patch.FromVersion = configRoot.SelectSingleNode("From").InnerText;
            patch.ToVersion = configRoot.SelectSingleNode("To").InnerText;
        }

        private static IPatchInstruction ParseInstruction(XmlElement root) {
            switch (root.LocalName) {
                case "Add":
                    return AddPatchInstruction.ParseFrom(root);

                case "Delete":
                    return DeletePatchInstruction.ParseFrom(root);

                case "Move":
                    return MovePatchInstruction.ParseFrom(root);

                default:
                    throw new InvalidOperationException(string.Format("Unknown instruction: {0}", root.LocalName));
            }
        }

        public void Dispose() {
            Directory.Delete(this.patchDataRoot);
        }
    }
}
