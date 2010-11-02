using System;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Hardcodet.Wpf.TaskbarNotification;
using NoCap.GUI.WPF.Settings;
using NoCap.GUI.WPF.Settings.Editors;
using NoCap.Library;
using NoCap.Library.Util;
using Windows7.DesktopIntegration;
using WinputDotNet;
using ICommand = NoCap.Library.ICommand;
using PixelFormat = System.Drawing.Imaging.PixelFormat;

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
            this.progressTracker.PropertyChanged += (sender, e) => {
                if (e.PropertyName == "Progress") {
                    UpdateProgress(this.progressTracker);
                }
            };

            this.taskbarIcon = (TaskbarIcon) Resources["taskbarIcon"];
            this.taskbarIcon.Icon = MakeIcon(128);

            LoadBindings();
            LoadSettings();
        }

        private Icon MakeIcon(int size) {
            var iconSource = (DrawingImage) Resources["logo"];

            var visual = new DrawingVisual();

            using (var context = visual.RenderOpen()) {
                context.DrawImage(iconSource, new Rect(0, 0, size, size));
            }

            var target = new RenderTargetBitmap(size, size, 96, 96, PixelFormats.Pbgra32);

            target.Render(visual);

            return BitmapSourceToIcon(target);
        }

        private static Icon BitmapSourceToIcon(BitmapSource target) {
            using (var bitmap = BitmapSourceToBitmap(target)) {
                var iconHandle = bitmap.GetHicon();

                return Icon.FromHandle(iconHandle);
            }
        }

        private static Bitmap BitmapSourceToBitmap(BitmapSource target) {
            if (target.Format != PixelFormats.Pbgra32) {
                throw new NotImplementedException("Must use PABGR32 format");
            }

            Int32[] data = new Int32[target.PixelWidth * target.PixelHeight];
            int stride = Math.Max(512, target.PixelWidth);  // CopyPixels needs stride > 512

            target.CopyPixels(data, stride, 0);

            var bitmap = new Bitmap(target.PixelWidth, target.PixelHeight, PixelFormat.Format32bppPArgb);

            var bitmapData = bitmap.LockBits(
                new Rectangle(0, 0, bitmap.Width, bitmap.Height),
                ImageLockMode.WriteOnly, bitmap.PixelFormat
            );
            
            bitmapData.Stride = stride;
            Marshal.Copy(data, 0, bitmapData.Scan0, data.Length);

            bitmap.UnlockBits(bitmapData);

            return bitmap;
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

            SetUpEverything(this.settings);
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

        private void UpdateProgress(IProgressTracker progress) {
            SetProgress(progress.Progress);
        }

        private void SetProgress(double progress) {
            if (this.settingsWindow != null) {
                SetWindowProgress(this.settingsWindow, progress);
            }

            // TODO Taskbar icon
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
            try {
                var data = highLevelCommand.Process(null, progress);

                if (data != null) {
                    data.Dispose();
                }
            } catch (CommandCancelledException) {
                // Eat it.
            }
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
