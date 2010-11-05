﻿using System;
using System.Windows;
using System.Windows.Interop;
using Hardcodet.Wpf.TaskbarNotification;
using NoCap.Library;
using Windows7.DesktopIntegration;

namespace NoCap.GUI.WPF {
    class TaskTracker {
        private readonly NoCapLogo logo;
        private readonly TaskbarIcon taskbarIcon;

        private readonly CommandRunner commandRunner = new CommandRunner();

        public TaskTracker(TaskbarIcon taskbarIcon, NoCapLogo logo) {
            this.taskbarIcon = taskbarIcon;
            this.logo = logo;

            UpdateIcon(1);

            this.commandRunner.ProgressUpdated += (sender, e) => UpdateWindows(e.Progress);
            this.commandRunner.ProgressUpdated += (sender, e) => UpdateIcon(e.Progress);
            this.commandRunner.ProgressUpdated += (sender, e) => UpdateIconToolTip(e.Progress);

            this.commandRunner.TaskCompleted += (sender, e) => ShowTaskDonePopup();
        }

        private static void UpdateWindows(double progress) {
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

        public CommandTask PerformTask(ICommand command) {
            return this.commandRunner.Run(command);
        }

        private void ShowTaskDonePopup() {
            this.taskbarIcon.Dispatcher.BeginInvoke(new Action(() => {
                this.taskbarIcon.ShowBalloonTip("Operation complete", "The requested opration has completed", BalloonIcon.Info);
            }));
        }
    }
}
