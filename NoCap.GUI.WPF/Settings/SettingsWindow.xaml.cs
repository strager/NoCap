using System.Windows.Controls;

namespace NoCap.GUI.WPF.Settings {
    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow {
        private readonly ProgramSettings programSettings;

        public ProgramSettings ProgramSettings {
            get {
                return this.programSettings;
            }
        }

        public SettingsWindow(ProgramSettings programSettings) {
            InitializeComponent();
            
            this.programSettings = programSettings;

            var editors = new ISettingsEditor[] {
                new ProviderSettingsEditor(programSettings),
                new BindingSettingEditor(programSettings),
                new ProcessorEditor(programSettings),
            };

            foreach (var editor in editors) {
                var tab = new TabItem {
                    Header = editor.DisplayName,
                    Content = editor
                };

                this.tabControl.Items.Add(tab);
            }
        }

        private void OkButtonClicked(object sender, System.Windows.RoutedEventArgs e) {
            DialogResult = true;

            Close();
        }
    }
}
