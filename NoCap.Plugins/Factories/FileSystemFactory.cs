﻿using System.ComponentModel.Composition;
using NoCap.Library;
using NoCap.Plugins.Commands;

namespace NoCap.Plugins.Factories {
    [Export(typeof(ICommandFactory))]
    class FileSystemFactory : ICommandFactory {
        public string Name {
            get { return "File system"; }
        }

        public ICommand CreateCommand() {
            return new FileSystem();
        }

        public void PopulateCommand(ICommand command, ICommandProvider commandProvider) {
            // Do nothing.
        }

        public ICommandEditor GetCommandEditor(ICommandProvider commandProvider) {
            return null;
        }

        public CommandFeatures CommandFeatures {
            get {
                return CommandFeatures.FileUploader;
            }
        }
    }
}
