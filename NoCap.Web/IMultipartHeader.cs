using System.Collections.Generic;
using System.IO;

namespace NoCap.Web {
    public interface IMultipartHeader {
        string Name { get; }
        string Value { get; }
        IDictionary<string, string> Properties { get; }

        void Write(TextWriter writer);
        long GetByteCount();
    }
}