using System.ComponentModel.Composition;
using NoCap.Extensions.Default.Commands;
using NoCap.Library;

namespace NoCap.Extensions.Default.Factories {
    [Export(typeof(ICommandFactory))]
    public class MinUsUploaderFactory : ICommandFactory {
        public string Name {
            get { return "min.us uploader"; }
        }

        public ICommand CreateCommand() {
            return new MinUsUploader();
        }

        public void PopulateCommand(ICommand command, ICommandProvider commandProvider) {
        }

        public ICommandEditor GetCommandEditor(ICommandProvider commandProvider) {
            return null;
        }

        public CommandFeatures CommandFeatures {
            get { return CommandFeatures.FileUploader; }
        }
    }
}