using System.IO;

namespace NoCap.Installer {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow {
        public MainWindow() {
            InitializeComponent();

            var zipData = Properties.Resources.InstallFiles;

            using (var zipStream = new MemoryStream(zipData)) {
                var zip = new Chiron.ZipArchive(zipStream, FileAccess.Read);
                var files = zip.Files;
            }
        }
    }
}
