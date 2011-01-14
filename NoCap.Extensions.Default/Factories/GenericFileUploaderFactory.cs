using System.ComponentModel.Composition;
using NoCap.Extensions.Default.Editors;
using NoCap.Library;

namespace NoCap.Extensions.Default.Commands {
    [Export(typeof(ICommandFactory))]
    class GenericFileUploaderFactory : ICommandFactory {
        public string Name {
            get { return "Generic file uploader"; }
        }

        public ICommand CreateCommand() {
            return new GenericFileUploader();
        }

        public void PopulateCommand(ICommand command, ICommandProvider commandProvider) {
        }

        public ICommandEditor GetCommandEditor(ICommandProvider commandProvider) {
            return new GenericFileUploaderEditor();
        }

        public CommandFeatures CommandFeatures {
            get { return CommandFeatures.FileUploader; }
        }
    }
}