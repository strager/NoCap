using System.Collections.ObjectModel;
using NoCap.Library;

namespace NoCap.GUI.WPF.Settings {
    internal class ProgramSettingsInfoStuff : IInfoStuff {
        private readonly ProgramSettings programSettings;

        public ProgramSettingsInfoStuff(ProgramSettings programSettings) {
            this.programSettings = programSettings;
        }

        public ObservableCollection<ICommand> Commands {
            get {
                return this.programSettings.Commands;
            }
        }
    }
}