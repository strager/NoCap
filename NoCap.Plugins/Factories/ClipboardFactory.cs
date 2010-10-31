using System;
using System.ComponentModel.Composition;
using NoCap.Library;
using NoCap.Plugins.Processors;

namespace NoCap.Plugins.Factories {
    [Export(typeof(ICommandFactory))]
    class ClipboardFactory : ICommandFactory {
        public string Name {
            get { return "Clipboard"; }
        }

        public ICommand CreateCommand(IInfoStuff infoStuff) {
            return new Clipboard();
        }

        public ICommandEditor GetCommandEditor(ICommand command, IInfoStuff infoStuff) {
            return null;
        }

        public CommandFeatures CommandFeatures {
            get {
                return 0;
            }
        }
    }
}
