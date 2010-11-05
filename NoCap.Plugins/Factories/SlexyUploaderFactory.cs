﻿using System.ComponentModel.Composition;
using NoCap.Library;
using NoCap.Plugins.Commands;

namespace NoCap.Plugins.Factories {
    [Export(typeof(ICommandFactory))]
    class SlexyUploaderFactory : ICommandFactory {
        public string Name {
            get { return "Slexy.org uploader"; }
        }

        public ICommand CreateCommand(IInfoStuff infoStuff) {
            return new SlexyUploader();
        }

        public ICommandEditor GetCommandEditor(ICommand command, IInfoStuff infoStuff) {
            return null;
        }

        public CommandFeatures CommandFeatures {
            get {
                return CommandFeatures.TextUploader;
            }
        }
    }
}