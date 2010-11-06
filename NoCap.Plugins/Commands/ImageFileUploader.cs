using System;
using System.Collections.Generic;
using System.ComponentModel;
using NoCap.Library;
using NoCap.Library.Commands;
using NoCap.Library.Imaging;
using NoCap.Library.Util;
using NoCap.Plugins.Factories;

namespace NoCap.Plugins.Commands {
    [Serializable]
    public class ImageFileUploader : ICommand, INotifyPropertyChanged {
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

        public ImageWriter ImageWriter {
            get {
                return this.imageWriter;
            }

            set {
                this.imageWriter = value;

                Notify("ImageWriter");
            }
        }

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

        public TypedData Process(TypedData data, IMutableProgressTracker progress) {
            return new CommandChain(ImageWriter, FileUploader).Process(data, progress);
        }

        public IEnumerable<TypedDataType> GetInputDataTypes() {
            return GetCommand().GetInputDataTypes();
        }

        public IEnumerable<TypedDataType> GetOutputDataTypes(TypedDataType input) {
            return GetCommand().GetOutputDataTypes(input);
        }

        public ICommandFactory GetFactory() {
            return new ImageFileUploaderFactory();
        }

        public ITimeEstimate ProcessTimeEstimate {
            get {
                return GetCommand().ProcessTimeEstimate;
            }
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
    }
}
