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

        private ICommand selectedCommand;

        public ICommand SelectedCommand {
            get {
                return this.selectedCommand;
            }

            set {
                this.selectedCommand = value;

                SetCommandEditor(value);

                Notify("SelectedCommand");
            }
        }

        private void SetCommandEditor(ICommand command) {
            this.commandEditorContainer.Content = null;

            if (command == null) {
                return;
            }

            var commandFactory = command.GetFactory();

            if (commandFactory == null) {
                return;
            }

            var editor = commandFactory.GetCommandEditor(command, new ProgramSettingsInfoStuff(ProgramSettings));

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
