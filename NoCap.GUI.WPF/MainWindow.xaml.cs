using System;
using System.Collections.ObjectModel;
using System.Windows.Interop;
using NoCap.GUI.WPF.Settings;
using NoCap.GUI.WPF.Commands;
using NoCap.Library.Util;
using WinputDotNet;
using ICommand = NoCap.GUI.WPF.Commands.ICommand;

namespace NoCap.GUI.WPF {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow {
        private readonly NotifyingProgressTracker progressTracker;

        private ProgramSettings settings;

        private ProgramSettings Settings {
            get {
                return this.settings;
            }

            set {
                var oldSettings = this.settings;

                this.settings = value;

                OnSettingsChanged(oldSettings, value);
            }
        }

        public MainWindow() {
            InitializeComponent();

            DataContext = this;

            this.progressTracker = new NotifyingProgressTracker();
            this.progressTracker.PropertyChanged += (sender, e) => {
                if (e.PropertyName == "Progress") {
                    SetProgress(this.progressTracker.Progress);
                }
            };

            var templates = new ICommand[] {
                new ClipboardUploaderCommand(),
                new CropShotUploaderCommand(),
            };

            Settings = new ProgramSettings {
                Commands = new ObservableCollection<ICommand>(templates)
            };
        }

        private void OnSettingsChanged(ProgramSettings oldSettings, ProgramSettings newSettings) {
            if (oldSettings != null) {
                ShutDownEverything(oldSettings);
            }

            if (newSettings != null) {
                SetUpEverything(newSettings);
            }
        }

        private void SetUpEverything(ProgramSettings newSettings) {
            SetUpInput(newSettings);
        }

        private void ShutDownEverything(ProgramSettings oldSettings) {
            ShutDownInput(oldSettings);
        }

        private void SetUpInput(ProgramSettings newSettings) {
            var inputProvider = newSettings.InputProvider;

            if (inputProvider == null) {
                return;
            }

            var handle = new WindowInteropHelper(this).Handle;
            
            inputProvider.CommandStateChanged += CommandStateChanged;
            inputProvider.SetBindings(newSettings.Bindings);
            inputProvider.Attach(handle);
        }

        private void ShutDownInput(ProgramSettings oldSettings) {
            var inputProvider = oldSettings.InputProvider;

            if (inputProvider == null) {
                return;
            }

            inputProvider.Detach();
            inputProvider.CommandStateChanged -= CommandStateChanged;
        }

        private void CommandStateChanged(object sender, CommandStateChangedEventArgs e) {
            if (e.State == InputState.On) {
                var command = (ICommand) e.Command;

                PerformRequestAsync(command);
            }
        }

        private void SetProgress(double progress) {
            var setProgress = new Action(() => this.progressBar.Value = progress);

            Dispatcher.BeginInvoke(setProgress);
        }

        private void SettingsClicked(object sender, EventArgs e) {
            var settingsWindow = new SettingsWindow(Settings.Clone());

            ShutDownInput(this.settings);

            if (settingsWindow.ShowDialog() == true) {
                Settings = settingsWindow.ProgramSettings;
            } else {
                SetUpInput(this.settings);
            }
        }

        private static void PerformRequestSync(ICommand command, IMutableProgressTracker progress) {
            command.Execute(progress);
        }

        private void PerformRequestAsync(ICommand command) {
            var func = new Action<ICommand, IMutableProgressTracker>(PerformRequestSync);

            func.BeginInvoke(command, this.progressTracker, func.EndInvoke, null);
        }
    }
}
