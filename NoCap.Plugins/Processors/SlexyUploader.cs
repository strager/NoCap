using System;
using System.Collections.Generic;
using System.Globalization;
using NoCap.Library;
using NoCap.Library.Commands;
using NoCap.Plugins.Factories;

namespace NoCap.Plugins.Processors {
    public class SlexyUploader : TextUploader {
        public override string Name {
            get { return "Slexy.org text uploader"; }
        }

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
            parameters["expire"] = Expiration.TotalSeconds.ToString(NumberFormatInfo.InvariantInfo);
            parameters["submit"] = "Submit Paste";
            parameters["tabbing"] = "true";
            parameters["tabtype"] = "real";

            return parameters;
        }

        public override ICommandFactory GetFactory() {
            return new SlexyUploaderFactory();
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