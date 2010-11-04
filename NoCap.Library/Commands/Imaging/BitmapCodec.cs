﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using NoCap.Library.Util;

namespace NoCap.Library.Commands.Imaging {
    [Serializable]
    public abstract class BitmapCodec : ICommand, INotifyPropertyChanged {
        public abstract string Name { get; set; }
        public abstract string Extension { get; }
        public abstract string Description { get; }

        public abstract bool CanEncode { get; }
        public abstract bool CanDecode { get; }

        public TypedData Process(TypedData data, IMutableProgressTracker progress) {
            switch (data.DataType) {
                case TypedDataType.Image:
                    return TypedData.FromStream(Encode((Bitmap) data.Data, progress), data.Name);

                case TypedDataType.Stream:
                    return TypedData.FromImage(Decode((Stream) data.Data, progress), data.Name);

                default:
                    throw new NotSupportedException();
            }
        }

        protected virtual Stream Encode(Bitmap image, IMutableProgressTracker progress) {
            throw new NotSupportedException();
        }

        protected virtual Bitmap Decode(Stream stream, IMutableProgressTracker progress) {
            throw new NotSupportedException();
        }

        public IEnumerable<TypedDataType> GetInputDataTypes() {
            var types = new List<TypedDataType>();

            if (CanEncode) {
                types.Add(TypedDataType.Image);
            }

            if (CanDecode) {
                types.Add(TypedDataType.Stream);
            }

            return types;
        }

        public IEnumerable<TypedDataType> GetOutputDataTypes(TypedDataType input) {
            switch (input) {
                case TypedDataType.Image:
                    return new[] { TypedDataType.Stream };

                case TypedDataType.Stream:
                    return new[] { TypedDataType.Image };

                default:
                    throw new NotSupportedException();
            }
        }

        ICommandFactory ICommand.GetFactory() {
            return GetFactory();
        }

        public abstract BitmapCodecFactory GetFactory();

        public virtual ITimeEstimate ProcessTimeEstimate {
            get {
                return TimeEstimates.ShortOperation;
            }
        }

        private PropertyChangedEventHandler propertyChanged;

        public event PropertyChangedEventHandler PropertyChanged {
            add    { propertyChanged += value; }
            remove { propertyChanged -= value; }
        }

        protected void Notify(string propertyName) {
            var handler = this.propertyChanged;

            if (handler != null) {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }

    public abstract class BitmapCodecFactory : ICommandFactory {
        public abstract string Name {
            get;
        }

        ICommand ICommandFactory.CreateCommand(IInfoStuff infoStuff) {
            return CreateCommand(infoStuff);
        }

        public abstract BitmapCodec CreateCommand(IInfoStuff infoStuff);

        public abstract ICommandEditor GetCommandEditor(ICommand command, IInfoStuff infoStuff);

        public virtual CommandFeatures CommandFeatures {
            get {
                return 0;
            }
        }
    }
}