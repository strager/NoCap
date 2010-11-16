using System;
using System.ComponentModel;
using NoCap.Library;
using NoCap.Library.Commands;
using NoCap.Library.Util;
using NoCap.Plugins.Factories;

namespace NoCap.Plugins.Commands {
    [Serializable]
    public sealed class ClipboardUploaderCommand : HighLevelCommand, INotifyPropertyChanged {
        private ICommand textUploader;
        private ICommand urlShortener;
        private ICommand imageUploader;

        private readonly Clipboard clipboard = new Clipboard();

        private string name = "Clipboard uploader";

        public override string Name {
            get {
                return this.name;
            }

            set {
                this.name = value;

                Notify("Name");
            }
        }

        public ICommand TextUploader {
            get {
                return this.textUploader;
            }

            set {
                this.textUploader = value;

                Notify("TextUploader");
            }
        }

        public ICommand UrlShortener {
            get {
                return this.urlShortener;
            }

            set {
                this.urlShortener = value;

                Notify("UrlShortener");
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
            return new ClipboardUploaderCommandFactory();
        }

        public override ITimeEstimate ProcessTimeEstimate {
            get {
                return TimeEstimates.LongOperation;
            }
        }

        public override bool IsValid() {
            return ImageUploader.IsValidAndNotNull()
                && TextUploader.IsValidAndNotNull()
                && UrlShortener.IsValidAndNotNull();
        }

        public override void Execute(IMutableProgressTracker progress) {
            var router = new DataRouter();

            router.Connect(TypedDataType.Image, new CommandChain(
                ImageUploader,
                this.clipboard
            ));

            router.Connect(TypedDataType.Text, new CommandChain(
                TextUploader,
                this.clipboard
            ));

            router.Connect(TypedDataType.Uri, new CommandChain(
                UrlShortener,
                this.clipboard
            ));

            var commandChain = new CommandChain(
                this.clipboard,
                router
            );

            using (commandChain.Process(null, progress)) {
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