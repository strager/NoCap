using System.ComponentModel.Composition;
using NoCap.Library;
using NoCap.Plugins.Commands;

namespace NoCap.Plugins.Factories {
    [Export(typeof(ICommandFactory))]
    class CropShotFactory : ICommandFactory {
        public string Name {
            get { return "Crop shot"; }
        }

        public ICommand CreateCommand(IInfoStuff infoStuff) {
            return new CropShot();
        }

        public ICommandEditor GetCommandEditor(ICommand command, IInfoStuff infoStuff) {
            return null;
        }

        public CommandFeatures CommandFeatures {
            get {
                return 0;
            }
        }
    }
}
