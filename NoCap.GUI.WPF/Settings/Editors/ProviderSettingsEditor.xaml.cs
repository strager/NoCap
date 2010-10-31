namespace NoCap.GUI.WPF.Settings.Editors {
    /// <summary>
    /// Interaction logic for ProviderEditor.xaml
    /// </summary>
    public partial class ProviderSettingsEditor : ISettingsEditor {
        public Providers Providers {
            get;
            private set;
        }

        public ProgramSettings ProgramSettings {
            get;
            private set;
        }

        public string DisplayName {
            get {
                return "Providers";
            }
        }

        public ProviderSettingsEditor(ProgramSettings programSettings) {
            InitializeComponent();
            
            ProgramSettings = programSettings;
            Providers = Providers.Instance;

            DataContext = this;
        }
    }
}
