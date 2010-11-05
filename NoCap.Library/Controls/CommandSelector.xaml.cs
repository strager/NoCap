using System;
using System.Windows;

namespace NoCap.Library.Controls {
    /// <summary>
    /// Interaction logic for CommandSelector.xaml
    /// </summary>
    public partial class CommandSelector {
        public readonly static DependencyProperty CommandProperty;
        public readonly static DependencyProperty IsSharedProperty;
        public readonly static DependencyProperty InfoStuffProperty;

        public readonly static RoutedEvent CommandChangedEvent;
        public readonly static RoutedEvent IsSharedChangedEvent;

        public ICommand Command {
            get { return (ICommand) GetValue(CommandProperty); }
            set { SetValue(CommandProperty, value); }
        }

        public bool IsShared {
            get { return (bool) GetValue(IsSharedProperty); }
            set { SetValue(IsSharedProperty, value); }
        }

        public IInfoStuff InfoStuff {
            get { return (IInfoStuff) GetValue(InfoStuffProperty); }
            set { SetValue(InfoStuffProperty, value);  }
        }

        public event RoutedPropertyChangedEventHandler<ICommand> CommandChanged {
            add    { AddHandler(CommandChangedEvent, value); }
            remove { RemoveHandler(CommandChangedEvent, value); }
        }

        public event RoutedPropertyChangedEventHandler<bool> IsSharedChangedChanged {
            add    { AddHandler(IsSharedChangedEvent, value); }
            remove { RemoveHandler(IsSharedChangedEvent, value); }
        }

        private Predicate<object> filter;

        public Predicate<object> Filter {
            get {
                return this.filter;
            }

            set {
                this.filter = value;

                this.commandFactoryList.Filter = value;
                this.sharedCommandList.Filter = value;
            }
        }

        static CommandSelector() {
            CommandProperty = DependencyProperty.Register(
                "Command",
                typeof(ICommand),
                typeof(CommandSelector),
                new PropertyMetadata(OnCommandChanged)
            );

            IsSharedProperty = DependencyProperty.Register(
                "IsShared",
                typeof(bool),
                typeof(CommandSelector),
                new PropertyMetadata(false, OnIsSharedChanged)
            );

            InfoStuffProperty = DependencyProperty.Register(
                "InfoStuff",
                typeof(IInfoStuff),
                typeof(CommandSelector),
                new PropertyMetadata(OnInfoStuffChanged)
            );

            CommandChangedEvent = EventManager.RegisterRoutedEvent(
                "CommandChanged",
                RoutingStrategy.Bubble,
                typeof(RoutedPropertyChangedEventHandler<ICommand>),
                typeof(CommandSelector)
            );

            IsSharedChangedEvent = EventManager.RegisterRoutedEvent(
                "IsSharedChanged",
                RoutingStrategy.Direct,
                typeof(RoutedPropertyChangedEventHandler<bool>),
                typeof(CommandSelector)
            );
        }

        private static void OnInfoStuffChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e) {
            var commandSelector = (CommandSelector) sender;

            commandSelector.SelectCommand(commandSelector.Command);
        }

        private static void OnCommandChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e) {
            var commandSelector = (CommandSelector) sender;

            var newCommand = (ICommand) e.NewValue;

            commandSelector.SelectCommand(newCommand);

            var args = new RoutedPropertyChangedEventArgs<ICommand>((ICommand) e.OldValue, newCommand) {
                RoutedEvent = CommandChangedEvent
            };

            commandSelector.RaiseEvent(args);
        }

        private static void OnIsSharedChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e) {
            var commandSelector = (CommandSelector) sender;

            bool newIsShared = (bool) e.NewValue;

            if (newIsShared) {
                commandSelector.UpdateFromShared();
            } else {
                commandSelector.UpdateFromFactory();
            }

            var args = new RoutedPropertyChangedEventArgs<bool>((bool) e.OldValue, newIsShared) {
                RoutedEvent = IsSharedChangedEvent
            };

            commandSelector.RaiseEvent(args);
        }

        public CommandSelector() :
            this(null) {
        }

        public CommandSelector(IInfoStuff infoStuff) {
            InitializeComponent();

            InfoStuff = infoStuff;
        }

        private void SelectCommand(ICommand command) {
            if (InfoStuff == null) {
                return;
            }

            bool isCommandShared = InfoStuff.Commands.Contains(command);

            IsShared = isCommandShared;

            if (isCommandShared) {
                this.sharedCommandList.Command = command;
            } else {
                this.commandFactoryList.Command = command;
            }
        }

        private void SharedCommandChanged(object sender, RoutedPropertyChangedEventArgs<ICommand> e) {
            UpdateFromShared();
        }

        private void CommandFactoryCommandChanged(object sender, RoutedPropertyChangedEventArgs<ICommand> e) {
            UpdateFromFactory();
        }

        private void UpdateFromShared() {
            Command = this.sharedCommandList.Command;
        }

        private void UpdateFromFactory() {
            Command = this.commandFactoryList.Command;
        }

        public void AutoLoad() {
            if (IsShared) {
                this.sharedCommandList.AutoLoad();
            } else {
                this.commandFactoryList.AutoLoad();
            }
        }
    }
}
