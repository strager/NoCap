using System.ComponentModel.Composition;
using NoCap.Library;
using NoCap.Plugins.Commands;
using NoCap.Plugins.Editors;

namespace NoCap.Plugins.Factories {
    [Export(typeof(ICommandFactory))]
    public class ClipboardUploaderCommandFactory : ICommandFactory {
        public string Name {
            get {
                return "Clipboard uploader";
            }
        }

        public ICommand CreateCommand() {
            return new ClipboardUploaderCommand();
        }

        public void PopulateCommand(ICommand command, ICommandProvider commandProvider) {
            var uploader = (ClipboardUploaderCommand) command;
            uploader.ImageUploader = commandProvider.GetDefaultCommand(CommandFeatures.ImageUploader);
            uploader.UrlShortener = commandProvider.GetDefaultCommand(CommandFeatures.UrlShortener);
            uploader.TextUploader = commandProvider.GetDefaultCommand(CommandFeatures.TextUploader);
        }

        public ICommandEditor GetCommandEditor(ICommandProvider commandProvider) {
            return new ClipboardUploaderCommandEditor();
        }

        public CommandFeatures CommandFeatures {
            get {
                return CommandFeatures.StandAlone;
            }
        }
    }
}