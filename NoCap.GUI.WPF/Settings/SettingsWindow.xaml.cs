using System.Windows.Controls;

namespace NoCap.GUI.WPF.Settings {
    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow {
        public SettingsWindow(ProgramSettings programSettings) {
            InitializeComponent();

            var editors = new ISettingsEditor[] {
                new ProviderSettingsEditor(programSettings)
            };

            foreach (var editor in editors) {
                var tab = new TabItem {
                    Header = editor.DisplayName,
                    Content = editor
                };

                this.tabControl.Items.Add(tab);
            }
        }
    }
}
