using System;
using System.IO;

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
            Process.QueueDOPE(Process.Quote("--delete", path));
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