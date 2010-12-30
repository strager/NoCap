using System.ComponentModel;
using System.Globalization;
using System.Runtime.Serialization;
using System.Threading;
using NoCap.Extensions.Default.Factories;
using NoCap.Library;
using NoCap.Library.Progress;
using StringLib;

namespace NoCap.Extensions.Default.Commands {
    [DataContract(Name = "Renamer")]
    public sealed class Renamer : ICommand, INotifyPropertyChanged, IExtensibleDataObject {
        public string Name {
            get {
                return "Renamer";
            }
        }

        private string nameFormat;

        [DataMember(Name = "Format")]
        public string NameFormat {
            get {
                return this.nameFormat ?? "{name}-{sequenceId}";
            }

            set {
                this.nameFormat = value;

                Notify("NameFormat");
            }
        }

        private readonly static HartFormatter.FormatterOptions NameFormatterOptions;

        static Renamer() {
            NameFormatterOptions = HartFormatter.FormatterOptions.HumaneOptions;
            NameFormatterOptions.FormatProvider = CultureInfo.InvariantCulture;
        }

        [DataMember(Name = "SequenceID", EmitDefaultValue = true)]
        private int sequenceId = 1;

        public TypedData Process(TypedData data, IMutableProgressTracker progress, CancellationToken cancelToken) {
            var newData = new TypedData(data.DataType, data.CloneData(), FormatName(data.Name));

            progress.Progress = 1;

            ++this.sequenceId;

            return newData;
        }

        private string FormatName(string oldName) {
            return NameFormat.HartFormat(new {
                name = oldName,
                sequence = this.sequenceId,
                sequenceId = this.sequenceId,
            }, NameFormatterOptions);
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

        public event PropertyChangedEventHandler PropertyChanged;

        private void Notify(string propertyName) {
            var handler = PropertyChanged;

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
