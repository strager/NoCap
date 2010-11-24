﻿using System;
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

        [DataMember(Name = "Defaults", Order = 0)]
        private readonly FeaturedCommandCollection defaultCommands;

        [DataMember(Name = "Commands", Order = 1)]
        private readonly ObservableCollection<ICommand> commands;

        [DataMember(Name = "Plugins", Order = 2)]
        private readonly PluginCollection plugins;

        public FeaturedCommandCollection DefaultCommands {
            get {
                return this.defaultCommands;
            }
        }

        public ObservableCollection<ICommand> Commands {
            get {
                return this.commands;
            }
        }

        public PluginCollection Plugins {
            get {
                return this.plugins;
            }
        }

        [IgnoreDataMember]
        private IRuntimeProvider runtimeProvider;

        [IgnoreDataMember]
        private ICommandProvider commandProvider;

        public IRuntimeProvider RuntimeProvider {
            get {
                if (this.runtimeProvider == null) {
                    throw new InvalidOperationException("Call Initialize");
                }

                return this.runtimeProvider;
            }
        }

        public ICommandProvider CommandProvider {
            get {
                if (this.commandProvider == null) {
                    throw new InvalidOperationException("Call Initialize");
                }

                return this.commandProvider;
            }
        }

        public ProgramSettings(IEnumerable<ICommand> commands) {
            this.plugins = new PluginCollection();
            this.defaultCommands = new FeaturedCommandCollection();
            this.commands = new ObservableCollection<ICommand>(commands);
        }

        public void Initialize(CommandRunner commandRunner, ExtensionManager extensionManager) {
            var commandProvider = new ProgramSettingsCommandProvider(this, extensionManager.CompositionContainer);
            var defaultRegistry = new ProgramFeatureRegistry(this.defaultCommands, commandProvider);
            var runtimeProvider = new ProgramRuntimeProvider(commandRunner, extensionManager, defaultRegistry);

            this.commandProvider = commandProvider;
            this.runtimeProvider = runtimeProvider;

            this.plugins.Initialize(runtimeProvider);
        }

        public void Dispose() {
            Plugins.Dispose();
        }

        /*public static IEnumerable<ICommand> LoadCommandDefaults(ICommandProvider commandProvider) {
            // TODO Clean this up

            var commandFactories = commandProvider.CommandFactories
                .Where((factory) => factory.CommandFeatures.HasFlag(CommandFeatures.StandAlone));

            var commandFactoriesToCommands = new Dictionary<ICommandFactory, ICommand>();

            // We use two passes because command population often requires the
            // presence of other commands (which may not have been constructed yet).
            // We thus construct all commands, then populate them.
            foreach (var commandFactory in commandFactories) {
                commandFactoriesToCommands[commandFactory] = commandFactory.CreateCommand();
            }

            foreach (var pair in commandFactoriesToCommands) {
                pair.Key.PopulateCommand(pair.Value, commandProvider);
            }

            return commandFactoriesToCommands.Values;
        }*/
    }

    internal class ProgramFeatureRegistry : IFeatureRegistry {
        private readonly IDictionary<CommandFeatures, string> registeredFeatures = new Dictionary<CommandFeatures, string>();

        private readonly FeaturedCommandCollection defaultCommands;
        private readonly ICommandProvider commandProvider;

        public ProgramFeatureRegistry(FeaturedCommandCollection defaultCommands, ICommandProvider commandProvider) {
            if (defaultCommands == null) {
                throw new ArgumentNullException("defaultCommands");
            }

            if (commandProvider == null) {
                throw new ArgumentNullException("commandProvider");
            }

            this.defaultCommands = defaultCommands;
            this.commandProvider = commandProvider;
        }

        public IEnumerable<CommandFeatures> RegisteredFeatures {
            get {
                return this.registeredFeatures.Keys;
            }
        }

        public string GetFeaturesName(CommandFeatures features) {
            return registeredFeatures[features];
        }

        public void Register(CommandFeatures features, string name) {
            if (this.registeredFeatures.ContainsKey(features)) {
                throw new InvalidOperationException("Features already registered");
            }

            this.registeredFeatures[features] = name;
            this.defaultCommands[features] = this.commandProvider.GetPreferredCommand(features);
        }
    }
}
