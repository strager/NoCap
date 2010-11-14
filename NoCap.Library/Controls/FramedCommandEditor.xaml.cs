using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace NoCap.Library.Controls {
    /// <summary>
    /// Interaction logic for FramedCommandEditor.xaml
    /// </summary>
    public partial class FramedCommandEditor {
        public readonly static DependencyProperty CommandProperty;
        public readonly static DependencyProperty IsDefaultProperty;
        public readonly static DependencyProperty InfoStuffProperty;
        public readonly static DependencyProperty FilterProperty;
        public readonly static DependencyProperty HeaderProperty;

        public readonly static RoutedEvent CommandChangedEvent;

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

        public string Header {
            get { return (string) GetValue(HeaderProperty); }
            set { SetValue(HeaderProperty, value);  }
        }

        public event RoutedPropertyChangedEventHandler<ICommand> CommandChanged {
            add    { AddHandler(CommandChangedEvent, value); }
            remove { RemoveHandler(CommandChangedEvent, value); }
        }

        static FramedCommandEditor() {
            CommandProperty = DependencyProperty.Register(
                "Command",
                typeof(ICommand),
                typeof(FramedCommandEditor),
                new PropertyMetadata(OnCommandChanged)
            );

            IsDefaultProperty = DependencyProperty.Register(
                "IsDefault",
                typeof(bool),
                typeof(FramedCommandEditor),
                new PropertyMetadata(false, OnIsDefaultChanged)
            );

            InfoStuffProperty = DependencyProperty.Register(
                "InfoStuff",
                typeof(IInfoStuff),
                typeof(FramedCommandEditor),
                new PropertyMetadata(OnInfoStuffChanged)
            );

            FilterProperty = DependencyProperty.Register(
                "Filter",
                typeof(CommandFeatures),
                typeof(FramedCommandEditor),
                new PropertyMetadata((CommandFeatures) 0, OnFilterChanged)
            );

            HeaderProperty = DependencyProperty.Register(
                "Header",
                typeof(string),
                typeof(FramedCommandEditor)
            );

            CommandChangedEvent = EventManager.RegisterRoutedEvent(
                "CommandChanged",
                RoutingStrategy.Bubble,
                typeof(RoutedPropertyChangedEventHandler<ICommand>),
                typeof(FramedCommandEditor)
            );
        }

        private static void OnFilterChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e) {
            var selector = (FramedCommandEditor) sender;
            var filter = (CommandFeatures) e.NewValue;

            selector.commandFactorySelector.Filter = CommandFeatureFilterConverter.GetPredicate(filter);
        }

        private static void OnInfoStuffChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e) {
            var commandSelector = (FramedCommandEditor) sender;

            commandSelector.SelectCommand(commandSelector.Command);
        }

        private static void OnCommandChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e) {
            var commandSelector = (FramedCommandEditor) sender;

            var newCommand = (ICommand) e.NewValue;

            commandSelector.SelectCommand(newCommand);

            var args = new RoutedPropertyChangedEventArgs<ICommand>((ICommand) e.OldValue, newCommand) {
                RoutedEvent = CommandChangedEvent
            };

            commandSelector.RaiseEvent(args);
        }

        private static void OnIsDefaultChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e) {
            var commandSelector = (FramedCommandEditor) sender;

            bool newIsDefault = (bool) e.NewValue;

            if (newIsDefault) {
                commandSelector.UpdateFromDefault();
            } else {
                commandSelector.UpdateFromFactory();
            }
        }

        public FramedCommandEditor() :
            this(null) {
        }

        public FramedCommandEditor(IInfoStuff infoStuff) {
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
                this.commandFactorySelector.Command = command;
            }
        }

        private void UpdateFromDefault() {
            Command = InfoStuff.GetDefaultCommand(Filter);
        }

        private void UpdateFromFactory() {
            Command = this.commandFactorySelector.Command;
        }

        public void AutoLoad() {
            if (!IsDefault) {
                this.commandFactorySelector.AutoLoad();
            }
        }

        private void CommandFactorySelectorChanged(object sender, RoutedPropertyChangedEventArgs<ICommand> e) {
            if (!IsDefault) {
                UpdateFromFactory();
            }
        }
    }
}
