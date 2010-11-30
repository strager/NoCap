using NoCap.Extensions.Default.Commands;
using NoCap.Extensions.Default.Editors;
using NoCap.Library;

namespace NoCap.Extensions.Default.Factories {
    public class RenamerFactory : ICommandFactory {
        public string Name {
            get {
                return "Renamer";
            }
        }

        public ICommand CreateCommand() {
            return new Renamer();
        }

        public void PopulateCommand(ICommand command, ICommandProvider commandProvider) {
            // Do nothing.
        }

        public ICommandEditor GetCommandEditor(ICommandProvider commandProvider) {
            return new RenamerEditor();
        }

        public CommandFeatures CommandFeatures {
            get {
                return CommandFeatures.None;
            }
        }
    }
}