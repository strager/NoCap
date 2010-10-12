using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace NoCap.Library.Destinations {
    public class ImageWriter : IDestination {
        public EncoderParameters EncoderParameters {
            get;
            private set;
        }

        public ImageCodecInfo CodecInfo {
            get;
            private set;
        }

        private string Extension {
            get {
                // FIXME I'm sure this isn't correct
                return CodecInfo.FormatDescription;
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

            CodecInfo = codecInfo;
            EncoderParameters = encoderParameters;
        }

        public static bool IsCodecValid(ImageCodecInfo codecInfo) {
            return
                codecInfo != null &&
                codecInfo.Flags.HasFlag(ImageCodecFlags.Encoder) &&
                codecInfo.Flags.HasFlag(ImageCodecFlags.SupportBitmap);
        }

        public IOperation<TypedData> Put(TypedData data) {
            if (data.DataType != TypedDataType.Image) {
                throw new ArgumentException("data must be an image", "data");
            }

            return new EasyOperation<TypedData>((op) => {
                byte[] rawData;

                using (var stream = new MemoryStream()) {
                    ((Image) data.Data).Save(stream, CodecInfo, EncoderParameters);

                    stream.Position = 0;

                    rawData = new byte[stream.Length];
                    stream.Read(rawData, 0, rawData.Length);
                }

                return TypedData.FromRawData(rawData, data.Name + "." + Extension);
            });
        }

        public IEnumerable<TypedDataType> GetInputDataTypes() {
            return new[] { TypedDataType.Image };
        }

        public IEnumerable<TypedDataType> GetOutputDataTypes(TypedDataType input) {
            return new[] { TypedDataType.RawData };
        }
    }
}
