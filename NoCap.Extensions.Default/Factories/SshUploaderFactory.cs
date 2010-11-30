using System.ComponentModel.Composition;
using NoCap.Extensions.Default.Commands;
using NoCap.Extensions.Default.Editors;
using NoCap.Library;

namespace NoCap.Extensions.Default.Factories {
    [Export(typeof(ICommandFactory))]
    public class SshUploaderFactory : ICommandFactory {
        public string Name {
            get {
                return "SSH file uploader";
            }
        }

        public ICommand CreateCommand() {
            return new SshUploader();
        }

        public void PopulateCommand(ICommand command, ICommandProvider commandProvider) {
            // Do nothing.
        }

        public ICommandEditor GetCommandEditor(ICommandProvider commandProvider) {
            return new SshUploaderEditor();
        }

        public CommandFeatures CommandFeatures {
            get {
                return CommandFeatures.FileUploader;
            }
        }
    }
}