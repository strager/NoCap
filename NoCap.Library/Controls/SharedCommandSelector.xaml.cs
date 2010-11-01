using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace NoCap.Library.Controls {
    /// <summary>
    /// Interaction logic for SharedCommandSelector.xaml
    /// </summary>
    public partial class SharedCommandSelector {
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

        private readonly CollectionViewSource viewSource;

        private Predicate<ICommand> filter;

        public Predicate<ICommand> Filter {
            get {
                return this.filter;
            }

            set {
                this.filter = value;

                // TODO
            }
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

            CommandChangedEvent = EventManager.RegisterRoutedEvent(
                "CommandChanged",
                RoutingStrategy.Bubble,
                typeof(RoutedPropertyChangedEventHandler<ICommand>),
                typeof(SharedCommandSelector)
            );
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

            // FIXME Duplicated from CommandFactorySelector
            commandSelector.Dispatcher.BeginInvoke(new Action(() => {
                commandSelector.viewSource.Source = infoStuff.Commands;
            }));
        }

        public SharedCommandSelector() {
            InitializeComponent();

            // FIXME Duplicated from CommandFactorySelector
            this.viewSource = new CollectionViewSource();
            this.viewSource.Filter += FilterItem;

            this.commandList.SetBinding(ItemsControl.ItemsSourceProperty, new Binding {
                Source = this.viewSource
            });
        }

        private void FilterItem(object sender, FilterEventArgs e) {
            // FIXME Duplicated from CommandFactorySelector

            if (Filter == null) {
                e.Accepted = true;

                return;
            }

            e.Accepted = Filter((ICommand) e.Item);
        }
    }
}
