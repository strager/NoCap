using System.Collections.Generic;
using System.Collections.ObjectModel;
using NoCap.Library;

namespace NoCap.GUI.WPF.Settings {
    internal class ProgramSettingsInfoStuff : IInfoStuff {
        private readonly ProgramSettings programSettings;
        private readonly Providers providers;

        public ProgramSettingsInfoStuff(ProgramSettings programSettings, Providers providers) {
            this.programSettings = programSettings;
            this.providers = providers;
        }

        public ObservableCollection<ICommand> Commands {
            get {
                return this.programSettings.Commands;
            }
        }

        public IEnumerable<ICommandFactory> CommandFactories {
            get {
                return this.providers.CommandFactories;
            }
        }
    }
}