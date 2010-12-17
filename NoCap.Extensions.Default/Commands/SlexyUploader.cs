using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.Serialization;
using NoCap.Extensions.Default.Factories;
using NoCap.Library;
using NoCap.Library.Commands;

namespace NoCap.Extensions.Default.Commands {
    [DataContract(Name = "SlexyUploader")]
    public sealed class SlexyUploader : TextUploaderCommand, IExtensibleDataObject {
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

        [DataMember(Name = "ExpirationDate")]
        public TimeSpan Expiration {
            get;
            set;
        }

        [DataMember(Name = "ShowLineNumbers")]
        public bool ShowLineNumbers {
            get;
            set;
        }

        [DataMember(Name = "IsPrivate")]
        public bool IsPrivate {
            get;
            set;
        }

        [DataMember(Name = "Language")]
        public string Language {
            get;
            set;
        }

        [DataMember(Name = "Author")]
        public string Author {
            get;
            set;
        }

        ExtensionDataObject IExtensibleDataObject.ExtensionData {
            get;
            set;
        }
    }
}