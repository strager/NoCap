using System.Collections.Generic;
using System.Net;
using NoCap.Library.Util;

namespace NoCap.Library.Commands {
    public abstract class UrlShortener : HttpUploader {
        public override TypedData Process(TypedData data, IMutableProgressTracker progress) {
            switch (data.DataType) {
                case TypedDataType.Uri:
                    return Upload(data, progress);

                default:
                    return null;
            }
        }

        protected override TypedData GetResponseData(HttpWebResponse response, TypedData originalData) {
            var urlText = GetResponseText(response);

            return TypedData.FromUri(urlText, originalData.Name);
        }

        public override IEnumerable<TypedDataType> GetInputDataTypes() {
            return new[] { TypedDataType.Uri };
        }

        public override IEnumerable<TypedDataType> GetOutputDataTypes(TypedDataType input) {
            return new[] { TypedDataType.Uri };
        }
    }
}
