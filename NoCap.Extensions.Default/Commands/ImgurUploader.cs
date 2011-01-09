using System;
using System.Configuration;
using System.IO;
using System.Net;
using System.Runtime.Serialization;
using System.Xml;
using NoCap.Extensions.Default.Factories;
using NoCap.Extensions.Default.Properties;
using NoCap.Library;
using NoCap.Library.Commands;
using NoCap.Library.Imaging;
using NoCap.Library.Util;
using NoCap.Web.Multipart;

namespace NoCap.Extensions.Default.Commands {
    [DataContract(Name = "ImgurUploader")]
    sealed class ImgurUploader : ImageUploaderCommand {
        public override string Name {
            get {
                return "imgur uploader";
            }
        }

        protected override Uri Uri {
            get {
                return new Uri(@"http://api.imgur.com/2/upload.xml");
            }
        }

        public ImgurUploader(ImageWriter writer)
            : base(writer) {
        }

        protected override MultipartData GetRequestData(TypedData data) {
            var builder = new MultipartDataBuilder();
            builder.KeyValuePair("key", Settings.Default.imgurApiKey);
            builder.KeyValuePair("name", data.Name);
            builder.File((Stream) data.Data, "image", data.Name);

            return builder.GetData();
        }

        protected override TypedData GetResponseData(HttpWebResponse response, TypedData originalData) {
            var xmlDoc = HttpRequest.GetResponseXml(response);

            var nodes = xmlDoc.SelectNodes("/upload/links/original");

            if (nodes == null || nodes.Count == 0) {
                return null;
            }

            return TypedData.FromUri(nodes[0].InnerText, originalData.Name);
        }

        public override ICommandFactory GetFactory() {
            return new ImgurUploaderFactory();
        }
    }
}
