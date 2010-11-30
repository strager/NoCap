using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using NoCap.Extensions.Default.Factories;
using NoCap.Library;
using NoCap.Library.Util;

namespace NoCap.Extensions.Default.Commands {
    [Serializable]
    public class Renamer : ICommand, INotifyPropertyChanged {
        public string Name {
            get {
                return "Renamer";
            }
        }

        private string nameFormat;

        public string NameFormat {
            get {
                return this.nameFormat;
            }

            set {
                this.nameFormat = value;

                Notify("NameFormat");
            }
        }

        private int sequenceId = 1;

        public TypedData Process(TypedData data, IMutableProgressTracker progress, CancellationToken cancelToken) {
            var newData = new TypedData(data.DataType, data.CloneData(), FormatName(data.Name));

            progress.Progress = 1;

            ++this.sequenceId;

            return newData;
        }

        private string FormatName(string oldName) {
            // TODO More robust parser
            return this.nameFormat.Replace("%sequence%", this.sequenceId.ToString());
        }

        public ICommandFactory GetFactory() {
            return new RenamerFactory();
        }

        public ITimeEstimate ProcessTimeEstimate {
            get {
                return TimeEstimates.Instantaneous;
            }
        }

        public bool IsValid() {
            // TODO Validate format
            return true;
        }

        [field: NonSerialized]
        public event PropertyChangedEventHandler PropertyChanged;

        protected void Notify(string propertyName) {
            var handler = PropertyChanged;

            if (handler != null) {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
