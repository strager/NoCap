using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Xml;

namespace NoCap.Update {
    public class PatchInfo {
        public IEnumerable<IPatchInstruction> Instructions {
            get;
            private set;
        }

        internal readonly string patchDataRoot;

        private PatchInfo(string patchDataRoot) {
            this.patchDataRoot = patchDataRoot;
        }

        public static PatchInfo LoadFrom(string patchDataRoot) {
            var config = new XmlDocument();
            config.Load(Path.Combine(patchDataRoot, "nocap.xml"));

            var patch = config.DocumentElement;

            return new PatchInfo(patchDataRoot) {
                Instructions = new ReadOnlyCollection<IPatchInstruction>(
                    patch.SelectNodes("Instructions/*").OfType<XmlElement>().Select(ParseInstruction).ToArray()
                ),
            };
        }

        private static IPatchInstruction ParseInstruction(XmlElement root) {
            switch (root.LocalName) {
                case "Add":
                    return AddPatchInstruction.ParseFrom(root);

                case "Delete":
                    return DeletePatchInstruction.ParseFrom(root);

                case "Move":
                    return MovePatchInstruction.ParseFrom(root);

                default:
                    throw new InvalidOperationException(string.Format("Unknown instruction: {0}", root.LocalName));
            }
        }
    }
}