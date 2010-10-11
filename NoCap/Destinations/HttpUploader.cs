using System;
using System.Collections.Specialized;
using System.Net;
using System.Web;
using NoCap.WebHelpers;

namespace NoCap.Destinations {
    public abstract class HttpUploader : IDestination {
        public abstract IOperation<TypedData> Put(TypedData data);

        protected abstract string GetUri();
        protected abstract NameValueCollection GetParameters(TypedData data);
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

                        request = (HttpWebRequest)WebRequest.Create(uriBuilder.Uri);
                        request.Method = requestMethod;
                        PreprocessRequest(request);

                        break;

                    case @"POST":
                        var helper = new MultipartHelper();

                        if (parameters != null) {
                            helper.Add(new NameValuePart(parameters));
                        }

                        request = (HttpWebRequest)WebRequest.Create(GetUri());
                        request.Method = requestMethod;
                        request.ContentType = "multipart/form-data; boundary=" + helper.Boundary;
                        PreprocessRequest(request);

                        PreprocessRequestData(helper, originalData);

                        var requestStream = request.GetRequestStream();
                        helper.CopyTo(requestStream);

                        break;

                    default:
                        throw new InvalidOperationException("Unknown request method");
                }
                
                request.BeginGetResponse((asyncResult) => {
                    var response = (HttpWebResponse)request.EndGetResponse(asyncResult);

                    op.Done(GetResponseData(response, originalData));
                }, null);

                return null;
            });
        }

        protected virtual void PreprocessRequestData(MultipartHelper helper, TypedData originalData) {
            // Do nothing
        }

        private static string ToQueryString(NameValueCollection nvc) {
            // http://stackoverflow.com/questions/829080/how-to-build-a-query-string-for-a-url-in-c/829138#829138
            return string.Join("&", Array.ConvertAll(nvc.AllKeys, key => string.Format("{0}={1}", HttpUtility.UrlEncode(key), HttpUtility.UrlEncode(nvc[key]))));
        }

        protected virtual string GetRequestMethod() {
            return @"POST";
        }

        protected virtual void PreprocessRequest(HttpWebRequest request) {
            // Do nothing
        }
    }
}
