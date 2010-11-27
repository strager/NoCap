using System.Windows;
using NoCap.Extensions.Default.Commands;
using NoCap.Library;

namespace NoCap.Extensions.Default.Editors {
    /// <summary>
    /// Interaction logic for FtpUploaderEditor.xaml
    /// </summary>
    public partial class FtpUploaderEditor : ICommandEditor {
        public FtpUploaderEditor() {
            InitializeComponent();
        }

        private void UpdatePassword(object sender, RoutedEventArgs e) {
            var command = (FtpUploader) DataContext;

            var oldPassword = command.Password;

            if (oldPassword != null) {
                oldPassword.Dispose();
            }

            command.Password = this.passwordField.SecurePassword.Copy();
        }
    }
}
