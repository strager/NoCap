using System.ComponentModel.Composition;
using NoCap.Library;
using NoCap.Library.Editors;
using NoCap.Library.Imaging;
using NoCap.Plugins.Commands;

namespace NoCap.Plugins.Factories {
    [Export(typeof(ICommandFactory))]
    class ImageBinUploaderFactory : ICommandFactory {
        public string Name {
            get { return "ImageBin.ca uploader"; }
        }

        public ICommand CreateCommand(IInfoStuff infoStuff) {
            return new ImageBinUploader(new ImageWriter { Codec = new PngBitmapCodec() });
        }

        public ICommandEditor GetCommandEditor(ICommand command, IInfoStuff infoStuff) {
            return new ImageWriterEditor(((ImageBinUploader) command).ImageWriter, infoStuff);
        }

        public CommandFeatures CommandFeatures {
            get {
                return CommandFeatures.ImageUploader;
            }
        }
    }
}
