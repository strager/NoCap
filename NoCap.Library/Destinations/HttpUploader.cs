using System;
using System.Collections.Generic;
using System.Net;
using NoCap.Library.Util;
using NoCap.Web;
using NoCap.Web.Multipart;

namespace NoCap.Library.Destinations {
    public abstract class HttpUploader : IDestination {
        public abstract TypedData Put(TypedData data, IMutableProgressTracker progress);

        public TypedData Upload(TypedData originalData, IMutableProgressTracker progress) {
            string requestMethod = RequestMethod;
            var parameters = GetParameters(originalData);

            var requestProgress = new NotifyingProgressTracker();
            var responseProgress = new NotifyingProgressTracker();

            var aggregateProgress = new AggregateProgressTracker(requestProgress, responseProgress);
            aggregateProgress.BindTo(progress);

            var request = BuildRequest(originalData, requestMethod, parameters, requestProgress);

            var response = (HttpWebResponse) request.GetResponse();
            responseProgress.Progress = 1;  // TODO

            return GetResponseData(response, originalData);
        }

        private HttpWebRequest BuildRequest(TypedData originalData, string requestMethod, IDictionary<string, string> parameters, IMutableProgressTracker progress) {
            switch (requestMethod) {
                case @"GET":
                    return BuildGetRequest(parameters, progress);

                case @"POST":
                    return BuildPostRequest(parameters, originalData, progress);

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

        private HttpWebRequest BuildPostRequest(IDictionary<string, string> parameters, TypedData originalData, IMutableProgressTracker progress) {
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
            builder.Write(progressStream);

            System.Diagnostics.Debug.Assert(progress.Progress == 1);

            return request;
        }

        protected abstract Uri Uri {
            get;
        }

        protected abstract IDictionary<string, string> GetParameters(TypedData data);

        protected abstract TypedData GetResponseData(HttpWebResponse response, TypedData originalData);

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
        public abstract IEnumerable<TypedDataType> GetOutputDataTypes(TypedDataType input);
    }
}
