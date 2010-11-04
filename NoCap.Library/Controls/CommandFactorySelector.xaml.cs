using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

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

        private readonly Filterer filterer = new Filterer();

        private bool allowPropertyChanges = true;
        private bool allowPropertyBehaviours = true;

        private PriorityItem updatePriority = PriorityItem.None;

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

        private Predicate<ICommandFactory> filter;

        public Predicate<ICommandFactory> Filter {
            get {
                return this.filter;
            }

            set {
                this.filter = value;

                this.filterer.Refresh();
            }
        }

        static CommandFactorySelector() {
            CommandProperty = DependencyProperty.Register(
                "Command",
                typeof(ICommand),
                typeof(CommandFactorySelector),
                new PropertyMetadata(null, OnCommandChanged, CoerceUpdates)
            );

            CommandFactoryProperty = DependencyProperty.Register(
                "CommandFactory",
                typeof(ICommandFactory),
                typeof(CommandFactorySelector),
                new PropertyMetadata(null, OnCommandFactoryChanged, CoerceUpdates)
            );

            InfoStuffProperty = DependencyProperty.Register(
                "InfoStuff",
                typeof(IInfoStuff),
                typeof(CommandFactorySelector),
                new PropertyMetadata(null, OnInfoStuffChanged, CoerceUpdates)
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

        private static bool AreCommandFactoriesEqual(ICommandFactory a, ICommandFactory b) {
            if (a == null && b == null) {
                return true;
            }

            if (a == null || b == null) {
                return false;
            }

            return a.GetType().Equals(b.GetType());
        }

        private ICommandFactory CoerceCommandFactory(ICommandFactory factory) {
            if (InfoStuff == null || factory == null) {
                return factory;
            }

            var displayFactories = this.commandFactoryList.ItemsSource.OfType<ICommandFactory>();

            return displayFactories.FirstOrDefault((f) => ReferenceEquals(factory, f))
                ?? displayFactories.FirstOrDefault((f) => AreCommandFactoriesEqual(factory, f))
                ?? factory;
        }

        private static object CoerceUpdates(DependencyObject sender, object value) {
            var commandSelector = (CommandFactorySelector) sender;

            if (!commandSelector.allowPropertyChanges) {
                return DependencyProperty.UnsetValue;
            }

            return value;
        }

        private static void OnInfoStuffChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e) {
            var commandSelector = (CommandFactorySelector) sender;
            var infoStuff = (IInfoStuff) e.NewValue;

            commandSelector.allowPropertyChanges = false;

            try {
                commandSelector.filterer.Source =
                    infoStuff == null
                        ? Enumerable.Empty<ICommandFactory>()
                        : infoStuff.CommandFactories;

                commandSelector.commandFactoryList.SelectedIndex = -1;
            } finally {
                commandSelector.allowPropertyChanges = true;
            }

            if (commandSelector.updatePriority == PriorityItem.Command) {
                commandSelector.SetCommandFactoryFromCommand(commandSelector.Command);
            } else if (commandSelector.updatePriority == PriorityItem.CommandFactory) {
                commandSelector.SetCommandFromFactory(commandSelector.CommandFactory);
            }
        }

        private static void OnCommandChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e) {
            var commandSelector = (CommandFactorySelector) sender;

            if (commandSelector.allowPropertyBehaviours) {
                commandSelector.SetCommandFactoryFromCommand((ICommand) e.NewValue);
            }

            var args = new RoutedPropertyChangedEventArgs<ICommand>((ICommand) e.OldValue, (ICommand) e.NewValue) {
                RoutedEvent = CommandChangedEvent
            };

            commandSelector.RaiseEvent(args);
        }

        private static void OnCommandFactoryChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e) {
            var commandSelector = (CommandFactorySelector) sender;

            if (commandSelector.allowPropertyBehaviours) {
                commandSelector.updatePriority = PriorityItem.CommandFactory;
                commandSelector.SetCommandFromFactory((ICommandFactory) e.NewValue);
            }

            var args = new RoutedPropertyChangedEventArgs<ICommandFactory>((ICommandFactory) e.OldValue, (ICommandFactory) e.NewValue) {
                RoutedEvent = CommandFactoryChangedEvent
            };

            commandSelector.RaiseEvent(args);
        }

        private void SetCommandFactoryFromCommand(ICommand command) {
            this.updatePriority = PriorityItem.Command;

            if (InfoStuff == null) {
                return;
            }

            this.updatePriority = PriorityItem.None;

            if (command == null) {
                CommandFactory = null;

                return;
            }

            this.allowPropertyBehaviours = false;

            try {
                CommandFactory = CoerceCommandFactory(command.GetFactory());
            } finally {
                this.allowPropertyBehaviours = true;
            }
        }

        private void SetCommandFromFactory(ICommandFactory commandFactory) {
            this.updatePriority = PriorityItem.CommandFactory;

            if (commandFactory == null || InfoStuff == null) {
                return;
            }

            if (InfoStuff.CommandFactories.Any((f) => AreCommandFactoriesEqual(f, commandFactory))) {
                this.updatePriority = PriorityItem.None;

                this.allowPropertyBehaviours = false;

                try {
                    Command = commandFactory.CreateCommand(InfoStuff);
                } finally {
                    this.allowPropertyBehaviours = true;
                }
            }
        }

        public CommandFactorySelector() {
            InitializeComponent();

            this.commandFactoryList.SetBinding(ItemsControl.ItemsSourceProperty, this.filterer.SourceBinding);

            this.filterer.Filter = (obj) => {
                var filter = Filter;

                if (filter == null) {
                    return true;
                }

                return filter((ICommandFactory) obj);
            };
        }

        enum PriorityItem {
            None,
            Command,
            CommandFactory
        }

        public void AutoLoad() {
            if (Command == null) {
                this.commandFactoryList.SelectedIndex = 0;
            }
        }
    }
}
