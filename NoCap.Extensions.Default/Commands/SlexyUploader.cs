using System;
using System.Collections.Generic;
using System.Globalization;
using NoCap.Extensions.Default.Factories;
using NoCap.Library;
using NoCap.Library.Commands;

namespace NoCap.Extensions.Default.Commands {
    [Serializable]
    public sealed class SlexyUploader : TextUploader {
        public override string Name {
            get { return "Slexy.org text uploader"; }
        }

        protected override Uri Uri {
            get {
                return new Uri(@"http://slexy.org/index.php/submit");
            }
        }

        public SlexyUploader() {
            Language = "text";
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

        public TimeSpan Expiration {
            get;
            set;
        }

        public bool ShowLineNumbers {
            get;
            set;
        }

        public bool IsPrivate {
            get;
            set;
        }

        public string Language {
            get;
            set;
        }

        public string Author {
            get;
            set;
        }
    }
}