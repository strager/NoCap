using System.ComponentModel.Composition;
using System.Drawing.Imaging;
using System.Linq;
using System.Windows.Controls;
using NoCap.Library;
using NoCap.Library.Processors;
using NoCap.Plugins.Processors;

namespace NoCap.Plugins.Factories {
    [Export(typeof(IProcessorFactory))]
    class ImageBinUploaderFactory : IProcessorFactory {
        public string Name {
            get { return "ImageBin.ca uploader"; }
        }

        public IProcessor CreateProcessor() {
            // FIXME Hack

            var imageCodecs = ImageCodecInfo.GetImageEncoders().Where(ImageWriter.IsCodecValid);

            return new ImageBinUploader(
                new ImageWriter(imageCodecs.FirstOrDefault(codec => codec.FormatDescription == "PNG"))
            );
        }

        public ContentControl GetProcessorEditor(IProcessor processor) {
            return null;
        }
    }
}
