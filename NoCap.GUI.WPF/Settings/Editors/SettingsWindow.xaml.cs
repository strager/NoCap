using System.Windows.Controls;

namespace NoCap.GUI.WPF.Settings.Editors {
    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow {
        internal SettingsWindow(ProgramSettings settings) {
            InitializeComponent();

            var commandProvider = settings.CommandProvider;

            Resources["commandProvider"] = commandProvider;

            this.tabControl.Items.Add(new TabItem {
                Content = new DefaultCommandsEditor(commandProvider, settings.RuntimeProvider.FeatureRegistry, settings.DefaultCommands),
                Header = "Defaults"
            });

            this.tabControl.Items.Add(new TabItem {
                Content = new CommandSettingsEditor(),
                Header = "Commands"
            });

            foreach (var plugin in settings.Plugins) {
                this.tabControl.Items.Add(new TabItem {
                    Content = plugin.GetEditor(commandProvider),
                    Header = plugin.Name
                });
            }
        }
    }
}
