using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using NoCap.Library;
using NoCap.Library.Destinations;
using NoCap.Library.Sources;

namespace NoCap.Plugins {
    [Export(typeof(IDestination))]
    public class Clipboard : ISource, IDestination {
        public IOperation<TypedData> Get() {
            return new EasyOperation<TypedData>((op) => {
                var thread = new Thread(() => op.Done(GetClipboardData()));

                // Clipboard object uses COM; make sure we're in STA
                thread.SetApartmentState(ApartmentState.STA);

                thread.Start();

                return null;
            });
        }

        public IOperation<TypedData> Put(TypedData data) {
            return new EasyOperation<TypedData>((op) => {
                var thread = new Thread(() => op.Done(SetClipboardData(data)));

                // Clipboard object uses COM; make sure we're in STA
                thread.SetApartmentState(ApartmentState.STA);

                thread.Start();

                return null;
            });
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

        private static TypedData SetClipboardData(TypedData data) {
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

            return data;
        }

        public IEnumerable<TypedDataType> GetInputDataTypes() {
            return new[] { TypedDataType.Text, TypedDataType.Uri, TypedDataType.Image };
        }

        public IEnumerable<TypedDataType> GetOutputDataTypes(TypedDataType input) {
            return new[] { input };
        }

        public IEnumerable<TypedDataType> GetOutputDataTypes() {
            return new[] { TypedDataType.Image, TypedDataType.Text, TypedDataType.Uri };
        }
    }
}
