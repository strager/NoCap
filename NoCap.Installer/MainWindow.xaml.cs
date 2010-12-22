using System;
using System.IO;

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

        private static void InstallTo(string outputPath) {
            EnsureDirectoryExists(outputPath);

            var zipData = Properties.Resources.InstallFiles;

            using (var zipStream = new MemoryStream(zipData)) {
                var zip = new Chiron.ZipArchive(zipStream, FileAccess.Read);

                foreach (var file in zip.Files) {
                    string outputFileName = Path.Combine(outputPath, file.Name);
                    EnsureDirectoryExists(Path.GetDirectoryName(outputFileName));

                    file.CopyToFile(outputFileName);
                }
            }
        }

        private void Install(object sender, System.Windows.RoutedEventArgs e) {
            InstallTo(DefaultInstallPath);
        }

        private void Cancel(object sender, System.Windows.RoutedEventArgs e) {
            Close();
        }
    }
}
