using System.ComponentModel.Composition;
using NoCap.Library;
using NoCap.Plugins.Processors;

namespace NoCap.Plugins.Factories {
    [Export(typeof(ICommandFactory))]
    class IsgdShortenerFactory : ICommandFactory {
        public string Name {
            get { return "is.gd shortener"; }
        }

        public ICommand CreateCommand(IInfoStuff infoStuff) {
            return new IsgdShortener();
        }

        public ICommandEditor GetCommandEditor(ICommand command, IInfoStuff infoStuff) {
            return null;
        }
    }
}
