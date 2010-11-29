using System;
using System.ComponentModel;
using System.Threading;
using NoCap.Extensions.Default.Factories;
using NoCap.Library;
using NoCap.Library.Util;
using NoCap.Library.Commands;

namespace NoCap.Extensions.Default.Commands {
    [Serializable]
    public sealed class CropShotUploaderCommand : HighLevelCommand, INotifyPropertyChanged {
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

        public override ICommandFactory GetFactory() {
            return new CropShotUploaderCommandFactory();
        }

        public override ITimeEstimate ProcessTimeEstimate {
            get {
                return TimeEstimates.LongOperation;
            }
        }

        public override bool IsValid() {
            return ImageUploader.IsValidAndNotNull();
        }

        public override void Execute(IMutableProgressTracker progress, CancellationToken cancelToken) {
            var commandChain = new CommandChain(
                new Screenshot(),
                new CropShot(),
                ImageUploader,
                new Clipboard()
            );

            using (commandChain.Process(null, progress, cancelToken)) {
                // Auto-dispose
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