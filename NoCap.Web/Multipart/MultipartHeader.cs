using System;
using System.Collections.Generic;
using System.IO;

namespace NoCap.Web.Multipart {
    public class MultipartHeader : IMultipartHeader {
        public string Name {
            get;
            private set;
        }

        public string Value {
            get;
            private set;
        }

        public IDictionary<string, string> Properties {
            get;
            private set;
        }

        public MultipartHeader(string name, string value, IDictionary<string, string> properties) {
            if (name == null) {
                throw new ArgumentNullException("name");
            }

            if (value == null) {
                throw new ArgumentNullException("value");
            }

            Name = name;
            Value = value;
            Properties = properties ?? new Dictionary<string, string>();
        }

        public void Write(TextWriter writer) {
            writer.Write("{0}: {1}", Name, Value);

            foreach (var property in Properties) {
                writer.Write("; ");
                writer.Write(KeyValuePair(property.Key, property.Value));
            }
        }

        public long GetByteCount() {
            using (var writer = new StringWriter()) {
                Write(writer);

                return Utility.Encoding.GetByteCount(writer.ToString());
            }
        }

        public static string KeyValuePair(string key, string value) {
            return string.Format("{0}={1}", key, value);
        }
    }
}