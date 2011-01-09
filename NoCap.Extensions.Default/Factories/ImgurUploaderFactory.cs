using System.ComponentModel.Composition;
using NoCap.Extensions.Default.Commands;
using NoCap.Library;
using NoCap.Library.Imaging;

namespace NoCap.Extensions.Default.Factories {
    [Export(typeof(ICommandFactory))]
    [PreferredCommandFactory(CommandFeatures.ImageUploader)]
    sealed class ImgurUploaderFactory : ICommandFactory {
        public string Name {
            get {
                return "imgur uploader";
            }
        }

        public ICommand CreateCommand() {
            return new ImgurUploader(new ImageWriter { Codec = new PngBitmapCodec() });
        }

        public void PopulateCommand(ICommand command, ICommandProvider commandProvider) {
        }

        public ICommandEditor GetCommandEditor(ICommandProvider commandProvider) {
            return null;
        }

        public CommandFeatures CommandFeatures {
            get {
                return CommandFeatures.ImageUploader;
            }
        }
    }
}