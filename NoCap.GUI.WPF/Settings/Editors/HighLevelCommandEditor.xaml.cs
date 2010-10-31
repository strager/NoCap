using System.ComponentModel;
using System.Linq;
using NoCap.Library;
using NoCap.Library.Commands;

namespace NoCap.GUI.WPF.Settings.Editors {
    /// <summary>
    /// Interaction logic for ProviderEditor.xaml
    /// </summary>
    public partial class HighLevelCommandEditor : ISettingsEditor, INotifyPropertyChanged {
        public Providers Providers {
            get;
            private set;
        }

        public ProgramSettings ProgramSettings {
            get;
            private set;
        }

        private readonly IInfoStuff infoStuff;

        private HighLevelCommand selectedCommand;

        public HighLevelCommand SelectedCommand {
            get {
                return this.selectedCommand;
            }

            set {
                this.selectedCommand = value;

                Notify("selectedCommand");
            }
        }

        public string DisplayName {
            get {
                return "Commands";
            }
        }

        public HighLevelCommandEditor(ProgramSettings programSettings) {
            InitializeComponent();
            
            ProgramSettings = programSettings;
            this.infoStuff = new ProgramSettingsInfoStuff(ProgramSettings, Providers.Instance);

            // TODO Move this out of code
            this.commandSelector.InfoStuff = this.infoStuff;
            this.commandEditor.InfoStuff = this.infoStuff;

            Providers = Providers.Instance;

            SelectedCommand = this.infoStuff.Commands.OfType<HighLevelCommand>().FirstOrDefault();

            DataContext = this;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void Notify(string propertyName) {
            var handler = PropertyChanged;

            if (handler != null) {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
