using System.IO;

namespace NoCap.Web.Multipart {
    public interface IMultipartEntry {
        void WriteHeaders(Stream stream);
        void WriteContents(Stream stream);

        long GetHeadersByteCount();
        long GetContentsByteCount();
    }
}