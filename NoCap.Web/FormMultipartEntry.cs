using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace NoCap.Web {
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

        public FormMultipartEntry(string name, string value, Encoding valueEncoding = null) {
            Name = name;
            Value = value;
            ValueEncoding = valueEncoding ?? Encoding.ASCII;
        }

        public override void WriteContents(Stream stream) {
            // Do not dispose (because it closes the stream)
            var writer = new StreamWriter(stream, ValueEncoding);

            writer.Write(Value);

            writer.Flush();
        }

        public override long GetContentsByteCount() {
            return ValueEncoding.GetByteCount(Value);
        }
    }
}