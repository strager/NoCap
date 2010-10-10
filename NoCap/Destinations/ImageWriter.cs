﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;

namespace NoCap.Destinations {
    public class ImageWriter : IDestination {
        private readonly ImageCodecInfo codecInfo;
        private readonly EncoderParameters encoderParameters;

        private string Extension {
            get {
                // FIXME I'm sure this isn't correct
                return this.codecInfo.FormatDescription;
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

            this.codecInfo = codecInfo;
            this.encoderParameters = encoderParameters;
        }

        public static bool IsCodecValid(ImageCodecInfo codecInfo) {
            return
                codecInfo != null &&
                codecInfo.Flags.HasFlag(ImageCodecFlags.Encoder) &&
                codecInfo.Flags.HasFlag(ImageCodecFlags.SupportBitmap);
        }

        public IOperation Put(TypedData data) {
            if (data.Type != TypedDataType.Image) {
                throw new ArgumentException("data must be an image", "data");
            }

            return new EasyOperation((op) => {
                byte[] rawData;

                using (var stream = new MemoryStream()) {
                    ((Image)data.Data).Save(stream, this.codecInfo, this.encoderParameters);

                    stream.Position = 0;

                    rawData = new byte[stream.Length];
                    stream.Read(rawData, 0, rawData.Length);
                }

                return TypedData.FromRawData(rawData, data.Name + "." + Extension);
            });
        }
    }
}
