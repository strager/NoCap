using System.ComponentModel.Composition;
using System.Windows.Controls;
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

        public ContentControl GetProcessorEditor(IProcessor processor) {
            return null;
        }
    }
}
