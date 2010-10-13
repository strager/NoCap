using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace NoCap.Web.Multipart {
    public class MultipartBuilder {
        public ICollection<IMultipartEntry> Entries {
            get;
            private set;
        }

        public string Boundary {
            get;
            set;
        }

        public MultipartBuilder() {
            Entries = new List<IMultipartEntry>();
            Boundary = Utility.GetRandomBoundary();
        }
        
        public MultipartBuilder(IEnumerable<IMultipartEntry> entries) {
            Entries = entries.ToList();
            Boundary = Utility.GetRandomBoundary();
        }

        public MultipartBuilder KeyValuePair(string name, string value) {
            Entries.Add(new FormMultipartEntry(name, value));

            return this;
        }

        public MultipartBuilder KeyValuePairs(IDictionary<string, string> pairs) {
            return pairs.Aggregate(
                this,
                (current, pair) => current.KeyValuePair(pair.Key, pair.Value)
            );
        }

        public MultipartBuilder File(Stream stream, string name = null, string fileName = null) {
            Entries.Add(new FileMultipartEntry(stream, name) {
                FileName = fileName
            });

            return this;
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
    }
}
