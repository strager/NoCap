using System.ComponentModel.Composition;
using System.Windows.Controls;
using NoCap.Library;
using NoCap.Plugins.Processors;

namespace NoCap.Plugins.Factories {
    [Export(typeof(IProcessorFactory))]
    class ScreenshotFactory : IProcessorFactory {
        public string Name {
            get { return "Screenshot"; }
        }

        public IProcessor CreateProcessor() {
            return new Screenshot();
        }

        public ContentControl GetProcessorEditor(IProcessor processor) {
            return null;
        }
    }
}
