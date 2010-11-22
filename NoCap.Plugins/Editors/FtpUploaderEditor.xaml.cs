using System.Windows;
using NoCap.Library;
using NoCap.Plugins.Commands;

namespace NoCap.Plugins.Editors {
    /// <summary>
    /// Interaction logic for FtpUploaderEditor.xaml
    /// </summary>
    public partial class FtpUploaderEditor : ICommandEditor {
        public readonly static DependencyProperty CommandProperty;
        
        public FtpUploader Command {
            get { return (FtpUploader) GetValue(CommandProperty); }
            set { SetValue(CommandProperty, value); }
        }

        static FtpUploaderEditor() {
            CommandProperty = DependencyProperty.Register(
                "Command",
                typeof(FtpUploader),
                typeof(FtpUploaderEditor)
            );
        }

        public FtpUploaderEditor(FtpUploader command) {
            InitializeComponent();

            Command = command;
        }

        private void UpdatePassword(object sender, RoutedEventArgs e) {
            var oldPassword = Command.Password;

            if (oldPassword != null) {
                oldPassword.Dispose();
            }

            Command.Password = this.passwordField.SecurePassword.Copy();
        }
    }
}
