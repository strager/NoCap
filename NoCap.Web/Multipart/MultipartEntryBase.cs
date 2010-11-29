using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace NoCap.Web.Multipart {
    public abstract class MultipartEntryBase : IMultipartEntry {
        public abstract ICollection<IMultipartHeader> Headers {
            get;
        }

        public void WriteHeaders(Stream stream) {
            WriteHeaders(stream, CancellationToken.None);
        }

        public void WriteHeaders(Stream stream, CancellationToken cancelToken) {
            if (Headers == null) {
                return;
            }

            // Do not dispose (because it closes the stream)
            var writer = new StreamWriter(stream, Utility.Encoding);

            foreach (var header in Headers) {
                cancelToken.ThrowIfCancellationRequested();

                header.Write(writer);

                cancelToken.ThrowIfCancellationRequested();

                writer.Write(Utility.LineSeparator);
            }

            cancelToken.ThrowIfCancellationRequested();

            writer.Flush();
        }

        public abstract void WriteContents(Stream stream);
        public abstract void WriteContents(Stream stream, CancellationToken cancelToken);

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