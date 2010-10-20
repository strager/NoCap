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

                SetTemplateEditor(value);

                Notify("SelectedCommand");
            }
        }

        private void SetTemplateEditor(ICommand command) {
            this.templateEditorContainer.Content = null;

            if (command == null) {
                return;
            }

            var templateFactory = command.GetFactory();

            if (templateFactory == null) {
                return;
            }

            var editor = templateFactory.GetCommandEditor(command);

            this.templateEditorContainer.Content = editor;
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
