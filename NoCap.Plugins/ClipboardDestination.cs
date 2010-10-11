using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Drawing;
using System.Windows.Forms;
using NoCap.Library;
using NoCap.Library.Destinations;

namespace NoCap.Plugins {
    [Export(typeof(IDestination))]
    public class ClipboardDestination : IDestination {
        public IOperation<TypedData> Put(TypedData data) {
            return new EasyOperation<TypedData>((op) => {
                switch (data.Type) {
                    case TypedDataType.Text:
                    case TypedDataType.Uri:
                        Clipboard.SetText(data.Data.ToString(), TextDataFormat.UnicodeText);

                        break;

                    case TypedDataType.Image:
                        Clipboard.SetImage((Image) data.Data);

                        break;

                    default:
                        break;
                }

                return data;
            });
        }

        public IEnumerable<TypedDataType> GetInputDataTypes() {
            return new[] { TypedDataType.Text, TypedDataType.Uri, TypedDataType.Image };
        }

        public IEnumerable<TypedDataType> GetOutputDataTypes(TypedDataType input) {
            return new[] { input };
        }
    }
}
