using System;
using System.Drawing;

namespace NoCap.Library {
    public enum TypedDataType {
        Any = 0,
        Image,
        Text,
        Uri,
        RawData,

        // Add new types here

        User = 9001
    }

    public class TypedData {
        public TypedDataType Type {
            get;
            private set;
        }

        public object Data {
            get;
            private set;
        }

        public string Name {
            get;
            private set;
        }

        public TypedData(TypedDataType type, object data, string name) {
            Type = type;
            Data = data;
            Name = name;
        }

        public static TypedData FromUri(string uri, string name) {
            return new TypedData(TypedDataType.Uri, uri, name);
        }

        public static TypedData FromUri(Uri uri, string name) {
            return FromUri(uri.ToString(), name);
        }

        public static TypedData FromImage(Image image, string name) {
            return new TypedData(TypedDataType.Image, image, name);
        }

        public static TypedData FromText(string text, string name) {
            return new TypedData(TypedDataType.Text, text, name);
        }

        public static TypedData FromRawData(byte[] rawData, string name) {
            return new TypedData(TypedDataType.RawData, rawData, name);
        }

        public override string ToString() {
            return string.Format("{0} ({1}: {2})", Name, Type, Data);
        }
    }
}
