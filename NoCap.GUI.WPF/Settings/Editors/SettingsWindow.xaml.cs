using System.Windows;
using System.Windows.Controls;

namespace NoCap.GUI.WPF.Settings.Editors {
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

            this.tabControl.Items.Add(new TabItem {
                Content = new CommandSettingsEditor(programSettings),
                Header = "Commands"
            });

            foreach (var plugin in programSettings.Plugins) {
                this.tabControl.Items.Add(new TabItem {
                    Content = plugin.GetEditor(programSettings.InfoStuff),
                    Header = plugin.Name
                });
            }
        }

        private void OkButtonClicked(object sender, RoutedEventArgs e) {
            DialogResult = true;

            Close();
        }
    }
}
