using System.ComponentModel.Composition;
using NoCap.Library;
using NoCap.Library.Editors;
using NoCap.Library.Imaging;
using NoCap.Plugins.Commands;
using NoCap.Plugins.Editors;

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

        public void PopulateCommand(ICommand command, ICommandProvider commandProvider) {
            // Do nothing.
        }

        public ICommandEditor GetCommandEditor(ICommandProvider commandProvider) {
            return new ImageBinUploaderEditor();
        }

        public CommandFeatures CommandFeatures {
            get {
                return CommandFeatures.ImageUploader;
            }
        }
    }
}
