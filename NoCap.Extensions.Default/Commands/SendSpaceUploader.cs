using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.Security;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Xml;
using NoCap.Extensions.Default.Factories;
using NoCap.Library;
using NoCap.Library.Progress;
using NoCap.Library.Util;
using NoCap.Web;
using NoCap.Web.Multipart;

namespace NoCap.Extensions.Default.Commands {
    [DataContract(Name = "SendSpaceUploader")]
    class SendSpaceUploader : ICommand, IExtensibleDataObject {
        private static readonly Uri ApiUri = new Uri("http://api.sendspace.com/rest/", UriKind.Absolute);
        private const string OutputFileUriFormat = "http://www.sendspace.com/file/{0}";

        public string Name {
            get { return "SendSpace uploader"; }
        }

        [DataMember(Name = "UserName")]
        public string UserName { get; set; }

        [IgnoreDataMember]
        public SecureString Password { get; set; }

        [DataMember(Name = "Password")]
        public byte[] EncryptedPassword {
            get {
                return Password == null ? null : Security.EncryptString(Password);
            }

            set {
                Password = value == null ? null : Security.DecryptString(value);
            }
        }

        public TypedData Process(TypedData data, IMutableProgressTracker progress, CancellationToken cancelToken) {
            if (string.IsNullOrEmpty(UserName)) {
                throw new InvalidOperationException("User name must be set");
            }

            if (Password == null) {
                throw new InvalidOperationException("Password must be set");
            }

            var progressTrackerCollection = new ProgressTrackerCollection {
                { new MutableProgressTracker(), 1 },
                { new MutableProgressTracker(), 1 },
                { new MutableProgressTracker(), 2 },
                { new MutableProgressTracker(), 100 },
            };

            var progressTrackers = progressTrackerCollection.Select((item) => item.ProgressTracker).OfType<IMutableProgressTracker>().ToArray();

            var aggregateProgressTracker = new AggregateProgressTracker(progressTrackerCollection);
            aggregateProgressTracker.BindTo(progress);

            string token = FetchToken(progressTrackers[0], cancelToken);
            string sessionKey = FetchSessionKey(token, progressTrackers[1], cancelToken);

            string uploadId, uploadInfo;
            ulong maxFileSize;
            var uploadUri = FetchUploadUri(sessionKey, out uploadId, out uploadInfo, out maxFileSize, progressTrackers[2], cancelToken);

            var uri = UploadData(data, uploadUri, uploadId, uploadInfo, maxFileSize, progressTrackers[3], cancelToken);

            return TypedData.FromUri(uri, data.Name);
        }

        private static Uri FetchUploadUri(string sessionKey, out string uploadId, out string uploadInfo, out ulong maxFileSize, IMutableProgressTracker progress, CancellationToken cancelToken) {
            var response = HttpRequest.Execute(new UriBuilder(ApiUri) {
                Query = HttpUtility.ToQueryString(new Dictionary<string, string> {
                    { "method", "upload.getInfo" },
                    { "session_key", sessionKey },
                }),
            }.Uri, null, HttpRequestMethod.Get, progress, cancelToken);

            var xmlDoc = GetXml(response);

            uploadId = xmlDoc.SelectSingleNode("/result/upload/@upload_identifier").InnerText;
            uploadInfo = xmlDoc.SelectSingleNode("/result/upload/@extra_info").InnerText;
            maxFileSize = ulong.Parse(xmlDoc.SelectSingleNode("/result/upload/@max_file_size").InnerText);

            return new Uri(xmlDoc.SelectSingleNode("/result/upload/@url").InnerText, UriKind.Absolute);
        }

        private static Uri UploadData(TypedData data, Uri uri, string uploadId, string uploadInfo, ulong maxFileSize, IMutableProgressTracker progress, CancellationToken cancelToken) {
            var postData = new MultipartDataBuilder()
                .KeyValuePair("MAX_FILE_SIZE", maxFileSize.ToString())
                .KeyValuePair("UPLOAD_IDENTIFIER", uploadId)
                .KeyValuePair("extra_info", uploadInfo)
                .File((Stream) data.Data, "userfile", data.Name)
                .GetData();

            var response = HttpRequest.Execute(uri, postData, HttpRequestMethod.Post, progress, cancelToken);

            string text = HttpRequest.GetResponseText(response);
            var match = new Regex(@"\bfile_id=(?<FileId>\S+)\b").Match(text);

            return new Uri(string.Format(OutputFileUriFormat, match.Groups["FileId"].Value), UriKind.Absolute);
        }

        private static string FetchToken(IMutableProgressTracker progress, CancellationToken cancelToken) {
            var response = HttpRequest.Execute(new UriBuilder(ApiUri) {
                Query = HttpUtility.ToQueryString(new Dictionary<string, string> {
                    { "method", "auth.createtoken" },
                    { "api_key", Properties.Settings.Default.sendSpaceApiKey },
                    { "api_version", "1.0" },
                    { "response_format", "XML" },
                    { "app_version", "NoCap" },
                }),
            }.Uri, null, HttpRequestMethod.Get, progress, cancelToken);

            var xmlDoc = GetXml(response);

            return xmlDoc.SelectSingleNode("/result/token").InnerText;
        }

        private string FetchSessionKey(string token, IMutableProgressTracker progress, CancellationToken cancelToken) {
            var response = HttpRequest.Execute(new UriBuilder(ApiUri) {
                Query = HttpUtility.ToQueryString(new Dictionary<string, string> {
                    { "method", "auth.login" },
                    { "token", token },
                    { "user_name", UserName },
                    { "tokened_password", GetTokenedPassword(token, Password) },
                }),
            }.Uri, null, HttpRequestMethod.Get, progress, cancelToken);

            var xmlDoc = GetXml(response);

            return xmlDoc.SelectSingleNode("/result/session_key").InnerText;
        }

        private static XmlDocument GetXml(HttpWebResponse response) {
            var text = HttpRequest.GetResponseText(response);
            var xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(text);

            switch (xmlDoc.SelectSingleNode("/result/@status").InnerText) {
                case "ok":
                    return xmlDoc;

                case "fail":
                default:
                    string error = xmlDoc.SelectNodes("/result/error/@text").OfType<XmlAttribute>()
                        .Select((attr) => attr.InnerText).FirstOrDefault();

                    if (error == null) {
                        error = xmlDoc.InnerText;
                    }

                    throw new Exception(error);
            }
        }

        private static string GetTokenedPassword(string token, SecureString password) {
            // Spec: http://www.sendspace.com/dev_method.html?method=auth.login
            // lowercase(md5(token+lowercase(md5(password))))

            return MD5(token + MD5(Security.ToInsecureString(password)).ToLowerInvariant()).ToLowerInvariant();
        }

        static public string MD5(string input) {
            var encoding = Encoding.UTF8;
            var md5 = System.Security.Cryptography.MD5.Create();

            byte[] hashBytes = md5.ComputeHash(encoding.GetBytes(input));

            return string.Join("", hashBytes.Select((b) => b.ToString("X2")));
        }

        public ICommandFactory GetFactory() {
            return new SendSpaceUploaderFactory();
        }

        public ITimeEstimate ProcessTimeEstimate {
            get { return TimeEstimates.LongOperation; }
        }

        public bool IsValid() {
            return true;
        }

        ExtensionDataObject IExtensibleDataObject.ExtensionData { get; set; }
    }
}
