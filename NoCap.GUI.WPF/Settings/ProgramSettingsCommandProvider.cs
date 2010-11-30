using System.ComponentModel.Composition.Hosting;
using Bindable.Linq;
using Bindable.Linq.Collections;
using NoCap.Library;

namespace NoCap.GUI.WPF.Settings {
    internal sealed class ProgramSettingsCommandProvider : ICommandProvider {
        private readonly ProgramSettings programSettings;
        private readonly CompositionContainer compositionContainer;

        private BindableCollection<ICommandFactory> commandFactories = new BindableCollection<ICommandFactory>();

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
            var newFactories = this.compositionContainer.GetExportedValues<ICommandFactory>().AsBindable();

            var transaction = this.commandFactories.BeginTransaction();
            this.commandFactories.Clear(transaction);
            this.commandFactories.AddRange(newFactories, transaction);
            transaction.Commit();
        }

        public IBindableCollection<ICommandFactory> CommandFactories {
            get {
                return this.commandFactories;
            }
        }

        public IBindableCollection<ICommand> StandAloneCommands {
            get {
                return Commands.WithFeatures(CommandFeatures.StandAlone);
            }
        }

        public IBindableCollection<ICommand> Commands {
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