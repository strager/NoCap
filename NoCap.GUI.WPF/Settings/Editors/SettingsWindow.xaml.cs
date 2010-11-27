using System.Windows.Controls;
using System.Windows.Input;

namespace NoCap.GUI.WPF.Settings.Editors {
    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow {
        internal SettingsWindow(ProgramSettings settings) {
            InitializeComponent();

            CommandBindings.Add(new CommandBinding(ApplicationCommands.Close, (sender, e) => Close()));

            LoadTabs(settings);
        }

        private void LoadTabs(ProgramSettings settings) {
            var commandProvider = settings.CommandProvider;

            Resources["commandProvider"] = commandProvider;

            this.tabControl.Items.Add(new TabItem {
                Content = new DefaultCommandsEditor(commandProvider, settings.PluginContext.FeatureRegistry, settings.DefaultCommands),
                Header = "Defaults"
            });

            this.tabControl.Items.Add(new TabItem {
                Content = new CommandSettingsEditor(),
                Header = "Commands"
            });

            foreach (var plugin in settings.Plugins) {
                var editor = plugin.GetEditor(commandProvider);

                if (editor == null) {
                    continue;
                }

                this.tabControl.Items.Add(new TabItem {
                    Content = editor,
                    Header = plugin.Name
                });
            }
        }
    }
}
