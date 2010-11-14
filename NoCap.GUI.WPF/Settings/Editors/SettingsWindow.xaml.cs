using System.Windows.Controls;

namespace NoCap.GUI.WPF.Settings.Editors {
    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow {
        internal SettingsWindow(ProgramSettings settings) {
            InitializeComponent();

            var infoStuff = settings.InfoStuff;

            this.tabControl.Items.Add(new TabItem {
                Content = new DefaultCommandsEditor(infoStuff, settings.DefaultCommands),
                Header = "Defaults"
            });

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
    }
}
