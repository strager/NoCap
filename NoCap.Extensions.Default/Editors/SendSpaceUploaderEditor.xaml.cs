using System.Windows;
using NoCap.Extensions.Default.Commands;
using NoCap.Library;

namespace NoCap.Extensions.Default.Editors {
    /// <summary>
    /// Interaction logic for SendSpaceUploaderEditor.xaml
    /// </summary>
    public partial class SendSpaceUploaderEditor : ICommandEditor {
        public SendSpaceUploaderEditor() {
            InitializeComponent();
        }

        private void UpdatePassword(object sender, RoutedEventArgs e) {
            var command = (SendSpaceUploader) DataContext;

            var oldPassword = command.Password;

            if (oldPassword != null) {
                oldPassword.Dispose();
            }

            command.Password = this.passwordField.SecurePassword.Copy();
        }
    }
}
