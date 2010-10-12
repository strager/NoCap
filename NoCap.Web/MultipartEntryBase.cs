using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace NoCap.Web {
    public abstract class MultipartEntryBase : IMultipartEntry {
        public abstract ICollection<IMultipartHeader> Headers {
            get;
        }

        private string GetBoundaryString() {
            var headers = Headers;

            if (headers == null) {
                return null;
            }

            var contentTypeHeaders = headers.Where((header) =>
                string.Compare(header.Name, "content-type", StringComparison.InvariantCultureIgnoreCase) == 0 &&
                header.Properties != null &&
                header.Properties.ContainsKey("boundary")
            );

            return contentTypeHeaders.Any() ?
                contentTypeHeaders.First().Properties["boundary"] :
                null;
        }

        public void WriteHeaders(Stream stream) {
            if (Headers == null) {
                return;
            }

            // Do not dispose (because it closes the stream)
            var writer = new StreamWriter(stream, Util.Encoding);

            foreach (var header in Headers) {
                header.Write(writer);
                writer.Write(Util.LineSeparator);
            }

            writer.Flush();
        }

        public abstract void WriteContents(Stream stream);
    }
}