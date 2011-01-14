using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.Serialization;
using System.Threading;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using Bindable.Linq.Collections;
using NoCap.Library;
using NoCap.Library.Progress;
using NoCap.Library.Util;
using NoCap.Web.Multipart;

namespace NoCap.Extensions.Default.Commands {
    class StringPair : IXmlSerializable {
        public StringPair() {
        }

        public StringPair(string key, string value) {
            Key = key;
            Value = value;
        }

        public string Key {
            get;
            set;
        }

        public string Value {
            get;
            set;
        }

        public XmlSchema GetSchema() {
            return new XmlSchema();
        }

        public void ReadXml(XmlReader reader) {
            reader.ReadStartElement();

            Key = reader.ReadElementString("Key");
            Value = reader.ReadElementString("Value");
        }

        public void WriteXml(XmlWriter writer) {
            writer.WriteElementString("Key", Key);
            writer.WriteElementString("Value", Value);
        }
    }

    [DataContract(Name = "GenericFileUploader")]
    sealed class GenericFileUploader : ICommand, INotifyPropertyChanged {
        private string name;

        [DataMember(Name = "Name")]
        public string Name {
            get {
                return this.name;
            }

            set {
                this.name = value;

                Notify("Name");
            }
        }

        [DataMember(Name = "Uri")]
        public Uri Uri {
            get;
            set;
        }

        [DataMember(Name = "FileParameter")]
        public string FileParameterName {
            get;
            set;
        }

        [DataMember(Name = "PostParameters")]
        public BindableCollection<StringPair> PostParameters {
            get;
            set;
        }

        public GenericFileUploader() {
            PostParameters = new BindableCollection<StringPair>();
        }

        public TypedData Process(TypedData data, IMutableProgressTracker progress, CancellationToken cancelToken) {
            var parameters = new {
                FileName = data.Name,
            };

            var dataBuilder = new MultipartDataBuilder();

            foreach (var parameter in PostParameters) {
                if (parameter.Key == null) {
                    continue;
                }

                dataBuilder.KeyValuePair(parameter.Key, StringLib.HartFormatter.HartFormat(parameter.Value, parameters));
            }

            dataBuilder.File((Stream) data.Data, FileParameterName, data.Name);

            progress.Status = "Uploading data";

            var response = HttpRequest.Execute(Uri, dataBuilder.GetData(), HttpRequestMethod.Post, progress, cancelToken);

            return TypedData.FromUri(response.ResponseUri, data.Name);
        }

        public ICommandFactory GetFactory() {
            return new GenericFileUploaderFactory();
        }

        public ITimeEstimate ProcessTimeEstimate {
            get { return TimeEstimates.LongOperation; }
        }

        public bool IsValid() {
            return Uri != null && !string.IsNullOrWhiteSpace(FileParameterName);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void Notify(string propertyName) {
            var handler = PropertyChanged;

            if (handler != null) {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
