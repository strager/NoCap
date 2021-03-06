﻿using System;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Net;
using System.Runtime.Serialization;
using System.Security;
using System.Threading;
using AlexPilotti.FTPS.Client;
using NoCap.Extensions.Default.Factories;
using NoCap.Library;
using NoCap.Library.Progress;
using NoCap.Library.Util;
using NoCap.Web;
using StringLib;

namespace NoCap.Extensions.Default.Commands {
    [DataContract(Name = "FtpUploader")]
    public sealed class FtpUploader : ICommand, INotifyPropertyChanged, IExtensibleDataObject {
        private string name = "FTP file uploader";

        private string host = "example.com";
        private int port = 21;
        private string userName = "foobar";
        private SecureString password;

        private string outputPath = "";
        private string resultFormat = "http://example.com/{filename}";

        private readonly static HartFormatter.FormatterOptions ResultStringFormatterOptions;

        static FtpUploader() {
            ResultStringFormatterOptions = HartFormatter.FormatterOptions.HumaneOptions;
            ResultStringFormatterOptions.FormatProvider = CultureInfo.InvariantCulture;
        }

        private const int Timeout = 20000; // Milliseconds

        public TypedData Process(TypedData data, IMutableProgressTracker progress, CancellationToken cancelToken) {
            string fileName = data.Name;
            var stream = (Stream) data.Data;

            UploadData(stream, fileName, progress, cancelToken);

            progress.Progress = 1;

            string result;
            bool success = TryGetResultString(fileName, out result);

            return TypedData.FromUri(new Uri(success && !string.IsNullOrWhiteSpace(result) ? result : data.Name, UriKind.Absolute), fileName);
        }

        private void UploadData(Stream stream, string fileName, IMutableProgressTracker progress, CancellationToken cancelToken) {
            using (var client = new FTPSClient()) {
                try {
                    progress.Status = "Connecting to FTP";

                    client.Connect(Host, Port, new NetworkCredential(UserName, Password), 0, null, null, 0, 0, 0, Timeout);
                } catch (TimeoutException e) {
                    throw new CommandCanceledException(this, "Connection to FTP server timed out", e);
                } catch (IOException e) {
                    throw new CommandCanceledException(this, "Connection to FTP server failed", e);
                }
                
                using (var outStream = client.PutFile(GetRemotePathName(fileName)))
                using (var outStreamWrapper = new ProgressTrackingStreamWrapper(outStream, stream.Length)) {
                    outStreamWrapper.BindTo(progress);

                    progress.Status = "Uploading file to FTP";

                    stream.CopyTo(outStreamWrapper, cancelToken);
                }
            }
        }

        private bool TryGetResultString(string fileName, out string result) {
            try {
                result = this.resultFormat.HartFormat(new {
                    filename = fileName
                }, ResultStringFormatterOptions);

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

        [DataMember(Name = "Host")]
        public string Host {
            get {
                return host;
            }

            set {
                this.host = value;

                Notify("Host");
            }
        }

        [DataMember(Name = "Port")]
        public int Port {
            get {
                return port;
            }

            set {
                this.port = value;

                Notify("Port");
            }
        }

        [DataMember(Name = "UserName")]
        public string UserName {
            get {
                return userName;
            }

            set {
                this.userName = value;

                Notify("UserName");
            }
        }

        [IgnoreDataMember]
        public SecureString Password {
            get {
                return this.password;
            }

            set {
                this.password = value;

                Notify("Password");
            }
        }

        [DataMember(Name = "Password")]
        public byte[] EncryptedPassword {
            get {
                return Password == null ? null : Security.EncryptString(Password);
            }

            set {
                Password = value == null ? null : Security.DecryptString(value);
            }
        }

        [DataMember(Name = "OutputPath")]
        public string OutputPath {
            get {
                return this.outputPath;
            }

            set {
                this.outputPath = value;

                Notify("OutputPath");
            }
        }

        [DataMember(Name = "ResultFormat")]
        public string ResultFormat {
            get {
                return this.resultFormat;
            }

            set {
                this.resultFormat = value;

                Notify("ResultFormat");
            }
        }

        public ICommandFactory GetFactory() {
            return new FtpUploaderFactory();
        }

        public ITimeEstimate ProcessTimeEstimate {
            get {
                return TimeEstimates.LongOperation;
            }
        }

        public bool IsValid() {
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
