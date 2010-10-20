using System.ComponentModel.Composition;
using System.Windows.Controls;
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

        public ContentControl GetProcessorEditor(IProcessor processor) {
            return null;
        }
    }
}
