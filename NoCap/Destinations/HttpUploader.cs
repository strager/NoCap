using System.Collections.Specialized;
using System.Net;
using NoCap.WebHelpers;

namespace NoCap.Destinations {
    public abstract class HttpUploader : IDestination {
        public abstract IOperation<TypedData> Put(TypedData data);

        protected abstract string GetUri();
        protected abstract NameValueCollection GetParameters(TypedData data);
        protected abstract TypedData GetResponseData(HttpWebResponse response, TypedData originalData);

        public IOperation<TypedData> Upload(TypedData originalData) {
            return new EasyOperation<TypedData>((op) => {
                var responseOp = Upload(GetUri(), GetParameters(originalData));
                responseOp.Completed += (sender, e) => {
                    op.Done(GetResponseData(e.Data, originalData));
                };

                responseOp.Start();

                return null;
            });
        }

        public static IOperation<HttpWebResponse> Upload(string uri, NameValueCollection parameters) {
            return new EasyOperation<HttpWebResponse>((op) => {
                var helper = new MultipartHelper();

                if (parameters != null) {
                    helper.Add(new NameValuePart(parameters));
                }

                var request = (HttpWebRequest)WebRequest.Create(uri);
                request.Method = "POST";
                request.ContentType = "multipart/form-data; boundary=" + helper.Boundary;

                var requestStream = request.GetRequestStream();
                helper.LoadInto(requestStream);

                request.BeginGetResponse((asyncResult) => {
                    var response = (HttpWebResponse)request.EndGetResponse(asyncResult);

                    op.Done(response);
                }, null);

                return null;
            });
        }
    }
}
