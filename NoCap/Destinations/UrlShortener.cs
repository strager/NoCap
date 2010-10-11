﻿using System;
using System.IO;
using System.Net;
using System.Text;

namespace NoCap.Destinations {
    public abstract class UrlShortener : HttpUploader {
        public override IOperation<TypedData> Put(TypedData data) {
            switch (data.Type) {
                case TypedDataType.Uri:
                    return Upload(data);

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
    }
}
