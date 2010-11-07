using System.Windows;
using System.Windows.Controls;

namespace NoCap.GUI.WPF.Settings.Editors {
    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow {
        private readonly ProgramSettings settings;

        public ProgramSettings Settings {
            get {
                return this.settings;
            }
        }

        public SettingsWindow(ProgramSettings settings) {
            this.settings = settings;

            InitializeComponent();

            var infoStuff = settings.InfoStuff;

            this.tabControl.Items.Add(new TabItem {
                Content = new CommandSettingsEditor(infoStuff),
                Header = "Commands"
            });

            foreach (var plugin in settings.Plugins) {
                this.tabControl.Items.Add(new TabItem {
                    Content = plugin.GetEditor(infoStuff),
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
