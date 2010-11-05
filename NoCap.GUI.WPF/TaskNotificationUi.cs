﻿using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Interop;
using Hardcodet.Wpf.TaskbarNotification;
using NoCap.Library;
using Windows7.DesktopIntegration;

namespace NoCap.GUI.WPF {
    class TaskNotificationUi : IDisposable {
        private readonly NoCapLogo logo;
        private readonly TaskbarIcon taskbarIcon;

        private readonly ICollection<CommandRunner> boundCommandRunners = new List<CommandRunner>();

        public TaskNotificationUi(TaskbarIcon taskbarIcon, NoCapLogo logo) {
            this.taskbarIcon = taskbarIcon;
            this.logo = logo;

            UpdateIcon(1);
        }

        public void BindFrom(CommandRunner commandRunner) {
            commandRunner.TaskStarted     += BeginTask;
            commandRunner.TaskCompleted   += EndTask;
            commandRunner.ProgressUpdated += UpdateProgress;

            this.boundCommandRunners.Add(commandRunner);
        }

        public void UnbindFrom(CommandRunner commandRunner) {
            commandRunner.TaskStarted     -= BeginTask;
            commandRunner.TaskCompleted   -= EndTask;
            commandRunner.ProgressUpdated -= UpdateProgress;

            this.boundCommandRunners.Remove(commandRunner);
        }

        public void BeginTask(object sender, CommandTaskEventArgs e) {
            // Do nothing
        }

        public void EndTask(object sender, CommandTaskEventArgs e) {
            TaskDonePopup();
        }

        public void UpdateProgress(object sender, CommandTaskProgressEventArgs e) {
            double progress = e.Progress;

            UpdateWindows(progress);
            UpdateIcon(progress);
            UpdateIconToolTip(progress);
        }

        private void UpdateWindows(double progress) {
            Application.Current.Dispatcher.BeginInvoke(new Action(() => {
                foreach (var window in Application.Current.Windows) {
                    SetWindowProgress((Window) window, progress);
                }
            }));
        }

        private void UpdateIcon(double progress) {
            this.logo.Dispatcher.BeginInvoke(new Action(() => {
                this.logo.Progress = progress;

                this.taskbarIcon.Dispatcher.BeginInvoke(new Action(() => {
                    this.taskbarIcon.IconSource = this.logo.MakeIcon(128);
                }));
            }));
        }

        private void UpdateIconToolTip(double progress) {
            this.taskbarIcon.Dispatcher.BeginInvoke(new Action(() => {
                this.taskbarIcon.ToolTipText = string.Format("Progress: {0}%", progress * 100);
            }));
        }

        private static void SetWindowProgress(Window window, double progress) {
            window.Dispatcher.BeginInvoke(new Action(() => {
                var handle = new WindowInteropHelper(window).Handle;

                if (progress >= 1) {
                    Windows7Taskbar.SetProgressState(handle, Windows7Taskbar.ThumbnailProgressState.NoProgress);
                } else {
                    Windows7Taskbar.SetProgressState(handle, Windows7Taskbar.ThumbnailProgressState.Normal);

                    const ulong max = 9001;

                    Windows7Taskbar.SetProgressValue(handle, (ulong) (progress * max), max);
                }
            }));
        }

        private void TaskDonePopup() {
            this.taskbarIcon.Dispatcher.BeginInvoke(new Action(() => {
                this.taskbarIcon.ShowBalloonTip("Operation complete", "The requested opration has completed", BalloonIcon.Info);
            }));
        }

        public void Dispose() {
            foreach (var commandRunner in boundCommandRunners) {
                UnbindFrom(commandRunner);
            }
        }
    }
}