using System.ComponentModel.Composition;
using NoCap.Library;
using NoCap.Plugins.Commands;
using NoCap.Plugins.Editors;

namespace NoCap.Plugins.Factories {
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

        public void PopulateCommand(ICommand command, IInfoStuff infoStuff) {
            var uploader = (CropShotUploaderCommand) command;
            uploader.ImageUploader = infoStuff.GetDefaultCommand(CommandFeatures.ImageUploader);
        }

        public ICommandEditor GetCommandEditor(IInfoStuff infoStuff) {
            return new CropShotUploaderCommandEditor();
        }

        public CommandFeatures CommandFeatures {
            get {
                return CommandFeatures.StandAlone;
            }
        }
    }
}