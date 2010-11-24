using System.ComponentModel.Composition;
using NoCap.Library;
using NoCap.Plugins.Commands;
using NoCap.Plugins.Editors;

namespace NoCap.Plugins.Factories {
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