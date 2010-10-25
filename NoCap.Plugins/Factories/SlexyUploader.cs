using System.ComponentModel.Composition;
using NoCap.Library;
using NoCap.Plugins.Processors;

namespace NoCap.Plugins.Factories {
    [Export(typeof(IProcessorFactory))]
    class SlexyUploaderFactory : IProcessorFactory {
        public string Name {
            get { return "Slexy.org uploader"; }
        }

        public IProcessor CreateProcessor(IInfoStuff infoStuff) {
            return new SlexyUploader();
        }

        public IProcessorEditor GetProcessorEditor(IProcessor processor, IInfoStuff infoStuff) {
            return null;
        }
    }
}
