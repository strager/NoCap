using System;
using System.ComponentModel.Composition.Hosting;
using NoCap.GUI.WPF.Settings;
using NoCap.Library;
using NoCap.Library.Tasks;

namespace NoCap.GUI.WPF {
    internal class ProgramRuntimePluginInfo : IRuntimePluginInfo {
        public ProgramRuntimePluginInfo(CommandRunner commandRunner, ExtensionManager extensionManager, ProgramSettings settings) {
            if (commandRunner == null) {
                throw new ArgumentNullException("commandRunner");
            }

            if (extensionManager == null) {
                throw new ArgumentNullException("extensionManager");
            }

            if (settings == null) {
                throw new ArgumentNullException("settings");
            }

            this.commandRunner = commandRunner;
            this.extensionManager = extensionManager;
            this.settings = settings;
        }

        private readonly CommandRunner commandRunner;
        private readonly ExtensionManager extensionManager;
        private readonly ProgramSettings settings;

        public CommandRunner CommandRunner {
            get {
                return this.commandRunner;
            }
        }

        public CompositionContainer CompositionContainer {
            get {
                return ExtensionManager.CompositionContainer;
            }
        }

        public ExtensionManager ExtensionManager {
            get {
                return this.extensionManager;
            }
        }

        public void RegisterDefaultType(CommandFeatures features, string name) {
            this.settings.DefaultCommands.RegisterDefaultType(features, name, this.settings.InfoStuff);
        }
    }
}