using System.ComponentModel.Composition;
using NoCap.Extensions.Default.Commands;
using NoCap.Library;

namespace NoCap.Extensions.Default.Factories {
    [Export(typeof(ICommandFactory))]
    [PreferredCommandFactory(CommandFeatures.UrlShortener)]
    class IsgdShortenerFactory : ICommandFactory {
        public string Name {
            get { return "is.gd shortener"; }
        }

        public ICommand CreateCommand() {
            return new IsgdShortener();
        }

        public void PopulateCommand(ICommand command, ICommandProvider commandProvider) {
            // Do nothing.
        }

        public ICommandEditor GetCommandEditor(ICommandProvider commandProvider) {
            return null;
        }

        public CommandFeatures CommandFeatures {
            get {
                return CommandFeatures.UrlShortener;
            }
        }
    }
}
