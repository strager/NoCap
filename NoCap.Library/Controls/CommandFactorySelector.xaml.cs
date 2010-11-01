using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace NoCap.Library.Controls {
    /// <summary>
    /// Interaction logic for CommandFactorySelector.xaml
    /// </summary>
    public partial class CommandFactorySelector {
        public readonly static DependencyProperty CommandProperty;
        public readonly static DependencyProperty CommandFactoryProperty;
        public readonly static DependencyProperty InfoStuffProperty;
        
        public readonly static RoutedEvent CommandChangedEvent;
        public readonly static RoutedEvent CommandFactoryChangedEvent;

        private bool generateCommandInstance = true;

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

        private readonly CollectionViewSource viewSource;

        private Predicate<ICommandFactory> filter;

        public Predicate<ICommandFactory> Filter {
            get {
                return this.filter;
            }

            set {
                this.filter = value;

                if (this.viewSource.View != null) {
                    this.viewSource.View.Refresh();
                }
            }
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
                typeof(CommandFactorySelector),
                new PropertyMetadata(OnInfoStuffChanged)
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

        private static void OnInfoStuffChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e) {
            var commandSelector = (CommandFactorySelector) sender;
            var infoStuff = (IInfoStuff) e.NewValue;

            // HACK We delay because blah blah blah
            commandSelector.Dispatcher.BeginInvoke(new Action(() => {
                commandSelector.viewSource.Source = infoStuff.CommandFactories;
            }));
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
                factory = this.commandFactoryList.Items.OfType<ICommandFactory>()
                    .FirstOrDefault((f) => IsCommandFromFactory(command, f));
            }

            this.generateCommandInstance = false;

            try {
                this.commandFactoryList.SelectedItem = factory;
            } finally {
                this.generateCommandInstance = true;
            }
        }

        private static bool IsCommandFromFactory(ICommand command, ICommandFactory factory) {
            var commandFactory = command.GetFactory();

            return commandFactory != null && factory.GetType() == commandFactory.GetType();
        }

        private static void OnCommandFactoryChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e) {
            var commandSelector = (CommandFactorySelector) sender;

            if (commandSelector.generateCommandInstance) {
                commandSelector.UpdateCommand((ICommandFactory) e.NewValue);
            }

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

            this.viewSource = new CollectionViewSource();
            this.viewSource.Filter += FilterItem;

            this.commandFactoryList.SetBinding(
                ItemsControl.ItemsSourceProperty,
                new Binding { Source = this.viewSource }
            );
        }

        private void FilterItem(object sender, FilterEventArgs e) {
            if (Filter == null) {
                e.Accepted = true;

                return;
            }

            e.Accepted = Filter((ICommandFactory) e.Item);
        }
    }
}
