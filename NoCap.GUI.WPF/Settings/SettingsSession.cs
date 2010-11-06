using System;
using NoCap.Library.Tasks;
using WinputDotNet;

namespace NoCap.GUI.WPF.Settings {
    class SettingsSession : IDisposable {
        private readonly CommandRunner commandRunner;
        private readonly ProgramSettings settings;

        private bool isSetUp = false;
        private bool isDisposed = false;

        public SettingsSession(CommandRunner commandRunner, ProgramSettings settings) {
            this.commandRunner = commandRunner;
            this.settings = settings;
        }

        public void SetUp() {
            EnsureNotDisposed();

            if (this.isSetUp) {
                throw new InvalidOperationException("Already set up");
            }

            SetUpInput();

            this.isSetUp = true;
        }

        public void ShutDown() {
            EnsureNotDisposed();

            if (!this.isSetUp) {
                throw new InvalidOperationException("Not set up");
            }

            ShutDownInput();

            this.isSetUp = false;
        }

        private void SetUpInput() {
            var inputProvider = this.settings.InputProvider;

            if (inputProvider == null) {
                return;
            }

            var handle = IntPtr.Zero;
            
            inputProvider.CommandStateChanged += CommandStateChanged;
            inputProvider.SetBindings(this.settings.Bindings);
            inputProvider.Attach(handle);
        }

        private void ShutDownInput() {
            var inputProvider = this.settings.InputProvider;

            if (inputProvider == null) {
                return;
            }

            inputProvider.Detach();
            inputProvider.CommandStateChanged -= CommandStateChanged;
        }

        private void CommandStateChanged(object sender, CommandStateChangedEventArgs e) {
            if (e.State == InputState.On) {
                var command = (BoundCommand) e.Command;

                // TODO Move this to a different class
                this.commandRunner.Run(command.Command);
            }
        }

        public void Dispose() {
            EnsureNotDisposed();

            if (this.isSetUp) {
                ShutDown();
            }

            this.isDisposed = true;
        }

        private void EnsureNotDisposed() {
            if (this.isDisposed) {
                throw new ObjectDisposedException("SettingsSession");
            }
        }
    }
}
