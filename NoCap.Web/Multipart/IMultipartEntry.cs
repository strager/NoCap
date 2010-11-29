using System.IO;
using System.Threading;

namespace NoCap.Web.Multipart {
    public interface IMultipartEntry {
        void WriteHeaders(Stream stream);
        void WriteHeaders(Stream stream, CancellationToken cancelToken);

        void WriteContents(Stream stream);
        void WriteContents(Stream stream, CancellationToken cancelToken);

        long GetHeadersByteCount();
        long GetContentsByteCount();
    }
}