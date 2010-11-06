using System;
using System.Windows;
using System.Windows.Threading;

namespace NoCap.GUI.WPF {
    class CountdownTimer : FrameworkElement {
        public static readonly DependencyProperty TicksRemainingProperty;
        public static readonly DependencyProperty TimeIntervalProperty;
        
        public static readonly RoutedEvent TicksRemainingChangedEvent;
        public static readonly RoutedEvent CountdownFinishedEvent;

        private readonly DispatcherTimer timer;

        public int TicksRemaining {
            get { return (int) GetValue(TicksRemainingProperty); }
            set { SetValue(TicksRemainingProperty, value); }
        }

        public TimeSpan TickInterval {
            get { return (TimeSpan) GetValue(TimeIntervalProperty); }
            set { SetValue(TimeIntervalProperty, value); }
        }

        public bool IsRunning {
            get {
                return this.timer.IsEnabled;
            }
        }

        public event RoutedPropertyChangedEventHandler<int> TicksRemainingChanged {
            add    { AddHandler(TicksRemainingChangedEvent, value); }
            remove { RemoveHandler(TicksRemainingChangedEvent, value); }
        }

        public event RoutedEventHandler CountdownFinished {
            add    { AddHandler(CountdownFinishedEvent, value); }
            remove { RemoveHandler(CountdownFinishedEvent, value); }
        }

        static CountdownTimer() {
            TicksRemainingProperty = DependencyProperty.Register(
                "TicksRemaining",
                typeof(int),
                typeof(CountdownTimer),
                new PropertyMetadata(OnTimeRemainingChanged)
            );
            
            TimeIntervalProperty = DependencyProperty.Register(
                "TimeInterval",
                typeof(TimeSpan),
                typeof(CountdownTimer),
                new PropertyMetadata(TimeSpan.FromSeconds(1), OnTimeIntervalChanged)
            );

            TicksRemainingChangedEvent = EventManager.RegisterRoutedEvent(
                "TicksRemainingChanged",
                RoutingStrategy.Bubble,
                typeof(RoutedPropertyChangedEventHandler<int>),
                typeof(CountdownTimer)
            );

            CountdownFinishedEvent = EventManager.RegisterRoutedEvent(
                "CountdownFinished",
                RoutingStrategy.Bubble,
                typeof(RoutedEventHandler),
                typeof(CountdownTimer)
            );
        }

        public CountdownTimer() {
            this.timer = new DispatcherTimer {
                Interval = TickInterval
            };

            this.timer.Tick += TimerTick;
        }

        public CountdownTimer(DispatcherTimer timer) {
            if (timer == null) {
                throw new ArgumentNullException("timer");
            }

            this.timer = timer;
            this.timer.Tick += TimerTick;

            TickInterval = timer.Interval;
        }

        public void Start() {
            this.timer.Start();
        }

        public void Stop() {
            this.timer.Stop();
        }

        private void TimerTick(object sender, EventArgs e) {
            --TicksRemaining;
        }

        private static void OnTimeRemainingChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e) {
            var countdownTimer = (CountdownTimer) sender;

            var args = new RoutedPropertyChangedEventArgs<int>((int) e.OldValue, (int) e.NewValue) {
                RoutedEvent = TicksRemainingChangedEvent
            };

            countdownTimer.RaiseEvent(args);

            if ((int) e.NewValue == 0) {
                OnCountdownFinished(countdownTimer);
            }
        }

        private static void OnCountdownFinished(CountdownTimer countdownTimer) {
            countdownTimer.Stop();

            var args = new RoutedEventArgs {
                RoutedEvent = CountdownFinishedEvent
            };

            countdownTimer.RaiseEvent(args);
        }

        private static void OnTimeIntervalChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e) {
            var countdownTimer = (CountdownTimer) sender;

            countdownTimer.timer.Interval = (TimeSpan) e.NewValue;
        }
    }
}
