using System;
using System.Net;
using System.Runtime.Serialization;
using System.Threading;
using NoCap.Library.Progress;
using NoCap.Library.Util;

namespace NoCap.Library.Commands {
    [DataContract(Name = "UrlShortener")]
    public abstract class UrlShortenerCommand : HttpUploaderCommand {
        public override TypedData Process(TypedData data, IMutableProgressTracker progress, CancellationToken cancelToken) {
            switch (data.DataType) {
                case TypedDataType.Uri:
                    return Upload(data, progress, cancelToken);

                default:
                    return null;
            }
        }

        protected override TypedData GetResponseData(HttpWebResponse response, TypedData originalData) {
            var urlText = HttpRequest.GetResponseText(response);

            return TypedData.FromUri(new Uri(urlText, UriKind.Absolute), originalData.Name);
        }
    }
}
