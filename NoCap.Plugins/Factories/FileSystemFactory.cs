using System.ComponentModel.Composition;
using NoCap.Library;
using NoCap.Plugins.Commands;

namespace NoCap.Plugins.Factories {
    [Export(typeof(ICommandFactory))]
    class FileSystemFactory : ICommandFactory {
        public string Name {
            get { return "File system"; }
        }

        public ICommand CreateCommand() {
            return new FileSystem();
        }

        public void PopulateCommand(ICommand command, IInfoStuff infoStuff) {
            // Do nothing.
        }

        public ICommandEditor GetCommandEditor(IInfoStuff infoStuff) {
            return null;
        }

        public CommandFeatures CommandFeatures {
            get {
                return CommandFeatures.FileUploader;
            }
        }
    }
}
