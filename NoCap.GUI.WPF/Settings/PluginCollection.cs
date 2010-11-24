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

        private CommandRunner commandRunner;
        private CompositionContainer compositionContainer;

        private bool isInitialized;

        public void Initialize(CommandRunner commandRunner, CompositionContainer compositionContainer) {
            if (this.isInitialized) {
                throw new InvalidOperationException("Already initialized");
            }

            this.commandRunner = commandRunner;
            this.compositionContainer = compositionContainer;

            Recompose(compositionContainer);

            foreach (var plugin in this.plugins) {
                plugin.Initialize(commandRunner, compositionContainer);
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
                plugin.Initialize(this.commandRunner, this.compositionContainer);
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