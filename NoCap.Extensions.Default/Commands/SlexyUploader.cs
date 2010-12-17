using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.Serialization;
using NoCap.Extensions.Default.Factories;
using NoCap.Library;
using NoCap.Library.Commands;
using NoCap.Web.Multipart;

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

        protected override MultipartData GetRequestData(TypedData data) {
            var builder = new MultipartDataBuilder();
            builder.KeyValuePairs(new Dictionary<string, string> {
                { "raw_paste", data.Data.ToString() },
                { "comment", "" },
                { "author", Author ?? "" },
                { "language", Language ?? "text" },
                { "permissions", IsPrivate ? "1" : "0" },
                { "desc", data.Name ?? "" },
                { "linenumbers", ShowLineNumbers ? "0" : "1" },
                { "expire", Expiration.TotalSeconds.ToString(NumberFormatInfo.InvariantInfo) },
                { "submit", "Submit Paste" },
                { "tabbing", "true" },
                { "tabtype", "real" },
            });

            return builder.GetData();
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