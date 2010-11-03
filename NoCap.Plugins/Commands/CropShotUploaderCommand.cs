using System;
using System.Collections.Generic;
using System.ComponentModel;
using NoCap.Library;
using NoCap.Library.Util;
using NoCap.Library.Commands;
using NoCap.Plugins.Factories;

namespace NoCap.Plugins.Commands {
    [Serializable]
    public class CropShotUploaderCommand : HighLevelCommand, INotifyPropertyChanged {
        private ICommand imageUploader;

        private string name = "Crop shot uploader";

        public override string Name {
            get {
                return this.name;
            }

            set {
                this.name = value;

                Notify("Name");
            }
        }

        public ICommand ImageUploader {
            get {
                return this.imageUploader;
            }

            set {
                this.imageUploader = value;

                Notify("ImageUploader");
            }
        }

        public HighLevelCommand Clone() {
            return new CropShotUploaderCommand {
                Name = Name,
                ImageUploader = ImageUploader,
            };
        }

        public override ICommandFactory GetFactory() {
            return new CropShotUploaderCommandFactory();
        }

        public override ITimeEstimate ProcessTimeEstimate {
            get {
                return TimeEstimates.LongOperation;
            }
        }

        public override void Execute(IMutableProgressTracker progress) {
            var source = new Screenshot();

            // TODO Progress
            var screenshotData = source.Process(null, progress);

            var destination = new CommandChain(
                new CropShot(),
                ImageUploader,
                new Clipboard()
            );

            var data = destination.Process(screenshotData, progress);

            if (data != null) {
                data.Dispose();
            }
        }

        public IEnumerable<TypedDataType> GetOutputDataTypes() {
            return null;
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