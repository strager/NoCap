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
    public sealed class PluginCollection : ICollection<IPlugin>, IDisposable, ISerializable, IDeserializationCallback {
        private readonly IList<IPlugin> plugins = new List<IPlugin>();

        [NonSerialized]
        private IEnumerable<IPlugin> queuedPlugins;

        [NonSerialized]
        private CommandRunner commandRunner;

        public CommandRunner CommandRunner {
            get {
                return this.commandRunner;
            }

            set {
                this.commandRunner = value;

                foreach (var plugin in this.plugins) {
                    plugin.CommandRunner = value;
                }
            }
        }

        public void Populate(CompositionContainer compositionContainer) {
            var composedPlugins = compositionContainer.GetExportedValues<IPlugin>();
            var newPlugins = composedPlugins.Except(this.plugins, new TypeComparer<IPlugin>());

            AddRange(newPlugins);

            foreach (var plugin in this.plugins) {
                plugin.Populate(compositionContainer);
            }
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

            plugin.CommandRunner = this.commandRunner;
            plugin.Init();
        }

        private void AddRange(IEnumerable<IPlugin> plugins) {
            if (plugins == null) {
                throw new ArgumentNullException("plugins");
            }

            foreach (var plugin in plugins) {
                Add(plugin);
            }
        }

        public void Clear() {
            foreach (var plugin in this.plugins) {
                plugin.Dispose();
            }

            this.plugins.Clear();
        }

        public bool Contains(IPlugin plugin) {
            return this.plugins.Contains(plugin);
        }

        void ICollection<IPlugin>.CopyTo(IPlugin[] array, int arrayIndex) {
            this.plugins.CopyTo(array, arrayIndex);
        }

        public bool Remove(IPlugin plugin) {
            bool success = this.plugins.Remove(plugin);

            if (success) {
                plugin.Dispose();
            }

            return success;
        }

        public int Count {
            get {
                return this.plugins.Count;
            }
        }

        bool ICollection<IPlugin>.IsReadOnly {
            get {
                return false;
            }
        }

        public void Dispose() {
            foreach (var plugin in this.plugins) {
                plugin.Dispose();
            }
        }
    }

    public class TypeComparer<T> : EqualityComparer<T>
        where T : class {
        public override bool Equals(T x, T y) {
            if (x == null && y == null) {
                return true;
            }

            if (x == null || y == null) {
                return false;
            }

            return x.GetType().Equals(y.GetType());
        }

        public override int GetHashCode(T obj) {
            return 0;
        }
    }
}