﻿using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Hardcodet.Wpf.TaskbarNotification;
using NoCap.GUI.WPF.Commands;
using NoCap.GUI.WPF.Settings;
using NoCap.Library;
using NoCap.Library.Util;
using WinputDotNet;
using ICommand = NoCap.Library.ICommand;

namespace NoCap.GUI.WPF {
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App {
        private IMutableProgressTracker progressTracker;
        private TaskbarIcon taskbarIcon;

        private ProgramSettings settings;
        private ProgramSettingsInfoStuff infoStuff;
        private SettingsWindow settingsWindow;

        private void Load() {
            this.progressTracker = new NotifyingProgressTracker();
            this.taskbarIcon = (TaskbarIcon) Resources["taskbarIcon"];

            LoadBindings();
            LoadSettings();
        }

        private void LoadBindings() {
            // FIXME Make close binding work!
            // This binding doesn't work (if uncommented and if matching XAML uncommented).
            // The menu item shows as disabled.
            //
            // var closeBinding = new System.Windows.Input.CommandBinding(ApplicationCommands.Close);
            // closeBinding.Executed += (sender, e) => Shutdown(0);
            // this.taskbarIcon.CommandBindings.Add(closeBinding);

            var settingsBinding = new System.Windows.Input.CommandBinding(ApplicationCommands.Properties);
            settingsBinding.Executed += (sender, e) => ShowSettingsEditor();
            this.taskbarIcon.CommandBindings.Add(settingsBinding);
        }

        private void LoadSettings() {
            this.settings = new ProgramSettings();

            this.infoStuff = new ProgramSettingsInfoStuff(this.settings, Providers.Instance);

            this.settings.Commands = new ObservableCollection<ICommand>(
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

            var handle = IntPtr.Zero;
            
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

            this.settingsWindow = new SettingsWindow(this.settings.Clone());
            this.settingsWindow.Closed += (sender, e) => CheckSettingsEditorResult();

            ShutDownInput(this.settings);

            this.settingsWindow.ShowDialog();
        }

        private void CheckSettingsEditorResult() {
            if (this.settingsWindow.DialogResult == true) {
                this.settings = this.settingsWindow.ProgramSettings;
            }

            this.settingsWindow.Close();
            this.settingsWindow = null;

            SetUpInput(this.settings);
        }

        private static void PerformRequestSync(ICommand highLevelCommand, IMutableProgressTracker progress) {
            highLevelCommand.Process(null, progress);
        }

        private void PerformRequestAsync(ICommand highLevelCommand) {
            var func = new Action<ICommand, IMutableProgressTracker>(PerformRequestSync);

            func.BeginInvoke(highLevelCommand, this.progressTracker, func.EndInvoke, null);
        }

        public void Start() {
            ShowSettingsEditor();
        }

        private void Application_Startup(object sender, StartupEventArgs e) {
            Load();
            Start();
        }

        private void ExitClicked(object sender, RoutedEventArgs e) {
            Shutdown(0);
        }
    }
}
