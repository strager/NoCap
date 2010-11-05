using System.Windows;
using NoCap.Library;
using NoCap.Plugins.Commands;

namespace NoCap.Plugins.Editors {
    /// <summary>
    /// Interaction logic for FtpUploaderEditor.xaml
    /// </summary>
    public partial class FtpUploaderEditor : ICommandEditor {
        public readonly static DependencyProperty CommandProperty;
        public readonly static DependencyProperty InfoStuffProperty;
        
        public FtpUploader Command {
            get { return (FtpUploader) GetValue(CommandProperty); }
            set { SetValue(CommandProperty, value); }
        }

        public IInfoStuff InfoStuff {
            get { return (IInfoStuff) GetValue(InfoStuffProperty); }
            set { SetValue(InfoStuffProperty, value); }
        }

        static FtpUploaderEditor() {
            CommandProperty = DependencyProperty.Register(
                "Command",
                typeof(FtpUploader),
                typeof(FtpUploaderEditor)
            );

            InfoStuffProperty = DependencyProperty.Register(
                "InfoStuff",
                typeof(IInfoStuff),
                typeof(FtpUploaderEditor)
            );
        }

        public FtpUploaderEditor(FtpUploader command, IInfoStuff infoStuff) {
            InitializeComponent();

            Command = command;
            InfoStuff = infoStuff;
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
