using System;
using System.Windows;

namespace NoCap.Library.Controls {
    /// <summary>
    /// Interaction logic for CommandEditor.xaml
    /// </summary>
    public partial class CommandEditor {
        public readonly static DependencyProperty CommandProperty;
        public readonly static DependencyProperty CommandProviderProperty;

        public readonly static RoutedEvent CommandChangedEvent;

        public ICommand Command {
            get { return (ICommand) GetValue(CommandProperty); }
            set { SetValue(CommandProperty, value); }
        }

        public ICommandProvider CommandProvider {
            get { return (ICommandProvider) GetValue(CommandProviderProperty); }
            set { SetValue(CommandProviderProperty, value);  }
        }

        public event RoutedPropertyChangedEventHandler<ICommand> CommandChanged {
            add    { AddHandler(CommandChangedEvent, value); }
            remove { RemoveHandler(CommandChangedEvent, value); }
        }

        static CommandEditor() {
            CommandProperty = DependencyProperty.Register(
                "Command",
                typeof(ICommand),
                typeof(CommandEditor),
                new FrameworkPropertyMetadata(OnCommandChanged)
            );
            
            CommandProviderProperty = NoCapControl.CommandProviderProperty.AddOwner(
                typeof(CommandEditor),
                new PropertyMetadata(OnCommandProviderChanged)
            );

            CommandChangedEvent = EventManager.RegisterRoutedEvent(
                "CommandChanged",
                RoutingStrategy.Bubble,
                typeof(RoutedPropertyChangedEventHandler<ICommand>),
                typeof(CommandEditor)
            );
        }

        private static void OnCommandProviderChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e) {
            // When commandProvider changes to or from null, update the editor viewer
            // because the editor viewer is not present when commandProvider is null
            if ((e.OldValue == null && e.NewValue != null) ||
                (e.OldValue != null && e.NewValue == null)) {
                var commandEditor = (CommandEditor) sender;

                commandEditor.SetActiveCommand(commandEditor.Command);
            }
        }

        private static void OnCommandChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e) {
            var commandEditor = (CommandEditor) sender;
            var command = (ICommand) e.NewValue;

            commandEditor.SetActiveCommand(command);

            var args = new RoutedPropertyChangedEventArgs<ICommand>((ICommand) e.OldValue, command) {
                RoutedEvent = CommandChangedEvent
            };

            commandEditor.RaiseEvent(args);
        }

        private void SetActiveCommand(ICommand command) {
            if (this.CommandProvider == null) {
                Content = null;

                return;
            }

            var factory = command == null ? null : command.GetFactory();

            var editor = (factory == null)
                ? null
                : factory.GetCommandEditor(this.CommandProvider);

            var editorFrameworkElement = editor as FrameworkElement;

            if (editorFrameworkElement != null) {
                editorFrameworkElement.DataContext = command;
            }

            // Must be after setting the data context
            Content = editor;
        }

        public CommandEditor() {
            InitializeComponent();

            SetResourceReference(CommandProviderProperty, "commandProvider");
        }
    }
}
