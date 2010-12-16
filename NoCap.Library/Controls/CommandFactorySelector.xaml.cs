using System;
using System.Collections.Generic;
using System.ComponentModel;
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
        public readonly static DependencyProperty CommandProviderProperty;
        public readonly static DependencyProperty FilterProperty;
        
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

        public ICommandProvider CommandProvider {
            get { return (ICommandProvider) GetValue(CommandProviderProperty); }
            set { SetValue(CommandProviderProperty, value);  }
        }
        
        [TypeConverter(typeof(CommandFeatureFilterConverter))]
        public object Filter {
            get { return GetValue(FilterProperty); }
            set { SetValue(FilterProperty, value);  }
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
            CommandProperty = CommandSelector.CommandProperty.AddOwner(
                typeof(CommandFactorySelector),
                new FrameworkPropertyMetadata(null, OnCommandChanged, CoerceUpdates) {
                    BindsTwoWayByDefault = true
                }
            );

            CommandFactoryProperty = DependencyProperty.Register(
                "CommandFactory",
                typeof(ICommandFactory),
                typeof(CommandFactorySelector),
                new PropertyMetadata(null, OnCommandFactoryChanged, CoerceUpdates)
            );

            CommandProviderProperty = NoCapControl.CommandProviderProperty.AddOwner(
                typeof(CommandFactorySelector),
                new PropertyMetadata(OnCommandProviderChanged)
            );

            FilterProperty = DependencyProperty.Register(
                "Filter",
                typeof(object),
                typeof(CommandFactorySelector),
                new PropertyMetadata(null, OnFilterChanged)
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

        private static void OnFilterChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e) {
            var commandFactory = (CommandFactorySelector) sender;

            // Refreshing the combo box also changes the selection.
            // To prevent binding problems because of this, make sure
            // the command is not changed when changing the filter.

            var oldCommand = commandFactory.Command;

            commandFactory.filterer.Refresh();

            commandFactory.Command = oldCommand;
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
            if (CommandProvider == null || factory == null) {
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

        private static void OnCommandProviderChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e) {
            var commandSelector = (CommandFactorySelector) sender;
            var commandProvider = (ICommandProvider) e.NewValue;

            commandSelector.allowPropertyChanges = false;

            try {
                commandSelector.filterer.Source =
                    commandProvider == null
                        ? Enumerable.Empty<ICommandFactory>()
                        : commandProvider.CommandFactories;

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

            if (CommandProvider == null) {
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

            if (commandFactory == null || CommandProvider == null) {
                return;
            }

            if (CommandProvider.CommandFactories.Any((f) => AreCommandFactoriesEqual(f, commandFactory))) {
                this.updatePriority = PriorityItem.None;

                this.allowPropertyBehaviours = false;

                try {
                    var command = commandFactory.CreateCommand();
                    commandFactory.PopulateCommand(command, CommandProvider);

                    Command = command;
                } finally {
                    this.allowPropertyBehaviours = true;
                }
            }
        }

        public CommandFactorySelector() {
            InitializeComponent();

            SetResourceReference(CommandProviderProperty, "commandProvider");

            this.commandFactoryList.SetBinding(ItemsControl.ItemsSourceProperty, this.filterer.SourceBinding);

            this.filterer.Filter = (obj) => {
                var filterPredicate = CommandFeatureFilterConverter.GetPredicate(Filter);

                return filterPredicate((ICommandFactory) obj);
            };
        }

        enum PriorityItem {
            None,
            Command,
            CommandFactory
        }

        public void AutoLoad() {
            if (Command != null) {
                return;
            }

            CommandFeatures features;

            if (CommandFeatureConverter.TryGetCommandFeatures(Filter, out features)) {
                var factories = (IEnumerable<ICommandFactory>) this.filterer.Source;

                if (factories != null) {
                    CommandFactory = Library.CommandProvider.GetPreferredCommandFactory(factories, features);
                }
            } else {
                this.commandFactoryList.SelectedIndex = 0;
            }
        }
    }
}
