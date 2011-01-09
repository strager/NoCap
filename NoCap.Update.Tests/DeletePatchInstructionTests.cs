using System;
using System.IO;
using NUnit.Framework;

namespace NoCap.Update.Tests {
    [TestFixture]
    public class DeletePatchInstructionTests : PatchInstructionTestsBase {
        [Test]
        public void ApplyThrowsOnBadPath() {
            var instruction = new DeletePatchInstruction {
                Path = "File1.txt",
            };

            Assert.Throws<FileNotFoundException>(
                                                 () => instruction.Apply(PatchDataRoot, ApplicationRoot)
                );
        }

        [Test]
        public void ApplyDeletes() {
            using (var writer = File.CreateText(Path.Combine(ApplicationRoot, "File1.txt"))) {
                writer.Write("Delete me");
            }

            var instruction = new DeletePatchInstruction {
                Path = "File1.txt",
            };

            instruction.Apply(PatchDataRoot, ApplicationRoot);

            Assert.IsFalse(File.Exists(Path.Combine(ApplicationRoot, "File1.txt")));
        }

        [Test]
        public void ApplyThrowsOnDirectoryPath() {
            Directory.CreateDirectory(Path.Combine(ApplicationRoot, "dir"));

            var instruction = new DeletePatchInstruction {
                Path = "dir",
            };

            Assert.Throws<InvalidOperationException>(
                                                     () => instruction.Apply(PatchDataRoot, ApplicationRoot)
                );
        }

        [Test]
        public void NullPathThrows() {
            var instruction = new DeletePatchInstruction {
                Path = null,
            };

            Assert.Throws<InvalidOperationException>(
                                                     () => instruction.Apply(PatchDataRoot, ApplicationRoot)
                );
        }
    }
}