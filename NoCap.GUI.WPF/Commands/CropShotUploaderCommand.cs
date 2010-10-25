using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
using NoCap.Library;
using NoCap.Library.Util;
using NoCap.Plugins.Processors;
using NoCap.Library.Processors;

namespace NoCap.GUI.WPF.Commands {
    public class CropShotUploaderCommand : HighLevelCommand, INotifyPropertyChanged {
        private IProcessor imageUploader;

        private string name = "Crop shot uploader";

        public override string Name {
            get {
                return this.name;
            }

            set {
                this.name = value;

                Notify("Name");
            }
        }

        public IProcessor ImageUploader {
            get {
                return this.imageUploader;
            }

            set {
                this.imageUploader = value;

                Notify("ImageUploader");
            }
        }

        public HighLevelCommand Clone() {
            return new CropShotUploaderCommand {
                Name = Name,
                ImageUploader = ImageUploader,
            };
        }

        public override IProcessorFactory GetFactory() {
            return new CropShotUploaderCommandFactory();
        }

        public override void Execute(IMutableProgressTracker progress) {
            var source = new Screenshot();

            // TODO Progress
            var screenshotData = source.Process(null, progress);

            var destination = new ProcessorChain(
                new CropShot(),
                ImageUploader,
                new Clipboard()
            );

            destination.Process(screenshotData, progress);
        }

        public IEnumerable<TypedDataType> GetOutputDataTypes() {
            return null;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void Notify(string propertyName) {
            var handler = PropertyChanged;

            if (handler != null) {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }

    [Export(typeof(IProcessorFactory))]
    public class CropShotUploaderCommandFactory : IProcessorFactory {
        public string Name {
            get {
                return "Clipboard uploader";
            }
        }

        public IProcessor CreateProcessor(IInfoStuff infoStuff) {
            return new CropShotUploaderCommand {
                ImageUploader = infoStuff.GetImageUploaders().FirstOrDefault()
            };
        }

        public IProcessorEditor GetProcessorEditor(IProcessor processor, IInfoStuff infoStuff) {
            return new CropShotUploaderCommandEditor((CropShotUploaderCommand) processor) {
                ImageUploaders = infoStuff.GetImageUploaders()
            };
        }
    }
}