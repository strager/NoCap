﻿using System.Collections.Generic;
using System.Net;
using NoCap.Library.Util;

namespace NoCap.Library.Processors {
    public abstract class TextUploader : HttpUploader {
        public override TypedData Process(TypedData data, IMutableProgressTracker progress) {
            switch (data.DataType) {
                case TypedDataType.Text:
                    return Upload(data, progress);

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

        public override System.Collections.Generic.IEnumerable<TypedDataType> GetOutputDataTypes(TypedDataType input) {
            return new[] { TypedDataType.Uri };
        }
    }
}