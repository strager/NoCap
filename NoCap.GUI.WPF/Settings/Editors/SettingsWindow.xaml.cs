using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using NoCap.Library.Controls;

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

            AddTab(
                new DefaultCommandsEditor(commandProvider, settings.PluginContext.FeatureRegistry, settings.DefaultCommands),
                "Defaults"
            );

            AddTab(new CommandSettingsEditor(), "Commands");

            foreach (var plugin in settings.Plugins) {
                var editor = plugin.GetEditor(commandProvider);

                if (editor == null) {
                    continue;
                }

                AddTab(editor, plugin.Name);
            }
        }

        private void AddTab(UIElement content, string header) {
            var tabItem = new TabItem {
                Content = content,
                Header = header
            };

            tabItem.SetBinding(VisibilityProperty, new Binding {
                Path = new PropertyPath(VisibilityProperty),
                Source = content
            });

            this.tabControl.Items.Add(tabItem);
        }
    }
}
