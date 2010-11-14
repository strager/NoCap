﻿using System.ComponentModel.Composition;
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

        public void PopulateCommand(ICommand command, IInfoStuff infoStuff) {
            var uploader = (ClipboardUploaderCommand) command;
            uploader.ImageUploader = infoStuff.GetPreferredCommand(CommandFeatures.ImageUploader);
            uploader.UrlShortener = infoStuff.GetPreferredCommand(CommandFeatures.UrlShortener);
            uploader.TextUploader = infoStuff.GetPreferredCommand(CommandFeatures.TextUploader);
        }

        public ICommandEditor GetCommandEditor(ICommand command, IInfoStuff infoStuff) {
            return new ClipboardUploaderCommandEditor((ClipboardUploaderCommand) command, infoStuff);
        }

        public CommandFeatures CommandFeatures {
            get {
                return CommandFeatures.StandAlone;
            }
        }
    }
}