using System.ComponentModel;
using System.Linq;
using NoCap.Library;

namespace NoCap.GUI.WPF.Settings {
    /// <summary>
    /// Interaction logic for ProviderEditor.xaml
    /// </summary>
    public partial class ProcessorEditor : ISettingsEditor, INotifyPropertyChanged {
        public Providers Providers {
            get;
            private set;
        }

        public ProgramSettings ProgramSettings {
            get;
            private set;
        }

        private readonly IInfoStuff infoStuff;

        private ICommand selectedCommand;

        public ICommand SelectedCommand {
            get {
                return this.selectedCommand;
            }

            set {
                this.selectedCommand = value;

                SetProcessorEditor(value);

                Notify("selectedCommand");
            }
        }

        private void SetProcessorEditor(ICommand command) {
            this.processorEditorContainer.Content = null;

            if (command == null) {
                return;
            }

            var processorFactory = command.GetFactory();

            if (processorFactory == null) {
                return;
            }

            var editor = processorFactory.GetCommandEditor(command, infoStuff);

            this.processorEditorContainer.Content = editor;
        }

        public string DisplayName {
            get {
                return "Processors";
            }
        }

        public ProcessorEditor(ProgramSettings programSettings) {
            InitializeComponent();
            
            ProgramSettings = programSettings;
            this.infoStuff = new ProgramSettingsInfoStuff(ProgramSettings);

            Providers = Providers.Instance;

            this.SelectedCommand = ProgramSettings.Processors.FirstOrDefault();

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
