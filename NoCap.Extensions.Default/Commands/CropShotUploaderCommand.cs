using System;
using System.ComponentModel;
using System.Threading;
using NoCap.Extensions.Default.Factories;
using NoCap.Library;
using NoCap.Library.Progress;
using NoCap.Library.Commands;

namespace NoCap.Extensions.Default.Commands {
    [Serializable]
    public sealed class CropShotUploaderCommand : HighLevelCommand, INotifyPropertyChanged {
        private ICommand imageUploader;
        private ICommand renamer;

        public override string Name {
            get { return "Crop shot uploader"; }
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

        public ICommand Renamer {
            get {
                return this.renamer;
            }

            set {
                this.renamer = value;

                Notify("Renamer");
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
                Renamer,
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