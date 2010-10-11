﻿using System.Net;

namespace NoCap.Destinations {
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
    }
}
