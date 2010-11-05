using System.Collections.Generic;
using System.IO;

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

        public FileMultipartEntry(Stream inputStream, string name, string contentType = null) {
            Name = name;
            InputStream = inputStream;
            ContentType = contentType;
        }

        public override void WriteContents(Stream stream) {
            // TODO .CopyTo
            byte[] readBuffer = new byte[1024];
            int bytesRead = InputStream.Read(readBuffer, 0, readBuffer.Length);

            while (bytesRead > 0) {
                stream.Write(readBuffer, 0, bytesRead);
                bytesRead = InputStream.Read(readBuffer, 0, readBuffer.Length);
            }
        }

        public override long GetContentsByteCount() {
            return InputStream.Length;
        }
    }
}