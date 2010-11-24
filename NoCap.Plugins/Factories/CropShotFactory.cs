using System.ComponentModel.Composition;
using NoCap.Library;
using NoCap.Plugins.Commands;

namespace NoCap.Plugins.Factories {
    [Export(typeof(ICommandFactory))]
    class CropShotFactory : ICommandFactory {
        public string Name {
            get { return "Crop shot"; }
        }

        public ICommand CreateCommand() {
            return new CropShot();
        }

        public void PopulateCommand(ICommand command, ICommandProvider commandProvider) {
            // Do nothing.
        }

        public ICommandEditor GetCommandEditor(ICommandProvider commandProvider) {
            return null;
        }

        public CommandFeatures CommandFeatures {
            get {
                return 0;
            }
        }
    }
}
