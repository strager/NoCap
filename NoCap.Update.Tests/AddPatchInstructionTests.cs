using System;
using System.IO;
using NUnit.Framework;

namespace NoCap.Update.Tests {
    [TestFixture]
    public class AddPatchInstructionTests : PatchInstructionTestsBase {
        [Test]
        public void ApplyCopiesFile() {
            var instruction = new AddPatchInstruction {
                Source = "File1.txt",
                Destination = "File1.txt",
            };

            instruction.Apply(PatchDataRoot, ApplicationRoot);

            FileAssert.AreEqual(
                Path.Combine(PatchDataRoot, "File1.txt"),
                Path.Combine(ApplicationRoot, "File1.txt")
            );
        }

        [Test]
        public void ApplyCopiesAndRenamesFile() {
            var instruction = new AddPatchInstruction {
                Source = "File1.txt",
                Destination = "File2.txt",
            };

            instruction.Apply(PatchDataRoot, ApplicationRoot);

            FileAssert.AreEqual(
                Path.Combine(PatchDataRoot, "File1.txt"),
                Path.Combine(ApplicationRoot, "File2.txt")
            );
        }

        [Test]
        public void ApplyDoesNotOverrideFile() {
            using (var file = File.OpenWrite(Path.Combine(ApplicationRoot, "File1.txt")))
            using (var writer = new StreamWriter(file)) {
                writer.Write("Override me!");
            }

            var instruction = new AddPatchInstruction {
                Source = "File1.txt",
                Destination = "File1.txt",
            };

            Assert.Throws<InvalidOperationException>(
                () => instruction.Apply(PatchDataRoot, ApplicationRoot)
            );
        }

        [Test]
        public void ApplyThrowsOnBadSource() {
            var instruction = new AddPatchInstruction {
                Source = "File3.txt",
                Destination = "File1.txt",
            };

            Assert.Throws<FileNotFoundException>(
                () => instruction.Apply(PatchDataRoot, ApplicationRoot)
            );
        }

        [Test]
        public void ApplyCopiesAndCreatesDirectory() {
            var instruction = new AddPatchInstruction {
                Source = "File1.txt",
                Destination = "dir/File2.txt",
            };

            instruction.Apply(PatchDataRoot, ApplicationRoot);

            FileAssert.AreEqual(
                Path.Combine(PatchDataRoot, "File1.txt"),
                Path.Combine(ApplicationRoot, "dir", "File2.txt")
            );
        }

        [Test]
        public void NoSourceThrows() {
            var instruction = new AddPatchInstruction {
                Source = null,
                Destination = "out",
            };

            Assert.Throws<InvalidOperationException>(
                () => instruction.Apply(PatchDataRoot, ApplicationRoot)
            );
        }

        [Test]
        public void NoDestinationThrows() {
            var instruction = new AddPatchInstruction {
                Source = "in",
                Destination = null,
            };

            Assert.Throws<InvalidOperationException>(
                () => instruction.Apply(PatchDataRoot, ApplicationRoot)
            );
        }

        [Test]
        public void MaliciousFileThrows() {
            AddPatchInstruction instruction;

            instruction = new AddPatchInstruction {
                Source = "../evil_input",
                Destination = "out",
            };

            Assert.Throws<InvalidOperationException>(
                () => instruction.Apply(PatchDataRoot, ApplicationRoot)
            );

            instruction = new AddPatchInstruction {
                Source = "/evil_input",
                Destination = "out",
            };

            Assert.Throws<InvalidOperationException>(
                () => instruction.Apply(PatchDataRoot, ApplicationRoot)
            );

            instruction = new AddPatchInstruction {
                Source = "/evil_input",
                Destination = "out",
            };

            Assert.Throws<InvalidOperationException>(
                () => instruction.Apply(PatchDataRoot, ApplicationRoot)
            );

            instruction = new AddPatchInstruction {
                Source = "in",
                Destination = "\\evil output",
            };

            Assert.Throws<InvalidOperationException>(
                () => instruction.Apply(PatchDataRoot, ApplicationRoot)
            );

            instruction = new AddPatchInstruction {
                Source = "in",
                Destination = "c:\\x",
            };

            Assert.Throws<InvalidOperationException>(
                () => instruction.Apply(PatchDataRoot, ApplicationRoot)
            );

            instruction = new AddPatchInstruction {
                Source = "in",
                Destination = "c:x",
            };

            Assert.Throws<InvalidOperationException>(
                () => instruction.Apply(PatchDataRoot, ApplicationRoot)
            );
        }
    }
}
