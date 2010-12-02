using System.ComponentModel.Composition;

namespace NoCap.Library.Imaging {
    [Export(typeof(ICommandFactory))]
    public class PngBitmapCodecFactory : BitmapCodecFactory {
        public override string Name {
            get {
                return "PNG codec";
            }
        }

        public override BitmapCodec CreateCommand() {
            return new PngBitmapCodec();
        }

        public override ICommandEditor GetCommandEditor(ICommandProvider commandProvider) {
            return null;
        }
    }
}