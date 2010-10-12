using System.IO;

namespace NoCap.Web {
    public interface IMultipartEntry {
        void WriteHeaders(Stream stream);
        void WriteContents(Stream stream);
    }
}