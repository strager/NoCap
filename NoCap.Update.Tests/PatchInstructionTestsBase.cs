using System.IO;
using NoCap.Library.Util;
using NUnit.Framework;

namespace NoCap.Update.Tests {
    public class PatchInstructionTestsBase {
        public string ApplicationRoot {
            get;
            private set;
        }

        public string PatchDataRoot {
            get;
            private set;
        }

        [SetUp]
        public void CreateDirectories() {
            ApplicationRoot = FileSystem.GetTempDirectory();
            PatchDataRoot = Path.Combine(Directory.GetCurrentDirectory(), "patchData");
        }

        [TearDown]
        public void DeleteDirectories() {
            Directory.Delete(ApplicationRoot, true);
        }
    }
}