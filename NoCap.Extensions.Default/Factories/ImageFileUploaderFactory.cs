using System.ComponentModel.Composition;
using NoCap.Extensions.Default.Commands;
using NoCap.Extensions.Default.Editors;
using NoCap.Library;
using NoCap.Library.Imaging;

namespace NoCap.Extensions.Default.Factories {
    [Export(typeof(ICommandFactory))]
    public class ImageFileUploaderFactory : ICommandFactory {
        public string Name {
            get {
                return "Image file uploader";
            }
        }

        public ICommand CreateCommand() {
            return new ImageFileUploader();
        }

        public void PopulateCommand(ICommand command, ICommandProvider commandProvider) {
            var uploader = (ImageFileUploader) command;
            uploader.FileUploader = commandProvider.GetDefaultCommand(CommandFeatures.FileUploader);
            uploader.ImageWriter.Codec = new PngBitmapCodec();
        }

        public ICommandEditor GetCommandEditor(ICommandProvider commandProvider) {
            return new ImageFileUploaderEditor();
        }

        public CommandFeatures CommandFeatures {
            get {
                return CommandFeatures.ImageUploader;
            }
        }
    }
}