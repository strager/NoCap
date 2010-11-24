using System;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;
using NoCap.Library;
using NoCap.Library.Util;
using ICommand = NoCap.Library.ICommand;

namespace NoCap.GUI.WPF.Settings {
    [DataContract(Name = "ProgramSettings")]
    sealed class ProgramSettings : IDisposable {
        [DataMember(Name = "Commands")]
        public ObservableCollection<ICommand> Commands {
            get;
            set;
        }

        [DataMember(Name = "Defaults")]
        public FeaturedCommandCollection DefaultCommands {
            get;
            set;
        }

        [DataMember(Name = "Plugins")]
        private readonly PluginCollection plugins;

        public PluginCollection Plugins {
            get {
                return this.plugins;
            }
        }

        public ExtensionManager ExtensionManager {
            get;
            set;
        }

        public IInfoStuff InfoStuff {
            get {
                return new ProgramSettingsInfoStuff(this, ExtensionManager);
            }
        }

        public ProgramSettings() {
            this.plugins = new PluginCollection();

            Commands = new ObservableCollection<ICommand>();

            DefaultCommands = new FeaturedCommandCollection();
            DefaultCommands[CommandFeatures.FileUploader] = null;
            DefaultCommands[CommandFeatures.TextUploader] = null;
            DefaultCommands[CommandFeatures.UrlShortener] = null;
            DefaultCommands[CommandFeatures.ImageUploader] = null;
        }

        public void Dispose() {
            this.plugins.Dispose();
        }
    }
}
