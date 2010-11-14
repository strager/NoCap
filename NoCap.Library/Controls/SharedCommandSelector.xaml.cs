using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace NoCap.Library.Controls {
    /// <summary>
    /// Interaction logic for SharedCommandSelector.xaml
    /// </summary>
    public partial class SharedCommandSelector {
        public readonly static DependencyProperty CommandProperty;
        public readonly static DependencyProperty CommandsSourceProperty;
        public readonly static DependencyProperty FilterProperty;
        
        public readonly static RoutedEvent CommandChangedEvent;
        
        private readonly Filterer filterer = new Filterer();

        public ICommand Command {
            get { return (ICommand) GetValue(CommandProperty); }
            set { SetValue(CommandProperty, value); }
        }

        public object CommandsSource {
            get { return GetValue(CommandsSourceProperty); }
            set { SetValue(CommandsSourceProperty, value);  }
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

        static SharedCommandSelector() {
            CommandProperty = DependencyProperty.Register(
                "Command",
                typeof(ICommand),
                typeof(SharedCommandSelector),
                new PropertyMetadata(OnCommandChanged)
            );

            CommandsSourceProperty = DependencyProperty.Register(
                "CommandsSource",
                typeof(object),
                typeof(SharedCommandSelector),
                new PropertyMetadata(OnCommandsSourceChanged)
            );

            FilterProperty = DependencyProperty.Register(
                "Filter",
                typeof(object),
                typeof(SharedCommandSelector),
                new PropertyMetadata(null, OnFilterChanged)
            );

            CommandChangedEvent = EventManager.RegisterRoutedEvent(
                "CommandChanged",
                RoutingStrategy.Bubble,
                typeof(RoutedPropertyChangedEventHandler<ICommand>),
                typeof(SharedCommandSelector)
            );
        }

        private static void OnCommandsSourceChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e) {
            var commandSelector = (SharedCommandSelector) sender;

            commandSelector.filterer.Source = e.NewValue;
        }

        private static void OnFilterChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e) {
            var commandSelector = (SharedCommandSelector) sender;

            commandSelector.filterer.Refresh();
        }

        private static void OnCommandChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e) {
            var commandSelector = (SharedCommandSelector) sender;

            var args = new RoutedPropertyChangedEventArgs<ICommand>((ICommand) e.OldValue, (ICommand) e.NewValue) {
                RoutedEvent = CommandChangedEvent
            };

            commandSelector.RaiseEvent(args);
        }

        public SharedCommandSelector() {
            InitializeComponent();

            this.commandList.SetBinding(ItemsControl.ItemsSourceProperty, this.filterer.SourceBinding);

            this.filterer.Filter = (obj) => {
                var filterPredicate = CommandFeatureFilterConverter.GetPredicate(Filter);

                return filterPredicate((ICommand) obj);
            };
        }

        public void AutoLoad() {
            if (Command == null) {
                this.commandList.SelectedIndex = 0;
            }
        }
    }
}
