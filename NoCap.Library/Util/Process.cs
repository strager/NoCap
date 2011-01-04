using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace NoCap.Library.Util {
    public static class Process {
        public static void StartOnProcessExit(ProcessStartInfo startInfo) {
            StartOnProcessExit(startInfo, false);
        }

        public static void StartOnProcessExit(ProcessStartInfo startInfo, bool isSilent) {
            System.Diagnostics.Process.GetCurrentProcess().Exited += (sender, e) => {
                if (isSilent) {
                    startInfo.ErrorDialog = false;
                    startInfo.UseShellExecute = false;
                    startInfo.CreateNoWindow = true;
                }

                System.Diagnostics.Process.Start(startInfo);
            };
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
            var argumentStrings = new List<string>();

            foreach (var arg in arguments) {
                var argString = arg as string;

                if (argString != null) {
                    argumentStrings.Add(argString);

                    continue;
                }

                var argEnumerable = arg as IEnumerable<string>;

                if (argEnumerable != null) {
                    argumentStrings.AddRange(argEnumerable);

                    continue;
                }

                throw new ArgumentException("Arguments must be of type string or IEnumerable<string>");
            }

            return Quote(argumentStrings);
        }

        public static string Quote(IEnumerable<string> arguments) {
            return string.Join(" ", arguments.Select(Quote));
        }
    }
}