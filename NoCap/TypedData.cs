﻿namespace NoCap {
    public enum TypedDataType {
        Any = 0,
        Image,
        Text,
        Uri,

        // Add new destination types here

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
    }
}
