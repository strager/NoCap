using System;
using System.ComponentModel;
using System.Windows;

namespace NoCap.Library.Controls {
    // TODO Test this (like with CommandFactorySelector)

    /// <summary>
    /// Interaction logic for CommandSelector.xaml
    /// </summary>
    public partial class CommandSelector {
        public readonly static DependencyProperty CommandProperty;
        public readonly static DependencyProperty IsDefaultProperty;
        public readonly static DependencyProperty InfoStuffProperty;
        public readonly static DependencyProperty FilterProperty;

        public readonly static RoutedEvent CommandChangedEvent;
        public readonly static RoutedEvent IsSharedChangedEvent;

        public ICommand Command {
            get { return (ICommand) GetValue(CommandProperty); }
            set { SetValue(CommandProperty, value); }
        }

        public bool IsDefault {
            get { return (bool) GetValue(IsDefaultProperty); }
            set { SetValue(IsDefaultProperty, value); }
        }

        public IInfoStuff InfoStuff {
            get { return (IInfoStuff) GetValue(InfoStuffProperty); }
            set { SetValue(InfoStuffProperty, value);  }
        }
        
        [TypeConverter(typeof(CommandFeatureConverter))]
        public CommandFeatures Filter {
            get { return (CommandFeatures) GetValue(FilterProperty); }
            set { SetValue(FilterProperty, value);  }
        }

        public event RoutedPropertyChangedEventHandler<ICommand> CommandChanged {
            add    { AddHandler(CommandChangedEvent, value); }
            remove { RemoveHandler(CommandChangedEvent, value); }
        }

        public event RoutedPropertyChangedEventHandler<bool> IsSharedChangedChanged {
            add    { AddHandler(IsSharedChangedEvent, value); }
            remove { RemoveHandler(IsSharedChangedEvent, value); }
        }

        static CommandSelector() {
            CommandProperty = DependencyProperty.Register(
                "Command",
                typeof(ICommand),
                typeof(CommandSelector),
                new PropertyMetadata(OnCommandChanged)
            );

            IsDefaultProperty = DependencyProperty.Register(
                "IsDefault",
                typeof(bool),
                typeof(CommandSelector),
                new PropertyMetadata(false, OnIsDefaultChanged)
            );

            InfoStuffProperty = DependencyProperty.Register(
                "InfoStuff",
                typeof(IInfoStuff),
                typeof(CommandSelector),
                new PropertyMetadata(OnInfoStuffChanged)
            );

            FilterProperty = DependencyProperty.Register(
                "Filter",
                typeof(CommandFeatures),
                typeof(CommandSelector),
                new PropertyMetadata((CommandFeatures) 0, OnFilterChanged)
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

        private static void OnFilterChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e) {
            var selector = (CommandSelector) sender;
            var filter = (CommandFeatures) e.NewValue;

            selector.commandFactoryList.Filter = CommandFeatureFilterConverter.GetPredicate(filter);
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

        private static void OnIsDefaultChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e) {
            var commandSelector = (CommandSelector) sender;

            bool newIsDefault = (bool) e.NewValue;

            if (newIsDefault) {
                commandSelector.UpdateFromDefault();
            } else {
                commandSelector.UpdateFromFactory();
            }

            var args = new RoutedPropertyChangedEventArgs<bool>((bool) e.OldValue, newIsDefault) {
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

            bool isDefault = InfoStuff.IsDefaultCommand(command);

            IsDefault = isDefault;

            if (!isDefault) {
                this.commandFactoryList.Command = command;
            }
        }

        private void CommandFactoryCommandChanged(object sender, RoutedPropertyChangedEventArgs<ICommand> e) {
            UpdateFromFactory();
        }

        private void UpdateFromDefault() {
            Command = InfoStuff.GetDefaultCommand(Filter);
        }

        private void UpdateFromFactory() {
            Command = this.commandFactoryList.Command;
        }

        public void AutoLoad() {
            if (!IsDefault) {
                this.commandFactoryList.AutoLoad();
            }
        }
    }
}
