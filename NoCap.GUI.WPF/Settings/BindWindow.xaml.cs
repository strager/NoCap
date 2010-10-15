using System;
using System.Windows.Interop;
using WinputDotNet;

namespace NoCap.GUI.WPF.Settings {
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
            InputSequence = null;

            var handle = new WindowInteropHelper(this).Handle;

            this.provider.AttachRecorder(handle, RecordingMade);
        }

        private void RecordingMade(IInputSequence input) {
            this.provider.Detach();

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
