using System;
using System.Diagnostics;
using System.IO;

namespace NoCap.DeleteOnParentExit {
    class Program {
        static void Main(string[] args) {
            try {
                var parentProcess = ProcessExtensions.ParentOf(Process.GetCurrentProcess());

                parentProcess.WaitForExit(5000); // Wait some reasonable amount of time.
            } catch (Exception e) {
                // We don't care about no exceptions!
                Console.WriteLine("Exception while waiting for parent process:");
                Console.WriteLine(e);
            }

            foreach (string fileName in args) {
                try {
                    if (Directory.Exists(fileName)) {
                        Directory.Delete(fileName, true);
                    } else if (File.Exists(fileName)) {
                        File.Delete(fileName);
                    }
                } catch (Exception e) {
                    // We don't care about no exceptions!
                    Console.WriteLine("Exception while deleting file or directory '{0}':", fileName);
                    Console.WriteLine(e);
                }
            }
        }
    }
}
