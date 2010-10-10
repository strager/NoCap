﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace NoCap.Sources {
    public class ClipboardSource : ISource {
        public IOperation Get() {
            return new EasyOperation((op) => {
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
