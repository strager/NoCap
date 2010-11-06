using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace NoCap.GUI.WPF {
    /// <summary>
    /// Interaction logic for TaskPopup.xaml
    /// </summary>
    public partial class TaskPopup {
        private readonly CountdownTimer countdownTimer;

        public TaskPopup() {
            InitializeComponent();

            var closeBinding = new CommandBinding(ApplicationCommands.Close);
            closeBinding.Executed += (sender, e) => Close();
            CommandBindings.Add(closeBinding);

            // I admit that the binding of the countdown timer is kinda hacky.
            // Okay, pretty hacky.
            // Forgive me and fix it.

            // TODO Move this to XAML
            this.countdownTimer = new CountdownTimer {
                TickInterval = TimeSpan.FromSeconds(1),
                TicksRemaining = 5,
            };

            // TODO Move this to XAML
            var cancelButtonFormatter = (CountdownTimerStringFormatConverter) Resources["CancelButtonFormatter"];
            cancelButtonFormatter.Timer = this.countdownTimer;

            this.closeButton.SetBinding(ContentControl.ContentProperty, new Binding {
                Source = this.countdownTimer,
                Path = new PropertyPath("TicksRemaining"),
                Converter = cancelButtonFormatter
            });
        }

        public void QueueClose() {
            if (!Dispatcher.CheckAccess()) {
                Dispatcher.BeginInvoke(new Action(QueueClose));

                return;
            }

            if (this.countdownTimer.IsRunning) {
                this.countdownTimer.TicksRemaining = 5;

                return;
            }

            // TODO Command binding?
            this.countdownTimer.CountdownFinished += CountdownFinished;
            this.countdownTimer.Start();
        }

        private void CountdownFinished(object sender, RoutedEventArgs e) {
            this.countdownTimer.CountdownFinished -= CountdownFinished;

            this.closeButton.Content = Resources["CloseButtonContent"];

            Close();
        }

        private void Close() {
            Visibility = Visibility.Collapsed;
        }
    }

    [ValueConversion(typeof(object), typeof(string))]
    class CountdownTimerStringFormatConverter : IValueConverter {
        public string StoppedStringFormat {
            get;
            set;
        }

        public string RunningStringFormat {
            get;
            set;
        }

        public IFormatProvider FormatProvider {
            get;
            set;
        }

        public CountdownTimer Timer {
            get;
            set;
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (value == null || Timer == null || !Timer.IsRunning) {
                return string.Format(FormatProvider, StoppedStringFormat);
            }

            return string.Format(FormatProvider, RunningStringFormat, value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            return value;
        }
    }
}
