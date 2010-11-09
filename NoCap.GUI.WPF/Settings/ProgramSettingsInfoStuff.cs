using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition.Hosting;
using NoCap.GUI.WPF.Plugins;
using NoCap.Library;

namespace NoCap.GUI.WPF.Settings {
    internal sealed class ProgramSettingsInfoStuff : IInfoStuff {
        private readonly ProgramSettings programSettings;
        private readonly ExtensionManager extensionManager;

        private IEnumerable<ICommandFactory> commandFactories;

        public ProgramSettingsInfoStuff(ProgramSettings programSettings, ExtensionManager extensionManager) {
            this.programSettings = programSettings;
            this.extensionManager = extensionManager;

            Recompose();

            this.extensionManager.CompositionContainer.ExportsChanged += Recompose;
        }

        private void Recompose(object sender, ExportsChangeEventArgs e) {
            Recompose();
        }

        private void Recompose() {
            this.commandFactories = this.extensionManager.CompositionContainer.GetExportedValues<ICommandFactory>();
        }

        public ObservableCollection<ICommand> Commands {
            get {
                return this.programSettings.Commands;
            }
        }

        public IEnumerable<ICommandFactory> CommandFactories {
            get {
                return this.commandFactories;
            }
        }
    }
}