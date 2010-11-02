using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using NoCap.Library;
using NoCap.Library.Util;
using NoCap.Plugins.Factories;

namespace NoCap.Plugins.Commands {
    public class Clipboard : ICommand {
        public string Name {
            get { return "Clipboard"; }
        }

        public TypedData Process(TypedData data, IMutableProgressTracker progress) {
            this.CheckValidInputType(data);

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

        public IEnumerable<TypedDataType> GetInputDataTypes() {
            return new[] { TypedDataType.None, TypedDataType.Text, TypedDataType.Uri, TypedDataType.Image };
        }

        public IEnumerable<TypedDataType> GetOutputDataTypes(TypedDataType input) {
            if (input == TypedDataType.None) {
                return new[] { TypedDataType.Image, TypedDataType.Text, TypedDataType.Uri };
            }

            return new[] { input };
        }

        public ICommandFactory GetFactory() {
            return new ClipboardFactory();
        }

        public TimeEstimate ProcessTimeEstimate {
            get {
                return TimeEstimate.NoTimeAtAll;
            }
        }
    }
}
