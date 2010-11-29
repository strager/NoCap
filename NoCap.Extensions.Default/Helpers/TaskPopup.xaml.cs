using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using NoCap.Library;

namespace NoCap.Extensions.Default.Helpers {
    /// <summary>
    /// Interaction logic for TaskPopup.xaml
    /// </summary>
    public partial class TaskPopup {
        private bool closing = false;

        public TaskPopup() {
            InitializeComponent();

            CommandBindings.Add(new CommandBinding(
                NoCapCommands.Cancel,
                (sender, e) => {
                    MessageBox.Show("Not implemented");
                }
            ));
        }

        public void QueueClose() {
            if (!Dispatcher.CheckAccess()) {
                Dispatcher.BeginInvoke(new Action(QueueClose));

                return;
            }

            if (this.closing) {
                return;
            }

            BeginAnimation(OpacityProperty, (DoubleAnimation) Resources["CloseAnimation"]);

            this.closing = true;
        }

        private void Close() {
            Visibility = Visibility.Collapsed;
        }

        private void Close(object sender, EventArgs e) {
            Close();
        }
    }
}
