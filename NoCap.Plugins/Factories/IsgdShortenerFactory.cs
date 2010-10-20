using System.ComponentModel.Composition;
using System.Windows.Controls;
using NoCap.Library;
using NoCap.Plugins.Processors;

namespace NoCap.Plugins.Factories {
    [Export(typeof(IProcessorFactory))]
    class IsgdShortenerFactory : IProcessorFactory {
        public string Name {
            get { return "is.gd shortener"; }
        }

        public IProcessor CreateProcessor() {
            return new IsgdShortener();
        }

        public ContentControl GetProcessorEditor(IProcessor processor) {
            return null;
        }
    }
}
