using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace NoCap.Library.Controls {
    /// <summary>
    /// Interaction logic for SharedCommandSelector.xaml
    /// </summary>
    public partial class SharedCommandSelector {
        public readonly static DependencyProperty CommandProperty;
        public readonly static DependencyProperty InfoStuffProperty;
        public readonly static DependencyProperty FilterProperty;
        
        public readonly static RoutedEvent CommandChangedEvent;
        
        private readonly Filterer filterer = new Filterer();

        public ICommand Command {
            get { return (ICommand) GetValue(CommandProperty); }
            set { SetValue(CommandProperty, value); }
        }

        public IInfoStuff InfoStuff {
            get { return (IInfoStuff) GetValue(InfoStuffProperty); }
            set { SetValue(InfoStuffProperty, value);  }
        }
        
        [TypeConverter(typeof(FeatureFilterConverter))]
        public Predicate<ICommand> Filter {
            get { return (Predicate<ICommand>) GetValue(FilterProperty); }
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

            InfoStuffProperty = DependencyProperty.Register(
                "InfoStuff",
                typeof(IInfoStuff),
                typeof(SharedCommandSelector),
                new PropertyMetadata(OnInfoStuffChanged)
            );

            FilterProperty = DependencyProperty.Register(
                "Filter",
                typeof(Predicate<ICommand>),
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

        private static void OnInfoStuffChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e) {
            var commandSelector = (SharedCommandSelector) sender;
            var infoStuff = (IInfoStuff) e.NewValue;

            commandSelector.filterer.Source = infoStuff == null ? null : infoStuff.Commands;
        }

        public SharedCommandSelector() {
            InitializeComponent();

            this.commandList.SetBinding(ItemsControl.ItemsSourceProperty, this.filterer.SourceBinding);

            this.filterer.Filter = (obj) => {
                var filter = Filter;

                if (filter == null) {
                    return true;
                }

                return filter((ICommand) obj);
            };
        }

        public void AutoLoad() {
            if (Command == null) {
                this.commandList.SelectedIndex = 0;
            }
        }
    }
}
