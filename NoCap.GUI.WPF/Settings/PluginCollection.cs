using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Composition.Hosting;
using System.Linq;
using System.Runtime.Serialization;
using NoCap.Library;
using NoCap.Library.Tasks;
using NoCap.Library.Util;

namespace NoCap.GUI.WPF.Settings {
    // TODO Observable
    [DataContract]
    public sealed class PluginCollection : IEnumerable<IPlugin>, IDisposable {
        [DataMember(Name = "Plugins")]
        private readonly IList<IPlugin> plugins = new List<IPlugin>();

        private IRuntimePluginInfo runtimePluginInfo;
        private bool isInitialized;

        public void Initialize(IRuntimePluginInfo runtimePluginInfo) {
            if (this.isInitialized) {
                throw new InvalidOperationException("Already initialized");
            }

            this.runtimePluginInfo = runtimePluginInfo;

            Recompose(runtimePluginInfo.CompositionContainer);

            foreach (var plugin in this.plugins) {
                plugin.Initialize(runtimePluginInfo);
            }

            this.isInitialized = true;
        }

        private void Recompose(CompositionContainer compositionContainer) {
            var composedPlugins = compositionContainer.GetExportedValues<IPlugin>();
            var newPlugins = composedPlugins.Except(this.plugins, new TypeComparer<IPlugin>());

            AddRange(newPlugins);
        }

        public IEnumerator<IPlugin> GetEnumerator() {
            return this.plugins.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }

        public void Add(IPlugin plugin) {
            if (plugin == null) {
                throw new ArgumentNullException("plugin");
            }

            this.plugins.Add(plugin);

            if (this.isInitialized) {
                plugin.Initialize(this.runtimePluginInfo);
            }
        }

        private void AddRange(IEnumerable<IPlugin> plugins) {
            if (plugins == null) {
                throw new ArgumentNullException("plugins");
            }

            foreach (var plugin in plugins) {
                Add(plugin);
            }
        }

        public void Dispose() {
            foreach (var plugin in this.plugins) {
                plugin.Dispose();
            }
        }
    }
}