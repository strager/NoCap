using System.ComponentModel.Composition;
using NoCap.Library;
using NoCap.Library.Editors;
using NoCap.Library.Imaging;
using NoCap.Plugins.Commands;

namespace NoCap.Plugins.Factories {
    [Export(typeof(ICommandFactory))]
    [PreferredCommandFactory(CommandFeatures.ImageUploader)]
    class ImageBinUploaderFactory : ICommandFactory {
        public string Name {
            get { return "ImageBin.ca uploader"; }
        }

        public ICommand CreateCommand() {
            return new ImageBinUploader(new ImageWriter(new PngBitmapCodec()));
        }

        public void PopulateCommand(ICommand command, IInfoStuff infoStuff) {
            // Do nothing.
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
