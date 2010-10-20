using System.ComponentModel.Composition;
using System.Windows.Controls;
using NoCap.Library;
using NoCap.Plugins.Processors;

namespace NoCap.Plugins.Factories {
    [Export(typeof(IProcessorFactory))]
    class SlexyUploaderFactory : IProcessorFactory {
        public string Name {
            get { return "Slexy.org uploader"; }
        }

        public IProcessor CreateProcessor() {
            return new SlexyUploader();
        }

        public ContentControl GetProcessorEditor(IProcessor processor) {
            return null;
        }
    }
}
