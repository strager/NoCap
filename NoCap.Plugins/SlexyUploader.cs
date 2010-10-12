﻿using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using NoCap.Library;
using NoCap.Library.Destinations;

namespace NoCap.Plugins {
    [Export(typeof(IDestination))]
    public class SlexyUploader : TextUploader {
        protected override Uri Uri {
            get {
                return new Uri(@"http://slexy.org/index.php/submit");
            }
        }

        protected override IDictionary<string, string> GetParameters(TypedData data) {
            IDictionary<string, string> parameters = new Dictionary<string, string>();
            parameters["raw_paste"] = data.Data.ToString();
            parameters["comment"] = "";
            parameters["author"] = Author ?? "";
            parameters["language"] = Language ?? "text";
            parameters["permissions"] = IsPrivate ? "1" : "0";
            parameters["desc"] = data.Name ?? "";
            parameters["linenumbers"] = ShowLineNumbers ? "0" : "1";
            parameters["expire"] = Expiration.TotalSeconds.ToString(); // TODO Format provider
            parameters["submit"] = "Submit Paste";
            parameters["tabbing"] = "true";
            parameters["tabtype"] = "real";

            return parameters;
        }

        protected TimeSpan Expiration {
            get;
            set;
        }

        protected bool ShowLineNumbers {
            get;
            set;
        }

        protected bool IsPrivate {
            get;
            set;
        }

        protected string Language {
            get;
            set;
        }

        protected string Author {
            get;
            set;
        }
    }
}