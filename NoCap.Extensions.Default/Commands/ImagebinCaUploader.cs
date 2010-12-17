using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;
using NoCap.Extensions.Default.Factories;
using NoCap.Library;
using NoCap.Library.Commands;
using NoCap.Library.Imaging;
using NoCap.Web.Multipart;

namespace NoCap.Extensions.Default.Commands {
    [DataContract(Name = "ImagebinCaUploader")]
    public sealed class ImagebinCaUploader : ImageUploaderCommand, IExtensibleDataObject {
        private static readonly Regex LinkInHtml = new Regex(
            @"http://imagebin.ca/view/(?<Code>.*?).html",
            RegexOptions.IgnoreCase | RegexOptions.Compiled
        );

        public override string Name {
            get { return "Imagebin.ca uploader"; }
        }

        public ImagebinCaUploader(ImageWriter imageWriter) :
            base(imageWriter) {
            IsPrivate = true;
        }

        protected override Uri Uri {
            get {
                return new Uri(@"http://imagebin.ca/upload.php");
            }
        }

        public override ICommandFactory GetFactory() {
            return new ImagebinCaUploaderFactory();
        }

        protected override MultipartData GetRequestData(TypedData data) {
            var builder = new MultipartDataBuilder();
            builder.File((Stream) data.Data, "f", data.Name);
            builder.KeyValuePairs(new Dictionary<string, string> {
                { "t", "file" },
                { "name", data.Name ?? "" },
                { "tags", Tags ?? "" },
                { "description", Description ?? "" },
                { "adult", IsPrivate ? "t" : "f" },
                { "sfile", "Upload" },
                { "url", "" },
            });

            return builder.GetData();
        }

        protected override TypedData GetResponseData(HttpWebResponse response, TypedData originalData) {
            string html = HttpRequest.GetResponseText(response);

            // Sorry for using regexp to parse HTML, but
            // including an HTML parsing library as a
            // dependency is just too much

            var match = LinkInHtml.Match(html);

            if (!match.Success) {
                return null;
            }

            return TypedData.FromUri(GetImageUriFromCode(match.Groups["Code"].Value), originalData.Name);
        }

        private Uri GetImageUriFromCode(string code) {
            return new Uri(string.Format(@"http://imagebin.ca/img/{0}", code));
        }

        [DataMember(Name = "TagsString")]
        public string Tags {
            get;
            set;
        }

        [DataMember(Name = "Description")]
        public string Description {
            get;
            set;
        }

        [DataMember(Name = "IsPrivate")]
        public bool IsPrivate {
            get;
            set;
        }

        ExtensionDataObject IExtensibleDataObject.ExtensionData {
            get;
            set;
        }
    }
}