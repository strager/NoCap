using System;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Threading;
using System.Windows.Forms;
using NoCap.Extensions.Default.Factories;
using NoCap.Library;
using NoCap.Library.Progress;

namespace NoCap.Extensions.Default.Commands {
    [DataContract(Name = "Clipboard")]
    public sealed class Clipboard : ICommand, IExtensibleDataObject {
        private readonly static TimeSpan SpinTimeout = TimeSpan.FromMilliseconds(500);
        private const int RetryTimes = 40;
        private const int RetryDelay = 50;

        public string Name {
            get { return "Clipboard"; }
        }

        public TypedData Process(TypedData data, IMutableProgressTracker progress, CancellationToken cancelToken) {
            Action operation;
            bool success = false;
            Exception exception = null;

            if (data == null) {
                operation = new Action(() => {
                    progress.Status = "Reading clipboard";

                    try {
                        data = GetClipboardData();
                        success = true;
                    } catch (Exception e) {
                        exception = e;
                    }
                });
            } else {
                operation = new Action(() => {
                    progress.Status = "Saving to clipboard";

                    try {
                        SetClipboardData(data);
                        success = true;
                    } catch (Exception e) {
                        exception = e;
                    }
                });
            }

            var thread = new Thread(new ThreadStart(operation));

            // Clipboard object uses COM; make sure we're in STA
            thread.SetApartmentState(ApartmentState.STA);

            thread.Start();
            thread.Join();

            if (!success) {
                if (exception == null) {
                    throw new CommandCanceledException(this);
                }

                throw exception;
            }

            progress.Progress = 1;

            return data;
        }

        private static TypedData GetClipboardData() {
            var clipboardData = GetRawClipboardData();

            if (clipboardData == null) {
                throw new InvalidOperationException("No data in clipboard");
            }

            string clipboardText = null;
            var clipboardTextObject = clipboardData.GetData(DataFormats.UnicodeText);
                
            if (clipboardTextObject != null) {
                clipboardText = clipboardTextObject.ToString();
            }

            // Image
            if (clipboardData.GetDataPresent(DataFormats.Bitmap)) {
                return TypedData.FromImage((Bitmap) clipboardData.GetData(DataFormats.Bitmap), clipboardText ?? "clipboard image");
            }

            // File(s)
            if (clipboardData.GetDataPresent(DataFormats.FileDrop)) {
                string[] filenames = (string[]) clipboardData.GetData(DataFormats.FileDrop);
                
                // TODO support multiple files
                if (filenames.Length == 1) {
                    return TypedData.FromStream(File.Open(filenames[0], FileMode.Open, FileAccess.Read, FileShare.Read | FileShare.Delete), Path.GetFileName(filenames[0]));
                }
            }

            // TODO Handle more clipboard data types

            // No text?
            if (string.IsNullOrWhiteSpace(clipboardText)) {
                return null;
            }

            // URI (link)
            if (Uri.IsWellFormedUriString(clipboardText, UriKind.Absolute)) {
                return TypedData.FromUri(clipboardText, "clipboard uri");
            }

            // Text
            return TypedData.FromText(clipboardText, "clipboard text");
        }

        private static void SetClipboardData(TypedData data) {
            switch (data.DataType) {
                case TypedDataType.Text:
                case TypedDataType.Uri:
                    SetRawClipboardData(data.Data.ToString());

                    break;

                case TypedDataType.Image:
                    SetRawClipboardData(data.Data);

                    break;

                default:
                    // Do nothing

                    break;
            }
        }

        [STAThread]
        private static IDataObject GetRawClipboardData() {
            retry:

            try {
                return System.Windows.Forms.Clipboard.GetDataObject();
            } catch (ExternalException e) {
                bool success;

                WaitForFreeClipboard(out success);

                if (!success) {
                    throw new OperationCanceledException("Could not read data from clipboard", e);
                }

                goto retry;
            }
        }

        [STAThread]
        private static void SetRawClipboardData(object data) {
            retry:

            try {
                System.Windows.Forms.Clipboard.SetDataObject(data, true, RetryTimes, RetryDelay);
            } catch (ExternalException e) {
                bool success;

                WaitForFreeClipboard(out success);

                if (!success) {
                    throw new OperationCanceledException("Could not store data into clipboard", e);
                }

                goto retry;
            }
        }

        public ICommandFactory GetFactory() {
            return new ClipboardFactory();
        }

        public ITimeEstimate ProcessTimeEstimate {
            get {
                return TimeEstimates.Instantaneous;
            }
        }

        public bool IsValid() {
            return true;
        }

        ExtensionDataObject IExtensibleDataObject.ExtensionData {
            get;
            set;
        }

        [DllImport("user32.dll")]
        private static extern IntPtr GetOpenClipboardWindow();

        private static void WaitForFreeClipboard(out bool success) {
            success = SpinWait.SpinUntil(() => GetOpenClipboardWindow() == IntPtr.Zero, SpinTimeout);
        }
    }
}
