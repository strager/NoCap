using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace NoCap.Library.Controls {
    /// <summary>
    /// Interaction logic for CommandSelector.xaml
    /// </summary>
    public partial class CommandSelector {
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

        static CommandSelector() {
            CommandProperty = DependencyProperty.Register(
                "Command",
                typeof(ICommand),
                typeof(CommandSelector),
                new FrameworkPropertyMetadata(OnCommandChanged) {
                    BindsTwoWayByDefault = true
                }
            );

            CommandsSourceProperty = DependencyProperty.Register(
                "CommandsSource",
                typeof(object),
                typeof(CommandSelector),
                new PropertyMetadata(OnCommandsSourceChanged)
            );

            FilterProperty = DependencyProperty.Register(
                "Filter",
                typeof(object),
                typeof(CommandSelector),
                new PropertyMetadata(null, OnFilterChanged)
            );

            CommandChangedEvent = EventManager.RegisterRoutedEvent(
                "CommandChanged",
                RoutingStrategy.Bubble,
                typeof(RoutedPropertyChangedEventHandler<ICommand>),
                typeof(CommandSelector)
            );
        }

        private static void OnCommandsSourceChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e) {
            var commandSelector = (CommandSelector) sender;

            commandSelector.filterer.Source = e.NewValue;
        }

        private static void OnFilterChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e) {
            var commandSelector = (CommandSelector) sender;

            commandSelector.filterer.Refresh();
        }

        private static void OnCommandChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e) {
            var commandSelector = (CommandSelector) sender;

            var args = new RoutedPropertyChangedEventArgs<ICommand>((ICommand) e.OldValue, (ICommand) e.NewValue) {
                RoutedEvent = CommandChangedEvent
            };

            commandSelector.RaiseEvent(args);
        }

        public CommandSelector() {
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
