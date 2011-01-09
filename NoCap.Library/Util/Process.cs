using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;

namespace NoCap.Library.Util {
    public static class Process {
        public static ProcessStartInfo Silence(this ProcessStartInfo startInfo) {
            startInfo.ErrorDialog = false;
            startInfo.UseShellExecute = false;
            startInfo.CreateNoWindow = true;

            return startInfo;
        }

        private readonly static ICollection<string> dopeArguments = new List<string>();

        public static void QueueDOPE(string arguments) {
            // TODO DOPE interface is a hack.  Create operations/instructions
            // (kinda like in the update system).

            dopeArguments.Add(arguments);
        }

        public static void FlushDOPE() {
            // FIXME Better way to get EXE path
            string dopeDirectory = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            string dopePath = Path.Combine(dopeDirectory, "DOPE.exe");

            System.Diagnostics.Process.Start(new ProcessStartInfo {
                FileName = dopePath,
                WorkingDirectory = dopeDirectory,
                Arguments = Quote("--wait", System.Diagnostics.Process.GetCurrentProcess().Id.ToString())
                    + " " + string.Join(" ", dopeArguments),
            }.Silence());
        }

        public static string Quote(string argument) {
            // M$ arguments allow anything but " between quotes.
            // " is escaped by doubling: "fo"obar"" => """fo""obar"""""
            // Source: http://stackoverflow.com/questions/2393384/escape-string-for-process-start/2393537#2393537

            return string.Format("\"{0}\"", argument.Replace("\"", "\"\""));
        }

        public static string Quote(params string[] arguments) {
            return Quote((IEnumerable<string>) arguments);
        }

        public static string Quote(params object[] arguments) {
            return Quote(FlattenArguments(arguments));
        }

        private static IEnumerable<string> FlattenArguments(IEnumerable args) {
            foreach (var arg in args) {
                var argString = arg as string;

                if (argString != null) {
                    yield return argString;

                    continue;
                }

                var argEnumerable = arg as IEnumerable;

                if (argEnumerable != null) {
                    foreach (var subarg in FlattenArguments(argEnumerable)) {
                        yield return subarg;
                    }

                    continue;
                }

                throw new ArgumentException("Arguments must be of type string or an IEnumerable of string");
            }
        }

        public static string Quote(IEnumerable<string> arguments) {
            return string.Join(" ", arguments.Select(Quote));
        }
    }
}