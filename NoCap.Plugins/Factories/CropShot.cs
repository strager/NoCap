using System.ComponentModel.Composition;
using NoCap.Library;
using NoCap.Plugins.Processors;

namespace NoCap.Plugins.Factories {
    [Export(typeof(IProcessorFactory))]
    class CropShotFactory : IProcessorFactory {
        public string Name {
            get { return "Crop shot"; }
        }

        public IProcessor CreateProcessor() {
            return new CropShot();
        }

        public IProcessorEditor GetProcessorEditor(IProcessor processor) {
            return null;
        }
    }
}
