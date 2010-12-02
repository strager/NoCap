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
    [Serializable]
    public abstract class HttpUploader : ICommand {
        private const string UserAgent = "NoCap HttpUploader";

        public abstract string Name { get; }

        public abstract TypedData Process(TypedData data, IMutableProgressTracker progress, CancellationToken cancelToken);

        public TypedData Upload(TypedData originalData, IMutableProgressTracker progress, CancellationToken cancelToken) {
            var parameters = GetParameters(originalData);

            var requestProgress = new MutableProgressTracker();
            var responseProgress = new MutableProgressTracker();

            var aggregateProgress = new AggregateProgressTracker(new[] {
                new AggregateProgressTrackerInformation(requestProgress,  TimeEstimates.LongOperation.ProgressWeight), 
                new AggregateProgressTrackerInformation(responseProgress, TimeEstimates.ShortOperation.ProgressWeight), 
            });

            aggregateProgress.BindTo(progress);

            try {
                var request = BuildRequest(originalData, RequestMethod, parameters, requestProgress, cancelToken);

                bool canCancel = true;

                cancelToken.Register(() => {
                    if (canCancel) {
                        request.Abort();
                    }
                });
                
                var response = (HttpWebResponse) request.GetResponse();
                var ret = GetResponseData(response, originalData);

                responseProgress.Progress = 1; // TODO HTTP download Progress

                canCancel = false;

                return ret;
            } catch (WebException e) {
                if (e.Status == WebExceptionStatus.RequestCanceled) {
                    throw new CommandCanceledException(this, null, e, cancelToken);
                }

                throw new CommandCanceledException(this, e.Message, e, cancelToken);
            }
        }

        private HttpWebRequest BuildRequest(TypedData originalData, HttpRequestMethod requestMethod, IDictionary<string, string> parameters, IMutableProgressTracker progress, CancellationToken cancelToken) {
            switch (requestMethod) {
                case HttpRequestMethod.Get:
                    return BuildGetRequest(parameters, progress);

                case HttpRequestMethod.Post:
                    return BuildPostRequest(parameters, originalData, progress, cancelToken);

                default:
                    throw new ArgumentException("Unknown request method", "requestMethod");
            }
        }

        private HttpWebRequest BuildGetRequest(IDictionary<string, string> parameters, IMutableProgressTracker progress) {
            var uriBuilder = new UriBuilder(Uri) {
                Query = HttpUtility.ToQueryString(parameters)
            };

            var request = GetRequest(uriBuilder.Uri, @"GET");
            PreprocessRequest(request);

            progress.Progress = 1;

            return request;
        }

        private HttpWebRequest BuildPostRequest(IDictionary<string, string> parameters, TypedData originalData, IMutableProgressTracker progress, CancellationToken cancelToken) {
            var request = GetRequest(Uri, @"POST");
            PreprocessRequest(request);

            var builder = new MultipartBuilder();

            if (parameters != null) {
                builder.KeyValuePairs(parameters);
            }

            PreprocessRequestData(builder, originalData);

            WritePostData(request, builder, progress, cancelToken);

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

        private static void WritePostData(HttpWebRequest request, MultipartBuilder builder, IMutableProgressTracker progress, CancellationToken cancelToken) {
            var boundary = builder.Boundary;

            request.ContentType = string.Format("multipart/form-data; {0}", MultipartHeader.KeyValuePair("boundary", boundary));
            request.ContentLength = Utility.GetBoundaryByteCount(boundary) + builder.GetByteCount();

            using (var requestStream = request.GetRequestStream())
            using (var progressStream = new ProgressTrackingStreamWrapper(requestStream, request.ContentLength)) {
                progressStream.BindTo(progress);

                Utility.WriteBoundary(progressStream, boundary);
                builder.Write(progressStream, cancelToken);
            }

            System.Diagnostics.Debug.Assert(progress.Progress == 1);
        }

        protected abstract Uri Uri { get; }

        protected abstract IDictionary<string, string> GetParameters(TypedData data);

        protected abstract TypedData GetResponseData(HttpWebResponse response, TypedData originalData);

        protected string GetResponseText(HttpWebResponse response) {
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

        protected virtual void PreprocessRequestData(MultipartBuilder helper, TypedData originalData) {
            // Do nothing
        }

        protected virtual HttpRequestMethod RequestMethod {
            get {
                return HttpRequestMethod.Post;
            }
        }

        protected virtual void PreprocessRequest(HttpWebRequest request) {
            // Do nothing
        }

        public abstract ICommandFactory GetFactory();

        public ITimeEstimate ProcessTimeEstimate {
            get {
                return TimeEstimates.LongOperation;
            }
        }

        public virtual bool IsValid() {
            return true;
        }
    }

    public enum HttpRequestMethod {
        Get,
        Post,
    }
}
