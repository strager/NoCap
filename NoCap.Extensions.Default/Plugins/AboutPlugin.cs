using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Net;
using System.Runtime.Serialization;
using System.Threading;
using System.Windows;
using NoCap.Library;
using NoCap.Library.Commands;
using NoCap.Library.Extensions;
using NoCap.Library.Progress;
using NoCap.Web.Multipart;

namespace NoCap.Extensions.Default.Plugins {
    [Export(typeof(IPlugin))]
    [DataContract(Name = "AboutPlugin")]
    sealed class AboutPlugin : IPlugin, IExtensibleDataObject {
        public void Dispose() {
            // Do nothing.
        }

        public string Name {
            get {
                return "About";
            }
        }

        [DataMember(Name = "FeedbackUserName")]
        public string FeedbackUserName {
            get;
            set;
        }

        public UIElement GetEditor(ICommandProvider commandProvider) {
            return new AboutEditor {
                DataContext = new {
                    Feedback = new Feedback(this)
                }
            };
        }

        public void Initialize(IPluginContext pluginContext) {
            // Do nothing.
        }

        ExtensionDataObject IExtensibleDataObject.ExtensionData {
            get;
            set;
        }
    }

    public class Feedback : INotifyPropertyChanged {
        private readonly static Uri Uri = new Uri(@"http://strager.net/nocap/feedback");

        private readonly AboutPlugin plugin;

        private string message;

        public string UserName {
            get {
                return this.plugin.FeedbackUserName;
            }

            set {
                this.plugin.FeedbackUserName = value;

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

        internal Feedback(AboutPlugin plugin) {
            this.plugin = plugin;
        }

        public void Submit() {
            var builder = new MultipartDataBuilder();
            builder.KeyValuePairs(new Dictionary<string, string> {
                    { "name", UserName ?? "" },
                    { "message", Message ?? "" },
            });

            ThreadPool.QueueUserWorkItem((o) => {
                HttpRequest.Execute(Uri, builder.GetData(), HttpRequestMethod.Post, new MutableProgressTracker(), CancellationToken.None);
            });
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
