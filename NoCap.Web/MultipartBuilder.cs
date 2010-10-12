﻿using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace NoCap.Web {
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
            Boundary = Util.GetRandomBoundary();
        }
        
        public MultipartBuilder(IEnumerable<IMultipartEntry> entries) {
            Entries = entries.ToList();
            Boundary = Util.GetRandomBoundary();
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

        public MultipartBuilder Stream(Stream stream, string name = null, string fileName = null) {
            Entries.Add(new FileMultipartEntry(stream, name) {
                FileName = fileName
            });

            return this;
        }

        public void Write(Stream stream) {
            foreach (var entry in Entries) {
                var separator = Util.Encoding.GetBytes(Util.LineSeparator);

                entry.WriteHeaders(stream);
                stream.Write(separator, 0, separator.Length);

                entry.WriteContents(stream);
                stream.Write(separator, 0, separator.Length);

                Util.WriteBoundary(stream, Boundary);
            }
        }
    }
}