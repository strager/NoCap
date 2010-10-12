using System;
using System.Collections.Generic;
using System.Net;
using NoCap.Web;

namespace NoCap.Library.Destinations {
    public abstract class HttpUploader : IDestination {
        public abstract TypedData Put(TypedData data, IProgressTracker progress);

        public TypedData Upload(TypedData originalData, IProgressTracker progress) {
            string requestMethod = RequestMethod;
            var parameters = GetParameters(originalData);

            var request = BuildRequest(originalData, requestMethod, parameters);

            var response = (HttpWebResponse) request.GetResponse();

            return GetResponseData(response, originalData);
        }

        private HttpWebRequest BuildRequest(TypedData originalData, string requestMethod, IDictionary<string, string> parameters) {
            switch (requestMethod) {
                case @"GET":
                    return BuildGetRequest(parameters);

                case @"POST":
                    return BuildPostRequest(parameters, originalData);

                default:
                    throw new InvalidOperationException("Unknown request method");
            }
        }

        private HttpWebRequest BuildGetRequest(IDictionary<string, string> parameters) {
            var uriBuilder = new UriBuilder(Uri) {
                Query = Utility.ToQueryString(parameters)
            };

            var request = (HttpWebRequest) WebRequest.Create(uriBuilder.Uri);
            request.Method = @"GET";
            PreprocessRequest(request);

            return request;
        }

        private HttpWebRequest BuildPostRequest(IDictionary<string, string> parameters, TypedData originalData) {
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

            var requestStream = request.GetRequestStream();
            Utility.WriteBoundary(requestStream, boundary);
            builder.Write(requestStream);

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
