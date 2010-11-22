﻿using System.ComponentModel.Composition;
using NoCap.Library;
using NoCap.Plugins.Commands;
using NoCap.Plugins.Editors;

namespace NoCap.Plugins.Factories {
    [Export(typeof(ICommandFactory))]
    public class FtpUploaderFactory : ICommandFactory {
        public string Name {
            get {
                return "FTP file uploader";
            }
        }

        public ICommand CreateCommand() {
            return new FtpUploader();
        }

        public void PopulateCommand(ICommand command, IInfoStuff infoStuff) {
            // Do nothing.
        }

        public ICommandEditor GetCommandEditor(IInfoStuff infoStuff) {
            return new FtpUploaderEditor();
        }

        public CommandFeatures CommandFeatures {
            get {
                return CommandFeatures.FileUploader;
            }
        }
    }
}