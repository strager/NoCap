﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using NoCap.Library.Editors;
using NoCap.Library.Util;

namespace NoCap.Library.Imaging {
    [Serializable]
    public sealed class ImageWriter : ICommand, INotifyPropertyChanged {
        private string name;
        private string extension;
        private BitmapCodec codec;

        public string Name {
            get {
                if (this.name == null) {
                    return Codec == null ? "Image writer" : string.Format("{0} writer", Codec.Description);
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
                return this.extension ?? (Codec == null ? null : Codec.Extension);
            }

            set {
                this.extension = value;

                Notify("Extension");
            }
        }

        public BitmapCodec Codec {
            get {
                return this.codec;
            }

            set {
                this.codec = value;

                Notify("Codec");

                if (this.name == null) {
                    Notify("Name");
                }

                if (this.extension == null) {
                    Notify("Extension");
                }
            }
        }

        public static bool IsCodecValid(BitmapCodec codec) {
            return
                codec != null &&
                codec.CanEncode;
        }

        public TypedData Process(TypedData data, IMutableProgressTracker progress) {
            if (data.DataType != TypedDataType.Image) {
                throw new ArgumentException("data must be an image", "data");
            }

            if (!IsCodecValid(Codec)) {
                throw new InvalidOperationException("Codec must be non-null and support bitmap encoding");
            }

            return Codec.Process(data, progress);
        }

        public IEnumerable<TypedDataType> GetInputDataTypes() {
            return Codec.GetInputDataTypes();
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

        public ImageWriter(BitmapCodec codec) {
            if (codec == null) {
                throw new ArgumentNullException("codec");
            }

            Codec = codec;
        }

        [NonSerialized]
        private PropertyChangedEventHandler propertyChanged;

        public event PropertyChangedEventHandler PropertyChanged {
            add    { this.propertyChanged += value; }
            remove { this.propertyChanged -= value; }
        }

        protected void Notify(string propertyName) {
            var handler = this.propertyChanged;

            if (handler != null) {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }

    public class ImageWriterFactory : ICommandFactory {
        public string Name {
            get {
                return "Image writer";
            }
        }

        public ICommand CreateCommand() {
            return new ImageWriter(new PngBitmapCodec());
        }

        public void PopulateCommand(ICommand command, IInfoStuff infoStuff) {
            // Do nothing.
        }

        public ICommandEditor GetCommandEditor(IInfoStuff infoStuff) {
            return new ImageWriterEditor(infoStuff);
        }

        public CommandFeatures CommandFeatures {
            get {
                return 0;
            }
        }
    }
}
