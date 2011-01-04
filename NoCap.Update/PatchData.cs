using System.IO;
using Ionic.Zip;
using NoCap.Library.Util;

namespace NoCap.Update {
    public class PatchData {
        internal readonly string patchDataRoot;

        private PatchData(string patchDataRoot) {
            this.patchDataRoot = patchDataRoot;
        }

        public static PatchData LoadFrom(string fileName) {
            using (var file = File.Open(fileName, FileMode.Open, FileAccess.Read)) {
                return LoadFrom(file);
            }
        }

        public static PatchData LoadFrom(Stream stream) {
            string patchPath = FileSystem.GetTempDirectory();

            using (var zip = ZipFile.Read(stream)) {
                zip.ExtractAll(patchPath, ExtractExistingFileAction.Throw);
            }

            return new PatchData(patchPath);
        }
    }
}
