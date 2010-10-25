﻿using System.ComponentModel.Composition;
using NoCap.Library;
using NoCap.Plugins.Processors;

namespace NoCap.Plugins.Factories {
    [Export(typeof(ICommandFactory))]
    class FileSystemFactory : ICommandFactory {
        public string Name {
            get { return "File system"; }
        }

        public ICommand CreateCommand(IInfoStuff infoStuff) {
            return new FileSystem();
        }

        public ICommandEditor GetCommandEditor(ICommand command, IInfoStuff infoStuff) {
            return null;
        }
    }
}
