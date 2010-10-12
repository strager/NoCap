using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Web;
using System.Linq;
using NoCap.Web;

namespace NoCap.Library.Destinations {
    public abstract class HttpUploader : IDestination {
        public abstract IOperation<TypedData> Put(TypedData data);

        protected abstract string GetUri();

        protected abstract IDictionary<string, string> GetParameters(TypedData data);

        protected abstract TypedData GetResponseData(HttpWebResponse response, TypedData originalData);

        public IOperation<TypedData> Upload(TypedData originalData) {
            return new EasyOperation<TypedData>((op) => {
                string requestMethod = GetRequestMethod();
                var parameters = GetParameters(originalData);

                HttpWebRequest request;
                
                switch (requestMethod) {
                    case @"GET":
                        var uriBuilder = new UriBuilder(GetUri());
                        uriBuilder.Query = ToQueryString(parameters);

                        request = (HttpWebRequest) WebRequest.Create(uriBuilder.Uri);
                        request.Method = requestMethod;
                        PreprocessRequest(request);

                        break;

                    case @"POST":
                        var builder = new MultipartBuilder();

                        if (parameters != null) {
                            builder.KeyValuePairs(parameters);
                        }
                        
                        var boundary = builder.Boundary;

                        request = (HttpWebRequest) WebRequest.Create(GetUri());
                        request.Method = requestMethod;
                        request.ContentType = string.Format("multipart/form-data; {0}", MultipartHeader.KeyValuePair("boundary", boundary));
                        PreprocessRequest(request);

                        PreprocessRequestData(builder, originalData);

                        var requestStream = request.GetRequestStream();
                        Util.WriteBoundary(requestStream, boundary);
                        builder.Write(requestStream);

                        break;

                    default:
                        throw new InvalidOperationException("Unknown request method");
                }
                
                request.BeginGetResponse((asyncResult) => {
                    var response = (HttpWebResponse) request.EndGetResponse(asyncResult);

                    op.Done(GetResponseData(response, originalData));
                }, null);

                return null;
            });
        }

        protected virtual void PreprocessRequestData(MultipartBuilder helper, TypedData originalData) {
            // Do nothing
        }

        private static string ToQueryString(IDictionary<string, string> pairs) {
            // http://stackoverflow.com/questions/829080/how-to-build-a-query-string-for-a-url-in-c/829138#829138
            return string.Join("&", pairs.Select(
                (pair) => string.Format("{0}={1}", HttpUtility.UrlEncode(pair.Key), HttpUtility.UrlEncode(pair.Value))
            ));
        }

        protected virtual string GetRequestMethod() {
            return @"POST";
        }

        protected virtual void PreprocessRequest(HttpWebRequest request) {
            // Do nothing
        }

        public abstract IEnumerable<TypedDataType> GetInputDataTypes();
        public abstract IEnumerable<TypedDataType> GetOutputDataTypes(TypedDataType input);
    }
}
