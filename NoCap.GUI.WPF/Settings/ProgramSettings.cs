using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Serialization;
using NoCap.Library;
using NoCap.Library.Tasks;
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
        public PluginCollection Plugins {
            get;
            set;
        }

        [IgnoreDataMember]
        public IRuntimePluginInfo RuntimePluginInfo {
            get;
            set;
        }

        [IgnoreDataMember]
        public IInfoStuff InfoStuff {
            get {
                if (RuntimePluginInfo == null) {
                    throw new InvalidOperationException("PluginInfo must not be null");
                }

                return new ProgramSettingsInfoStuff(this, RuntimePluginInfo.CompositionContainer);
            }
        }

        public ProgramSettings() {
            Plugins = new PluginCollection();
            Commands = new ObservableCollection<ICommand>();
            DefaultCommands = new FeaturedCommandCollection();
        }

        public void Dispose() {
            Plugins.Dispose();
        }

        public static ProgramSettings LoadDefaultSettings(CommandRunner commandRunner, ExtensionManager extensionManager) {
            // TODO Clean this up

            var settings = new ProgramSettings();
            settings.RuntimePluginInfo = new ProgramRuntimePluginInfo(commandRunner, extensionManager, settings);

            var commandFactories = settings.InfoStuff.CommandFactories
                .Where((factory) => factory.CommandFeatures.HasFlag(CommandFeatures.StandAlone));

            var commandFactoriesToCommands = new Dictionary<ICommandFactory, ICommand>();

            // We use two passes because command population often requires the
            // presence of other commands (which may not have been constructed yet).
            // We thus construct all commands, then populate them.
            foreach (var commandFactory in commandFactories) {
                commandFactoriesToCommands[commandFactory] = commandFactory.CreateCommand();
            }

            settings.Commands = new ObservableCollection<ICommand>(commandFactoriesToCommands.Values);

            foreach (var pair in commandFactoriesToCommands) {
                pair.Key.PopulateCommand(pair.Value, settings.InfoStuff);
            }

            return settings;
        }
    }
}
