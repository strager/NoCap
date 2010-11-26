using System.Windows;
using System.Windows.Input;
using NoCap.Library.Controls;
using ICommand = NoCap.Library.ICommand;

namespace NoCap.GUI.WPF.Settings.Editors {
    /// <summary>
    /// Interaction logic for CommandEditorWindow.xaml
    /// </summary>
    public partial class CommandEditorWindow {
        public static readonly DependencyProperty CommandProperty;
        public static readonly DependencyProperty CommandProviderProperty;

        public ICommand Command {
            get { return (ICommand) GetValue(CommandProperty); }
            set { SetValue(CommandProperty, value); }
        }

        static CommandEditorWindow() {
            CommandProperty = CommandEditor.CommandProperty.AddOwner(typeof(CommandEditorWindow));
        }

        public CommandEditorWindow() {
            InitializeComponent();

            CommandBindings.Add(new CommandBinding(ApplicationCommands.Close, (sender, e) => Close()));
        }
    }
}
