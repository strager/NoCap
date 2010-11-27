using System.ComponentModel.Composition;
using NoCap.Extensions.Default.Commands;
using NoCap.Extensions.Default.Editors;
using NoCap.Library;

namespace NoCap.Extensions.Default.Factories {
    [Export(typeof(ICommandFactory))]
    [PreferredCommandFactory(CommandFeatures.TextUploader)]
    class SlexyUploaderFactory : ICommandFactory {
        public string Name {
            get { return "Slexy.org uploader"; }
        }

        public ICommand CreateCommand() {
            return new SlexyUploader();
        }

        public void PopulateCommand(ICommand command, ICommandProvider commandProvider) {
            // Do nothing.
        }

        public ICommandEditor GetCommandEditor(ICommandProvider commandProvider) {
            return new SlexyUploaderEditor();
        }

        public CommandFeatures CommandFeatures {
            get {
                return CommandFeatures.TextUploader;
            }
        }
    }
}
