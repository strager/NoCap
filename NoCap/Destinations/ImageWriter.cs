using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace NoCap.Destinations {
    public class ImageWriter : IDestination {
        private readonly EncoderParameters encoderParameters;

        public ImageCodecInfo CodecInfo {
            get;
            private set;
        }

        private string Extension {
            get {
                // FIXME I'm sure this isn't correct
                return this.CodecInfo.FormatDescription;
            }
        }

        public ImageWriter(ImageCodecInfo codecInfo, EncoderParameters encoderParameters = null) {
            if (codecInfo == null) {
                throw new ArgumentNullException("codecInfo");
            }

            if (!codecInfo.Flags.HasFlag(ImageCodecFlags.Encoder)) {
                throw new ArgumentException("Codec must support encoding", "codecInfo");
            }

            if (!codecInfo.Flags.HasFlag(ImageCodecFlags.SupportBitmap)) {
                throw new ArgumentException("Codec must support bitmap writing", "codecInfo");
            }

            this.CodecInfo = codecInfo;
            this.encoderParameters = encoderParameters;
        }

        public static bool IsCodecValid(ImageCodecInfo codecInfo) {
            return
                codecInfo != null &&
                codecInfo.Flags.HasFlag(ImageCodecFlags.Encoder) &&
                codecInfo.Flags.HasFlag(ImageCodecFlags.SupportBitmap);
        }

        public IOperation<TypedData> Put(TypedData data) {
            if (data.Type != TypedDataType.Image) {
                throw new ArgumentException("data must be an image", "data");
            }

            return new EasyOperation<TypedData>((op) => {
                byte[] rawData;

                using (var stream = new MemoryStream()) {
                    ((Image)data.Data).Save(stream, this.CodecInfo, this.encoderParameters);

                    stream.Position = 0;

                    rawData = new byte[stream.Length];
                    stream.Read(rawData, 0, rawData.Length);
                }

                return TypedData.FromRawData(rawData, data.Name + "." + Extension);
            });
        }
    }
}
