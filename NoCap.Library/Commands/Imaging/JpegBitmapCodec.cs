using System;
using System.ComponentModel.Composition;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using NoCap.Library.Util;

namespace NoCap.Library.Commands.Imaging {
    [Serializable]
    public class JpegBitmapCodec : BitmapCodec {
        private static readonly ImageFormat EncoderFormat = ImageFormat.Jpeg;

        private string name;
        private int quality = 80;

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
                return "jpg";
            }
        }

        public override string Description {
            get {
                return "JPEG";
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

        public int Quality {
            get {
                return this.quality;
            }

            set {
                if (value < 0 || value > 100) {
                    throw new ArgumentOutOfRangeException("value");
                }

                this.quality = value;

                Notify("Quality");
            }
        }

        public override BitmapCodecFactory GetFactory() {
            return new JpegBitmapCodecFactory();
        }

        protected override Stream Encode(Bitmap image, IMutableProgressTracker progress) {
            // TODO Progress

            var stream = new MemoryStream();
            var encoder = ImageCodecInfo.GetImageEncoders().First((enc) => enc.FormatID.Equals(EncoderFormat.Guid));

            var parameters = new EncoderParameters(1);
            parameters.Param[0] = new EncoderParameter(Encoder.Quality, Quality);

            image.Save(stream, encoder, parameters);

            stream.Position = 0;

            progress.Progress = 1;

            return stream;
        }
    }
    
    [Export(typeof(ICommandFactory))]
    public class JpegBitmapCodecFactory : BitmapCodecFactory {
        public override string Name {
            get {
                return "JPEG codec";
            }
        }

        public override BitmapCodec CreateCommand(IInfoStuff infoStuff) {
            return new JpegBitmapCodec();
        }

        public override ICommandEditor GetCommandEditor(ICommand command, IInfoStuff infoStuff) {
            return new JpegBitmapCodecEditor((JpegBitmapCodec) command);
        }
    }
}