using System;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Threading;
using NoCap.Library.Progress;

namespace NoCap.Library.Imaging {
    [DataContract(Name = "ImageWriter")]
    public sealed class ImageWriter : ICommand, INotifyPropertyChanged {
        private BitmapCodec codec;

        public string Name {
            get {
                return Codec == null ? "Image writer" : string.Format("{0} writer", Codec.Description);
            }
        }

        public string Extension {
            get {
                return Codec == null ? null : Codec.Extension;
            }
        }

        [DataMember(Name = "Codec")]
        public BitmapCodec Codec {
            get {
                return this.codec;
            }

            set {
                this.codec = value;

                Notify("Codec");
                Notify("Name");
                Notify("Extension");
            }
        }

        public static bool IsCodecValid(BitmapCodec codec) {
            return codec != null && codec.CanEncode;
        }

        public TypedData Process(TypedData data, IMutableProgressTracker progress, CancellationToken cancelToken) {
            if (data.DataType != TypedDataType.Image) {
                throw new ArgumentException("data must be an image", "data");
            }

            if (!IsCodecValid(Codec)) {
                throw new InvalidOperationException("Codec must be non-null and support bitmap encoding");
            }

            // No dispose
            var processedImage = Codec.Process(data, progress, cancelToken);

            var name = processedImage.Name;
            return new TypedData(processedImage.DataType, processedImage.Data, GetDataName(name));
        }

        private string GetDataName(string originalName) {
            string name = originalName;
            string extension = Extension;

            if (!string.IsNullOrEmpty(extension)) {
                name += "." + extension;
            }

            return name;
        }

        public ICommandFactory GetFactory() {
            return new ImageWriterFactory();
        }

        public ITimeEstimate ProcessTimeEstimate {
            get {
                return Codec == null ? TimeEstimates.ShortOperation : Codec.ProcessTimeEstimate;
            }
        }

        public bool IsValid() {
            return this.codec.IsValidAndNotNull();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void Notify(string propertyName) {
            var handler = PropertyChanged;

            if (handler != null) {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
