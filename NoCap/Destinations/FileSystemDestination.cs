using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;

namespace NoCap.Destinations {
    public class FileSystemDestination : IDestination {
        private readonly string rootPath;

        public FileSystemDestination(string rootPath) {
            this.rootPath = rootPath;
        }

        public bool Put(DestinationType type, object data, string name, IResultThing result) {
            switch (type) {
                case DestinationType.Image:
                    string path = Path.Combine(this.rootPath, name + ".bmp");
                    result.Start();

                    ((Image)data).Save(path, ImageFormat.Bmp);

                    result.Done(path);

                    return true;

                default:
                    return false;
            }
        }
    }
}
