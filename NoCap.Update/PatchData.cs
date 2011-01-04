using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using Ionic.Zip;
using NoCap.Library.Util;
using Process = NoCap.Library.Util.Process;

namespace NoCap.Update {
    public class PatchData {
        private static readonly Queue<PatchData> PatchQueue = new Queue<PatchData>();

        private readonly string patchDataRoot;

        static PatchData() {
            Application.Current.Exit += ApplyQueuedPatches;
        }

        private static void ApplyQueuedPatches(object sender, ExitEventArgs e) {
            string applicationRoot = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string tempDirectory = FileSystem.GetTempDirectory();

            var computer = new Microsoft.VisualBasic.Devices.Computer();

            computer.FileSystem.CopyDirectory(applicationRoot, tempDirectory);

            Process.StartOnProcessExit(new ProcessStartInfo {
                FileName = Path.Combine(applicationRoot, "NoCap.exe"), // FIXME Better way to get EXE path
                Arguments = Process.Quote("--patch", applicationRoot, "--", PatchQueue.Select((patch) => patch.patchDataRoot)),
            }, true);
        }

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

        public void ApplyLater() {
            PatchQueue.Enqueue(this);
        }
    }
}
