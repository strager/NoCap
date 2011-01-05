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
            Console.WriteLine(string.Join(" ", args));
            bool showHelp = false;
            bool copyDope = true;

            string applicationEnvironmentPath = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            string replacementEnvironmentPath = null;

            var filesToDelete = new List<string>();
            var executables = new List<List<string>>();

            var waitPids = new List<int>();

            var optionSet = new OptionSet {
                { "h|help", "show this message and exit", (v) => showHelp = v != null },
                { "w=|wait=", "wait for the given PID to exit before executing any commands (can be specified multiple times)", (v) => waitPids.Add(Convert.ToInt32(v)) },
                { "replace-with=", "replaces this application with the given application", (v) => replacementEnvironmentPath = v },
                { "approot=", "sets the application environment for the `replace-with' options", (v) => applicationEnvironmentPath = v },
                { "no-copy", "do not copy DOPE to a temporary directory before executing commands", (v) => copyDope = false },
                { "delete", "deletes a file or directory (can be specified multiple times)", (v) => filesToDelete.Add(v) },
                { "exec=", "executes the command after all other commands (can be specified multiple times)", (v) => executables.Add(new List<string> { v }) },
                { "execarg=", "specifies an argument to the last `exec' command", (v) => executables.Last().Add(v) },
            };

            try {
                optionSet.Parse(args);
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

            foreach (int pid in waitPids) {
                try {
                    Process.GetProcessById(pid).WaitForExit();
                } catch (Exception e) {
                    // We don't care about no exceptions!
                    Console.WriteLine("Exception while waiting for process '{0}':", pid);
                    Console.WriteLine(e);

                    // Fall through.
                }
            }

            if (copyDope) {
                CopyDope(new[] {
                    "--wait", Process.GetCurrentProcess().Id.ToString(),
                    "--no-copy",
                    "--approot", applicationEnvironmentPath,
                }.Concat(args));

                exitCode = 0;

                return;
            }

            if (replacementEnvironmentPath != null) {
                var applicationEnvironment = PatchingEnvironment.FromExisting(applicationEnvironmentPath);
                var replacementEnvironment = PatchingEnvironment.FromExisting(replacementEnvironmentPath);

                applicationEnvironment.ReplaceWith(replacementEnvironment);
            }

            DeleteFiles(filesToDelete);

            foreach (var executable in executables) {
                string fileName = executable.First();
                var arguments = executable.Skip(1);

                Process.Start(new ProcessStartInfo {
                    FileName = fileName,
                    WorkingDirectory = Path.GetDirectoryName(fileName),
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
                WorkingDirectory = tempDir,
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

        private static void ShowHelp(OptionSet optionSet) {
            Console.WriteLine("Usage: DOPE [options]");
            Console.WriteLine();
            Console.WriteLine("Options:");
            optionSet.WriteOptionDescriptions(Console.Out);
        }
    }
}
