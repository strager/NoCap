using System.ComponentModel.Composition;
using NoCap.Library;
using NoCap.Plugins.Commands;

namespace NoCap.Plugins.Factories {
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
            return null;
        }

        public CommandFeatures CommandFeatures {
            get {
                return CommandFeatures.TextUploader;
            }
        }
    }
}
