using System.Collections.Generic;
using System.IO;

namespace NoCap.Web {
    public abstract class MultipartEntryBase : IMultipartEntry {
        public abstract ICollection<IMultipartHeader> Headers {
            get;
        }

        public void WriteHeaders(Stream stream) {
            if (Headers == null) {
                return;
            }

            // Do not dispose (because it closes the stream)
            var writer = new StreamWriter(stream, Utility.Encoding);

            foreach (var header in Headers) {
                header.Write(writer);
                writer.Write(Utility.LineSeparator);
            }

            writer.Flush();
        }

        public abstract void WriteContents(Stream stream);
    }
}