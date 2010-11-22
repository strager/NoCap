using System;
using System.Windows;

namespace NoCap.Library.Controls {
    /// <summary>
    /// Interaction logic for CommandEditor.xaml
    /// </summary>
    public partial class CommandEditor {
        public readonly static DependencyProperty CommandProperty;
        public readonly static DependencyProperty InfoStuffProperty;

        public readonly static RoutedEvent CommandChangedEvent;

        public ICommand Command {
            get { return (ICommand) GetValue(CommandProperty); }
            set { SetValue(CommandProperty, value); }
        }

        public IInfoStuff InfoStuff {
            get { return (IInfoStuff) GetValue(InfoStuffProperty); }
            set { SetValue(InfoStuffProperty, value);  }
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
            
            InfoStuffProperty = InfoStuffWpf.InfoStuffProperty.AddOwner(
                typeof(CommandEditor),
                new PropertyMetadata(OnInfoStuffChanged)
            );

            CommandChangedEvent = EventManager.RegisterRoutedEvent(
                "CommandChanged",
                RoutingStrategy.Bubble,
                typeof(RoutedPropertyChangedEventHandler<ICommand>),
                typeof(CommandEditor)
            );
        }

        private static void OnInfoStuffChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e) {
            // When InfoStuff changes to or from null, update the editor viewer
            // because the editor viewer is not present when InfoStuff is null
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
            if (InfoStuff == null) {
                Content = null;

                return;
            }

            var factory = command == null ? null : command.GetFactory();

            var editor = (factory == null)
                ? null
                : factory.GetCommandEditor(command, InfoStuff);

            Content = editor;
        }

        public CommandEditor() {
            InitializeComponent();

            SetResourceReference(InfoStuffProperty, "InfoStuff");
        }
    }
}
