using System;
using System.IO;
using System.Net;
using System.Threading;
using System.Windows;
using AlexPilotti.FTPS.Client;
using NoCap.Extensions.Default.Commands;
using NoCap.Extensions.Default.Helpers;
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

        private void TestFtp(object sender, RoutedEventArgs ev) {
            var connectionTesterWindow = new ConnectionTester();

            var ftpUploader = (FtpUploader) DataContext;
            var testThread = new Thread(() => TestFtp(connectionTesterWindow, ftpUploader));
            testThread.Start();

            connectionTesterWindow.ShowDialog();

            testThread.Abort(); // I'M SORRY !!!
        }

        private static void TestFtp(ConnectionTester connectionTesterWindow, FtpUploader command) {
            connectionTesterWindow.StatusText = string.Format("Testing FTP connection to {0}...", command.Host);
            connectionTesterWindow.TryStatus = TryStatus.Trying;

            using (var client = new FTPSClient()) {
                try {
                    const int timeout = 20 * 1000;

                    client.Connect(command.Host, command.Port, new NetworkCredential(command.UserName, command.Password), 0, null, null, 0, 0, 0, timeout);
                } catch (TimeoutException) {
                    connectionTesterWindow.StatusText = "Connection timed out";
                    connectionTesterWindow.TryStatus = TryStatus.Failure;

                    return;
                } catch (Exception e) {
                    connectionTesterWindow.StatusText = string.Format("Connection to FTP server failed: {0}", e.Message);
                    connectionTesterWindow.TryStatus = TryStatus.Failure;

                    return;
                }

                connectionTesterWindow.StatusText = "Connection successful";
                connectionTesterWindow.TryStatus = TryStatus.Success;
            }
        }
    }
}
