﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.Text;
using System.Text.RegularExpressions;
using NoCap.Extensions.Default.Factories;
using NoCap.Library;
using NoCap.Library.Commands;
using NoCap.Library.Imaging;
using NoCap.Library.Util;
using NoCap.Web.Multipart;

namespace NoCap.Extensions.Default.Commands {
    [DataContract(Name = "PostImageUploader")]
    class PostImageUploader : ImageUploaderCommand, IExtensibleDataObject {
        private static readonly Regex LinkInHtml = new Regex(
            @"http://postimage.org/image/.*?/",
            RegexOptions.IgnoreCase | RegexOptions.Compiled
        );

        public override string Name {
            get {
                return "postimage.org uploader";
            }
        }

        protected override Uri Uri {
            get {
                return new Uri("http://www.postimage.org/", UriKind.Absolute);
            }
        }

        public PostImageUploader(ImageWriter writer)
            : base(writer) {
            IsAdult = true;
        }

        protected override MultipartData GetRequestData(TypedData data) {
            var builder = new MultipartDataBuilder();
            builder.File((Stream) data.Data, "upload", data.Name);
            builder.KeyValuePairs(new Dictionary<string, string> {
                // FIXME Remove and adjust as required.  Just copy-pasted all
                // this from the main page.  (Don't shoot me.)
                { "mode", "local" },
                { "formurl", "http://www.postimage.org/" },
                { "tpl", "." },
                { "addform", "" },
                { "mforum", "" },
                { "um", "image" },
                { "adult", IsAdult ? "yes" : "no" },
                { "ui", "" },
                { "hash", "943" },
                { "optsize", "0" },
                { "submit", "Upload It!" },
            });

            return builder.GetData();
        }

        [DataMember(Name = "IsAdultContent")]
        public bool IsAdult {
            get;
            set;
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

            return TypedData.FromUri(match.Value, originalData.Name);
        }

        public override ICommandFactory GetFactory() {
            return new PostImageUploaderFactory();
        }

        ExtensionDataObject IExtensibleDataObject.ExtensionData {
            get;
            set;
        }
    }
}
