using System.Collections.Generic;
using System.Net;

namespace NoCap.Library.Destinations {
    public abstract class TextUploader : HttpUploader {
        public override IOperation<TypedData> Put(TypedData data) {
            switch (data.Type) {
                case TypedDataType.Text:
                    return Upload(data);

                default:
                    return null;
            }
        }

        protected override TypedData GetResponseData(HttpWebResponse response, TypedData originalData) {
            return TypedData.FromUri(response.ResponseUri.OriginalString, originalData.Name);
        }

        public override IEnumerable<TypedDataType> GetInputDataTypes() {
            return new[] {
                TypedDataType.Text
            };
        }

        public override System.Collections.Generic.IEnumerable<TypedDataType> GetOutputDataTypes(TypedDataType input) {
            return new[] { TypedDataType.Uri };
        }
    }
}
