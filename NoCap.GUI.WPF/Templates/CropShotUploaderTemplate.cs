using System.Drawing.Imaging;
using System.Linq;
using NoCap.GUI.WPF.Settings;
using NoCap.Plugins;
using NoCap.Library.Destinations;

namespace NoCap.GUI.WPF.Templates {
    public class CropShotUploaderTemplate : ITemplate {
        private readonly ImageUploader imageUploader;

        private string name = "Crop shot uploader";

        public string Name {
            get {
                return this.name;
            }
            set {
                this.name = value;
            }
        }

        public CropShotUploaderTemplate() {
            var imageCodecs = ImageCodecInfo.GetImageEncoders().Where(ImageWriter.IsCodecValid);

            this.imageUploader = new ImageBinUploader(
                new ImageWriter(imageCodecs.FirstOrDefault(codec => codec.FormatDescription == "PNG"))
            );
        }

        public SourceDestinationCommand GetCommand() {
            var source = new ScreenshotSource();

            var destination = new DestinationChain(
                new CropShot(),
                this.imageUploader,
                new Clipboard()
            );

            return new SourceDestinationCommand(Name, source, destination);
        }
    }
}