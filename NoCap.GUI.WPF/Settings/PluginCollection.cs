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
    [Serializable]
    public sealed class PluginCollection : IEnumerable<IPlugin>, IDisposable, ISerializable, IDeserializationCallback {
        private readonly IList<IPlugin> plugins = new List<IPlugin>();

        [NonSerialized]
        private IEnumerable<IPlugin> queuedPlugins;

        [NonSerialized]
        private CommandRunner commandRunner;

        [NonSerialized]
        private CompositionContainer compositionContainer;

        [NonSerialized]
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

        public PluginCollection() {
        }

        private PluginCollection(SerializationInfo info, StreamingContext context) {
            this.queuedPlugins = info.GetValue<IEnumerable<IPlugin>>("Plugins");
        }

        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context) {
            info.AddValue("Plugins", this.plugins);
        }

        void IDeserializationCallback.OnDeserialization(object sender) {
            AddRange(this.queuedPlugins);
            this.queuedPlugins = null;
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