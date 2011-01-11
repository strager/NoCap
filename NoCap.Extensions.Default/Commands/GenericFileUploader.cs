using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.Serialization;
using System.Threading;
using NoCap.Library;
using NoCap.Library.Progress;
using NoCap.Library.Util;
using NoCap.Web.Multipart;

namespace NoCap.Extensions.Default.Commands {
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
        public IDictionary<string, string> PostParameters {
            get;
            set;
        }

        public GenericFileUploader() {
            PostParameters = new Dictionary<string, string>();
        }

        public TypedData Process(TypedData data, IMutableProgressTracker progress, CancellationToken cancelToken) {
            var parameters = new {
                FileName = data.Name,
            };

            var dataBuilder = new MultipartDataBuilder();

            foreach (var parameter in PostParameters) {
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
