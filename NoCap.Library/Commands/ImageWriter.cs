using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using NoCap.Library.Util;

namespace NoCap.Library.Commands {
    public class ImageWriter : ICommand, INotifyPropertyChanged {
        private string name;
        private string extension;
        private EncoderParameters encoderParameters;
        private ImageCodecInfo codecInfo;

        public string Name {
            get {
                if (this.name == null) {
                    return CodecInfo == null ? "Image writer" : string.Format("{0} writer", CodecInfo.FormatDescription);
                }

                return this.name;
            }

            set {
                this.name = value;

                Notify("Name");
            }
        }

        public string Extension {
            get {
                return this.extension ?? CodecInfo.FormatDescription;
            }

            set {
                this.extension = value;

                Notify("Extension");
            }
        }

        public EncoderParameters EncoderParameters {
            get {
                return this.encoderParameters;
            }

            set {
                this.encoderParameters = value;

                Notify("EncoderParameters");
            }
        }

        public ImageCodecInfo CodecInfo {
            get {
                return this.codecInfo;
            }

            set {
                this.codecInfo = value;

                Notify("CodecInfo");

                if (this.name == null) {
                    Notify("Name");
                }

                if (this.extension == null) {
                    Notify("Extension");
                }
            }
        }

        public ImageWriter(ImageCodecInfo codecInfo = null, EncoderParameters encoderParameters = null) {
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

            if (!IsCodecValid(CodecInfo)) {
                throw new InvalidOperationException("Codec must be non-null and support bitmap encoding");
            }

            byte[] rawData;

            using (var stream = new MemoryStream()) {
                ((Image) data.Data).Save(stream, CodecInfo, EncoderParameters);

                stream.Position = 0;

                rawData = new byte[stream.Length];
                stream.Read(rawData, 0, rawData.Length);
            }

            progress.Progress = 1;  // TODO Image writing progress (?)

            return TypedData.FromRawData(rawData, data.Name + "." + Extension);
        }

        public IEnumerable<TypedDataType> GetInputDataTypes() {
            return new[] { TypedDataType.Image };
        }

        public IEnumerable<TypedDataType> GetOutputDataTypes(TypedDataType input) {
            return new[] { TypedDataType.RawData };
        }

        public ICommandFactory GetFactory() {
            return null;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void Notify(string propertyName) {
            var handler = PropertyChanged;

            if (handler != null) {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
