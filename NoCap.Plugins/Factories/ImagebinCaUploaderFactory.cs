using System.ComponentModel.Composition;
using NoCap.Library;
using NoCap.Library.Editors;
using NoCap.Library.Imaging;
using NoCap.Plugins.Commands;
using NoCap.Plugins.Editors;

namespace NoCap.Plugins.Factories {
    [Export(typeof(ICommandFactory))]
    [PreferredCommandFactory(CommandFeatures.ImageUploader)]
    class ImagebinCaUploaderFactory : ICommandFactory {
        public string Name {
            get { return "Imagebin.ca uploader"; }
        }

        public ICommand CreateCommand() {
            return new ImagebinCaUploader(new ImageWriter(new PngBitmapCodec()));
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
