using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Cache;
using System.Text;
using System.Threading;
using System.Xml;
using Newtonsoft.Json.Linq;
using NoCap.Library.Progress;
using NoCap.Web;
using NoCap.Web.Multipart;

namespace NoCap.Library.Util {
    public enum HttpRequestMethod {
        Get,
        Post,
    }

    public static class HttpRequest {
        private const string UserAgent = "NoCap HttpUploader";

        public static HttpWebResponse Execute(Uri uri, Stream requestData, IMutableProgressTracker progress, CancellationToken cancelToken) {
            // QUICK HACK

            var requestProgress = new MutableProgressTracker();
            var responseProgress = new MutableProgressTracker();

            var aggregateProgress = new AggregateProgressTracker(new ProgressTrackerCollection {
                { requestProgress,  TimeEstimates.LongOperation.ProgressWeight },
                { responseProgress, TimeEstimates.ShortOperation.ProgressWeight }, 
            });

            aggregateProgress.BindTo(progress);

            try {
                var request = CreateRequest(uri, @"POST");
                request.ContentLength = requestData.Length;

                using (var requestStream = request.GetRequestStream())
                using (var progressStream = new ProgressTrackingStreamWrapper(requestStream, request.ContentLength)) {
                    progressStream.BindTo(requestProgress);

                    requestData.CopyTo(progressStream, cancelToken);
                }

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

        public static HttpWebResponse Execute(Uri uri, MultipartData requestData, HttpRequestMethod requestMethod, IMutableProgressTracker progress, CancellationToken cancelToken) {
            var requestProgress = new MutableProgressTracker();
            var responseProgress = new MutableProgressTracker();

            var aggregateProgress = new AggregateProgressTracker(new ProgressTrackerCollection {
                { requestProgress,  TimeEstimates.LongOperation.ProgressWeight },
                { responseProgress, TimeEstimates.ShortOperation.ProgressWeight }, 
            });

            aggregateProgress.BindTo(progress);

            try {
                var request = BuildRequest(uri, requestMethod, requestData, requestProgress, cancelToken);

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

        private static HttpWebRequest BuildRequest(Uri uri, HttpRequestMethod requestMethod, MultipartData data, IMutableProgressTracker progress, CancellationToken cancelToken) {
            switch (requestMethod) {
                case HttpRequestMethod.Get:
                    return BuildGetRequest(uri, data, progress, cancelToken);

                case HttpRequestMethod.Post:
                    return BuildPostRequest(uri, data, progress, cancelToken);

                default:
                    throw new ArgumentException("Unknown request method", "requestMethod");
            }
        }

        private static HttpWebRequest BuildGetRequest(Uri uri, MultipartData data, IMutableProgressTracker progress, CancellationToken cancelToken) {
            if (data != null) {
                var parameters = new Dictionary<string, string>();

                foreach (var entry in data.Entries) {
                    var formEntry = entry as FormMultipartEntry;

                    if (formEntry == null) {
                        throw new ArgumentException("Data must contain only form entries", "data");
                    }

                    parameters[formEntry.Name] = formEntry.Value;
                }

                var uriBuilder = new UriBuilder(uri) {
                    Query = HttpUtility.ToQueryString(parameters)
                };

                uri = uriBuilder.Uri;
            }

            var request = CreateRequest(uri, @"GET");

            progress.Progress = 1;

            return request;
        }

        private static HttpWebRequest BuildPostRequest(Uri uri, MultipartData data, IMutableProgressTracker progress, CancellationToken cancelToken) {
            var request = CreateRequest(uri, @"POST");

            if (data != null) {
                WritePostData(request, data, progress, cancelToken);
            }

            progress.Progress = 1;

            return request;
        }

        private static HttpWebRequest CreateRequest(Uri uri, string method) {
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

            request.ContentType = string.Format(@"multipart/form-data; {0}", MultipartHeader.KeyValuePair("boundary", boundary));
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

        public static XmlDocument GetResponseXml(HttpWebResponse response) {
            string responseText = GetResponseText(response);

            var document = new XmlDocument();
            document.LoadXml(responseText);

            return document;
        }

        public static JObject GetResponseJson(HttpWebResponse response) {
            string responseText = GetResponseText(response);

            return JObject.Parse(responseText);
        }
    }
}
