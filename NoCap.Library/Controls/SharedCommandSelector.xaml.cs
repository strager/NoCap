using System.Windows;

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
                typeof(SharedCommandSelector)
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

        public SharedCommandSelector() {
            InitializeComponent();
        }
    }
}
