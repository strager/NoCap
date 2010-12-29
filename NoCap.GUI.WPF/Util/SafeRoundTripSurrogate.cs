using System;
using System.CodeDom;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Runtime.Serialization;

namespace NoCap.GUI.WPF.Util {
    // TODO
    // TODO
    // TODO
    class SafeRoundTripSurrogate : IDataContractSurrogate {
        public Type GetDataContractType(Type type) {
            return type;
        }

        public object GetObjectToSerialize(object obj, Type targetType) {
            // TODO Round-trip dummy

            return obj;
        }

        public object GetDeserializedObject(object obj, Type targetType) {
            // TODO Round-trip dummy

            return obj;
        }

        public object GetCustomDataToExport(MemberInfo memberInfo, Type dataContractType) {
            throw new NotSupportedException();
        }

        public object GetCustomDataToExport(Type clrType, Type dataContractType) {
            throw new NotSupportedException();
        }

        public void GetKnownCustomDataTypes(Collection<Type> customDataTypes) {
            // TODO?
        }

        public Type GetReferencedTypeOnImport(string typeName, string typeNamespace, object customData) {
            return null;
        }

        public CodeTypeDeclaration ProcessImportedType(CodeTypeDeclaration typeDeclaration, CodeCompileUnit compileUnit) {
            return typeDeclaration;
        }
    }
}