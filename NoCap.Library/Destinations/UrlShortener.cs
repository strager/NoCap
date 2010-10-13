using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using NoCap.Library.Util;

namespace NoCap.Library.Destinations {
    public abstract class UrlShortener : HttpUploader {
        public override TypedData Put(TypedData data, IMutableProgressTracker progress) {
            switch (data.DataType) {
                case TypedDataType.Uri:
                    return Upload(data, progress);

                default:
                    return null;
            }
        }

        protected override TypedData GetResponseData(HttpWebResponse response, TypedData originalData) {
            var stream = response.GetResponseStream();
            
            if (stream == null) {
                throw new InvalidOperationException("Response stream should not be null");
            }

            using (var reader = new StreamReader(stream, Encoding.UTF8)) {  // FIXME should this be UTF-8?
                return TypedData.FromUri(reader.ReadToEnd(), originalData.Name);
            }
        }

        public override IEnumerable<TypedDataType> GetInputDataTypes() {
            return new[] { TypedDataType.Uri };
        }

        public override IEnumerable<TypedDataType> GetOutputDataTypes(TypedDataType input) {
            return new[] { TypedDataType.Uri };
        }
    }
}
