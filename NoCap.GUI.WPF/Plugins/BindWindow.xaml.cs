using System;
using System.Windows;
using System.Windows.Interop;
using WinputDotNet;

namespace NoCap.GUI.WPF.Plugins {
    /// <summary>
    /// Interaction logic for BindWindow.xaml
    /// </summary>
    public partial class BindWindow {
        private readonly IInputProvider provider;

        public IInputSequence InputSequence {
            get;
            private set;
        }

        public BindWindow(IInputProvider provider) {
            InitializeComponent();

            this.provider = provider;

            Loaded += (sender, e) => StartRecording();
        }

        private void StartRecording() {
            if (!Dispatcher.CheckAccess()) {
                Dispatcher.BeginInvoke(new Action(StartRecording));

                return;
            }

            InputSequence = null;

            var handle = new WindowInteropHelper(this).Handle;

            this.provider.AttachRecorder(handle, RecordingMade);
        }

        private void RecordingMade(IInputSequence input) {
            this.provider.Detach();

            if (input.IsSystem) {
                var result = MessageBox.Show(
                    string.Format("The input you provided ({0}) is a known system sequence.  Binding this to a command may make your computer inoperable.  Do you want to bind the sequence anyway?", input.HumanString),
                    "System Binding Detected",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning
                );

                if (result != MessageBoxResult.Yes) {
                    StartRecording();

                    return;
                }
            }

            if (input.IsCommon) {
                var result = MessageBox.Show(
                    string.Format("The input you provided ({0}) is a known commonly-used sequence.  Binding this to a command may make your computer inoperable or difficult to use.  Do you want to bind the sequence anyway?", input.HumanString),
                    "Common Binding Detected",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning
                );

                if (result != MessageBoxResult.Yes) {
                    StartRecording();

                    return;
                }
            }

            InputSequence = input;

            var closeSuccess = new Action(() => {
                if (IsVisible) {
                    DialogResult = true;
                    Close();
                }
            });

            Dispatcher.BeginInvoke(closeSuccess);
        }
    }
}
