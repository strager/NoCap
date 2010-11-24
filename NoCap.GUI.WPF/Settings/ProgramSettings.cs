using System;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;
using NoCap.Library;
using NoCap.Library.Util;
using ICommand = NoCap.Library.ICommand;

namespace NoCap.GUI.WPF.Settings {
    [DataContract(Name = "ProgramSettings")]
    sealed class ProgramSettings : IDisposable {
        // Data member orders are important because they enforce "ownership" in
        // the XML.  A command may reference a default command but it does not
        // own the default command; the default command belongs to the
        // DefaultCommands collection.  Thus, DefaultCommands should be read
        // before Commands.
        //
        // Plugins may reference commands, but do not own commands.  Plugins
        // are thus output after commands.

        [DataMember(Name = "Commands", Order = 1)]
        public ObservableCollection<ICommand> Commands {
            get;
            set;
        }

        [DataMember(Name = "Defaults", Order = 0)]
        public FeaturedCommandCollection DefaultCommands {
            get;
            set;
        }

        [DataMember(Name = "Plugins", Order = 2)]
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
