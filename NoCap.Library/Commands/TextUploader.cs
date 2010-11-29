using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using NoCap.Library.Util;

namespace NoCap.Library.Commands {
    [Serializable]
    public abstract class TextUploader : HttpUploader {
        public override TypedData Process(TypedData data, IMutableProgressTracker progress, CancellationToken cancelToken) {
            switch (data.DataType) {
                case TypedDataType.Text:
                    return Upload(data, progress, cancelToken);

                default:
                    return null;
            }
        }

        protected override TypedData GetResponseData(HttpWebResponse response, TypedData originalData) {
            return TypedData.FromUri(response.ResponseUri, originalData.Name);
        }

        public override IEnumerable<TypedDataType> GetInputDataTypes() {
            return new[] { TypedDataType.Text };
        }
    }
}
