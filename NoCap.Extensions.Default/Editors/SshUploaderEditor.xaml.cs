using System.Windows;
using NoCap.Extensions.Default.Commands;
using NoCap.Library;

namespace NoCap.Extensions.Default.Editors {
    /// <summary>
    /// Interaction logic for SshUploaderEditor.xaml
    /// </summary>
    public partial class SshUploaderEditor : ICommandEditor {
        public SshUploaderEditor() {
            InitializeComponent();
        }

        private void UpdatePassword(object sender, RoutedEventArgs e) {
            var command = (SshUploader) DataContext;

            var oldPassword = command.Password;

            if (oldPassword != null) {
                oldPassword.Dispose();
            }

            command.Password = this.passwordField.SecurePassword.Copy();
        }
    }
}
