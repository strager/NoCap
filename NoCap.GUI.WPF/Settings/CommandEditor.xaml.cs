using System.ComponentModel;
using System.Linq;
using NoCap.GUI.WPF.Commands;

namespace NoCap.GUI.WPF.Settings {
    /// <summary>
    /// Interaction logic for ProviderEditor.xaml
    /// </summary>
    public partial class CommandEditor : ISettingsEditor, INotifyPropertyChanged {
        public Providers Providers {
            get;
            private set;
        }

        public ProgramSettings ProgramSettings {
            get;
            private set;
        }

        private HighLevelCommand selectedCommand;

        public HighLevelCommand SelectedCommand {
            get {
                return this.selectedCommand;
            }

            set {
                this.selectedCommand = value;

                SetCommandEditor(value);

                Notify("SelectedCommand");
            }
        }

        private void SetCommandEditor(HighLevelCommand highLevelCommand) {
            this.commandEditorContainer.Content = null;

            if (highLevelCommand == null) {
                return;
            }

            var commandFactory = highLevelCommand.GetFactory();

            if (commandFactory == null) {
                return;
            }

            var editor = commandFactory.GetProcessorEditor(highLevelCommand, new ProgramSettingsInfoStuff(ProgramSettings));

            this.commandEditorContainer.Content = editor;
        }

        public string DisplayName {
            get {
                return "Commands";
            }
        }

        public CommandEditor(ProgramSettings programSettings) {
            InitializeComponent();
            
            ProgramSettings = programSettings;
            Providers = Providers.Instance;

            SelectedCommand = ProgramSettings.Commands.FirstOrDefault();

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
