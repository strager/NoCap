using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading;
using NoCap.Library.Progress;

namespace NoCap.Library.Imaging {
    [DataContract(Name = "JpegBitmapCodec")]
    public sealed class JpegBitmapCodec : BitmapCodec, IExtensibleDataObject {
        private static readonly ImageFormat EncoderFormat = ImageFormat.Jpeg;

        private int quality = 80;

        [IgnoreDataMember]
        public override string Extension {
            get {
                return "jpg";
            }
        }

        [IgnoreDataMember]
        public override string Description {
            get {
                return "JPEG";
            }
        }

        [IgnoreDataMember]
        public override bool CanEncode {
            get {
                return true;
            }
        }

        [IgnoreDataMember]
        public override bool CanDecode {
            get {
                return false;
            }
        }

        [DataMember(Name = "Quality")]
        public int Quality {
            get {
                return this.quality;
            }

            set {
                if (value < 0 || value > 100) {
                    throw new ArgumentOutOfRangeException("value");
                }

                this.quality = value;
            }
        }

        public override BitmapCodecFactory GetFactory() {
            return new JpegBitmapCodecFactory();
        }

        protected override Stream Encode(Bitmap image, IMutableProgressTracker progress, CancellationToken cancelToken) {
            var stream = new MemoryStream();
            var encoder = ImageCodecInfo.GetImageEncoders().First((enc) => enc.FormatID.Equals(EncoderFormat.Guid));

            var parameters = new EncoderParameters(1);
            parameters.Param[0] = new EncoderParameter(Encoder.Quality, Quality);

            image.Save(stream, encoder, parameters);

            stream.Position = 0;

            progress.Progress = 1;

            return stream;
        }

        ExtensionDataObject IExtensibleDataObject.ExtensionData {
            get;
            set;
        }
    }
}