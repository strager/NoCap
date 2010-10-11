using System;
using System.IO;

namespace NoCap.Destinations {
    public class FileSystemDestination : IDestination {
        private readonly string rootPath;

        public FileSystemDestination(string rootPath) {
            this.rootPath = rootPath;
        }

        public IOperation<TypedData> Put(TypedData data) {
            switch (data.Type) {
                case TypedDataType.RawData:
                    return new EasyOperation<TypedData>((op) => {
                        string path = Path.Combine(this.rootPath, data.Name);

                        var file = File.Open(path, FileMode.Create, FileAccess.Write);

                        try {
                            var rawData = (byte[])data.Data;

                            // TODO Async
                            file.BeginWrite(rawData, 0, rawData.Length, (asyncResult) => {
                                file.EndWrite(asyncResult);

                                file.Dispose();

                                op.Done(TypedData.FromUri(path, "output file"));
                            }, null);
                        } catch (Exception) {
                            file.Dispose();

                            throw;
                        }

                        return null;
                    });

                default:
                    return null;
            }
        }
    }
}
