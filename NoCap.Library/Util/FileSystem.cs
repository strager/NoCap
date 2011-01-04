using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace NoCap.Library.Util {
    public static class FileSystem {
        public static string GetTempDirectory() {
            string directoryName = Path.GetRandomFileName();
            string directoryPath = Path.Combine(Path.GetTempPath(), directoryName);

            Directory.CreateDirectory(directoryPath);

            return directoryPath;
        }

        public static bool AreDirectoriesSame(string directoryA, string directoryB) {
            return directoryA == directoryB;
        }

        public static void DeleteLater(string path) {
            Process.StartOnProcessExit(new ProcessStartInfo {
                FileName = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "DOPE.exe"), // FIXME Better way to get EXE path
                Arguments = Process.Quote("--delete", "--", path),
            }.Silence());
        }

        public static bool IsSafePath(string path) {
            if (path == null) {
                throw new ArgumentNullException("path");
            }

            // TODO Make sure this is really safe
            path = path.Replace("/", "\\");

            return !path.StartsWith("\\")
                && !path.Contains(":")
                && !path.Contains("..\\")
                && !path.EndsWith("..");
        }
    }
}