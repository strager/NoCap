using System;
using System.ComponentModel.Composition;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;

namespace NoCap.Library.Commands.Imaging {
    [Serializable]
    public class PngBitmapCodec : BitmapCodec {
        private static readonly Guid EncoderGuid = new Guid("b96b3caf-0728-11d3-9d7b-0000f81ef32e");

        public override string Name {
            get;
            set;
        }

        public override string Extension {
            get {
                return "png";
            }
        }

        public override string Description {
            get {
                return "PNG";
            }
        }

        public override bool CanEncode {
            get {
                return true;
            }
        }

        public override bool CanDecode {
            get {
                return false;
            }
        }

        public override BitmapCodecFactory GetFactory() {
            return new PngBitmapCodecFactory();
        }

        protected override Stream Encode(Bitmap image, Util.IMutableProgressTracker progress) {
            // TODO Encoder parameters

            var stream = new MemoryStream();
            var encoder = ImageCodecInfo.GetImageEncoders().First((enc) => enc.FormatID.Equals(EncoderGuid));
            image.Save(stream, encoder, null);

            stream.Position = 0;

            progress.Progress = 1;

            return stream;
        }
    }
    
    [Export(typeof(ICommandFactory))]
    public class PngBitmapCodecFactory : BitmapCodecFactory {
        public override string Name {
            get {
                return "PNG codec";
            }
        }

        public override BitmapCodec CreateCommand(IInfoStuff infoStuff) {
            return new PngBitmapCodec();
        }

        public override ICommandEditor GetCommandEditor(ICommand command, IInfoStuff infoStuff) {
            return null;
        }
    }
}