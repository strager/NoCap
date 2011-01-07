using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.Threading;
using System.Windows;
using System.Xml;
using NoCap.Library;
using NoCap.Library.Extensions;
using NoCap.Library.Progress;
using NoCap.Library.Util;
using NoCap.Update;

namespace NoCap.GUI.WPF {
    [Export(typeof(IPlugin))]
    [DataContract(Name = "AutoUpdaterPlugin")]
    public class UpdaterPlugin : IPlugin {
        private PatchingEnvironment patchingEnvironment;

        private Thread updateThread;
        private CancellationTokenSource updateCancelTokenSource;

        public UpdaterPlugin() {
            CreateNewPatchingEnvironment();
        }

        [OnDeserialized]
        private void CreateNewPatchingEnvironment(StreamingContext context) {
            CreateNewPatchingEnvironment();
        }

        private void CreateNewPatchingEnvironment() {
            this.patchingEnvironment = PatchingEnvironment.Create(PatchingEnvironment.GetCurrent());
        }

        public void CheckForUpdatesForever(CancellationToken cancelToken) {
            while (!cancelToken.IsCancellationRequested) {
                try {
                    CheckForUpdates(cancelToken);
                } catch (Exception e) {
                    // TODO Exception logging
                }

                cancelToken.WaitHandle.WaitOne(Extensions.Default.Properties.Settings.Default.updateFrequency);
            }
        }

        public void CheckForUpdates(CancellationToken cancelToken) {
            CheckForUpdates(Extensions.Default.Properties.Settings.Default.updateUri, cancelToken);
        }

        public void CheckForUpdates(Uri updateUri, CancellationToken cancelToken) {
            var uriBuilder = new UriBuilder(updateUri);

            uriBuilder.Path = uriBuilder.Path + "check";
            uriBuilder.Query = Web.HttpUtility.ToQueryString(new Dictionary<string, string> {
                { "version", this.patchingEnvironment.Version },
            });

            var response = HttpRequest.Execute(uriBuilder.Uri, null, HttpRequestMethod.Get, new MutableProgressTracker(), cancelToken);

            ProcessPatchSetXml(HttpRequest.GetResponseXml(response));
        }

        private void ProcessPatchSetXml(XmlDocument xmlDocument) {
            // TODO CancellationToken this fucker

            var rootElement = xmlDocument.DocumentElement;

            // TODO Make sure the patches are in order.
            var rawPatches = rootElement.SelectNodes("Patch").OfType<XmlElement>().Select((patch) => new {
                Source = new Uri(patch.SelectSingleNode("Source").InnerText, UriKind.Absolute),
            });

            var patches = rawPatches.AsParallel().Select((patch) => {
                using (var client = new WebClient()) {
                    return Patch.LoadFromArchive(client.DownloadData(patch.Source));
                }
            });

            try {
                foreach (var patch in patches) {
                    this.patchingEnvironment.ApplyPatch(patch);
                }
            } finally {
                foreach (var patch in patches) {
                    patch.Dispose();
                }
            }
        }

        public void Commit() {
            if (!this.patchingEnvironment.IsModified) {
                // If the patching environment wasn't modified,
                // don't apply any changes.
                return;
            }

            Process.QueueDOPE(Process.Quote(
                "--approot", PatchingEnvironment.GetCurrent().ApplicationRoot,
                "--replace-with", this.patchingEnvironment.ApplicationRoot,
                "--delete", this.patchingEnvironment.ApplicationRoot
            ));
        }

        void IDisposable.Dispose() {
            this.updateCancelTokenSource.Cancel();
            this.updateThread.Join();

            Commit();
        }

        public string Name {
            get { return "NoCap auto-updater"; }
        }

        public UIElement GetEditor(ICommandProvider commandProvider) {
            return null;
        }

        public void Initialize(IPluginContext pluginContext) {
            this.updateCancelTokenSource = new CancellationTokenSource();

            this.updateThread = new Thread(() => CheckForUpdatesForever(this.updateCancelTokenSource.Token)) {
                Name = "Updater",
                Priority = ThreadPriority.Lowest,
            };

            this.updateThread.Start();
        }
    }
}