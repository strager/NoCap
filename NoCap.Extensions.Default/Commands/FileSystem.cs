using System;
using System.Collections.Generic;
using System.IO;
using NoCap.Extensions.Default.Factories;
using NoCap.Library;
using NoCap.Library.Util;

namespace NoCap.Extensions.Default.Commands {
    [Serializable]
    public sealed class FileSystem : ICommand {
        public string Name {
            get { return "File system"; }
        }

        public string RootPath {
            get;
            set;
        }

        public FileSystem() {
        }

        public FileSystem(string rootPath) {
            RootPath = rootPath;
        }

        public TypedData Process(TypedData data, IMutableProgressTracker progress) {
            switch (data.DataType) {
                case TypedDataType.Stream:
                    string path = Path.Combine(RootPath, data.Name);

                    using (var file = File.Open(path, FileMode.Create, FileAccess.Write)) {
                        var inputStream = (Stream) data.Data;

                        CopyStream(inputStream, file, progress);

                        var uriBuilder = new UriBuilder {
                            Scheme = Uri.UriSchemeFile,
                            Path = path
                        };

                        return TypedData.FromUri(uriBuilder.Uri, "output file");
                    }

                default:
                    return null;
            }
        }

        private static void CopyStream(Stream inputStream, Stream outputStream, IMutableProgressTracker progress) {
            long originalByteCount;
            long bytesProcessed = 0;

            try {
                originalByteCount = inputStream.Length;
            } catch (NotSupportedException) {
                // .Length is not supported; length is undefined
                originalByteCount = -1;
            }
                        
            const int bufferSize = 1024;
            byte[] readBuffer = new byte[bufferSize];

            while (true) {
                if (originalByteCount > 0) {
                    progress.Progress = (double) bytesProcessed / originalByteCount;
                }

                int bytesRead = inputStream.Read(readBuffer, 0, readBuffer.Length);

                outputStream.Write(readBuffer, 0, bytesRead);
                bytesProcessed += bufferSize;

                if (bytesRead < bufferSize) {
                    // We hit end-of-file
                    break;
                }
            }

            progress.Progress = 1;
        }

        public IEnumerable<TypedDataType> GetInputDataTypes() {
            return new[] { TypedDataType.Stream };
        }

        public ICommandFactory GetFactory() {
            return new FileSystemFactory();
        }

        public ITimeEstimate ProcessTimeEstimate {
            get {
                return TimeEstimates.Instantaneous;
            }
        }

        public bool IsValid() {
            return true;
        }
    }
}