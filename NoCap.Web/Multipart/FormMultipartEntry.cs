using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;

namespace NoCap.Web.Multipart {
    public class FormMultipartEntry : MultipartEntryBase {
        public string Name {
            get;
            private set;
        }

        public string Value {
            get;
            private set;
        }

        public Encoding ValueEncoding {
            get;
            private set;
        }

        public override ICollection<IMultipartHeader> Headers {
            get {
                return new[] {
                    new MultipartHeader("Content-Disposition", "form-data", new Dictionary<string, string> {
                        { "name", Name }
                    }),
                    new MultipartHeader("Content-Type", "text/plain", new Dictionary<string, string> {
                        { "charset", ValueEncoding.WebName }
                    })
                };
            }
        }

        public FormMultipartEntry(string name, string value) :
            this(name, value, Encoding.ASCII) {
        }

        public FormMultipartEntry(string name, string value, Encoding valueEncoding) {
            if (valueEncoding == null) {
                throw new ArgumentNullException("valueEncoding");
            }

            Name = name;
            Value = value;
            ValueEncoding = valueEncoding;
        }

        public override void WriteContents(Stream stream) {
            WriteContents(stream, CancellationToken.None);
        }

        public override void WriteContents(Stream stream, CancellationToken cancelToken) {
            // Do not dispose (because it closes the stream)
            var writer = new StreamWriter(stream, ValueEncoding);
            
            cancelToken.ThrowIfCancellationRequested();

            writer.Write(Value);
            
            cancelToken.ThrowIfCancellationRequested();

            writer.Flush();
        }

        public override long GetContentsByteCount() {
            return ValueEncoding.GetByteCount(Value);
        }
    }
}