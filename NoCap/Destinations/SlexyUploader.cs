using System;
using System.Collections.Specialized;
using System.Net;
using NoCap.WebHelpers;

namespace NoCap.Destinations {
    public class SlexyUploader : IDestination {
        public IOperation Put(TypedData data) {
            switch (data.Type) {
                case TypedDataType.Text:
                    return new EasyOperation((op) => {
                        var parameters = new NameValueCollection();
                        parameters["raw_paste"] = data.Data.ToString();
                        parameters["comment"] = "";
                        parameters["author"] = Author ?? "";
                        parameters["language"] = Language ?? "text";
                        parameters["permissions"] = IsPrivate ? "1" : "0";
                        parameters["desc"] = data.Name ?? "";
                        parameters["linenumbers"] = ShowLineNumbers ? "0" : "1";
                        parameters["expire"] = Expiration.TotalSeconds.ToString(); // TODO Format provider
                        parameters["submit"] = "Submit Paste";
                        parameters["tabbing"] = "true";
                        parameters["tabtype"] = "real";

                        var helper = new MultipartHelper();
                        helper.Add(new NameValuePart(parameters));

                        var request = (HttpWebRequest)WebRequest.Create(@"http://slexy.org/index.php/submit");
                        request.Method = "POST";
                        request.ContentType = "multipart/form-data; boundary=" + helper.Boundary;

                        var requestStream = request.GetRequestStream();
                        helper.LoadInto(requestStream);

                        request.BeginGetResponse((asyncResult) => {
                            var response = (HttpWebResponse)request.EndGetResponse(asyncResult);

                            op.Done(TypedData.FromUri(response.ResponseUri.OriginalString, data.Name));
                        }, null);

                        return null;
                    });

                default:
                    return null;
            }
        }

        protected TimeSpan Expiration {
            get;
            set;
        }

        protected bool ShowLineNumbers {
            get;
            set;
        }

        protected bool IsPrivate {
            get;
            set;
        }

        protected string Language {
            get;
            set;
        }

        protected string Author {
            get;
            set;
        }
    }
}
