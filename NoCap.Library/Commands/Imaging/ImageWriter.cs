﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using NoCap.Library.Util;

namespace NoCap.Library.Commands.Imaging {
    [Serializable]
    public class ImageWriter : ICommand, INotifyPropertyChanged {
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
                return this.extension ?? Codec.Extension;
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

        public IEnumerable<TypedDataType> GetOutputDataTypes(TypedDataType input) {
            return Codec.GetOutputDataTypes(input);
        }

        public ICommandFactory GetFactory() {
            return null;
        }

        public ITimeEstimate ProcessTimeEstimate {
            get {
                return Codec == null ? TimeEstimates.ShortOperation : Codec.ProcessTimeEstimate;
            }
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
}