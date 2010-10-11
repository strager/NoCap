using System.Collections.Specialized;
using NoCap.Library.Destinations;

namespace NoCap.Plugins {
    public class IsgdShortener : UrlShortener {
        protected override string GetUri() {
            return @"http://is.gd/api.php";
        }

        protected override NameValueCollection GetParameters(TypedData data) {
            var parameters = new NameValueCollection();
            parameters["longurl"] = data.Data.ToString();

            return parameters;
        }

        protected override string GetRequestMethod() {
            return @"GET";
        }
    }
}