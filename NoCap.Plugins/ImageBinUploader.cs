using System;
using System.Collections.Specialized;
using System.ComponentModel.Composition;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using NoCap.Library.Destinations;
using NoCap.WebHelpers;

namespace NoCap.Plugins {
    [Export(typeof(IDestination))]
    public class ImageBinUploader : ImageUploader {
        private static readonly Regex LinkInHtml = new Regex(
            @"http://imagebin.ca/view/(?<Code>.*?).html",
            RegexOptions.IgnoreCase | RegexOptions.Compiled
        );

        [ImportingConstructor]
        public ImageBinUploader([Import(AllowDefault = true)] ImageWriter imageWriter) :
            base(imageWriter) {
        }

        protected override string GetUri() {
            return @"http://imagebin.ca/upload.php";
        }

        protected override void PreprocessRequestData(MultipartHelper helper, TypedData originalData) {
            var stream = new MemoryStream((byte[]) originalData.Data);

            helper.Add(new FilePart(stream, @"f", ImageWriter.CodecInfo.MimeType) {
                FileName = originalData.Name
            });
        }

        protected override NameValueCollection GetParameters(TypedData data) {
            var parameters = new NameValueCollection();
            parameters["t"] = "file";
            parameters["name"] = Name ?? "";
            parameters["tags"] = "zscreen";
            parameters["description"] = data.Name ?? "";
            parameters["adult"] = "t";
            parameters["sfile"] = "Upload";
            parameters["url"] = "";

            return parameters;
        }

        protected override TypedData GetResponseData(HttpWebResponse response, TypedData originalData) {
            // TODO Un-copy-pasta from UrlShortener
            var stream = response.GetResponseStream();
            
            if (stream == null) {
                throw new InvalidOperationException("Response stream should not be null");
            }

            using (var reader = new StreamReader(stream, Encoding.UTF8)) {  // FIXME should this be UTF-8?
                string html = reader.ReadToEnd();

                // Sorry for using regexp to parse HTML, but
                // including an HTML parsing library as a
                // dependency is just too much

                var match = LinkInHtml.Match(html);

                if (match.Success) {
                    return TypedData.FromUri(GetImageUriFromCode(match.Groups["Code"].Value), originalData.Name);
                } else {
                    return null;
                }
            }
        }

        private string GetImageUriFromCode(string code) {
            return string.Format(@"http://imagebin.ca/img/{0}", code);
        }

        protected string Name {
            get;
            set;
        }
    }
}