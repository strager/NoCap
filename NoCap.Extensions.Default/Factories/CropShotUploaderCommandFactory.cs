using System.ComponentModel.Composition;
using NoCap.Extensions.Default.Commands;
using NoCap.Extensions.Default.Editors;
using NoCap.Library;

namespace NoCap.Extensions.Default.Factories {
    [Export(typeof(ICommandFactory))]
    public class CropShotUploaderCommandFactory : ICommandFactory {
        public string Name {
            get {
                return "Clipboard uploader";
            }
        }

        public ICommand CreateCommand() {
            return new CropShotUploaderCommand();
        }

        public void PopulateCommand(ICommand command, ICommandProvider commandProvider) {
            var uploader = (CropShotUploaderCommand) command;
            uploader.ImageUploader = commandProvider.GetDefaultCommand(CommandFeatures.ImageUploader);
        }

        public ICommandEditor GetCommandEditor(ICommandProvider commandProvider) {
            return new CropShotUploaderCommandEditor();
        }

        public CommandFeatures CommandFeatures {
            get {
                return CommandFeatures.StandAlone;
            }
        }
    }
}