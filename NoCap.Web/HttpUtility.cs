using System.Collections.Generic;
using System.Linq;

namespace NoCap.Web {
    public static class HttpUtility {
        public static string ToQueryString(IDictionary<string, string> pairs) {
            // http://stackoverflow.com/questions/829080/how-to-build-a-query-string-for-a-url-in-c/829138#829138
            return string.Join("&", pairs.Select(
                (pair) => string.Format("{0}={1}", UrlEncode(pair.Key), UrlEncode(pair.Value))
            ));
        }

        public static string UrlEncode(string source) {
            return System.Web.HttpUtility.UrlEncode(source);
        }
    }
}