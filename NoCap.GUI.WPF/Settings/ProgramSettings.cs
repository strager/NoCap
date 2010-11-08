using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;
using NoCap.GUI.WPF.Plugins;
using NoCap.Library;
using NoCap.Library.Util;
using ICommand = NoCap.Library.ICommand;

namespace NoCap.GUI.WPF.Settings {
    [Serializable]
    public sealed class ProgramSettings : ISerializable, IDisposable {
        public ObservableCollection<ICommand> Commands {
            get;
            set;
        }

        public IEnumerable<IPlugin> Plugins {
            get;
            private set;
        }

        [NonSerialized]
        private readonly IInfoStuff infoStuff;

        public IInfoStuff InfoStuff {
            get {
                return this.infoStuff;
            }
        }

        public ProgramSettings() :
            this(Providers.Instance) {
        }

        public ProgramSettings(Providers providers) {
            Plugins = new IPlugin[] { new InputBindingsPlugin(), new TaskbarPlugin() };

            foreach (var plugin in Plugins) {
                plugin.Populate(Providers.CompositionContainer);
            }

            Commands = new ObservableCollection<ICommand>();

            this.infoStuff = new ProgramSettingsInfoStuff(this, providers);
        }

        private ProgramSettings(SerializationInfo info, StreamingContext context) {
            Commands = info.GetValue<ObservableCollection<ICommand>>("Commands");
            Plugins = info.GetValue<IEnumerable<IPlugin>>("Plugins");

            this.infoStuff = new ProgramSettingsInfoStuff(this, Providers.Instance);
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context) {
            info.AddValue("Commands", Commands);
            info.AddValue("Plugins", Plugins);
        }

        public void Dispose() {
            foreach (var plugin in Plugins) {
                plugin.Dispose();
            }
        }
    }
}
