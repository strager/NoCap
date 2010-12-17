using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;

namespace NoCap.Web.Multipart {
    public class MultipartData {
        private readonly IEnumerable<IMultipartEntry> entries;
        private readonly string boundary;

        public IEnumerable<IMultipartEntry> Entries {
            get {
                return this.entries;
            }
        }

        public string Boundary {
            get {
                return this.boundary;
            }
        }

        public MultipartData(IEnumerable<IMultipartEntry> entries) :
            this(entries, Utility.GetRandomBoundary()) {
        }

        public MultipartData(IEnumerable<IMultipartEntry> entries, string boundary) {
            this.entries = new ReadOnlyCollection<IMultipartEntry>(entries.ToList());
            this.boundary = boundary;
        }

        public long GetByteCount() {
            long separatorLength = Utility.Encoding.GetByteCount(Utility.LineSeparator);
            long boundaryLength = Utility.GetBoundaryByteCount(Boundary);

            return Entries.AsParallel().Aggregate((long) 0, (count, entry) => {
                count += entry.GetHeadersByteCount();
                count += separatorLength;
                count += entry.GetContentsByteCount();
                count += separatorLength;
                count += boundaryLength;

                return count;
            });
        }

        public void Write(Stream stream) {
            var separator = Utility.Encoding.GetBytes(Utility.LineSeparator);

            foreach (var entry in Entries) {
                entry.WriteHeaders(stream);
                stream.Write(separator, 0, separator.Length);

                entry.WriteContents(stream);
                stream.Write(separator, 0, separator.Length);

                Utility.WriteBoundary(stream, Boundary);
            }
        }

        public void Write(Stream stream, CancellationToken cancelToken) {
            var separator = Utility.Encoding.GetBytes(Utility.LineSeparator);

            foreach (var entry in Entries) {
                cancelToken.ThrowIfCancellationRequested();

                entry.WriteHeaders(stream, cancelToken);
                stream.Write(separator, 0, separator.Length);

                cancelToken.ThrowIfCancellationRequested();

                entry.WriteContents(stream, cancelToken);
                stream.Write(separator, 0, separator.Length);

                cancelToken.ThrowIfCancellationRequested();

                Utility.WriteBoundary(stream, Boundary);
            }
        }
    }
}