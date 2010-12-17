using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Threading;
using NoCap.Extensions.Default.Factories;
using NoCap.Library;
using NoCap.Library.Commands;
using NoCap.Library.Imaging;
using NoCap.Library.Progress;
using NoCap.Library.Util;

namespace NoCap.Extensions.Default.Commands {
    [DataContract(Name = "ImageFileUploader")]
    public sealed class ImageFileUploader : ICommand, INotifyPropertyChanged, IExtensibleDataObject {
        private string name = "Image file uploader";

        private ImageWriter imageWriter;
        private ICommand fileUploader;

        public string Name {
            get {
                return this.name;
            }

            set {
                this.name = value;

                Notify("Name");
            }
        }

        [DataMember(Name = "ImageWriter")]
        public ImageWriter ImageWriter {
            get {
                return this.imageWriter;
            }

            set {
                this.imageWriter = value;

                Notify("ImageWriter");
            }
        }

        [DataMember(Name = "FileUploader")]
        public ICommand FileUploader {
            get {
                return this.fileUploader;
            }

            set {
                this.fileUploader = value;

                Notify("FileUploader");
            }
        }

        public ImageFileUploader() {
            this.imageWriter = new ImageWriter();
        }

        private ICommand GetCommand() {
            return new CommandChain(ImageWriter, FileUploader);
        }

        public TypedData Process(TypedData data, IMutableProgressTracker progress, CancellationToken cancelToken) {
            return new CommandChain(ImageWriter, FileUploader).Process(data, progress, cancelToken);
        }

        public ICommandFactory GetFactory() {
            return new ImageFileUploaderFactory();
        }

        public ITimeEstimate ProcessTimeEstimate {
            get {
                return GetCommand().ProcessTimeEstimate;
            }
        }

        public bool IsValid() {
            return ImageWriter.IsValidAndNotNull() && FileUploader.IsValidAndNotNull();
        }

        [NonSerialized]
        private PropertyChangedEventHandler propertyChanged;

        public event PropertyChangedEventHandler PropertyChanged {
            add    { this.propertyChanged += value; }
            remove { this.propertyChanged -= value; }
        }

        private void Notify(string propertyName) {
            var handler = this.propertyChanged;

            if (handler != null) {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        ExtensionDataObject IExtensibleDataObject.ExtensionData {
            get;
            set;
        }
    }
}
