﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Bindable.Linq;
using Bindable.Linq.Collections;
using NoCap.GUI.WPF.Runtime;
using NoCap.Library;
using NoCap.Library.Extensions;
using NoCap.Library.Tasks;
using ICommand = NoCap.Library.ICommand;

namespace NoCap.GUI.WPF.Settings {
    [DataContract(Name = "ProgramSettings")]
    sealed class ProgramSettingsData : IExtensibleDataObject {
        public ProgramSettingsData() {
            this.plugins = new PluginCollection();
            this.defaultCommands = new FeaturedCommandCollection();
            this.commands = new BindableCollection<ICommand>();
        }

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
        private readonly BindableCollection<ICommand> commands;

        [DataMember(Name = "Plugins", Order = 2)]
        private readonly PluginCollection plugins;

        public FeaturedCommandCollection DefaultCommands {
            get {
                return this.defaultCommands;
            }
        }

        public BindableCollection<ICommand> Commands {
            get {
                return this.commands;
            }
        }

        public PluginCollection Plugins {
            get {
                return this.plugins;
            }
        }

        ExtensionDataObject IExtensibleDataObject.ExtensionData {
            get;
            set;
        }
    }

    sealed class ProgramSettings : IDisposable {
        private readonly ProgramSettingsData settingsData;

        public FeaturedCommandCollection DefaultCommands {
            get {
                return SettingsData.DefaultCommands;
            }
        }

        public BindableCollection<ICommand> Commands {
            get {
                return SettingsData.Commands;
            }
        }

        public PluginCollection Plugins {
            get {
                return SettingsData.Plugins;
            }
        }

        private IPluginContext pluginContext;
        private ICommandProvider commandProvider;

        public ICommandProvider CommandProvider {
            get {
                return this.commandProvider;
            }
        }

        public IFeatureRegistry FeatureRegistry {
            get {
                return this.pluginContext.FeatureRegistry;
            }
        }

        public ProgramSettingsData SettingsData {
            get {
                return this.settingsData;
            }
        }

        private ProgramSettings(ProgramSettingsData settingsData) {
            this.settingsData = settingsData;
        }

        public static ProgramSettings Create(ProgramSettingsData settingsData, CommandRunner commandRunner, ExtensionManager extensionManager) {
            var settings = new ProgramSettings(settingsData);
            settings.Initialize(commandRunner, extensionManager);

            return settings;
        }

        private void Initialize(CommandRunner commandRunner, ExtensionManager extensionManager) {
            var commandProvider = new ProgramSettingsCommandProvider(this, extensionManager.CommandCompositionContainer);
            var defaultRegistry = new ProgramFeatureRegistry(DefaultCommands, commandProvider);
            var pluginContext = new ProgramPluginContext(commandRunner, defaultRegistry, commandProvider);

            this.commandProvider = commandProvider;
            this.pluginContext = pluginContext;

            Plugins.Initialize(pluginContext, extensionManager.CommandCompositionContainer);
        }

        public void Dispose() {
            Plugins.Dispose();
        }

        public void LoadCommandDefaults() {
            var commandFactories = CommandProvider.CommandFactories.WithFeatures(CommandFeatures.StandAlone);

            // We use two passes because command population often requires the
            // presence of other commands (which may not have been constructed yet).
            // We thus construct all commands, then populate them.

            var commandInstances = commandFactories.Select((factory) => new {
                Factory = factory,
                Command = factory.CreateCommand()
            });

            foreach (var commandInstance in commandInstances) {
                var factory = commandInstance.Factory;
                var command = commandInstance.Command;

                factory.PopulateCommand(command, commandProvider);

                SettingsData.Commands.Add(commandInstance.Command);
            }
        }
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

            if (!this.defaultCommands.ContainsKey(features)) {
                this.defaultCommands[features] = this.commandProvider.GetPreferredCommand(features);
            }
        }
    }
}
