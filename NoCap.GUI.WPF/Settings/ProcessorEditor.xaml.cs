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

        private IProcessor selectedProcessor;

        public IProcessor SelectedProcessor {
            get {
                return this.selectedProcessor;
            }

            set {
                this.selectedProcessor = value;

                SetProcessorEditor(value);

                Notify("selectedProcessor");
            }
        }

        private void SetProcessorEditor(IProcessor command) {
            this.processorEditorContainer.Content = null;

            if (command == null) {
                return;
            }

            var processorFactory = command.GetFactory();

            if (processorFactory == null) {
                return;
            }

            var editor = processorFactory.GetProcessorEditor(command, null);

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
            Providers = Providers.Instance;

            SelectedProcessor = ProgramSettings.Processors.FirstOrDefault();

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
