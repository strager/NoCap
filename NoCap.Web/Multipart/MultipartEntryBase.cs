using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace NoCap.Web.Multipart {
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

        public long GetHeadersByteCount() {
            long separatorLength = Utility.Encoding.GetByteCount(Utility.LineSeparator);

            return Headers.AsParallel().Aggregate((long) 0, (count, header) => {
                count += header.GetByteCount();
                count += separatorLength;

                return count;
            });
        }

        public abstract long GetContentsByteCount();
    }
}