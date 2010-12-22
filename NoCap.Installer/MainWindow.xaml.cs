using System;
using System.IO;
using vbAccelerator.Components.Shell;

namespace NoCap.Installer {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow {
        private static readonly string DefaultInstallPath
            = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "NoCap",
                "Application"
            );

        public MainWindow() {
            InitializeComponent();
        }

        private static void EnsureDirectoryExists(string directory) {
            if (string.IsNullOrEmpty(directory)) {
                return;
            }

            if (Directory.Exists(directory)) {
                return;
            }

            EnsureDirectoryExists(Path.GetDirectoryName(directory));

            Directory.CreateDirectory(directory);
        }

        private static void InstallBinaries(string installPath) {
            EnsureDirectoryExists(installPath);

            var zipData = Properties.Resources.InstallFiles;

            using (var zipStream = new MemoryStream(zipData)) {
                var zip = new Chiron.ZipArchive(zipStream, FileAccess.Read);

                foreach (var file in zip.Files) {
                    string outputFileName = Path.Combine(installPath, file.Name);
                    EnsureDirectoryExists(Path.GetDirectoryName(outputFileName));

                    file.CopyToFile(outputFileName);
                }
            }
        }

        private static void InstallStartMenuEntries(string installPath) {
            string startMenuPath = Environment.GetFolderPath(Environment.SpecialFolder.StartMenu);
            string noCapMenuPath = Path.Combine(startMenuPath, "NoCap");

            EnsureDirectoryExists(noCapMenuPath);

            string shortcutPath = Path.Combine(noCapMenuPath, "NoCap.lnk");

            using (var shortcut = new ShellLink()) {
                shortcut.Target = Path.Combine(installPath, "NoCap.exe");
                shortcut.WorkingDirectory = installPath;
                shortcut.Description = "NoCap";
                shortcut.DisplayMode = ShellLink.LinkDisplayMode.edmNormal;
                shortcut.Save(shortcutPath);
            }
        }

        private void Install(string installPath) {
            InstallBinaries(installPath);

            if (this.startMenuEntry.IsChecked == true) {
                InstallStartMenuEntries(installPath);
            }
        }

        private void Install(object sender, System.Windows.RoutedEventArgs e) {
            Install(DefaultInstallPath);
        }

        private void Cancel(object sender, System.Windows.RoutedEventArgs e) {
            Close();
        }
    }
}
