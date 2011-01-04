using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using Mono.Options;
using NoCap.Library.Util;
using NoCap.Update;
using Process = System.Diagnostics.Process;

namespace NoCap.DeleteOnParentExit {
    class Program {
        static void Main(string[] args) {
            try {
                var parentProcess = Process.GetCurrentProcess().GetParent();

                parentProcess.WaitForExit(5000); // Wait some reasonable amount of time.
            } catch (Exception e) {
                // We don't care about no exceptions!
                Console.WriteLine("Exception while waiting for parent process:");
                Console.WriteLine(e);

                // Fall through.
            }

            int exitCode = 0;

            try {
                ParseArguments(args, out exitCode);
            } catch (Exception e) {
                // We don't care about no exceptions!
                Console.WriteLine(e);

                if (exitCode == 0) {
                    exitCode = 1;
                }
            }

            Environment.Exit(exitCode);
        }

        private static void ParseArguments(IEnumerable<string> args, out int exitCode) {
            bool showHelp = false;
            bool copyDope = true;

            string applicationRoot = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            string moveDestination = null;

            var patchFiles = new List<string>();
            var filesToDelete = new List<string>();
            var executables = new List<string>();

            var optionSet = new OptionSet {
                { "h|help", "show this message and exit", (v) => showHelp = v != null },
                { "patch=", "apply a patches (can be specified multiple times)", (v) => patchFiles.Add(v) },
                { "move-to=", "move the application to the given directory", (v) => moveDestination = v },
                { "approot=", "sets the application root for the `patch' and `clone' options", (v) => applicationRoot = v },
                { "no-copy", "do not copy DOPE to a temporary directory before executing commands", (v) => copyDope = false },
                { "delete", "deletes a file (can be specifieed multiple times)", (v) => filesToDelete.Add(v) },
                { "exec=", "executes the specified file with optional arguments after all other commands", (v) => executables.Add(v) },
            };

            List<string> arguments;

            try {
                arguments = optionSet.Parse(args);
            } catch (OptionException e) {
                Console.WriteLine("DOPE: {0}", e.Message);
                Console.WriteLine("Try `DOPE --help' for more information.");

                exitCode = 1;

                return;
            }

            if (showHelp) {
                ShowHelp(optionSet);

                exitCode = 0;

                return;
            }

            if (copyDope) {
                CopyDope(new[] { "--no-copy", "--approot", applicationRoot }.Concat(args));

                exitCode = 0;

                return;
            }

            DeleteFiles(filesToDelete);

            ApplyPatches(applicationRoot, patchFiles);

            if (moveDestination != null) {
                MoveApplication(applicationRoot, moveDestination);
            }

            foreach (string executable in executables) {
                Process.Start(new ProcessStartInfo {
                    FileName = executable,
                    WorkingDirectory = Path.GetDirectoryName(executable),
                    Arguments = Library.Util.Process.Quote(arguments),
                }.Silence());
            }

            exitCode = 0;
        }

        private static void CopyDope(IEnumerable<string> args) {
            string tempDir = FileSystem.GetTempDirectory();

            var assemblies = new[] {
                typeof(Program).Assembly,
                typeof(NoCap.Library.ICommand).Assembly,
                typeof(NoCap.Update.Patch).Assembly,
            };

            var files = assemblies.Select((assembly) => assembly.Location);

            foreach (string file in files) {
                File.Copy(file, Path.Combine(tempDir, Path.GetFileName(file)));
            }

            Process.Start(new ProcessStartInfo {
                FileName = Path.Combine(tempDir, "DOPE.exe"),
                Arguments = Library.Util.Process.Quote(args),
            }.Silence());
        }

        private static void DeleteFiles(IEnumerable<string> fileNames) {
            foreach (string fileName in fileNames) {
                if (Directory.Exists(fileName)) {
                    Directory.Delete(fileName, true);
                } else if (File.Exists(fileName)) {
                    File.Delete(fileName);
                }
            }
        }

        private static void MoveApplication(string source, string destination) {
            // Rename the destination,
            // move source to (old) destination,
            // delete renamed destination

            string oldApplicationPath = Path.Combine(Path.GetDirectoryName(destination), string.Format("NoCap-{0}", Path.GetRandomFileName()));

            bool success;

            success = false;

            try {
                Directory.Move(destination, oldApplicationPath);

                success = true;
            } finally {
                if (!success) {
                    if (Directory.Exists(oldApplicationPath)) {
                        Directory.Move(oldApplicationPath, destination);
                    }
                }
            }

            success = false;

            try {
                Directory.Move(source, destination);

                success = true;
            } finally {
                if (!success) {
                    if (Directory.Exists(destination)) {
                        Directory.Delete(destination, true);
                    }

                    Directory.Move(oldApplicationPath, destination);
                }
            }

            Directory.Delete(oldApplicationPath, true);
        }

        private static void ApplyPatches(string applicationRoot, IEnumerable<string> patchDataRoots) {
            var patches = patchDataRoots.Select((patchDataRoot) => new Patch(PatchInfo.LoadFrom(patchDataRoot)));

            foreach (var patch in patches) {
                patch.Apply(applicationRoot);
            }
        }

        private static void ShowHelp(OptionSet optionSet) {
            Console.WriteLine("Usage: DOPE [options]");
            Console.WriteLine();
            Console.WriteLine("Options:");
            optionSet.WriteOptionDescriptions(Console.Out);
        }
    }
}
