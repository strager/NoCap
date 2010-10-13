using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace NoCap.Web.Multipart {
    public class MultipartEntry : MultipartEntryBase {
        private readonly ICollection<IMultipartHeader> headers = new HashSet<IMultipartHeader>();

        public override ICollection<IMultipartHeader> Headers {
            get {
                return this.headers;
            }
        }

        public MultipartEntry() {
        }

        public MultipartEntry(IEnumerable<IMultipartHeader> headers) {
            this.headers = headers.ToList();
        }

        public override void WriteContents(Stream stream) {
            // Do nothing
        }

        public override long GetContentsByteCount() {
            throw new NotImplementedException();
        }
    }
}
