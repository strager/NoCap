using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace NoCap.Web.Multipart {
    public class MultipartDataBuilder {
        private readonly List<IMultipartEntry> entries;

        public ICollection<IMultipartEntry> Entries {
            get {
                return this.entries;
            }
        }

        public MultipartDataBuilder() {
            this.entries = new List<IMultipartEntry>();
        }
        
        public MultipartDataBuilder(IEnumerable<IMultipartEntry> entries) {
            if (entries == null) {
                throw new ArgumentNullException("entries");
            }

            this.entries = entries.ToList();
        }

        public MultipartDataBuilder KeyValuePair(string name, string value) {
            Entries.Add(new FormMultipartEntry(name, value));

            return this;
        }

        public MultipartDataBuilder KeyValuePairs(IDictionary<string, string> pairs) {
            return pairs.Aggregate(
                this,
                (current, pair) => current.KeyValuePair(pair.Key, pair.Value)
            );
        }

        public MultipartDataBuilder File(Stream stream, string name = null, string fileName = null) {
            Entries.Add(new FileMultipartEntry(stream, name) {
                FileName = fileName
            });

            return this;
        }

        public MultipartDataBuilder Data(MultipartData data) {
            this.entries.AddRange(data.Entries);

            return this;
        }

        public MultipartData GetData() {
            return new MultipartData(Entries);
        }

        public MultipartData GetData(string boundary) {
            return new MultipartData(Entries, boundary);
        }
    }
}
