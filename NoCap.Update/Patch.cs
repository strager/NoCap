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

        private Patch(string patchDataRoot) {
            this.patchDataRoot = patchDataRoot;
        }

        public static Patch LoadFromArchive(string fileName) {
            using (var file = File.Open(fileName, FileMode.Open, FileAccess.Read)) {
                return LoadFromArchive(file);
            }
        }

        public static Patch LoadFromArchive(Stream stream) {
            string patchDataRoot = FileSystem.GetTempDirectory();

            using (var zip = ZipFile.Read(stream)) {
                zip.ExtractAll(patchDataRoot, ExtractExistingFileAction.Throw);
            }

            return InitializeFrom(patchDataRoot);
        }

        private static Patch InitializeFrom(string patchDataRoot) {
            var config = new XmlDocument();
            config.Load(Path.Combine(patchDataRoot, "nocap.xml"));

            var patch = config.DocumentElement;

            return new Patch(patchDataRoot) {
                Instructions = new ReadOnlyCollection<IPatchInstruction>(
                    patch.SelectNodes("Instructions/*").OfType<XmlElement>().Select(ParseInstruction).ToArray()
                ),
            };
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
