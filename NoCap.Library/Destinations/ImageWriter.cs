using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using NoCap.Library.Util;

namespace NoCap.Library.Destinations {
    public class ImageWriter : IProcessor {
        public string Name {
            get { return CodecInfo == null ? "Image writer" : string.Format("{0} writer", CodecInfo.FormatDescription); }
        }

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

        public TypedData Process(TypedData data, IMutableProgressTracker progress) {
            if (data.DataType != TypedDataType.Image) {
                throw new ArgumentException("data must be an image", "data");
            }

            byte[] rawData;

            using (var stream = new MemoryStream()) {
                ((Image) data.Data).Save(stream, CodecInfo, EncoderParameters);

                stream.Position = 0;

                rawData = new byte[stream.Length];
                stream.Read(rawData, 0, rawData.Length);
            }

            progress.Progress = 1;  // TODO

            return TypedData.FromRawData(rawData, data.Name + "." + Extension);
        }

        public IEnumerable<TypedDataType> GetInputDataTypes() {
            return new[] { TypedDataType.Image };
        }

        public IEnumerable<TypedDataType> GetOutputDataTypes(TypedDataType input) {
            return new[] { TypedDataType.RawData };
        }
    }
}
