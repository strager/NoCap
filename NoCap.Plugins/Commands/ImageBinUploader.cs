using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using NoCap.Library;
using NoCap.Library.Commands;
using NoCap.Library.Imaging;
using NoCap.Plugins.Factories;
using NoCap.Web.Multipart;

namespace NoCap.Plugins.Commands {
    [Serializable]
    public class ImageBinUploader : ImageUploader {
        private static readonly Regex LinkInHtml = new Regex(
            @"http://imagebin.ca/view/(?<Code>.*?).html",
            RegexOptions.IgnoreCase | RegexOptions.Compiled
        );

        public override string Name {
            get { return "ImageBin.ca uploader"; }
        }

        public ImageBinUploader(ImageWriter imageWriter) :
            base(imageWriter) {
        }

        protected override Uri Uri {
            get {
                return new Uri(@"http://imagebin.ca/upload.php");
            }
        }

        protected override void PreprocessRequestData(MultipartBuilder helper, TypedData originalData) {
            helper.File((Stream) originalData.Data, "f", originalData.Name);
            System.Threading.Thread.Sleep(3000);
        }

        public override ICommandFactory GetFactory() {
            return new ImageBinUploaderFactory();
        }

        protected override IDictionary<string, string> GetParameters(TypedData data) {
            IDictionary<string, string> parameters = new Dictionary<string, string>();
            parameters["t"] = "file";
            parameters["name"] = UploaderName ?? "";
            parameters["tags"] = "zscreen";
            parameters["description"] = data.Name ?? "";
            parameters["adult"] = "t";
            parameters["sfile"] = "Upload";
            parameters["url"] = "";

            return parameters;
        }

        protected override TypedData GetResponseData(HttpWebResponse response, TypedData originalData) {
            string html = GetResponseText(response);

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

        protected string UploaderName {
            get;
            set;
        }
    }
}