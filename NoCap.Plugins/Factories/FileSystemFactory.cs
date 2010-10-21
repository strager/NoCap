using System.ComponentModel.Composition;
using NoCap.Library;
using NoCap.Plugins.Processors;

namespace NoCap.Plugins.Factories {
    [Export(typeof(IProcessorFactory))]
    class FileSystemFactory : IProcessorFactory {
        public string Name {
            get { return "File system"; }
        }

        public IProcessor CreateProcessor() {
            return new FileSystem();
        }

        public IProcessorEditor GetProcessorEditor(IProcessor processor) {
            return null;
        }
    }
}
