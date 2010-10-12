using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using NoCap.Library;
using NoCap.Library.Destinations;

namespace NoCap.Plugins {
    [Export(typeof(IDestination))]
    public class IsgdShortener : UrlShortener {
        protected override Uri Uri {
            get {
                return new Uri(@"http://is.gd/api.php");
            }
        }

        protected override IDictionary<string, string> GetParameters(TypedData data) {
            IDictionary<string, string> parameters = new Dictionary<string, string>();
            parameters["longurl"] = data.Data.ToString();

            return parameters;
        }

        protected override string RequestMethod {
            get {
                return @"GET";
            }
        }
    }
}