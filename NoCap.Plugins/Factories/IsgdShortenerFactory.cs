using System.ComponentModel.Composition;
using NoCap.Library;
using NoCap.Plugins.Processors;

namespace NoCap.Plugins.Factories {
    [Export(typeof(IProcessorFactory))]
    class IsgdShortenerFactory : IProcessorFactory {
        public string Name {
            get { return "is.gd shortener"; }
        }

        public IProcessor CreateProcessor(IInfoStuff infoStuff) {
            return new IsgdShortener();
        }

        public IProcessorEditor GetProcessorEditor(IProcessor processor, IInfoStuff infoStuff) {
            return null;
        }
    }
}
