﻿using System.ComponentModel.Composition;
using System.Linq;
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

        public ICommand CreateCommand(IInfoStuff infoStuff) {
            return new ClipboardUploaderCommand {
                ImageUploader = infoStuff.GetImageUploaders().FirstOrDefault(),
                UrlShortener = infoStuff.GetUrlShorteners().FirstOrDefault(),
                TextUploader = infoStuff.GetTextUploaders().FirstOrDefault(),
            };
        }

        public ICommandEditor GetCommandEditor(ICommand command, IInfoStuff infoStuff) {
            return new ClipboardUploaderCommandEditor((ClipboardUploaderCommand) command) {
                ImageUploaders = infoStuff.GetImageUploaders(),
                UrlShorteners = infoStuff.GetUrlShorteners(),
                TextUploaders = infoStuff.GetTextUploaders(),
            };
        }

        public CommandFeatures CommandFeatures {
            get {
                return CommandFeatures.StandAlone;
            }
        }
    }
}