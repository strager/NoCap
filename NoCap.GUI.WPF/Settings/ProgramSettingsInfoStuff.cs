using System.Collections.ObjectModel;
using NoCap.Library;

namespace NoCap.GUI.WPF.Settings {
    internal class ProgramSettingsInfoStuff : IInfoStuff {
        private readonly ProgramSettings programSettings;

        public ProgramSettingsInfoStuff(ProgramSettings programSettings) {
            this.programSettings = programSettings;
        }

        public ObservableCollection<ICommand> Processors {
            get {
                return this.programSettings.Processors;
            }
        }
    }
}