using System.ComponentModel.Composition;
using NoCap.Library;
using NoCap.Plugins.Commands;

namespace NoCap.Plugins.Factories {
    [Export(typeof(ICommandFactory))]
    class ScreenshotFactory : ICommandFactory {
        public string Name {
            get { return "Screenshot"; }
        }

        public ICommand CreateCommand() {
            return new Screenshot();
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
