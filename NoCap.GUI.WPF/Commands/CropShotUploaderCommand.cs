using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Linq;
using System.Windows.Controls;
using NoCap.Library;
using NoCap.Library.Util;
using NoCap.Plugins.Processors;
using NoCap.Library.Processors;

namespace NoCap.GUI.WPF.Commands {
    public class CropShotUploaderCommand : ICommand {
        private ImageUploader imageUploader;

        private string name = "Crop shot uploader";

        public string Name {
            get {
                return this.name;
            }
            set {
                this.name = value;
            }
        }

        public ImageUploader ImageUploader {
            get {
                return this.imageUploader;
            }

            set {
                this.imageUploader = value;
            }
        }

        public ICommand Clone() {
            return new CropShotUploaderCommand(ImageUploader);
        }

        public ICommandFactory GetFactory() {
            return new CropShotUploaderCommandFactory();
        }

        public CropShotUploaderCommand() {
            var imageCodecs = ImageCodecInfo.GetImageEncoders().Where(ImageWriter.IsCodecValid);

            this.imageUploader = new ImageBinUploader(
                new ImageWriter(imageCodecs.FirstOrDefault(codec => codec.FormatDescription == "PNG"))
            );
        }

        private CropShotUploaderCommand(ImageUploader imageUploader) {
            ImageUploader = imageUploader;
        }

        public void Execute(IMutableProgressTracker progress) {
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
    }

    public class CropShotUploaderCommandFactory : ICommandFactory {
        public string Name {
            get {
                return "Clipboard uploader";
            }
        }

        public ICommand CreateCommand() {
            return new ClipboardUploaderCommand();
        }

        public ICommandEditor GetCommandEditor(ICommand command) {
            return new CropShotUploaderCommandEditor();
        }
    }
}