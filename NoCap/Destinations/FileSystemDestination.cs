using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace NoCap.Destinations {
    public class FileSystemDestination : IDestination {
        private readonly string rootPath;

        public FileSystemDestination(string rootPath) {
            this.rootPath = rootPath;
        }

        public IOperation Put(TypedData data) {
            switch (data.Type) {
                case TypedDataType.Image:
                    return new EasyOperation((op) => {
                        string path = Path.Combine(this.rootPath, data.Name + ".bmp");

                        ((Image)data.Data).Save(path, ImageFormat.Bmp);

                        return TypedData.FromUri(path, "output file");
                    });

                default:
                    return null;
            }
        }
    }
}
