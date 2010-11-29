using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using NoCap.Library.Util;
using NoCap.Web;
using NoCap.Web.Multipart;

namespace NoCap.Library.Commands {
    [Serializable]
    public abstract class HttpUploader : ICommand {
        public abstract string Name { get; }

        public abstract TypedData Process(TypedData data, IMutableProgressTracker progress, CancellationToken cancelToken);

        public TypedData Upload(TypedData originalData, IMutableProgressTracker progress, CancellationToken cancelToken) {
            string requestMethod = RequestMethod;
            var parameters = GetParameters(originalData);

            var requestProgress = new NotifyingProgressTracker(TimeEstimates.LongOperation);
            var responseProgress = new NotifyingProgressTracker(TimeEstimates.ShortOperation);

            var aggregateProgress = new AggregateProgressTracker(requestProgress, responseProgress);
            aggregateProgress.BindTo(progress);

            var request = BuildRequest(originalData, requestMethod, parameters, requestProgress, cancelToken);
            var response = (HttpWebResponse) request.GetResponse();

            var ret = GetResponseData(response, originalData);
            
            responseProgress.Progress = 1;  // TODO HTTP download Progress

            return ret;
        }

        private HttpWebRequest BuildRequest(TypedData originalData, string requestMethod, IDictionary<string, string> parameters, IMutableProgressTracker progress, CancellationToken cancelToken) {
            switch (requestMethod) {
                case @"GET":
                    return BuildGetRequest(parameters, progress);

                case @"POST":
                    return BuildPostRequest(parameters, originalData, progress, cancelToken);

                default:
                    throw new ArgumentException("Unknown request method", "requestMethod");
            }
        }

        private HttpWebRequest BuildGetRequest(IDictionary<string, string> parameters, IMutableProgressTracker progress) {
            var uriBuilder = new UriBuilder(Uri) {
                Query = HttpUtility.ToQueryString(parameters)
            };

            var request = (HttpWebRequest) WebRequest.Create(uriBuilder.Uri);
            request.Method = @"GET";
            PreprocessRequest(request);

            progress.Progress = 1;

            return request;
        }

        private HttpWebRequest BuildPostRequest(IDictionary<string, string> parameters, TypedData originalData, IMutableProgressTracker progress, CancellationToken cancelToken) {
            var builder = new MultipartBuilder();

            if (parameters != null) {
                builder.KeyValuePairs(parameters);
            }

            PreprocessRequestData(builder, originalData);

            var boundary = builder.Boundary;

            var request = (HttpWebRequest) WebRequest.Create(Uri);
            request.Method = @"POST";
            request.ContentType = string.Format("multipart/form-data; {0}", MultipartHeader.KeyValuePair("boundary", boundary));
            PreprocessRequest(request);

            request.ContentLength = Utility.GetBoundaryByteCount(boundary) + builder.GetByteCount();

            var requestStream = request.GetRequestStream();
            var progressStream = new ProgressTrackingStreamWrapper(requestStream, request.ContentLength);
            progressStream.BindTo(progress);

            Utility.WriteBoundary(progressStream, boundary);
            builder.Write(progressStream, cancelToken);

            System.Diagnostics.Debug.Assert(progress.Progress == 1);

            return request;
        }

        protected abstract Uri Uri {
            get;
        }

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

        protected virtual string RequestMethod {
            get {
                return @"POST";
            }
        }

        protected virtual void PreprocessRequest(HttpWebRequest request) {
            // Do nothing
        }

        public abstract IEnumerable<TypedDataType> GetInputDataTypes();
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
}
