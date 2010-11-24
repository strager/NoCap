using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition.Hosting;
using NoCap.Library;

namespace NoCap.GUI.WPF.Settings {
    internal sealed class ProgramSettingsCommandProvider : ICommandProvider {
        private readonly ProgramSettings programSettings;
        private readonly CompositionContainer compositionContainer;

        private IEnumerable<ICommandFactory> commandFactories;

        public ProgramSettingsCommandProvider(ProgramSettings programSettings, CompositionContainer compositionContainer) {
            this.programSettings = programSettings;
            this.compositionContainer = compositionContainer;

            Recompose();

            this.compositionContainer.ExportsChanged += Recompose;
        }

        private void Recompose(object sender, ExportsChangeEventArgs e) {
            Recompose();
        }

        private void Recompose() {
            this.commandFactories = this.compositionContainer.GetExportedValues<ICommandFactory>();
        }

        public IEnumerable<ICommandFactory> CommandFactories {
            get {
                return this.commandFactories;
            }
        }

        public ObservableCollection<ICommand> Commands {
            get {
                return this.programSettings.Commands;
            }
        }

        public ICommand GetDefaultCommand(CommandFeatures features) {
            return this.programSettings.DefaultCommands.GetProxy(features);
        }

        public bool IsDefaultCommand(ICommand command) {
            return this.programSettings.DefaultCommands.Contains(command);
        }
    }
}