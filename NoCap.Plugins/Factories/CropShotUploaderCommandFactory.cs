using System.ComponentModel.Composition;
using System.Linq;
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

        public ICommand CreateCommand(IInfoStuff infoStuff) {
            return new CropShotUploaderCommand {
                ImageUploader = infoStuff.GetImageUploaders().FirstOrDefault()
            };
        }

        public ICommandEditor GetCommandEditor(ICommand command, IInfoStuff infoStuff) {
            return new CropShotUploaderCommandEditor((CropShotUploaderCommand) command, infoStuff);
        }

        public CommandFeatures CommandFeatures {
            get {
                return CommandFeatures.StandAlone;
            }
        }
    }
}