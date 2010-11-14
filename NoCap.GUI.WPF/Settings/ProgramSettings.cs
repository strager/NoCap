using System;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;
using NoCap.Library;
using NoCap.Library.Util;
using ICommand = NoCap.Library.ICommand;

namespace NoCap.GUI.WPF.Settings {
    [Serializable]
    sealed class ProgramSettings : ISerializable, IDisposable {
        public ObservableCollection<ICommand> Commands {
            get;
            set;
        }

        public FeaturedCommandCollection DefaultCommands {
            get;
            set;
        }

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

        private ProgramSettings(SerializationInfo info, StreamingContext context) {
            Commands = info.GetValue<ObservableCollection<ICommand>>("Commands");
            this.plugins = info.GetValue<PluginCollection>("Plugins");

            DefaultCommands = new FeaturedCommandCollection();
            DefaultCommands[CommandFeatures.FileUploader] = null;
            DefaultCommands[CommandFeatures.TextUploader] = null;
            DefaultCommands[CommandFeatures.UrlShortener] = null;
            DefaultCommands[CommandFeatures.ImageUploader] = null;
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context) {
            info.AddValue("Commands", Commands);
            info.AddValue("Plugins", Plugins);
        }

        public void Dispose() {
            this.plugins.Dispose();
        }
    }

    public class FeaturedCommand {
        private readonly CommandFeatures features;

        public CommandFeatures Features {
            get {
                return this.features;
            }
        }

        public ICommand Command {
            get;
            set;
        }

        public FeaturedCommand(CommandFeatures features, ICommand command) {
            this.features = features;
            Command = command;
        }
    }
}
