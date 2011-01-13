using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Threading;
using NoCap.Extensions.Default.Factories;
using NoCap.Library;
using NoCap.Library.Progress;
using NoCap.Library.Util;
using NoCap.Web;
using NoCap.Web.Multipart;

namespace NoCap.Extensions.Default.Commands {
    class GalleryAccessData {
        public string EditorId {
            get;
            set;
        }

        public string Key {
            get;
            set;
        }
    }

    [DataContract(Name = "MinUsUploader")]
    class MinUsUploader : ICommand {
        public string Name {
            get { return "min.us uploader"; }
        }

        private static readonly string ApiUriFormat = "http://min.us/api/{0}";
        private static readonly string DownloadUriFormat = "http://min.us/i{0}";

        protected MultipartData GetRequestData(TypedData data) {
            return new MultipartDataBuilder()
                .File((Stream) data.Data, null, data.Name)
                .GetData();
        }

        public TypedData Process(TypedData data, IMutableProgressTracker progress, CancellationToken cancelToken) {
            progress.Status = "Uploading data";

            var galleryProgress = new MutableProgressTracker();
            var uploadProgress = new MutableProgressTracker();

            var aggregateProgress = new AggregateProgressTracker(new ProgressTrackerCollection {
                { galleryProgress, TimeEstimates.ShortOperation.ProgressWeight },
                { uploadProgress,  TimeEstimates.LongOperation.ProgressWeight },
            });

            aggregateProgress.BindTo(progress);

            var galleryAccessData = CreateGallery(galleryProgress, cancelToken);

            return UploadData(galleryAccessData, data, uploadProgress, cancelToken);
        }

        private static TypedData UploadData(GalleryAccessData accessData, TypedData data, IMutableProgressTracker progress, CancellationToken cancelToken) {
            var uri = new UriBuilder(string.Format(ApiUriFormat, "UploadItem")) {
                Query = HttpUtility.ToQueryString(new Dictionary<string, string> {
                    { "editor_id", accessData.EditorId },
                    { "key", accessData.Key },
                    { "filename", data.Name },
                }),
            }.Uri;

            var response = HttpRequest.Execute(uri, (Stream) data.Data, progress, cancelToken);
            var json = HttpRequest.GetResponseJson(response);

            string id = json.Value<string>("id");

            // min.us pulls this bullshit where everything after and including the last '.'
            // in the filename is appended to the URL.
            // =|
            string extension = Path.GetExtension(data.Name);

            if (!string.IsNullOrEmpty(extension)) {
                id += extension;
            }

            return TypedData.FromUri(string.Format(DownloadUriFormat, HttpUtility.UrlEncode(id)), data.Name);
        }

        private static GalleryAccessData CreateGallery(IMutableProgressTracker progress, CancellationToken cancelToken) {
            var uri = new Uri(string.Format(ApiUriFormat, "CreateGallery"));

            var response = HttpRequest.Execute(uri, null, HttpRequestMethod.Post, progress, cancelToken);
            var json = HttpRequest.GetResponseJson(response);

            return new GalleryAccessData {
                EditorId = json.Value<string>("editor_id"),
                Key = json.Value<string>("key"),
            };
        }

        public ICommandFactory GetFactory() {
            return new MinUsUploaderFactory();
        }

        public ITimeEstimate ProcessTimeEstimate {
            get { return TimeEstimates.LongOperation; }
        }

        public bool IsValid() {
            return true;
        }
    }
}
