using System;
using System.Collections.Generic;
using System.IO;
using NoCap.Library;
using NoCap.Library.Util;
using NoCap.Plugins.Factories;

namespace NoCap.Plugins.Commands {
    public class FileSystem : ICommand {
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
                case TypedDataType.RawData:
                    string path = Path.Combine(RootPath, data.Name);

                    using (var file = File.Open(path, FileMode.Create, FileAccess.Write)) {
                        var rawData = (byte[]) data.Data;

                        file.Write(rawData, 0, rawData.Length);

                        var uriBuilder = new UriBuilder {
                            Scheme = Uri.UriSchemeFile,
                            Path = path
                        };

                        progress.Progress = 1;  // TODO File reading progress (?)

                        return TypedData.FromUri(uriBuilder.Uri, "output file");
                    }

                default:
                    return null;
            }
        }

        public IEnumerable<TypedDataType> GetInputDataTypes() {
            return new[] { TypedDataType.RawData };
        }

        public IEnumerable<TypedDataType> GetOutputDataTypes(TypedDataType input) {
            return new[] { TypedDataType.Uri };
        }

        public ICommandFactory GetFactory() {
            return new FileSystemFactory();
        }
    }
}