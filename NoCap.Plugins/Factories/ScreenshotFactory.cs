using System.ComponentModel.Composition;
using NoCap.Library;
using NoCap.Plugins.Processors;

namespace NoCap.Plugins.Factories {
    [Export(typeof(ICommandFactory))]
    class ScreenshotFactory : ICommandFactory {
        public string Name {
            get { return "Screenshot"; }
        }

        public ICommand CreateCommand(IInfoStuff infoStuff) {
            return new Screenshot();
        }

        public ICommandEditor GetCommandEditor(ICommand command, IInfoStuff infoStuff) {
            return null;
        }
    }
}
