using System.ComponentModel.Composition;
using NoCap.Library;
using NoCap.Plugins.Processors;

namespace NoCap.Plugins.Factories {
    [Export(typeof(IProcessorFactory))]
    class FileSystemFactory : IProcessorFactory {
        public string Name {
            get { return "File system"; }
        }

        public IProcessor CreateProcessor(IInfoStuff infoStuff) {
            return new FileSystem();
        }

        public IProcessorEditor GetProcessorEditor(IProcessor processor, IInfoStuff infoStuff) {
            return null;
        }
    }
}
