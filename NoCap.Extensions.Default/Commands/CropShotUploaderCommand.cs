using System;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Threading;
using NoCap.Extensions.Default.Factories;
using NoCap.Library;
using NoCap.Library.Progress;
using NoCap.Library.Commands;
using WinputDotNet.Providers;

namespace NoCap.Extensions.Default.Commands {
    [DefaultBinding(typeof(DirectInputProvider), typeof(DirectInputSequence), "6f1d2b61-d5a0-11cf-bfc7-444553540000|LeftControl+SYSRQ")]
    [DataContract(Name = "CropShotUploader")]
    public sealed class CropShotUploaderCommand : HighLevelCommand, INotifyPropertyChanged, IExtensibleDataObject {
        private ICommand imageUploader;
        private ICommand renamer;

        public override string Name {
            get { return "Crop shot uploader"; }
        }

        [DataMember(Name = "ImageUploader")]
        public ICommand ImageUploader {
            get {
                return this.imageUploader;
            }

            set {
                this.imageUploader = value;

                Notify("ImageUploader");
            }
        }

        [DataMember(Name = "Renamer")]
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

            progress.Status = "Image URL saved to clipboard";
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