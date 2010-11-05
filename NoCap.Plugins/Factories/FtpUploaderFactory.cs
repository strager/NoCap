using System.ComponentModel.Composition;
using NoCap.Library;
using NoCap.Plugins.Commands;
using NoCap.Plugins.Editors;

namespace NoCap.Plugins.Factories {
    [Export(typeof(ICommandFactory))]
    public class FtpUploaderFactory : ICommandFactory {
        public string Name {
            get {
                return "FTP Uploader";
            }
        }

        public ICommand CreateCommand(IInfoStuff infoStuff) {
            return new FtpUploader();
        }

        public ICommandEditor GetCommandEditor(ICommand command, IInfoStuff infoStuff) {
            return new FtpUploaderEditor((FtpUploader) command, infoStuff);
        }

        public CommandFeatures CommandFeatures {
            get {
                return CommandFeatures.FileUploader | CommandFeatures.ImageUploader;
            }
        }
    }
}