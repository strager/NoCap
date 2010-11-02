using System.ComponentModel.Composition;
using System.Drawing.Imaging;
using System.Linq;
using NoCap.Library;
using NoCap.Library.Editors;
using NoCap.Library.Commands;
using NoCap.Plugins.Commands;

namespace NoCap.Plugins.Factories {
    [Export(typeof(ICommandFactory))]
    class ImageBinUploaderFactory : ICommandFactory {
        public string Name {
            get { return "ImageBin.ca uploader"; }
        }

        public ICommand CreateCommand(IInfoStuff infoStuff) {
            // FIXME Hack

            var imageCodecs = ImageWriter.DefaultImageCodecs;

            return new ImageBinUploader(new ImageWriter(
                imageCodecs.FirstOrDefault(codec => codec.FormatDescription == "PNG") ?? imageCodecs.First()
            ));
        }

        public ICommandEditor GetCommandEditor(ICommand command, IInfoStuff infoStuff) {
            return new ImageWriterEditor(((ImageBinUploader) command).ImageWriter);
        }

        public CommandFeatures CommandFeatures {
            get {
                return CommandFeatures.ImageUploader;
            }
        }
    }
}
