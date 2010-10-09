using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;

namespace NoCap.Destinations {
    public class FileSystemDestination : IImageDestination {
        private readonly string rootPath;

        public FileSystemDestination(string rootPath) {
            this.rootPath = rootPath;
        }

        public void PutImage(Image image, string name, IResultThing result) {
            string path = Path.Combine(this.rootPath, name + ".bmp");
            result.Start();

            image.Save(path, ImageFormat.Bmp);

            result.Done(path);
        }
    }
}
