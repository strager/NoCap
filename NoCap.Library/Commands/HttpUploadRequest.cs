using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Cache;
using System.Text;
using System.Threading;
using NoCap.Library.Progress;
using NoCap.Web;
using NoCap.Web.Multipart;

namespace NoCap.Library.Commands {
    // TODO Clean up heavily
    public sealed class HttpUploadRequest {
        private const string UserAgent = "NoCap HttpUploader";

        public IDictionary<string, string> Parameters {
            get;
            set;
        }

        public Uri Uri {
            get;
            set;
        }

        public MultipartData PostData {
            get;
            set;
        }

        // TODO Make these not events
        // (not adhering to the convention is a reminder!)
        public event Action<object, HttpWebRequest> PreprocessRequest;

        private void OnPreprocessRequest(HttpWebRequest request) {
            var handler = PreprocessRequest;

            if (handler != null) {
                handler(this, request);
            }
        }

        public HttpWebResponse Execute(IMutableProgressTracker progress, CancellationToken cancelToken, HttpRequestMethod requestMethod) {
            var requestProgress = new MutableProgressTracker();
            var responseProgress = new MutableProgressTracker();

            var aggregateProgress = new AggregateProgressTracker(new ProgressTrackerCollection {
                { requestProgress,  TimeEstimates.LongOperation.ProgressWeight },
                { responseProgress, TimeEstimates.ShortOperation.ProgressWeight }, 
            });

            aggregateProgress.BindTo(progress);

            try {
                var request = BuildRequest(requestMethod, Parameters, requestProgress, cancelToken);

                bool canCancel = true;

                cancelToken.Register(() => {
                    if (canCancel) {
                        request.Abort();
                    }
                });

                var response = (HttpWebResponse) request.GetResponse();

                responseProgress.Progress = 1; // TODO HTTP download Progress

                canCancel = false;

                return response;
            } catch (WebException e) {
                if (e.Status == WebExceptionStatus.RequestCanceled) {
                    throw new OperationCanceledException(e.Message, e, cancelToken);
                }

                throw new OperationCanceledException(e.Message, e, cancelToken);
            }
        }

        private HttpWebRequest BuildRequest(HttpRequestMethod requestMethod, IDictionary<string, string> parameters, IMutableProgressTracker progress, CancellationToken cancelToken) {
            switch (requestMethod) {
                case HttpRequestMethod.Get:
                    return BuildGetRequest(parameters, progress);

                case HttpRequestMethod.Post:
                    return BuildPostRequest(parameters, progress, cancelToken);

                default:
                    throw new ArgumentException("Unknown request method", "requestMethod");
            }
        }

        private HttpWebRequest BuildGetRequest(IDictionary<string, string> parameters, IMutableProgressTracker progress) {
            var uriBuilder = new UriBuilder(Uri) {
                Query = HttpUtility.ToQueryString(parameters)
            };

            var request = GetRequest(uriBuilder.Uri, @"GET");
            OnPreprocessRequest(request);

            progress.Progress = 1;

            return request;
        }

        private HttpWebRequest BuildPostRequest(IDictionary<string, string> parameters, IMutableProgressTracker progress, CancellationToken cancelToken) {
            var request = GetRequest(Uri, @"POST");
            OnPreprocessRequest(request);

            var builder = new MultipartDataBuilder();

            if (PostData != null) {
                builder.Data(PostData);
            }

            if (parameters != null) {
                builder.KeyValuePairs(parameters);
            }

            WritePostData(request, builder.GetData(), progress, cancelToken);

            return request;
        }

        private static HttpWebRequest GetRequest(Uri uri, string method) {
            var request = (HttpWebRequest) WebRequest.Create(uri);
            request.CachePolicy = new RequestCachePolicy(RequestCacheLevel.BypassCache);
            request.Method = method;
            request.KeepAlive = false;
            request.AllowWriteStreamBuffering = false;
            request.UserAgent = UserAgent;

            return request;
        }

        private static void WritePostData(HttpWebRequest request, MultipartData data, IMutableProgressTracker progress, CancellationToken cancelToken) {
            var boundary = data.Boundary;

            request.ContentType = string.Format("multipart/form-data; {0}", MultipartHeader.KeyValuePair("boundary", boundary));
            request.ContentLength = Utility.GetBoundaryByteCount(boundary) + data.GetByteCount();

            using (var requestStream = request.GetRequestStream())
            using (var progressStream = new ProgressTrackingStreamWrapper(requestStream, request.ContentLength)) {
                progressStream.BindTo(progress);

                Utility.WriteBoundary(progressStream, boundary);
                data.Write(progressStream, cancelToken);
            }

            System.Diagnostics.Debug.Assert(progress.Progress == 1);
        }

        public static string GetResponseText(HttpWebResponse response) {
            if (response == null) {
                throw new ArgumentNullException("response");
            }

            var stream = response.GetResponseStream();

            if (stream == null) {
                throw new ArgumentException("Response stream should not be null", "response");
            }

            var encoding = Encoding.UTF8;   // Default encoding

            // FIXME Do this without exceptions
            try {
                if (response.CharacterSet != null) {
                    encoding = Encoding.GetEncoding(response.CharacterSet);
                }
            } catch (ArgumentException) {
                // Bad character set given; ignore exception
            }

            using (var reader = new StreamReader(stream, encoding)) {
                return reader.ReadToEnd();
            }
        }
    }
}