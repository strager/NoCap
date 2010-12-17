using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using NoCap.Extensions.Default.Factories;
using NoCap.Library;
using NoCap.Library.Commands;
using NoCap.Web.Multipart;

namespace NoCap.Extensions.Default.Commands {
    [DataContract(Name = "IsgdShortener")]
    public sealed class IsgdShortener : UrlShortenerCommand, IExtensibleDataObject {
        public override string Name {
            get { return "is.gd URL shortener"; }
        }

        protected override Uri Uri {
            get {
                return new Uri(@"http://is.gd/api.php");
            }
        }

        protected override MultipartData GetRequestData(TypedData data) {
            var builder = new MultipartDataBuilder();
            builder.KeyValuePair("longurl", data.Data.ToString());

            return builder.GetData();
        }

        protected override HttpRequestMethod RequestMethod {
            get { return HttpRequestMethod.Get; }
        }

        public override ICommandFactory GetFactory() {
            return new IsgdShortenerFactory();
        }

        ExtensionDataObject IExtensibleDataObject.ExtensionData {
            get;
            set;
        }
    }
}