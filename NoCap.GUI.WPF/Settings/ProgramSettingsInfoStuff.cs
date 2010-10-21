using System.Collections.ObjectModel;
using NoCap.GUI.WPF.Commands;
using NoCap.Library;

namespace NoCap.GUI.WPF.Settings {
    internal class ProgramSettingsInfoStuff : IInfoStuff {
        private readonly ProgramSettings programSettings;

        public ProgramSettingsInfoStuff(ProgramSettings programSettings) {
            this.programSettings = programSettings;
        }

        public ObservableCollection<IProcessor> Processors {
            get {
                return this.programSettings.Processors;
            }
        }
    }
}