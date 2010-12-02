using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization;
using System.Security;
using System.Text;
using System.Threading;
using Granados;
using NoCap.Extensions.Default.Factories;
using NoCap.Library;
using NoCap.Library.Progress;
using NoCap.Library.Util;

namespace NoCap.Extensions.Default.Commands {
    [Serializable]
    public sealed class SshUploader : ICommand, INotifyPropertyChanged, ISerializable {
        private string name = "SSH file uploader";

        private string host = "example.com";
        private int port = 22;
        private string userName = "foobar";
        private SecureString password;

        private string outputPath = "";
        private string resultFormat = "http://example.com/{0}";

        private int timeout = 20000;

        public TypedData Process(TypedData data, IMutableProgressTracker progress, CancellationToken cancelToken) {
            string fileName = data.Name;
            var stream = (Stream) data.Data;

            UploadData(stream, fileName, progress);

            progress.Progress = 1;

            string result;
            bool success = TryGetResultString(fileName, out result);

            return TypedData.FromUri(new Uri(success && !string.IsNullOrWhiteSpace(result) ? result : data.Name, UriKind.Absolute), fileName);
        }

        private void UploadData(Stream stream, string fileName, IMutableProgressTracker progress) {
            var reader = new Reader();

            using (var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)) {
                //socket.Blocking = true;

                socket.Connect(new IPEndPoint(IPAddress.Parse(@"127.0.0.1"), Port));

                var sshConnection = SSHConnection.Connect(
                    new SSHConnectionParameter {
                        AuthenticationType = AuthenticationType.Password,
                        UserName = UserName,
                        Password = Security.ToInsecureString(Password),
                        Protocol = SSHProtocol.SSH2,
                    },
                    reader,
                    socket
                );

                try {
                    sshConnection.AutoDisconnect = false;

                    reader._conn = sshConnection;

                    sshConnection.ExecuteSCP(new ScpParameter {
                        Direction = SCPCopyDirection.LocalToRemote,
                        LocalSource = new ScpLocalSource(@"F:\Documents\da.conf"),
                        RemoteFilename = "/C/da.conf",
                    });
                } finally {
                    sshConnection.Disconnect("");
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

        public ICommandFactory GetFactory() {
            return new SshUploaderFactory();
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

        protected void Notify(string propertyName) {
            var handler = PropertyChanged;

            if (handler != null) {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public SshUploader() {
        }

        private SshUploader(SerializationInfo info, StreamingContext context) {
            Name = info.GetValue<string>("Name");

            Host = info.GetValue<string>("Host");
            Port = info.GetValue<int>("Port");
            UserName = info.GetValue<string>("UserName");

            var encryptedPassword = info.GetValue<byte[]>("Password encrypted");
            Password = encryptedPassword == null ? null : Security.DecryptString(encryptedPassword);

            OutputPath = info.GetValue<string>("OutputPath");
            ResultFormat = info.GetValue<string>("ResultFormat");
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context) {
            info.AddValue("Name", Name);

            info.AddValue("Host", Host);
            info.AddValue("Port", Port);
            info.AddValue("UserName", UserName);
            info.AddValue("Password encrypted", Password == null ? null : Security.EncryptString(Password));

            info.AddValue("OutputPath", OutputPath);
            info.AddValue("ResultFormat", ResultFormat);
        }
    }

    class Reader : ISSHConnectionEventReceiver, ISSHChannelEventReceiver {
		public SSHConnection _conn;
		public bool _ready;

		public void OnData(byte[] data, int offset, int length) {
			System.Console.Write(Encoding.ASCII.GetString(data, offset, length));
		}
		public void OnDebugMessage(bool always_display, byte[] data) {
			Debug.WriteLine("DEBUG: "+ Encoding.ASCII.GetString(data));
		}
		public void OnIgnoreMessage(byte[] data) {
			Debug.WriteLine("Ignore: "+ Encoding.ASCII.GetString(data));
		}
		public void OnAuthenticationPrompt(string[] msg) {
			Debug.WriteLine("Auth Prompt "+msg[0]);
		}
		public void OnChannelClosed() {
			Debug.WriteLine("Channel closed");
			_conn.Disconnect("");
			//_conn.AsyncReceive(this);
		}
		public void OnChannelEOF() {
			_pf.Close();
			Debug.WriteLine("Channel EOF");
		}

        public void OnChannelError(Exception error) {
            throw new NotImplementedException();
        }

        public void OnExtendedData(int type, byte[] data) {
			Debug.WriteLine("EXTENDED DATA");
		}

        public void OnError(Exception error) {
            throw new NotImplementedException();
        }

        public void OnConnectionClosed() {
			Debug.WriteLine("Connection closed");
		}
		public void OnUnknownMessage(byte type, byte[] data) {
			Debug.WriteLine("Unknown Message " + type);
		}
		public void OnChannelReady() {
			_ready = true;
		}
		public void OnMiscPacket(byte type, byte[] data, int offset, int length) {
		}

		public PortForwardingCheckResult CheckPortForwardingRequest(string host, int port, string originator_host, int originator_port) {
			PortForwardingCheckResult r = new PortForwardingCheckResult();
			r.allowed = true;
			r.channel = this;
			return r;
		}
		public void EstablishPortforwarding(ISSHChannelEventReceiver rec, SSHChannel channel) {
			_pf = channel;
		}

		public SSHChannel _pf;
	}
}
