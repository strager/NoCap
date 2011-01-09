using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Composition.Hosting;
using System.Linq;
using System.Runtime.Serialization;
using NoCap.GUI.WPF.Settings;
using NoCap.GUI.WPF.Util;
using NoCap.Library.Extensions;

namespace NoCap.GUI.WPF.Runtime {
    // TODO Observable
    [DataContract(Name = "PluginCollection", Namespace = "http://strager.net/nocap/gui")]
    public sealed class PluginCollection : IEnumerable<IPlugin>, IDisposable, IExtensibleDataObject {
        // Must be declared as List<IPlugin>; see bug:
        // https://www.pivotaltracker.com/story/show/8073575
        [DataMember(Name = "Plugins")]
        private readonly List<IPlugin> plugins = new List<IPlugin>();

        private IPluginContext pluginContext;
        private bool isInitialized;

        public void Initialize(IPluginContext pluginContext, CompositionContainer commandCompositionContainer) {
            if (this.isInitialized) {
                throw new InvalidOperationException("Already initialized");
            }

            this.pluginContext = pluginContext;

            Recompose(commandCompositionContainer);

            foreach (var plugin in this.plugins) {
                plugin.Initialize(pluginContext);
            }

            this.isInitialized = true;
        }

        private void Recompose(ExportProvider commandCompositionContainer) {
            var composedPlugins = commandCompositionContainer.GetExportedValues<IPlugin>();
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
                plugin.Initialize(this.pluginContext);
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

        ExtensionDataObject IExtensibleDataObject.ExtensionData {
            get;
            set;
        }
    }
}