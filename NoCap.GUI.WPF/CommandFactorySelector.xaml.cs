using System.Linq;
using System.Windows;
using NoCap.Library;

namespace NoCap.GUI.WPF {
    /// <summary>
    /// Interaction logic for CommandFactorySelector.xaml
    /// </summary>
    public partial class CommandFactorySelector {
        public readonly static DependencyProperty CommandProperty;
        public readonly static DependencyProperty CommandFactoryProperty;
        public readonly static DependencyProperty InfoStuffProperty;
        
        public readonly static RoutedEvent CommandChangedEvent;
        public readonly static RoutedEvent CommandFactoryChangedEvent;
        
        public ICommand Command {
            get { return (ICommand) GetValue(CommandProperty); }
            set { SetValue(CommandProperty, value); }
        }
        
        public ICommandFactory CommandFactory {
            get { return (ICommandFactory) GetValue(CommandFactoryProperty); }
            set { SetValue(CommandFactoryProperty, value); }
        }

        public IInfoStuff InfoStuff {
            get { return (IInfoStuff) GetValue(InfoStuffProperty); }
            set { SetValue(InfoStuffProperty, value);  }
        }

        public event RoutedPropertyChangedEventHandler<ICommand> CommandChanged {
            add    { AddHandler(CommandChangedEvent, value); }
            remove { RemoveHandler(CommandChangedEvent, value); }
        }

        public event RoutedPropertyChangedEventHandler<ICommandFactory> CommandFactoryChanged {
            add    { AddHandler(CommandFactoryChangedEvent, value); }
            remove { RemoveHandler(CommandFactoryChangedEvent, value); }
        }

        static CommandFactorySelector() {
            CommandProperty = DependencyProperty.Register(
                "Command",
                typeof(ICommand),
                typeof(CommandFactorySelector),
                new PropertyMetadata(OnCommandChanged)
            );

            CommandFactoryProperty = DependencyProperty.Register(
                "CommandFactory",
                typeof(ICommandFactory),
                typeof(CommandFactorySelector),
                new PropertyMetadata(OnCommandFactoryChanged)
            );

            InfoStuffProperty = DependencyProperty.Register(
                "InfoStuff",
                typeof(IInfoStuff),
                typeof(CommandFactorySelector)
            );

            CommandChangedEvent = EventManager.RegisterRoutedEvent(
                "CommandChanged",
                RoutingStrategy.Bubble,
                typeof(RoutedPropertyChangedEventHandler<ICommand>),
                typeof(CommandFactorySelector)
            );

            CommandFactoryChangedEvent = EventManager.RegisterRoutedEvent(
                "CommandFactoryChanged",
                RoutingStrategy.Bubble,
                typeof(RoutedPropertyChangedEventHandler<ICommand>),
                typeof(CommandFactorySelector)
            );
        }

        private static void OnCommandChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e) {
            var commandSelector = (CommandFactorySelector) sender;

            commandSelector.SelectCommand((ICommand) e.NewValue);

            var args = new RoutedPropertyChangedEventArgs<ICommand>((ICommand) e.OldValue, (ICommand) e.NewValue) {
                RoutedEvent = CommandChangedEvent
            };

            commandSelector.RaiseEvent(args);
        }

        private void SelectCommand(ICommand command) {
            ICommandFactory factory;

            if (command == null) {
                factory = null;
            } else {
                // FIXME Make a more elegant way of "does this factory make this type of command"
                // (or something)
                factory = this.commandFactoryList.Items.OfType<ICommandFactory>().FirstOrDefault((f) => {
                    var commandFactory = command.GetFactory();

                    return commandFactory != null && f.GetType() == commandFactory.GetType();
                });
            }

            this.commandFactoryList.SelectedItem = factory;
        }

        private static void OnCommandFactoryChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e) {
            var commandSelector = (CommandFactorySelector) sender;

            commandSelector.UpdateCommand((ICommandFactory) e.NewValue);

            var args = new RoutedPropertyChangedEventArgs<ICommandFactory>((ICommandFactory) e.OldValue, (ICommandFactory) e.NewValue) {
                RoutedEvent = CommandFactoryChangedEvent
            };

            commandSelector.RaiseEvent(args);
        }

        private void UpdateCommand(ICommandFactory commandFactory) {
            Command = commandFactory.CreateCommand(InfoStuff);
        }

        public CommandFactorySelector() {
            InitializeComponent();
        }
    }
}
