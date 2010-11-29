﻿using System;
using System.Collections.Generic;
using NoCap.Extensions.Default.Factories;
using NoCap.Library;
using NoCap.Library.Commands;

namespace NoCap.Extensions.Default.Commands {
    [Serializable]
    public sealed class IsgdShortener : UrlShortener {
        public override string Name {
            get { return "is.gd URL shortener"; }
        }

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

        public override ICommandFactory GetFactory() {
            return new IsgdShortenerFactory();
        }
    }
}