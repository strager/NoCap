using System.ComponentModel;
using System.Linq;
using NoCap.GUI.WPF.Commands;
using NoCap.Library;

namespace NoCap.GUI.WPF.Settings {
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

                SetProcessorEditor(value);

                Notify("selectedCommand");
            }
        }

        private void SetProcessorEditor(HighLevelCommand command) {
            this.commandEditorContainer.Content = null;

            if (command == null) {
                return;
            }

            var processorFactory = command.GetFactory();

            if (processorFactory == null) {
                return;
            }

            var editor = processorFactory.GetCommandEditor(command, infoStuff);

            this.commandEditorContainer.Content = editor;
        }

        public string DisplayName {
            get {
                return "Commands";
            }
        }

        public HighLevelCommandEditor(ProgramSettings programSettings) {
            InitializeComponent();
            
            ProgramSettings = programSettings;
            this.infoStuff = new ProgramSettingsInfoStuff(ProgramSettings);

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
