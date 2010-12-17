using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading;
using NoCap.Library.Progress;

namespace NoCap.Library.Imaging {
    [DataContract(Name = "PngBitmapCodec")]
    public sealed class PngBitmapCodec : BitmapCodec, IExtensibleDataObject {
        private static readonly ImageFormat EncoderFormat = ImageFormat.Png;

        [IgnoreDataMember]
        public override string Extension {
            get {
                return "png";
            }
        }

        [IgnoreDataMember]
        public override string Description {
            get {
                return "PNG";
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

        public override BitmapCodecFactory GetFactory() {
            return new PngBitmapCodecFactory();
        }

        protected override Stream Encode(Bitmap image, IMutableProgressTracker progress, CancellationToken cancelToken) {
            var stream = new MemoryStream();
            var encoder = ImageCodecInfo.GetImageEncoders().First((enc) => enc.FormatID.Equals(EncoderFormat.Guid));
            image.Save(stream, encoder, null);

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