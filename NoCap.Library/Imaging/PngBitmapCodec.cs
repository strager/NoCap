using System;
using System.ComponentModel.Composition;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using NoCap.Library.Util;

namespace NoCap.Library.Imaging {
    [Serializable]
    public sealed class PngBitmapCodec : BitmapCodec {
        private static readonly ImageFormat EncoderFormat = ImageFormat.Png;

        private string name;

        public override string Name {
            get {
                return this.name;
            }

            set {
                this.name = value;

                Notify("Name");
            }
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

        protected override Stream Encode(Bitmap image, IMutableProgressTracker progress) {
            var stream = new MemoryStream();
            var encoder = ImageCodecInfo.GetImageEncoders().First((enc) => enc.FormatID.Equals(EncoderFormat.Guid));
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