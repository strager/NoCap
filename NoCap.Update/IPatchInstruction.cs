using System;
using System.IO;
using System.Xml;
using NoCap.Library.Util;

namespace NoCap.Update {
    public interface IPatchInstruction {
        void Apply(string patchDataRoot, string applicationRoot);
    }

    public class AddPatchInstruction : IPatchInstruction {
        public static AddPatchInstruction ParseFrom(XmlElement root) {
            var pi = new AddPatchInstruction();

            var sourceNode = root.SelectSingleNode("Source");
            var destinationNode = root.SelectSingleNode("Destination");

            if (sourceNode != null) {
                pi.Source = sourceNode.InnerText;
            }

            if (destinationNode != null) {
                pi.Destination = destinationNode.InnerText;
            }

            return pi;
        }

        public void Apply(string patchDataRoot, string applicationRoot) {
            if (Source == null || Destination == null) {
                throw new InvalidOperationException("Add instruction must have a source and a destination");
            }

            if (!FileSystem.IsSafePath(Source) || !FileSystem.IsSafePath(Destination)) {
                throw new InvalidOperationException("Paths must be relative to and descend from the current directory");
            }

            string sourceFile = Path.Combine(patchDataRoot, Source);
            string destinationFile = Path.Combine(applicationRoot, Destination);

            if (!File.Exists(sourceFile)) {
                throw new FileNotFoundException(string.Format("Source file '{0}' does not exist", sourceFile));
            }

            if (File.Exists(destinationFile)) {
                throw new InvalidOperationException(string.Format("Destination file '{0}' already exists", destinationFile));
            }

            if (!Directory.Exists(Path.GetDirectoryName(destinationFile))) {
                Directory.CreateDirectory(Path.GetDirectoryName(destinationFile));
            }

            File.Copy(sourceFile, destinationFile, false);
        }

        public string Source {
            get;
            set;
        }

        public string Destination {
            get;
            set;
        }
    }

    public class DeletePatchInstruction : IPatchInstruction {
        public static DeletePatchInstruction ParseFrom(XmlElement root) {
            var pi = new DeletePatchInstruction();

            var pathNode = root.SelectSingleNode("Path");

            if (pathNode != null) {
                pi.Path = pathNode.InnerText;
            }

            return pi;
        }

        public void Apply(string patchDataRoot, string applicationRoot) {
            if (Path == null) {
                throw new InvalidOperationException("Delete instruction must have a path to a file to delete");
            }

            if (!FileSystem.IsSafePath(Path)) {
                throw new InvalidOperationException("Paths must be relative to and descend from the current directory");
            }

            var file = System.IO.Path.Combine(applicationRoot, Path);

            if (Directory.Exists(file)) {
                throw new InvalidOperationException(string.Format("Cannot delete a directory '{0}'", file));
            }

            if (!File.Exists(file)) {
                throw new FileNotFoundException(string.Format("File '{0}' does not exist", file));
            }

            File.Delete(file);
        }

        public string Path {
            get;
            set;
        }
    }

    public class MovePatchInstruction : IPatchInstruction {
        public static MovePatchInstruction ParseFrom(XmlElement root) {
            throw new NotImplementedException();
        }

        public void Apply(string patchDataRoot, string applicationRoot) {
            throw new NotImplementedException();
        }
    }
}