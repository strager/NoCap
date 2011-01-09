using System.ComponentModel.Composition;

namespace NoCap.Library.Imaging {
    [Export(typeof(ICommandFactory))]
    public class JpegBitmapCodecFactory : BitmapCodecFactory {
        public override string Name {
            get {
                return "JPEG codec";
            }
        }

        public override BitmapCodec CreateCommand() {
            return new JpegBitmapCodec();
        }

        public override ICommandEditor GetCommandEditor(ICommandProvider commandProvider) {
            return new JpegBitmapCodecEditor();
        }
    }
}