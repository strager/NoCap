using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace NoCap.Web.Multipart {
    public class FileMultipartEntry : MultipartEntryBase {
        public string Name {
            get;
            set;
        }

        public string FileName {
            get;
            set;
        }

        public Stream InputStream {
            get;
            set;
        }

        public string ContentType {
            get;
            set;
        }

        public override ICollection<IMultipartHeader> Headers {
            get {
                var parts = new Dictionary<string, string>();

                if (Name != null) {
                    parts["name"] = Name;
                }

                if (FileName != null) {
                    parts["filename"] = FileName;
                }

                return new[] {
                    new MultipartHeader("Content-Disposition", "file", parts),
                    new MultipartHeader("Content-Type", ContentType ?? "application/octet-stream", new Dictionary<string, string>())
                };
            }
        }

        public FileMultipartEntry(Stream inputStream, string name) :
            this(inputStream, name, null) {
        }

        public FileMultipartEntry(Stream inputStream, string name, string contentType) {
            Name = name;
            InputStream = inputStream;
            ContentType = contentType;
        }

        public override void WriteContents(Stream stream) {
            InputStream.CopyTo(stream);
        }

        public override void WriteContents(Stream stream, CancellationToken cancelToken) {
            const int bufferSize = 1024;

            byte[] buffer = new byte[bufferSize];

            while (true) {
                cancelToken.ThrowIfCancellationRequested();

                int readBytes = InputStream.Read(buffer, 0, bufferSize);

                if (readBytes == 0) {
                    break;
                }

                cancelToken.ThrowIfCancellationRequested();

                stream.Write(buffer, 0, readBytes);
            }
        }

        public override long GetContentsByteCount() {
            return InputStream.Length;
        }
    }
}