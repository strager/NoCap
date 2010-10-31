using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using Hardcodet.Wpf.TaskbarNotification;
using NoCap.GUI.WPF.Commands;
using NoCap.GUI.WPF.Settings;
using NoCap.Library;
using NoCap.Library.Util;
using WinputDotNet;
using ICommand = NoCap.Library.ICommand;

namespace NoCap.GUI.WPF {
    public class Program {
        private readonly IMutableProgressTracker progressTracker;
        private readonly TaskbarIcon taskbarIcon;

        private ProgramSettings settings;
        private ProgramSettingsInfoStuff infoStuff;
        private SettingsWindow settingsWindow;

        public ProgramSettings Settings {
            get {
                return this.settings;
            }

            private set {
                var oldSettings = this.settings;

                this.settings = value;

                OnSettingsChanged(oldSettings, value);
            }
        }

        public Program() {
            this.progressTracker = new NotifyingProgressTracker();
            this.taskbarIcon = new TaskbarIcon {
                Visibility = Visibility.Collapsed,
            };

            this.taskbarIcon.TrayMouseDoubleClick += (sender, e) => ShowSettingsEditor();

            LoadSettings();
        }

        private void LoadSettings() {
            Settings = new ProgramSettings();

            this.infoStuff = new ProgramSettingsInfoStuff(Settings, Providers.Instance);

            Settings.Commands = new ObservableCollection<ICommand>(
                Providers.Instance.ProcessorFactories
                    .Where((factory) => factory.CommandFeatures.HasFlag(CommandFeatures.StandAlone))
                    .Select((factory) => factory.CreateCommand(this.infoStuff))
            );
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

            var handle = new IntPtr(42);    // Hope this doesn't crash.
            
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
                var command = (BoundCommand) e.Command;

                PerformRequestAsync(command.Command);
            }
        }

        private void SetProgress(double progress) {
            //var setProgress = new Action(() => this.progressBar.Value = progress);

            //setProgress.BeginInvoke(setProgress.EndInvoke, null);
        }

        private void ShowSettingsEditor() {
            if (this.settingsWindow != null) {
                this.settingsWindow.Show();
            }

            this.settingsWindow = new SettingsWindow(Settings.Clone());
            this.settingsWindow.Closed += (sender, e) => CheckSettingsEditorResult();
            this.settingsWindow.Show();
        }

        private void CheckSettingsEditorResult() {
            if (this.settingsWindow.DialogResult == true) {
                ShutDownInput(this.settings);
                Settings = this.settingsWindow.ProgramSettings;
                SetUpInput(this.settings);
            }

            this.settingsWindow.Close();
            this.settingsWindow = null;
        }

        private static void PerformRequestSync(ICommand highLevelCommand, IMutableProgressTracker progress) {
            highLevelCommand.Process(null, progress);
        }

        private void PerformRequestAsync(ICommand highLevelCommand) {
            var func = new Action<ICommand, IMutableProgressTracker>(PerformRequestSync);

            func.BeginInvoke(highLevelCommand, this.progressTracker, func.EndInvoke, null);
        }

        public void Run() {
            this.taskbarIcon.Visibility = Visibility.Visible;

            ShowSettingsEditor();
        }
    }
}