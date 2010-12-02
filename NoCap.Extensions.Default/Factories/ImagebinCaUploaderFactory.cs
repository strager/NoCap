using System.ComponentModel.Composition;
using NoCap.Extensions.Default.Commands;
using NoCap.Extensions.Default.Editors;
using NoCap.Library;
using NoCap.Library.Imaging;

namespace NoCap.Extensions.Default.Factories {
    [Export(typeof(ICommandFactory))]
    [PreferredCommandFactory(CommandFeatures.ImageUploader)]
    class ImagebinCaUploaderFactory : ICommandFactory {
        public string Name {
            get { return "Imagebin.ca uploader"; }
        }

        public ICommand CreateCommand() {
            return new ImagebinCaUploader(new ImageWriter { Codec = new PngBitmapCodec() });
        }

        public void PopulateCommand(ICommand command, ICommandProvider commandProvider) {
            // Do nothing.
        }

        public ICommandEditor GetCommandEditor(ICommandProvider commandProvider) {
            return new ImagebinCaUploaderEditor();
        }

        public CommandFeatures CommandFeatures {
            get {
                return CommandFeatures.ImageUploader;
            }
        }
    }
}
