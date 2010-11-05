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

        public ICommand CreateCommand(IInfoStuff infoStuff) {
            return new ImageFileUploader();
        }

        public ICommandEditor GetCommandEditor(ICommand command, IInfoStuff infoStuff) {
            return new ImageFileUploaderEditor((ImageFileUploader) command, infoStuff);
        }

        public CommandFeatures CommandFeatures {
            get {
                return CommandFeatures.ImageUploader;
            }
        }
    }
}