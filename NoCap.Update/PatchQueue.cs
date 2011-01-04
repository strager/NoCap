using System.Collections.Generic;
using System.IO;
using System.Reflection;
using NoCap.Library.Util;

namespace NoCap.Update {
    public class PatchQueue {
        private readonly Queue<PatchData> queue = new Queue<PatchData>();

        public void ApplyQueuedPatches(bool rerunNoCap, params string[] noCapArguments) {
            string applicationRoot = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string tempDirectory = FileSystem.GetTempDirectory();

            var computer = new Microsoft.VisualBasic.Devices.Computer();

            computer.FileSystem.CopyDirectory(applicationRoot, tempDirectory);

            var arguments = new List<object> {
                "--root", tempDirectory,
                GetPatchArguments(this.queue),
                "--move-to", applicationRoot
            };

            if (rerunNoCap) {
                arguments.Add("--exec");
                arguments.Add(Assembly.GetEntryAssembly().Location);
                arguments.Add("--");
                arguments.Add(noCapArguments);
            }

            Process.DOPE(Process.Quote(arguments));

            this.queue.Clear();
        }

        private static IEnumerable<string> GetPatchArguments(IEnumerable<PatchData> patches) {
            foreach (var patch in patches) {
                yield return "--patch";
                yield return patch.patchDataRoot;
            }
        }

        public void EnqueuePatch(PatchData patch) {
            this.queue.Enqueue(patch);
        }
    }
}