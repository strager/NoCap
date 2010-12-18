using System;
using System.Drawing;
using System.IO;
using System.Runtime.Serialization;
using System.Threading;
using System.Windows.Forms;
using NoCap.Extensions.Default.Factories;
using NoCap.Library;
using NoCap.Library.Progress;

namespace NoCap.Extensions.Default.Commands {
    [DataContract(Name = "Clipboard")]
    public sealed class Clipboard : ICommand, IExtensibleDataObject {
        public string Name {
            get { return "Clipboard"; }
        }

        public TypedData Process(TypedData data, IMutableProgressTracker progress, CancellationToken cancelToken) {
            Action operation;

            if (data == null) {
                operation = () => data = GetClipboardData();
            } else {
                operation = () => SetClipboardData(data);
            }

            var thread = new Thread(new ThreadStart(operation));

            // Clipboard object uses COM; make sure we're in STA
            thread.SetApartmentState(ApartmentState.STA);

            thread.Start();
            thread.Join();

            progress.Progress = 1;

            return data;
        }

        private static TypedData GetClipboardData() {
            var clipboardData = System.Windows.Forms.Clipboard.GetDataObject();

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
                    System.Windows.Forms.Clipboard.SetText(data.Data.ToString(), TextDataFormat.UnicodeText);

                    break;

                case TypedDataType.Image:
                    System.Windows.Forms.Clipboard.SetImage((Image) data.Data);

                    break;

                default:
                    break;
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
    }
}
