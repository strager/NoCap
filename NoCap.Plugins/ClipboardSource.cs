using System;
using System.Drawing;
using System.Windows.Forms;
using NoCap.Library.Sources;
using NoCap.Sources;

namespace NoCap.Plugins {
    public class ClipboardSource : ISource {
        public IOperation<TypedData> Get() {
            return new EasyOperation<TypedData>((op) => {
                var clipboardData = Clipboard.GetDataObject();

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
                    return TypedData.FromImage((Bitmap)clipboardData.GetData(DataFormats.Bitmap), clipboardText ?? "clipboard image");
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
            });
        }
    }
}