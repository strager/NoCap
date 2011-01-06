using System.ComponentModel.Composition;
using NoCap.Extensions.Default.Editors;
using NoCap.Library;

namespace NoCap.Extensions.Default.Commands {
    [Export(typeof(ICommandFactory))]
    class SendSpaceUploaderFactory : ICommandFactory {
        public string Name {
            get { return "SendSpace uploader"; }
        }

        public ICommand CreateCommand() {
            return new SendSpaceUploader();
        }

        public void PopulateCommand(ICommand command, ICommandProvider commandProvider) {
        }

        public ICommandEditor GetCommandEditor(ICommandProvider commandProvider) {
            return new SendSpaceUploaderEditor();
        }

        public CommandFeatures CommandFeatures {
            get { return CommandFeatures.FileUploader; }
        }
    }
}