using System.IO;

namespace NoCap.Destinations {
    public class FileSystemDestination : IDestination {
        private readonly string rootPath;

        public FileSystemDestination(string rootPath) {
            this.rootPath = rootPath;
        }

        public IOperation Put(TypedData data) {
            switch (data.Type) {
                case TypedDataType.RawData:
                    return new EasyOperation((op) => {
                        string path = Path.Combine(this.rootPath, data.Name);

                        using (var file = File.Open(path, FileMode.CreateNew, FileAccess.Write)) {
                            var rawData = (byte[])data.Data;

                            // TODO Async
                            file.Write(rawData, 0, rawData.Length);
                        }

                        return TypedData.FromUri(path, "output file");
                    });

                default:
                    return null;
            }
        }
    }
}
