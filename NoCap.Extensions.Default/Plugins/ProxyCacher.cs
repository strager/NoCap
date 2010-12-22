using System;
using System.ComponentModel.Composition;
using System.Net;
using System.Runtime.Serialization;
using System.Threading;
using System.Windows;
using NoCap.Library;
using NoCap.Library.Extensions;

namespace NoCap.Extensions.Default.Plugins {
    [DataContract(Name = "ProxyCacher")]
    [Export(typeof(IPlugin))]
    sealed class ProxyCacher : IPlugin {
        private readonly static Uri ProxyTestUri = new Uri(@"http://google.com");

        public string Name {
            get {
                return "Proxy cacher";
            }
        }

        public void Initialize(IPluginContext pluginContext) {
            ThreadPool.QueueUserWorkItem((obj) => CacheProxy());
        }

        private static void CacheProxy() {
            WebRequest.DefaultWebProxy.GetProxy(ProxyTestUri);
        }

        UIElement IPlugin.GetEditor(ICommandProvider commandProvider) {
            return null;
        }

        void IDisposable.Dispose() {
        }
    }
}
