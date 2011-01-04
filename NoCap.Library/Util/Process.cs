using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;

namespace NoCap.Library.Util {
    public static class Process {
        public static void StartOnProcessExit(ProcessStartInfo startInfo) {
            System.Diagnostics.Process.GetCurrentProcess().Exited +=
                (sender, e) => System.Diagnostics.Process.Start(startInfo);
        }

        public static ProcessStartInfo Silence(this ProcessStartInfo startInfo) {
            startInfo.ErrorDialog = false;
            startInfo.UseShellExecute = false;
            startInfo.CreateNoWindow = true;

            return startInfo;
        }

        public static void DOPE(string arguments) {
            // FIXME Better way to get EXE path
            string dopePath = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "DOPE.exe");

            System.Diagnostics.Process.Start(new ProcessStartInfo {
                FileName = dopePath,
                Arguments = arguments,
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