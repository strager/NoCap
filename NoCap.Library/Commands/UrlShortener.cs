using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using NoCap.Library.Util;

namespace NoCap.Library.Commands {
    [Serializable]
    public abstract class UrlShortener : HttpUploader {
        public override TypedData Process(TypedData data, IMutableProgressTracker progress, CancellationToken cancelToken) {
            switch (data.DataType) {
                case TypedDataType.Uri:
                    return Upload(data, progress);

                default:
                    return null;
            }
        }

        protected override TypedData GetResponseData(HttpWebResponse response, TypedData originalData) {
            var urlText = GetResponseText(response);

            return TypedData.FromUri(new Uri(urlText, UriKind.Absolute), originalData.Name);
        }

        public override IEnumerable<TypedDataType> GetInputDataTypes() {
            return new[] { TypedDataType.Uri };
        }
    }
}
