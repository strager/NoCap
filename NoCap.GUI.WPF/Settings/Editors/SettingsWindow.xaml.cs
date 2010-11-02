using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

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

            var editors = new ISettingsEditor[] {
                new ProviderSettingsEditor(programSettings),
                new BindingSettingsEditor(programSettings),
                new CommandSettingsEditor(programSettings),
            };

            foreach (var editor in editors) {
                var tab = new TabItem { Content = editor };
                tab.SetBinding(HeaderedContentControl.HeaderProperty, new Binding { Source = editor, Path = new PropertyPath("DisplayName") });

                this.tabControl.Items.Add(tab);
            }
        }

        private void OkButtonClicked(object sender, RoutedEventArgs e) {
            DialogResult = true;

            Close();
        }
    }
}
