using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using NoCap.Library;
using NoCap.Library.Tasks;

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
                    var task = (ICommandTask) DataContext;

                    task.Cancel();
                },
                (sender, e) => {
                    e.CanExecute = !this.closing;
                    e.Handled = true;
                }
            ));

            CommandBindings.Add(new CommandBinding(
                ApplicationCommands.Close,
                (sender, e) => QueueClose()
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

            var storyboard = (Storyboard) Resources["CloseAnimation"];

            storyboard.Begin(this);

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
