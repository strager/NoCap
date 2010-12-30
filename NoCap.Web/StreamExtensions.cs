using System.IO;
using System.Threading;

namespace NoCap.Web {
    public static class StreamExtensions {
        public static void CopyTo(this Stream inputStream, Stream outputStream, CancellationToken cancelToken) {
            inputStream.CopyTo(outputStream, 1024, cancelToken);
        }

        public static void CopyTo(this Stream inputStream, Stream outputStream, int bufferSize, CancellationToken cancelToken) {
            byte[] buffer = new byte[bufferSize];

            while (true) {
                cancelToken.ThrowIfCancellationRequested();

                int readBytes = inputStream.Read(buffer, 0, bufferSize);

                if (readBytes == 0) {
                    break;
                }

                cancelToken.ThrowIfCancellationRequested();

                outputStream.Write(buffer, 0, readBytes);
            }
        }
    }
}