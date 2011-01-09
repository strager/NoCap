using System;
using System.Windows.Input;

namespace NoCap.Extensions.Default.Helpers {
    public enum TryStatus {
        Trying,
        Success,
        Failure,
    }

    /// <summary>
    /// Interaction logic for ConnectionTester.xaml
    /// </summary>
    public partial class ConnectionTester {
        private string statusText;
        private TryStatus tryStatus;

        public string StatusText {
            get {
                return this.statusText;
            }

            set {
                this.statusText = value;

                Dispatcher.BeginInvoke(new Action(() => {
                    this.statusTextBlock.Text = value;
                }));
            }
        }

        public TryStatus TryStatus {
            get {
                return this.tryStatus;
            }

            set {
                this.tryStatus = value;

                Dispatcher.BeginInvoke(new Action(() => {
                    switch (value) {
                        case TryStatus.Failure:
                            this.progressBar.IsIndeterminate = false;
                            this.progressBar.Value = 0;

                            break;

                        case TryStatus.Success:
                            this.progressBar.IsIndeterminate = false;
                            this.progressBar.Value = 1;

                            break;

                        case TryStatus.Trying:
                            this.progressBar.IsIndeterminate = true;

                            break;
                    }
                }));
            }
        }

        public ConnectionTester() {
            InitializeComponent();

            TryStatus = TryStatus.Trying;
            StatusText = "";

            CommandBindings.Add(new CommandBinding(ApplicationCommands.Close, (sender, e) => Close()));
        }
    }
}
