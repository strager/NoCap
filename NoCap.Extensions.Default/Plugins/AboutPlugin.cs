using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Windows;
using NoCap.Library;
using NoCap.Library.Extensions;
using NoCap.Web.Multipart;

namespace NoCap.Extensions.Default.Plugins {
    [Export(typeof(IPlugin))]
    [Serializable]
    class AboutPlugin : IPlugin {
        public void Dispose() {
            // Do nothing.
        }

        public string Name {
            get {
                return "About";
            }
        }

        public UIElement GetEditor(ICommandProvider commandProvider) {
            return new AboutEditor() {
                DataContext = new {
                    Feedback = new Feedback()
                }
            };
        }

        public void Initialize(IPluginContext pluginContext) {
            // Do nothing.
        }
    }

    public class Feedback : INotifyPropertyChanged {
        private readonly static Uri Uri = new Uri(@"http://strager.net/nocap/feedback");

        private string userName;
        private string message;

        public string UserName {
            get {
                return this.userName;
            }

            set {
                this.userName = value;

                Notify("UserName");
            }
        }

        public string Message {
            get {
                return this.message;
            }

            set {
                this.message = value;

                Notify("Message");
            }
        }

        public void Submit() {
            // FIXME SORRY; COPYPASTED FROM NoCap.Library.Commands.HttpUploader

            var parameters = new Dictionary<string, string> {
                { "name", UserName ?? "" },
                { "message", Message ?? "" },
            };

            var builder = new MultipartBuilder();
            builder.KeyValuePairs(parameters);

            var boundary = builder.Boundary;

            var request = (HttpWebRequest) WebRequest.Create(Uri);
            request.Method = @"POST";
            request.ContentType = string.Format("multipart/form-data; {0}", MultipartHeader.KeyValuePair("boundary", boundary));

            request.ContentLength = Utility.GetBoundaryByteCount(boundary) + builder.GetByteCount();

            request.BeginGetRequestStream((ar) => {
                var requestStream = request.EndGetRequestStream(ar);

                Utility.WriteBoundary(requestStream, boundary);
                builder.Write(requestStream);

                request.BeginGetResponse((ar2) => request.EndGetResponse(ar2), null);
            }, null);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void Notify(string propertyName) {
            var handler = PropertyChanged;

            if (handler != null) {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
