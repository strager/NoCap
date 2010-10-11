using System.Collections.Specialized;
using System.Net;

namespace NoCap.WebHelpers {
    public static class HttpUploader {
        public static IOperation<WebResponse> Upload(string uri, NameValueCollection parameters) {
            return new EasyOperation<WebResponse>((op) => {
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