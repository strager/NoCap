using System.ComponentModel.Composition;
using NoCap.Library;
using NoCap.Plugins.Processors;

namespace NoCap.Plugins.Factories {
    [Export(typeof(IProcessorFactory))]
    class CropShotFactory : IProcessorFactory {
        public string Name {
            get { return "Crop shot"; }
        }

        public IProcessor CreateProcessor(IInfoStuff infoStuff) {
            return new CropShot();
        }

        public IProcessorEditor GetProcessorEditor(IProcessor processor, IInfoStuff infoStuff) {
            return null;
        }
    }
}
