using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Runtime.Serialization;
using System.Security;
using AlexPilotti.FTPS.Client;
using NoCap.Library;
using NoCap.Library.Util;
using NoCap.Plugins.Factories;

namespace NoCap.Plugins.Commands {
    [Serializable]
    public class FtpUploader : ICommand, INotifyPropertyChanged, ISerializable {
        private string name = "FTP file uploader";

        private string host = "example.com";
        private int port = 21;
        private string userName = "foobar";
        private SecureString password;

        private string outputPath = "";
        private string resultFormat = "http://example.com/{0}";

        public TypedData Process(TypedData data, IMutableProgressTracker progress) {
            // TODO Progress

            string fileName = data.Name;
            var stream = (Stream) data.Data;

            UploadData(stream, fileName);

            progress.Progress = 1;

            string result;
            bool success = TryGetResultString(fileName, out result);

            return TypedData.FromUri(success ? result : ResultFormat, fileName);
        }

        private void UploadData(Stream stream, string fileName) {
            using (var client = new FTPSClient()) {
                client.Connect(Host, Port, new NetworkCredential(UserName, Password), 0, null, null, 0, 0, 0, null);

                using (var outStream = client.PutFile(GetRemotePathName(fileName))) {
                    stream.CopyTo(outStream);
                }
            }
        }

        private bool TryGetResultString(string fileName, out string result) {
            try {
                result = string.Format(resultFormat, fileName);

                return true;
            } catch (FormatException) {
                result = null;

                return false;
            }
        }

        private string GetRemotePathName(string fileName) {
            // FIXME Is there a better way?
            // This is kinda messy, I admit...
            return OutputPath + "/" + fileName.Replace(@"\", @"\\").Replace(@"/", @"\/");
        }

        public string Name {
            get {
                return this.name;
            }

            set {
                this.name = value;

                Notify("Name");
            }
        }

        public string Host {
            get {
                return host;
            }

            set {
                this.host = value;

                Notify("Host");
            }
        }

        public int Port {
            get {
                return port;
            }

            set {
                this.port = value;

                Notify("Port");
            }
        }

        public string UserName {
            get {
                return userName;
            }

            set {
                this.userName = value;

                Notify("UserName");
            }
        }

        public SecureString Password {
            get {
                return this.password;
            }

            set {
                this.password = value;

                Notify("Password");
            }
        }

        public string OutputPath {
            get {
                return this.outputPath;
            }

            set {
                this.outputPath = value;

                Notify("OutputPath");
            }
        }

        public string ResultFormat {
            get {
                return this.resultFormat;
            }

            set {
                this.resultFormat = value;

                Notify("ResultFormat");
            }
        }

        public IEnumerable<TypedDataType> GetInputDataTypes() {
            return new[] { TypedDataType.Stream };
        }

        public IEnumerable<TypedDataType> GetOutputDataTypes(TypedDataType input) {
            return new[] { TypedDataType.Uri };
        }

        public ICommandFactory GetFactory() {
            return new FtpUploaderFactory();
        }

        public ITimeEstimate ProcessTimeEstimate {
            get {
                return TimeEstimates.LongOperation;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void Notify(string propertyName) {
            var handler = PropertyChanged;

            if (handler != null) {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public FtpUploader() {
        }

        public FtpUploader(SerializationInfo info, StreamingContext context) {
            Name = info.GetValue<string>("Name");

            Host = info.GetValue<string>("Host");
            Port = info.GetValue<int>("Port");
            UserName = info.GetValue<string>("UserName");
            Password = Security.DecryptString(info.GetValue<byte[]>("Password encrypted"));

            OutputPath = info.GetValue<string>("OutputPath");
            ResultFormat = info.GetValue<string>("ResultFormat");
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context) {
            info.AddValue("Name", Name);

            info.AddValue("Host", Host);
            info.AddValue("Port", Port);
            info.AddValue("UserName", UserName);
            info.AddValue("Password encrypted", Security.EncryptString(Password));

            info.AddValue("OutputPath", OutputPath);
            info.AddValue("ResultFormat", ResultFormat);
        }
    }
}
